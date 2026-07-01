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
    ///   id   := "[a-zA-Z_][a-zA-Z0-9_]*" ;   # token rule (a named regex terminal)
    /// </code>
    /// <c>'x'</c> is a literal terminal, <c>"x"</c> is a regex pattern, a bare name refers to a rule or
    /// token, <c>|</c> separates alternatives, <c>( … )</c> groups, <c>* + ?</c> repeat/optional a
    /// preceding item, and <c>#</c> starts a line comment.
    /// </summary>
    public static class EbnfGrammarReader
    {
        public static EbnfReadResult Read(string text)
        {
            try
            {
                var tokens = Tokenize(text ?? string.Empty);
                var model = new Parser(tokens).ParseGrammar();
                return new EbnfReadResult(new EbnfGrammar(model), new List<string>());
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

        private enum Kind { Ident, Colon, ColonEquals, Pipe, Semicolon, Literal, Pattern, LParen, RParen, Star, Plus, Question, Eof }

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
                if (c == '*') { tokens.Add(new Tok { Kind = Kind.Star, Line = line }); i++; continue; }
                if (c == '+') { tokens.Add(new Tok { Kind = Kind.Plus, Line = line }); i++; continue; }
                if (c == '?') { tokens.Add(new Tok { Kind = Kind.Question, Line = line }); i++; continue; }

                if (c == '\'' || c == '"')
                {
                    char quote = c;
                    int openLine = line;
                    i++;
                    var sb = new StringBuilder();
                    while (i < text.Length && text[i] != quote)
                    {
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
        //   rule       := IDENT ( ':' altList | ':=' PATTERN ) ';'
        //   altList    := sequence ( '|' sequence )*
        //   sequence   := factor+
        //   factor     := atom ( '*' | '+' | '?' )?
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
                    string pattern = Expect(Kind.Pattern, "a \"...\" pattern").Text;
                    Expect(Kind.Semicolon, "';'");
                    model.Tokens.Add(new EbnfTokenDef { Name = name, Pattern = pattern });
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
                while (Is(Kind.Ident) || Is(Kind.Literal) || Is(Kind.LParen))
                    sequence.Add(ParseFactor());

                if (sequence.Count == 0) throw new EbnfException($"line {Cur.Line}: empty alternative");
                return sequence;
            }

            private EbnfSym ParseFactor()
            {
                var atom = ParseAtom();

                if (Is(Kind.Star)) { Take(); atom.Quantifier = EbnfQuantifier.ZeroOrMore; }
                else if (Is(Kind.Plus)) { Take(); atom.Quantifier = EbnfQuantifier.OneOrMore; }
                else if (Is(Kind.Question)) { Take(); atom.Quantifier = EbnfQuantifier.Optional; }

                // one quantifier per atom — a stacked '*'/'+'/'?' (e.g. `a*+`) is a clear error here,
                // rather than a confusing "expected ';'" further along.
                if (Is(Kind.Star) || Is(Kind.Plus) || Is(Kind.Question))
                    throw new EbnfException($"line {Cur.Line}: a quantifier (* + ?) cannot be stacked");

                return atom;
            }

            private EbnfSym ParseAtom()
            {
                if (Is(Kind.Ident)) return EbnfSym.Ref(Take().Text);
                if (Is(Kind.Literal)) return EbnfSym.Literal(Take().Text);
                if (Is(Kind.LParen))
                {
                    Take();
                    var alternatives = ParseAltList();
                    Expect(Kind.RParen, "')'");
                    return EbnfSym.GroupOf(alternatives);
                }
                throw new EbnfException($"line {Cur.Line}: expected a name, a '...' literal, or '('");
            }
        }
    }
}
