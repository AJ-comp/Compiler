using Janglim.FrontEnd;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas.LR;
using Janglim.FrontEnd.RegularGrammar;

// DESIGN MODEL ONLY. This file is not compiled into any product project.
// Calculate creates its own table. It never changes relation.LookAheadTable,
// canonical states, LR items, the parsing table, or product source.
internal static class WorklistCalculation
{
    internal sealed class Result
    {
        internal Dictionary<(int State, LRItem Item), TerminalSet> Table { get; } = new();
        internal List<string> Updates { get; } = new();
        internal int EdgeCount { get; set; }
        internal int DequeuedCount { get; set; }
    }

    internal static Result Calculate(CanonicalRelation relation, FirstFollowAnalyzer firstFollow,
        NonTerminal virtualStart,
        Func<CanonicalState, LRItem, IEnumerable<CanonicalState>> origins)
    {
        var result = new Result();
        var table = result.Table;
        var nodes = relation.IndexStateDic.OrderBy(pair => pair.Key)
            .SelectMany(pair => pair.Value.Select(item => (State: pair.Key, Item: item))).ToArray();

        // A node is a (state, LR item), including items before their handles end.
        // Its empty set is an initial approximation, never a completed-cache flag.
        foreach (var node in nodes) table.Add(node, new TerminalSet());

        // If target inherits parent, save the notification direction parent -> target.
        var dependents = new Dictionary<(int State, LRItem Item), HashSet<(int State, LRItem Item)>>();
        foreach (var target in nodes)
        {
            var lhs = target.Item.SingleNT.ToNonTerminal();
            if (lhs == virtualStart)
            {
                table[target].Add(new EndMarker());
                continue;
            }

            foreach (var origin in origins(relation.IndexStateDic[target.State], target.Item))
            {
                foreach (var parent in origin)
                {
                    if (parent.MarkSymbol != lhs) continue;
                    var beta = parent.SymbolListAfterMarkSymbol;
                    var firstBeta = firstFollow.First(beta);

                    // Directly known terminals: FIRST(beta) minus epsilon.
                    foreach (var terminal in firstBeta)
                        if (!(terminal is Epsilon)) table[target].Add(terminal);

                    // Empty beta is nullable. A nonempty FIRST set containing epsilon
                    // also proves nullability; an empty FIRST set alone does not.
                    if (beta.Count == 0 || firstBeta.Contains(new Epsilon()))
                    {
                        var source = (State: origin.StateNumber, Item: parent);
                        if (!table.ContainsKey(source))
                            throw new InvalidOperationException("The dependency parent is not an LR node.");
                        if (!dependents.TryGetValue(source, out var targets))
                            dependents.Add(source, targets = new HashSet<(int State, LRItem Item)>());
                        if (targets.Add(target)) result.EdgeCount++;
                    }
                }
            }
        }

        // Only nonempty seeds can initially contribute information.
        var pending = new Queue<(int State, LRItem Item)>();
        var queued = new HashSet<(int State, LRItem Item)>();
        foreach (var node in nodes)
            if (table[node].Count != 0 && queued.Add(node)) pending.Enqueue(node);

        while (pending.Count != 0)
        {
            var source = pending.Dequeue();
            queued.Remove(source);
            result.DequeuedCount++;
            if (!dependents.TryGetValue(source, out var targets)) continue;
            foreach (var target in targets)
            {
                int before = table[target].Count;
                table[target].UnionWith(table[source]);
                if (table[target].Count == before) continue;
                result.Updates.Add($"I{source.State} {source.Item} -> I{target.State} {target.Item}: {table[target]}");
                if (queued.Add(target)) pending.Enqueue(target);
            }
        }

        if (table.Values.Any(set => set.Any(t => t is Epsilon)))
            throw new InvalidOperationException("A lookahead set must not contain epsilon.");
        return result;
    }
}
