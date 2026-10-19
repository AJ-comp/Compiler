using System.Collections.Generic;
using System.Text;

namespace Janglim.FrontEnd.Grammars.Ebnf
{
    /// <summary>The outcome of reading grammar text: a runnable grammar, or the errors that stopped it.</summary>
    public sealed class EbnfReadResult
    {
        public EbnfGrammar Grammar { get; }
        public IReadOnlyList<string> Errors { get; }
        public bool Success => Grammar != null;

        public EbnfReadResult(EbnfGrammar grammar, IReadOnlyList<string> errors)
        {
            Grammar = grammar;
            Errors = errors;
        }
    }

    /// <summary>
    /// Reads EBNF grammar text into a runnable <see cref="EbnfGrammar"/>. Notation:
    /// <code>
    ///   Expr : Expr '+' Term | Term ;        # non-terminal rule (the FIRST rule's head is the start symbol)
    ///   List : id ( ',' id )* ;              # groups '( … )' and quantifiers '* + ?'
    ///   Throw : 'throw' {NoLineBreak()}? id ; # a condition registered by the C# host
    ///   id   := "[a-zA-Z_][a-zA-Z0-9_]*" ;   # token rule (a named regex terminal)
    ///   ch   := CharLiteral "'[^'\r\n]'" ;  # a typed token; its pattern is a raw regex
    /// </code>
    /// <c>'x'</c> is a literal terminal, <c>"x"</c> is a regex pattern, a bare name refers to a rule or
    /// token, <c>|</c> separates alternatives, <c>( … )</c> groups, <c>* + ?</c> repeat/optional a
    /// preceding item, <c>{Name()}?</c> evaluates a registered zero-width condition, and <c>#</c>
    /// starts a line comment. Token rules optionally specify
    /// <c>Identifier</c>, <c>StringLiteral</c>, <c>CharLiteral</c>, <c>NumberLiteral</c>,
    /// <c>Comment</c>, or <c>Reject</c> before the pattern; comments are excluded from parsing
    /// and Reject rules mark lexical errors. Neither kind can appear in productions;
    /// omitted kinds default to <c>Identifier</c>. Inside patterns, double a double quote
    /// to include it in the regex. Backslashes are preserved verbatim; single-quoted
    /// literals retain their original syntax, including adjacent literals.
    /// </summary>
    public static class EbnfGrammarReader
    {
        public static EbnfReadResult Read(string text)
            => Read(text, null);

        /// <summary>
        /// Reads grammar text with host-registered conditions. The predicate syntax is exactly
        /// <c>{Name()}?</c>: a registered name, no arguments, and no embedded C# statements.
        /// </summary>
        public static EbnfReadResult Read(string text, EbnfReaderOptions options)
        {
            try
            {
                var tokens = Tokenize(text ?? string.Empty);
                var model = new Parser(tokens).ParseGrammar();
                return new EbnfReadResult(new EbnfGrammar(model, options), new List<string>());
            }
            catch (EbnfException ex)
            {
                return new EbnfReadResult(null, new List<string> { ex.Message });
            }
            catch (System.Exception ex)
            {
                // Read() promises errors-in-a-result, not a throw. An unexpected exception while building
                // the grammar (anything that is not one of our own EbnfExceptions) is surfaced as an error
                // rather than crashing the caller. (StackOverflowException is uncatchable in .NET and is
                // not covered here; groups nest only as deep as the source text.)
                return new EbnfReadResult(null, new List<string> { $"internal error: {ex.Message}" });
            }
        }

        // ---------- lexer ----------

        private enum Kind { Ident, Colon, ColonEquals, Pipe, Semicolon, Literal, Pattern, LParen, RParen, LBrace, RBrace, Star, Plus, Question, Eof }

        private struct Tok
        {
            public Kind Kind;
            public string Text;
            public int Line;
        }

