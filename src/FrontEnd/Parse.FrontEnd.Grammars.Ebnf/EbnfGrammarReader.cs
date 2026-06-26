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
    ///   id   := "[a-zA-Z_][a-zA-Z0-9_]*" ;   # token rule (a named regex terminal)
    /// </code>
    /// <c>'x'</c> is a literal terminal, <c>"x"</c> is a regex pattern, a bare name refers to a rule or
    /// token, <c>|</c> separates alternatives, and <c>#</c> starts a line comment.
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
        }

        // ---------- lexer ----------

        private enum Kind { Ident, Colon, ColonEquals, Pipe, Semicolon, Literal, Pattern, Eof }

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
        //   grammar  := rule* EOF
        //   rule     := IDENT ( ':' altList | ':=' PATTERN ) ';'
        //   altList  := sequence ( '|' sequence )*
        //   sequence := ( IDENT | LITERAL )+

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
                    rule.Alternatives.Add(ParseSequence());
                    while (Is(Kind.Pipe)) { Take(); rule.Alternatives.Add(ParseSequence()); }
                    Expect(Kind.Semicolon, "';'");
                    model.Rules.Add(rule);
                }
                else throw new EbnfException($"line {Cur.Line}: expected ':' or ':=' after '{name}'");
            }

            private List<EbnfSym> ParseSequence()
            {
                var sequence = new List<EbnfSym>();
                while (Is(Kind.Ident) || Is(Kind.Literal))
                    sequence.Add(Is(Kind.Ident) ? EbnfSym.Ref(Take().Text) : EbnfSym.Literal(Take().Text));

                if (sequence.Count == 0) throw new EbnfException($"line {Cur.Line}: empty alternative");
                return sequence;
            }
        }
    }
}
