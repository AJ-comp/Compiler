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
        public override NonTerminal EbnfRoot => StartSymbol;

        public EbnfGrammar(EbnfModel model)
        {
            // base() already ran: reflected fields (none here), wired AutoGenerator to this.keyManager /
            // this.RegistAutoGenerateNT, and added the EndMarker to TerminalSet.

            if (model.Rules.Count == 0) throw new EbnfException("the grammar has no rules");

            // 1. token rules ( Name := "regex" ) -> meaningful regex terminals, looked up by name
            var tokens = new Dictionary<string, Terminal>();
            foreach (var tok in model.Tokens)
            {
                var t = new Terminal(TokenType.Identifier, tok.Pattern, tok.Name, true, true);
                RegisterTerminal(t);
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
                // keyword wins the lexer tie against the identifier instead of losing to its longer pattern.
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
                nonterminals[rule.Name] = nt;
            }

            // an anonymous non-terminal that holds a '( … )' group's alternatives
            int groupCount = 0;
            NonTerminal MakeGroup(List<List<EbnfSym>> alternatives)
            {
                var nt = new NonTerminal($"__grp{++groupCount}", false);
                keyManager.AllocateUniqueKey(nt);
                NonTerminalMultiples.Add(nt);
                foreach (var alt in alternatives)
                    AddAlternative(nt, alt.Select(Resolve).ToList());
                return nt;
            }

            // resolve one EBNF symbol to a grammar Symbol: the base (literal / name / group), then the
            // quantifier. ZeroOrMore()/OneOrMore()/Optional() build auto-generated NTs that self-register
            // through the base Grammar's AutoGenerator callback.
            Symbol Resolve(EbnfSym sym)
            {
                Symbol baseSym;
                if (sym.IsGroup) baseSym = MakeGroup(sym.Group);
                else if (sym.IsLiteral) baseSym = Literal(sym.Text);
                else if (nonterminals.TryGetValue(sym.Text, out var nt)) baseSym = nt;
                else if (tokens.TryGetValue(sym.Text, out var t)) baseSym = t;
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
            this.Optimization();
            foreach (var nt in NonTerminalMultiples)
                foreach (var single in nt)
                    NonTerminalSingles.Add(single as NonTerminalSingle);
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
