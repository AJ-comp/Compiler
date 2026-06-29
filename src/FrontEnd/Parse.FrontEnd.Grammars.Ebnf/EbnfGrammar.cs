using Janglim;
using Janglim.FrontEnd.RegularGrammar;
using System;
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
    /// </summary>
    public sealed class EbnfGrammar : Grammar
    {
        // No NonTerminal/Terminal fields here on purpose: the base ctor reflects the subclass's fields
        // (which run their initializers BEFORE base()), so a null field would crash key allocation.
        // The start symbol is set via SetStartSymbol and surfaced through StartSymbol.
        public override NonTerminal EbnfRoot => StartSymbol;

        public EbnfGrammar(EbnfModel model)
        {
            // base() already ran: reflected fields (none here), wired AutoGenerator to this.keyManager,
            // and added the EndMarker to TerminalSet.

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
                // bWordPattern must be FALSE for both: a literal's Value is matched verbatim, not as a
                // regex. A word-like keyword literal ('qreg') is just as much a literal as a symbol one
                // ('['), so it takes false too — that makes the lexer sort these literals AHEAD of the
                // identifier token rule (which is a real word-pattern, true), so keywords win the tie
                // against the identifier instead of losing to its longer pattern.
                var t = isWord
                    ? new Terminal(TokenType.Keyword, value, value, false, false)
                    : new Terminal(TokenType.Operator, value, value, false, false);
                RegisterTerminal(t);
                literals[value] = t;
                return t;
            }

            // 3. non-terminals (one per rule name); the first rule's head is the start symbol
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

            // 4. wire up the productions
            foreach (var rule in model.Rules)
            {
                var head = nonterminals[rule.Name];
                foreach (var alternative in rule.Alternatives)
                {
                    var symbols = alternative.Select(s => Resolve(s, tokens, nonterminals, Literal)).ToList();
                    AddAlternative(head, symbols);
                }
            }

            // 5. collect the per-production singles and mark the start symbol
            foreach (var nt in NonTerminalMultiples)
                foreach (var single in nt)
                    NonTerminalSingles.Add(single as NonTerminalSingle);

            SetStartSymbol(nonterminals[startName]);
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

        private static Symbol Resolve(EbnfSym sym, Dictionary<string, Terminal> tokens,
                                      Dictionary<string, NonTerminal> nonterminals, Func<string, Terminal> literal)
        {
            if (sym.IsLiteral) return literal(sym.Text);
            if (nonterminals.TryGetValue(sym.Text, out var nt)) return nt;
            if (tokens.TryGetValue(sym.Text, out var t)) return t;

            throw new EbnfException($"undefined symbol '{sym.Text}'");
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