        private static List<Tok> Tokenize(string text)
        {
            var tokens = new List<Tok>();
            int i = 0, line = 1;

            while (i < text.Length)
            {
                char c = text[i];

                if (c == '\n') { line++; i++; continue; }
                if (char.IsWhiteSpace(c)) { i++; continue; }
                if (c == '#') { while (i < text.Length && text[i] != '\n') i++; continue; }

                if (c == ':')
                {
                    if (i + 1 < text.Length && text[i + 1] == '=') { tokens.Add(new Tok { Kind = Kind.ColonEquals, Line = line }); i += 2; }
                    else { tokens.Add(new Tok { Kind = Kind.Colon, Line = line }); i++; }
                    continue;
                }
                if (c == '|') { tokens.Add(new Tok { Kind = Kind.Pipe, Line = line }); i++; continue; }
                if (c == ';') { tokens.Add(new Tok { Kind = Kind.Semicolon, Line = line }); i++; continue; }
                if (c == '(') { tokens.Add(new Tok { Kind = Kind.LParen, Line = line }); i++; continue; }
                if (c == ')') { tokens.Add(new Tok { Kind = Kind.RParen, Line = line }); i++; continue; }
                if (c == '{') { tokens.Add(new Tok { Kind = Kind.LBrace, Line = line }); i++; continue; }
                if (c == '}') { tokens.Add(new Tok { Kind = Kind.RBrace, Line = line }); i++; continue; }
                if (c == '*') { tokens.Add(new Tok { Kind = Kind.Star, Line = line }); i++; continue; }
                if (c == '+') { tokens.Add(new Tok { Kind = Kind.Plus, Line = line }); i++; continue; }
                if (c == '?') { tokens.Add(new Tok { Kind = Kind.Question, Line = line }); i++; continue; }

                if (c == '\'' || c == '"')
                {
                    char quote = c;
                    int openLine = line;
                    i++;
                    var sb = new StringBuilder();
                    while (i < text.Length)
                    {
                        if (text[i] == quote)
                        {
                            // Only regex patterns support doubled quotes. Applying this to literals
                            // would change the existing sequence 'a''b' into the single literal a'b.
                            if (quote == '"' && i + 1 < text.Length && text[i + 1] == quote)
                            {
                                sb.Append(quote);
                                i += 2;
                                continue;
                            }
                            break;
                        }
                        if (text[i] == '\n') line++;
                        sb.Append(text[i]);
                        i++;
                    }
                    if (i >= text.Length)
                        throw new EbnfException($"line {openLine}: unterminated {(quote == '\'' ? "literal" : "pattern")}");
                    i++; // closing quote
                    tokens.Add(new Tok { Kind = quote == '\'' ? Kind.Literal : Kind.Pattern, Text = sb.ToString(), Line = openLine });
                    continue;
                }

                if (char.IsLetter(c) || c == '_')
                {
                    int start = i;
                    while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] == '_')) i++;
                    tokens.Add(new Tok { Kind = Kind.Ident, Text = text.Substring(start, i - start), Line = line });
                    continue;
                }

