using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.RegularGrammar;

namespace Janglim.FrontEnd.Grammars
{
    /// <summary>
    /// 조건절을 거쳐 토큰 소비 없이 같은 규칙으로 돌아오는 좌재귀 경로를 검사합니다.
    /// <para>1. 조건절이 있는 좌재귀 후보를 모읍니다.</para>
    /// <para>2. 재진입 경로에서 토큰을 소비하기 전에 조건절을 지나는지 확인합니다.</para>
    /// <para>3. 해당 경로를 문법 오류로 처리하며, 일반적인 좌재귀는 허용합니다.</para>
    /// <para>
    /// <see href="../../../docs/ko/predicate-left-recursion-validation.html">클래스 동작 문서 — 쉬운 설명과 예제</see>
    /// </para>
    /// </summary>
    internal static class PredicateLeftRecursionValidation
    {
        public static void Validate(
            IEnumerable<NonTerminal> nonTerminals,
            IEnumerable<NonTerminal> diagnosticRules)
        {
            if (nonTerminals == null) throw new ArgumentNullException(nameof(nonTerminals));
            if (diagnosticRules == null) throw new ArgumentNullException(nameof(diagnosticRules));

            // 정의된 모든 규칙을 읽습니다. First/Follow도 사용하지 않는 규칙을 검사하므로
            // 시작 규칙에서 도달할 수 있는 규칙으로만 대상을 제한하지 않습니다.
            var rules = nonTerminals
                .OrderBy(rule => rule.Name, StringComparer.Ordinal)
                .ThenBy(rule => rule.UniqueKey)
                .ToDictionary(rule => rule, rule => rule.ToArray());
            if (!rules.Values.Any(productions => productions.Any(production => production.Predicate != null)))
                return;
            var namedRules = new HashSet<NonTerminal>(diagnosticRules);

            // 조건 함수의 참/거짓을 실행해서 추측하지 않고, 문법 구조만 분석합니다.
            var emptyPaths = AnalyzeEmptyPaths(rules);
            var edges = BuildLeftCornerEdges(rules, emptyPaths);

            foreach (var outgoing in edges.Values)
            {
                foreach (var edge in outgoing)
                {
                    if (edge.PredicatesBeforeTarget.Length == 0) continue;

                    // 조건을 지난 연결이 좌재귀 순환에 속하는지 확인합니다.
                    // S -> Guard A에서 A -> S 경로가 없다면 Guard는 순환 바깥에 있습니다.
                    var returnPath = FindReturnPath(edges, edge.Target, edge.Source);
                    if (returnPath == null) continue;

                    var cycle = new List<NonTerminal> { edge.Source };
                    cycle.AddRange(returnPath);
                    throw CreateException(edge, cycle, namedRules);
                }
            }
        }

        private static EmptyPaths AnalyzeEmptyPaths(
            IReadOnlyDictionary<NonTerminal, NonTerminalSingle[]> rules)
        {
            var result = new EmptyPaths(rules.Keys);
            bool changed;
            do
            {
                changed = false;
                foreach (var rule in rules)
                {
                    foreach (var production in rule.Value)
                    {
                        // 대안 전체가 비어 있을 수 있을 때만 그 대안의 조건을 전파합니다.
                        // B : Guard 'z' | 'q'? 에서 B는 nullable이지만,
                        // Guard를 실행하는 대안에는 필수 z가 있으므로 빈 경로에 Guard가 없습니다.
                        if (!production.All(symbol => IsNullable(symbol, result.Nullable))) continue;

                        changed |= result.Nullable.Add(rule.Key);
                        var predicates = result.PredicatesByRule[rule.Key];
                        if (production.Predicate != null)
                            changed |= predicates.Add(production.Predicate.Name);

                        foreach (var symbol in production.OfType<NonTerminal>())
                        {
                            foreach (var name in result.PredicatesByRule[symbol])
                                changed |= predicates.Add(name);
                        }
                    }
                }
            } while (changed);
            return result;
        }

