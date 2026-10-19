using System.Collections.Generic;
using System.Linq;

namespace Janglim.FrontEnd.Grammars.Ebnf
{
    /// <summary>
    /// Checks repetition bodies before lowering creates recursive grammar nodes. Every successful
    /// iteration of '*' or '+' must consume at least one token. Predicates consume no tokens and
    /// are potentially true; validation never invokes a registered callback.
    /// </summary>
    internal static class EbnfRepetitionValidation
    {
        public static void Validate(EbnfModel model)
        {
            var symbols = CollectSymbols(model);
            if (!symbols.Any(symbol => symbol.Quantifier == EbnfQuantifier.ZeroOrMore ||
                                       symbol.Quantifier == EbnfQuantifier.OneOrMore)) return;

            var nullableNames = new HashSet<string>(System.StringComparer.Ordinal);
            var nullableBodies = new HashSet<EbnfSym>();

            // This fixed point works with forward references, mutually recursive named rules,
            // nested groups, and repeated rule definitions. A name becomes nullable only when
            // one of its complete alternatives can finish without consuming a token.
            bool changed;
            do
            {
                changed = false;
                foreach (var symbol in symbols)
                {
                    bool nullable = symbol.IsPredicate ||
                        (!symbol.IsLiteral && (symbol.IsGroup
                            ? symbol.Group.Any(alt => alt.All(child => IsNullable(child, nullableBodies)))
                            : nullableNames.Contains(symbol.Text)));
                    if (nullable) changed |= nullableBodies.Add(symbol);
                }

                foreach (var rule in model.Rules)
                    if (rule.Alternatives.Any(alt => alt.All(symbol => IsNullable(symbol, nullableBodies))))
                        changed |= nullableNames.Add(rule.Name);
            } while (changed);

            foreach (var symbol in symbols)
            {
                if (symbol.Quantifier != EbnfQuantifier.ZeroOrMore &&
                    symbol.Quantifier != EbnfQuantifier.OneOrMore) continue;
                // Inspect the BODY, before its own quantifier. Every '*' is nullable as a whole;
                // that does not make a repetition of an ordinary nonempty token invalid.
                if (!nullableBodies.Contains(symbol)) continue;

                string marker = symbol.Quantifier == EbnfQuantifier.ZeroOrMore ? "*" : "+";
                string location = symbol.SourceLine > 0 ? $"line {symbol.SourceLine}: " : string.Empty;
                throw new EbnfException($"{location}the body of '{marker}' repetition can consume no input; every repeated alternative must consume at least one token");
            }
        }

        private static bool IsNullable(EbnfSym symbol, ISet<EbnfSym> nullableBodies)
            => symbol.Quantifier == EbnfQuantifier.Optional ||
               symbol.Quantifier == EbnfQuantifier.ZeroOrMore || nullableBodies.Contains(symbol);

        private static List<EbnfSym> CollectSymbols(EbnfModel model)
        {
            var result = new List<EbnfSym>();
            var seen = new HashSet<EbnfSym>();
            var activeGroups = new HashSet<EbnfSym>();
            var pending = new Stack<(EbnfSym Symbol, bool Exit)>();
            foreach (var symbol in model.Rules.SelectMany(rule => rule.Alternatives).SelectMany(alt => alt).Reverse())
                pending.Push((symbol, false));

            while (pending.Count != 0)
            {
                var visit = pending.Pop();
                var symbol = visit.Symbol;
                if (symbol == null) throw new EbnfException("a production cannot contain a null symbol");
                if (visit.Exit) { activeGroups.Remove(symbol); continue; }
                if (activeGroups.Contains(symbol))
                    throw new EbnfException("an inline group model contains a cycle; express recursion with a named rule reference");
                if (!seen.Add(symbol)) continue;

                if (symbol.IsPredicate && symbol.Quantifier != EbnfQuantifier.None)
                {
                    string location = symbol.SourceLine > 0 ? $"line {symbol.SourceLine}: " : string.Empty;
                    throw new EbnfException($"{location}a predicate cannot have a quantifier (* + ?)");
                }
                result.Add(symbol);
                if (!symbol.IsGroup) continue;
                if (symbol.Group == null) throw new EbnfException("an inline group must define its alternatives");

                activeGroups.Add(symbol);
                pending.Push((symbol, true));
                foreach (var child in symbol.Group.SelectMany(alt => alt).Reverse()) pending.Push((child, false));
            }
            return result;
        }
    }
}