                throw new EbnfException($"line {line}: unexpected character '{c}'");
            }

            tokens.Add(new Tok { Kind = Kind.Eof, Line = line });
            return tokens;
        }

        // ---------- parser (recursive descent) ----------
        //   grammar    := rule* EOF
        //   rule       := IDENT ( ':' altList | ':=' tokenKind? PATTERN ) ';'
        //   tokenKind  := 'Identifier' | 'StringLiteral' | 'CharLiteral' | 'NumberLiteral' | 'Comment' | 'Reject'
        //   altList    := sequence ( '|' sequence )*
        //   sequence   := factor+
        //   factor     := predicate | atom ( '*' | '+' | '?' )?
        //   predicate  := '{' IDENT '(' ')' '}' '?'
        //   atom       := IDENT | LITERAL | '(' altList ')'

        private sealed class Parser
        {
            private readonly List<Tok> _tokens;
            private int _pos;

            public Parser(List<Tok> tokens) { _tokens = tokens; }

            private Tok Cur => _tokens[_pos];
            private bool Is(Kind k) => Cur.Kind == k;
            private Tok Take() => _tokens[_pos++];

            private Tok Expect(Kind k, string what)
            {
                if (!Is(k)) throw new EbnfException($"line {Cur.Line}: expected {what}");
                return Take();
            }

            public EbnfModel ParseGrammar()
            {
                var model = new EbnfModel();
                while (!Is(Kind.Eof)) ParseRule(model);

                if (model.Rules.Count == 0) throw new EbnfException("the grammar has no non-terminal rules");
                return model;
            }

            private void ParseRule(EbnfModel model)
            {
                string name = Expect(Kind.Ident, "a rule name").Text;

                if (Is(Kind.ColonEquals))
                {
                    Take();
                    var tokenKind = EbnfTokenKind.Identifier;
                    if (Is(Kind.Ident))
                    {
                        var kind = Take();
                        switch (kind.Text)
                        {
                            case "Identifier": tokenKind = EbnfTokenKind.Identifier; break;
                            case "StringLiteral": tokenKind = EbnfTokenKind.StringLiteral; break;
                            case "CharLiteral": tokenKind = EbnfTokenKind.CharLiteral; break;
                            case "NumberLiteral": tokenKind = EbnfTokenKind.NumberLiteral; break;
                            case "Comment": tokenKind = EbnfTokenKind.Comment; break;
                            case "Reject": tokenKind = EbnfTokenKind.Reject; break;
                            default:
                                throw new EbnfException($"line {kind.Line}: unknown token kind '{kind.Text}'; expected Identifier, StringLiteral, CharLiteral, NumberLiteral, Comment, or Reject");
                        }
                    }
                    string pattern = Expect(Kind.Pattern, "a \"...\" pattern").Text;
                    Expect(Kind.Semicolon, "';'");
                    model.Tokens.Add(new EbnfTokenDef { Name = name, Pattern = pattern, Kind = tokenKind });
                }
                else if (Is(Kind.Colon))
                {
                    Take();
                    var rule = new EbnfRuleDef { Name = name };
                    foreach (var alt in ParseAltList()) rule.Alternatives.Add(alt);
                    Expect(Kind.Semicolon, "';'");
                    model.Rules.Add(rule);
                }
                else throw new EbnfException($"line {Cur.Line}: expected ':' or ':=' after '{name}'");
            }

            private List<List<EbnfSym>> ParseAltList()
            {
                var alternatives = new List<List<EbnfSym>> { ParseSequence() };
                while (Is(Kind.Pipe)) { Take(); alternatives.Add(ParseSequence()); }
                return alternatives;
            }

            private List<EbnfSym> ParseSequence()
            {
                var sequence = new List<EbnfSym>();
                while (Is(Kind.Ident) || Is(Kind.Literal) || Is(Kind.LParen) || Is(Kind.LBrace))
                    sequence.Add(ParseFactor());

                if (sequence.Count == 0) throw new EbnfException($"line {Cur.Line}: empty alternative");
                return sequence;
            }

            private EbnfSym ParseFactor()
            {
                if (Is(Kind.LBrace)) return ParsePredicate();
                var atom = ParseAtom();

                if (Is(Kind.Star)) { atom.SourceLine = Take().Line; atom.Quantifier = EbnfQuantifier.ZeroOrMore; }
                else if (Is(Kind.Plus)) { atom.SourceLine = Take().Line; atom.Quantifier = EbnfQuantifier.OneOrMore; }
                else if (Is(Kind.Question)) { atom.SourceLine = Take().Line; atom.Quantifier = EbnfQuantifier.Optional; }

                // one quantifier per atom — a stacked '*'/'+'/'?' (e.g. `a*+`) is a clear error here,
                // rather than a confusing "expected ';'" further along.
                if (Is(Kind.Star) || Is(Kind.Plus) || Is(Kind.Question))
                    throw new EbnfException($"line {Cur.Line}: a quantifier (* + ?) cannot be stacked");

                return atom;
            }

            private EbnfSym ParsePredicate()
            {
                var opening = Take();
                var name = Expect(Kind.Ident, "a registered predicate name in '{Name()}?'");
                Expect(Kind.LParen, "'(' after the predicate name");
                Expect(Kind.RParen, "')' (predicate calls take no arguments)");
                Expect(Kind.RBrace, "'}' after the predicate call");
                Expect(Kind.Question, "'?' after '{Name()}'");

                // This '?' belongs to the condition syntax. Treating another quantifier as an
                // optional condition would let a false check be skipped, changing its meaning.
                if (Is(Kind.Star) || Is(Kind.Plus) || Is(Kind.Question))
                    throw new EbnfException($"line {Cur.Line}: a predicate cannot have a quantifier (* + ?)");

                return new EbnfSym { IsPredicate = true, Text = name.Text, SourceLine = opening.Line };
            }

            private EbnfSym ParseAtom()
            {
                if (Is(Kind.Ident))
                {
                    var token = Take();
                    return new EbnfSym { Text = token.Text, SourceLine = token.Line };
                }
                if (Is(Kind.Literal))
                {
                    var token = Take();
                    return new EbnfSym { IsLiteral = true, Text = token.Text, SourceLine = token.Line };
                }
                if (Is(Kind.LParen))
                {
                    var opening = Take();
                    var alternatives = ParseAltList();
                    Expect(Kind.RParen, "')'");
                    return new EbnfSym { IsGroup = true, Group = alternatives, SourceLine = opening.Line };
                }
                throw new EbnfException($"line {Cur.Line}: expected a name, a '...' literal, or '('");
            }
        }
    }
}