        private static Dictionary<NonTerminal, List<LeftCornerEdge>> BuildLeftCornerEdges(
            IReadOnlyDictionary<NonTerminal, NonTerminalSingle[]> rules,
            EmptyPaths emptyPaths)
        {
            var edges = rules.Keys.ToDictionary(rule => rule, _ => new List<LeftCornerEdge>());
            foreach (var rule in rules)
            {
                foreach (var production in rule.Value)
                {
                    var prefixPredicates = new HashSet<string>(StringComparer.Ordinal);
                    foreach (var symbol in production)
                    {
                        if (symbol is Epsilon) continue;
                        if (!(symbol is NonTerminal target)) break; // 필수 토큰을 만나면 탐색을 멈춥니다.

                        if (edges.ContainsKey(target))
                            edges[rule.Key].Add(new LeftCornerEdge(rule.Key, target, prefixPredicates));

                        // 재진입 대상 뒤의 조건은 그 대상에 도달하기 전의 연결에 붙이지 않습니다.
                        // A : A Guard 'x'의 A -> A는 일반적인 좌재귀입니다.
                        if (!emptyPaths.Nullable.Contains(target)) break;
                        prefixPredicates.UnionWith(emptyPaths.PredicatesByRule[target]);
                    }
                }
            }
            return edges;
        }

        private static List<NonTerminal> FindReturnPath(
            IReadOnlyDictionary<NonTerminal, List<LeftCornerEdge>> edges,
            NonTerminal from,
            NonTerminal to)
        {
            // 큐와 방문 기록으로 유한한 문법 그래프를 검사합니다.
            // 검사기 자체가 간접 재귀를 재귀 호출로 계속 따라가는 일을 방지합니다.
            var previous = new Dictionary<NonTerminal, NonTerminal> { [from] = null };
            var pending = new Queue<NonTerminal>();
            pending.Enqueue(from);
            while (pending.Count != 0)
            {
                var current = pending.Dequeue();
                if (current == to)
                {
                    var path = new List<NonTerminal>();
                    for (var node = current; node != null; node = previous[node]) path.Add(node);
                    path.Reverse();
                    return path;
                }

                foreach (var edge in edges[current])
                {
                    if (previous.ContainsKey(edge.Target)) continue;
                    previous.Add(edge.Target, current);
                    pending.Enqueue(edge.Target);
                }
            }
            return null;
        }

        private static PredicateLeftRecursionException CreateException(
            LeftCornerEdge edge,
            List<NonTerminal> cycle,
            HashSet<NonTerminal> namedRules)
        {
            // 그룹을 펼치며 생긴 자동 생성 규칙보다 사용자가 정의한 규칙을 먼저 표시합니다.
            // 순환의 출발점만 바꾸고, 중간에 거치는 규칙 이름도 진단에 남깁니다.
            int start = cycle.FindIndex(0, cycle.Count - 1, namedRules.Contains);
            if (start < 0) start = 0;
            int length = cycle.Count - 1;
            var path = new List<string>();
            for (int i = 0; i < length; i++) path.Add(cycle[(start + i) % length].Name);
            path.Add(path[0]);

            return new PredicateLeftRecursionException(
                path[0], edge.PredicatesBeforeTarget[0], path);
        }

        private static bool IsNullable(Symbol symbol, HashSet<NonTerminal> nullable)
            => symbol is Epsilon || symbol is NonTerminal rule && nullable.Contains(rule);

        private sealed class EmptyPaths
        {
            public HashSet<NonTerminal> Nullable { get; } = new HashSet<NonTerminal>();
            public Dictionary<NonTerminal, HashSet<string>> PredicatesByRule { get; }

            public EmptyPaths(IEnumerable<NonTerminal> rules)
            {
                PredicatesByRule = rules.ToDictionary(rule => rule,
                    _ => new HashSet<string>(StringComparer.Ordinal));
            }
        }

        private sealed class LeftCornerEdge
        {
            public NonTerminal Source { get; }
            public NonTerminal Target { get; }
            public string[] PredicatesBeforeTarget { get; }

            public LeftCornerEdge(NonTerminal source, NonTerminal target, IEnumerable<string> predicates)
            {
                Source = source;
                Target = target;
                // 같은 Source/Target에 다른 대안이 있더라도 조건 정보를 잃지 않습니다.
                // 접두부 집합이 이후 바뀌어도 이 연결의 정보는 그대로 유지합니다.
                PredicatesBeforeTarget = predicates.OrderBy(name => name, StringComparer.Ordinal).ToArray();
            }
        }
    }
}
