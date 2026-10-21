using Janglim;
using Janglim.FrontEnd.RegularGrammar;
using System.Collections.Generic;
using System.Linq;

namespace Janglim.FrontEnd.Grammars.Ebnf
{
    /// <summary>
    /// A <see cref="Grammar"/> built dynamically from a parsed <see cref="EbnfModel"/> — i.e. from EBNF
    /// text rather than from C# fields. The base Grammar normally discovers its symbols by reflecting
    /// over the subclass's fields; a dynamic grammar has none, so it registers its terminals and
    /// non-terminals straight into the public collections (TerminalSet / NonTerminalMultiples), allocates
    /// their keys via the protected key manager, wires up the rules with AddItem, and marks the start
    /// symbol with the SetStartSymbol hook.
    ///
    /// Groups (<c>( … )</c>) and quantifiers (<c>* + ?</c>) in the model are lowered here, reusing the
    /// exact same machinery as the C# grammar API: a group becomes an anonymous non-terminal, and a
    /// quantifier calls <see cref="Symbol.ZeroOrMore"/>/<see cref="Symbol.OneOrMore"/>/<see cref="Symbol.Optional"/>.
    /// The auto-generated non-terminals those create register themselves through the base Grammar's
    /// AutoGenerator callback, and a final <see cref="Grammar.Optimization"/> absorbs optionals / flattens
    /// them — so EBNF text and the C# API produce identical grammars.
    /// </summary>
    public sealed class EbnfGrammar : Grammar
    {
        private readonly HashSet<Terminal> rejectedTokens = new HashSet<Terminal>();
        private readonly HashSet<NonTerminal> namedRules = new HashSet<NonTerminal>();

        protected override IEnumerable<NonTerminal> NamedRulesForDiagnostics => namedRules;

        public override NonTerminal EbnfRoot => StartSymbol;

        /// <summary>Whether a lexical-only token explicitly marks input that must be rejected.</summary>
        public bool IsRejectedToken(Terminal terminal) => rejectedTokens.Contains(terminal);

        public EbnfGrammar(EbnfModel model) : this(model, null) { }

