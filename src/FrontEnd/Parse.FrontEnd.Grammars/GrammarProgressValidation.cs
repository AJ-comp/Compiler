using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.RegularGrammar;

namespace Janglim.FrontEnd.Grammars
{
    internal static class GrammarProgressValidation
    {
        public static void Validate(IEnumerable<NonTerminal> nonTerminals)
        {
            // Read the productions once. Nullability is a structural property: callbacks are
            // never invoked while building a grammar, and a guard is potentially true epsilon.
            var rules = nonTerminals.ToDictionary(rule => rule, rule => rule.ToArray());
            var nullable = new HashSet<NonTerminal>();
            var predicateNullable = new HashSet<NonTerminal>();
            bool changed;
            do
            {
                changed = false;
                foreach (var rule in rules)
                {
                    foreach (var production in rule.Value)
                    {
                        if (production.Predicate != null && !production.IsEpsilon)
                            throw new GrammarProgressException("Grammar predicates must be attached to epsilon productions.");
                        if (!production.All(symbol => IsNullable(symbol, nullable))) continue;
                        changed |= nullable.Add(rule.Key);
                        if (production.Predicate != null ||
                            production.OfType<NonTerminal>().Any(predicateNullable.Contains))
                            changed |= predicateNullable.Add(rule.Key);
                    }
                }
            } while (changed);

            foreach (var rule in rules.Keys)
            {
                if (rule.RepetitionOperand != null && IsNullable(rule.RepetitionOperand, nullable))
                    throw new GrammarProgressException(
                        $"Repetition '{rule.Name}' has an operand that can match without consuming a token; " +
                        "each iteration of * or + must consume at least one token.");
            }

            if (predicateNullable.Count == 0) return;

            // A -> prefix B suffix can return to B without consuming anything only when
            // EVERY other symbol is nullable. A -> Guard A 'x' contributes a required x,
            // so its predicate-before-left-recursion restriction is checked separately by
            // PredicateLeftRecursionValidation rather than this zero-yield cycle check.
            var edges = rules.Keys.ToDictionary(rule => rule, _ => new HashSet<NonTerminal>());
            var guardedEdges = new HashSet<(NonTerminal Source, NonTerminal Target)>();
            foreach (var rule in rules)
            {
                foreach (var production in rule.Value)
                {
                    for (int i = 0; i < production.Count; i++)
                    {
                        if (!(production[i] is NonTerminal target) || !edges.ContainsKey(target)) continue;
                        bool zeroWidthContext = true;
                        for (int j = 0; j < production.Count; j++)
                        {
                            if (i != j && !IsNullable(production[j], nullable))
                            {
                                zeroWidthContext = false;
                                break;
                            }
                        }
                        if (zeroWidthContext)
                        {
                            edges[rule.Key].Add(target);
                            if (Enumerable.Range(0, production.Count).Any(j => j != i &&
                                production[j] is NonTerminal context && predicateNullable.Contains(context)))
                                guardedEdges.Add((rule.Key, target));
                        }
                    }
                }
            }

            // Inspect all grammar rules, including unused named rules: the existing
            // First/Follow analyzer also visits all of them. An iterative search avoids
            // adding another recursive algorithm to the build-time protection itself.
            foreach (var origin in predicateNullable)
            {
                var seen = new HashSet<NonTerminal>();
                var pending = new Stack<NonTerminal>(edges[origin]);
                while (pending.Count > 0)
                {
                    var current = pending.Pop();
                    if (current == origin)
                        throw new GrammarProgressException(
                            $"Rule '{origin.Name}' participates in a non-consuming predicate cycle; " +
                            "a recursive cycle must require an input token.");
                    if (!seen.Add(current)) continue;
                    foreach (var target in edges[current]) pending.Push(target);
                }
            }

            // A -> Guard A | 'x' is not nullable, but its guarded self-edge still loops
            // without consuming input. A marked edge is cyclic when its target can reach
            // its source through other zero-width edges; mandatory-token suffixes break it.
            foreach (var edge in guardedEdges)
            {
                var seen = new HashSet<NonTerminal>();
                var pending = new Stack<NonTerminal>();
                pending.Push(edge.Target);
                while (pending.Count > 0)
                {
                    var current = pending.Pop();
                    if (current == edge.Source)
                        throw new GrammarProgressException(
                            $"Rule '{edge.Source.Name}' participates in a non-consuming predicate cycle; " +
                            "a recursive cycle must require an input token.");
                    if (!seen.Add(current)) continue;
                    foreach (var target in edges[current]) pending.Push(target);
                }
            }
        }

        private static bool IsNullable(Symbol symbol, HashSet<NonTerminal> nullable)
            => symbol is Epsilon || symbol is NonTerminal rule && nullable.Contains(rule);
    }
}