        public EbnfGrammar(EbnfModel model, EbnfReaderOptions options)
        {
            // base() already ran: reflected fields (none here), wired AutoGenerator to this.keyManager /
            // this.RegistAutoGenerateNT, and added the EndMarker to TerminalSet.

            if (model.Rules.Count == 0) throw new EbnfException("the grammar has no rules");
            var predicates = options?.SnapshotPredicates() ?? new Dictionary<string, GrammarPredicate>();
            EbnfRepetitionValidation.Validate(model);

            // 1. Token rules -> meaningful regex terminals, looked up by name. All regex
            // token kinds preserve authored patterns, including explicit boundary assertions.
            var tokens = new Dictionary<string, Terminal>();
            foreach (var tok in model.Tokens)
            {
                // The native lexer deduplicates equal token types and patterns. Reject shares
                // the raw numeric-regex path; do not let a valid number silently hide a trap.
                if ((tok.Kind == EbnfTokenKind.NumberLiteral || tok.Kind == EbnfTokenKind.Reject) &&
                    tokens.Values.Any(existing => existing.TokenType == TokenType.Literal.Digit10 &&
                        existing.Value == tok.Pattern &&
                        IsRejectedToken(existing) != (tok.Kind == EbnfTokenKind.Reject)))
                    throw new EbnfException($"token '{tok.Name}' has the same pattern as a conflicting NumberLiteral or Reject rule");
                var t = new Terminal(ResolveTokenType(tok.Kind), tok.Pattern, tok.Name,
                    tok.Kind != EbnfTokenKind.Comment && tok.Kind != EbnfTokenKind.Reject, true);
                RegisterTerminal(t);
                if (tok.Kind == EbnfTokenKind.Reject) rejectedTokens.Add(t);
                tokens[tok.Name] = t;
            }

            // 2. literal terminals ( 'x' ), created on demand and de-duplicated by value
            var literals = new Dictionary<string, Terminal>();
            Terminal Literal(string value)
            {
                if (literals.TryGetValue(value, out var found)) return found;

                bool isWord = value.Length > 0 && (char.IsLetter(value[0]) || value[0] == '_');
                // A word-like keyword literal ('qreg') is a literal like a symbol one ('['), so bWordPattern
                // is false for both — that sorts these literals AHEAD of the identifier token rule, so a
                // keyword wins an equal-input-length tie against the identifier. A longer
                // identifier match always wins, independently of regex pattern text length.
                var t = isWord
                    ? new Terminal(TokenType.Keyword, value, value, false, false)
                    : new Terminal(TokenType.Operator, value, value, false, false);
                RegisterTerminal(t);
                literals[value] = t;
                return t;
            }

            // 3. one non-terminal per named rule; the first rule's head is the start symbol
            string startName = model.Rules[0].Name;
            var nonterminals = new Dictionary<string, NonTerminal>();
            foreach (var rule in model.Rules)
            {
                if (nonterminals.ContainsKey(rule.Name)) continue;

                var nt = new NonTerminal(rule.Name, rule.Name == startName);
                keyManager.AllocateUniqueKey(nt);
                NonTerminalMultiples.Add(nt);
                namedRules.Add(nt);
                nonterminals[rule.Name] = nt;
            }

            // an anonymous non-terminal that holds a '( … )' group's alternatives
            int groupCount = 0;
            NonTerminal MakeGroup(List<List<EbnfSym>> alternatives)
            {
                // A helper must have its own symbol key even when a source rule or token
                // uses the usual helper prefix. KeyManager identifies names when allocating keys.
                string name;
                do { name = $"__grp{++groupCount}"; }
                while (nonterminals.ContainsKey(name) || tokens.ContainsKey(name));

                var nt = new NonTerminal(name, false);
                keyManager.AllocateUniqueKey(nt);
                NonTerminalMultiples.Add(nt);
                foreach (var alt in alternatives)
                    AddAlternative(nt, alt.Select(Resolve).ToList());
                return nt;
            }

            // Each occurrence has its own zero-width production. Keeping it as a named (rather
            // than flattenable auto-generated) node preserves the exact token boundary at which
            // the parser evaluates the condition, including consecutive conditions.
            int predicateCount = 0;
            NonTerminal MakePredicate(EbnfSym sym)
            {
                string location = sym.SourceLine > 0 ? $"line {sym.SourceLine}: " : string.Empty;
                if (sym.Quantifier != EbnfQuantifier.None)
                    throw new EbnfException($"{location}a predicate cannot have a quantifier (* + ?)");
                if (sym.Text == null || !predicates.TryGetValue(sym.Text, out var predicate))
                    throw new EbnfException($"{location}predicate '{sym.Text}' is not registered; register its C# callback before reading the grammar");

                string name;
                do { name = $"__predicate{++predicateCount}"; }
                while (model.Rules.Any(rule => rule.Name == name) || model.Tokens.Any(token => token.Name == name));

                var nt = new NonTerminal(name, false);
                keyManager.AllocateUniqueKey(nt);
                nt.Add(new NonTerminalConcat(new Epsilon()) { Predicate = predicate });
                NonTerminalMultiples.Add(nt);
                return nt;
            }

            // resolve one EBNF symbol to a grammar Symbol: the base (literal / name / group), then the
            // quantifier. ZeroOrMore()/OneOrMore()/Optional() build auto-generated NTs that self-register
            // through the base Grammar's AutoGenerator callback.
            Symbol Resolve(EbnfSym sym)
            {
                Symbol baseSym;
                if (sym.IsPredicate) return MakePredicate(sym);
                if (sym.IsGroup) baseSym = MakeGroup(sym.Group);
                else if (sym.IsLiteral) baseSym = Literal(sym.Text);
                else if (nonterminals.TryGetValue(sym.Text, out var nt)) baseSym = nt;
                else if (tokens.TryGetValue(sym.Text, out var t))
                {
                    if (t.TokenType is Comment || IsRejectedToken(t))
                        throw new EbnfException($"token '{sym.Text}' is lexical-only and cannot be used in a production");
                    baseSym = t;
                }
                else throw new EbnfException($"undefined symbol '{sym.Text}'");

                switch (sym.Quantifier)
                {
                    case EbnfQuantifier.ZeroOrMore: return baseSym.ZeroOrMore();
                    case EbnfQuantifier.OneOrMore: return baseSym.OneOrMore();
                    case EbnfQuantifier.Optional: return baseSym.Optional();
                    default: return baseSym;
                }
            }

            // 4. wire up the productions
            foreach (var rule in model.Rules)
            {
                var head = nonterminals[rule.Name];
                foreach (var alternative in rule.Alternatives)
                    AddAlternative(head, alternative.Select(Resolve).ToList());
            }

            SetStartSymbol(nonterminals[startName]);

            // 5. normalize (absorb optionals / flatten auto-generated repetition), then collect the
            //    per-production singles the parser needs.
            try
            {
                this.Optimization();
            }
            catch (GrammarProgressException ex)
            {
                // The shared tree-grammar validator also protects manually expressed nullable
                // cycles. Expose these as grammar diagnostics through the reader's usual result.
                throw new EbnfException(ex.Message);
            }
            catch (PredicateLeftRecursionException ex)
            {
                // Return the shared validator's rule, predicate and cycle information as
                // an ordinary grammar diagnostic, before constructing the parsing table.
                throw new EbnfException(ex.Message);
            }
            foreach (var nt in NonTerminalMultiples)
                foreach (var single in nt)
                    NonTerminalSingles.Add(single as NonTerminalSingle);
        }

        private static TokenType ResolveTokenType(EbnfTokenKind kind)
        {
            switch (kind)
            {
                case EbnfTokenKind.Identifier: return TokenType.Identifier;
                case EbnfTokenKind.StringLiteral: return TokenType.Literal.StringLiteral;
                case EbnfTokenKind.CharLiteral: return TokenType.Literal.CharLiteral;
                case EbnfTokenKind.NumberLiteral: return TokenType.Literal.Digit10;
                case EbnfTokenKind.Comment: return TokenType.SpecialToken.Comment;
                // Reject tokens use the existing raw-regex path, never represent numeric values,
                // and cannot enter productions. Consumers detect their explicit rejection policy
                // with IsRejectedToken before invoking the parser.
                case EbnfTokenKind.Reject: return TokenType.Literal.Digit10;
                default: throw new EbnfException($"unsupported token kind '{kind}'");
            }
        }

        private void RegisterTerminal(Terminal terminal)
        {
            keyManager.AllocateUniqueKey(terminal);
            TerminalSet.Add(terminal);

            // The base ctor seeds DelimiterDic from the (field) terminals; our terminals are added after
            // it ran, so mirror that for operator literals (used by the lexer for token boundaries).
            if (terminal.TokenType == TokenType.Operator && !DelimiterDic.ContainsKey(terminal.Value))
                DelimiterDic.Add(terminal.Value, false);
        }

        private static void AddAlternative(NonTerminal head, List<Symbol> symbols)
        {
            if (symbols.Count == 1)
            {
                if (symbols[0] is Terminal terminal) head.AddItem(terminal);
                else head.AddItem((NonTerminal)symbols[0]);
                return;
            }

            Symbol sequence = symbols[0];
            for (int i = 1; i < symbols.Count; i++) sequence = sequence + symbols[i];
            head.AddItem((NonTerminal)sequence);
        }
    }
}
