using Janglim.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Janglim.FrontEnd
{
    public partial class FirstFollowAnalyzer
    {
        private bool _bChanged = false;
        private Dictionary<NonTerminalSingle, TerminalSet> _cache = new Dictionary<NonTerminalSingle, TerminalSet>();

        public TerminalSet First(NonTerminalSingle key) => _cache[key];
        public TerminalSet First(NonTerminalConcat concat)
        {
            // 기호가 하나도 없는 경우도 명시적인 ε와 같은 빈 기호열입니다.
            if (concat.Count == 0) return new TerminalSet(new Epsilon());

            var result = new TerminalSet();

            foreach (var symbol in concat)
            {
                result = result.RingSum(First(symbol));
            }

            return result;
        }

        public TerminalSet First(Symbol key)
        {
            TerminalSet result = new TerminalSet();
            if (key is Terminal)
            {
                result.Add(key as Terminal);
                return result;
            }

            var nonTerminal = key as NonTerminal;
            foreach (var cacheData in _cache)
            {
                if (nonTerminal.IsSubSet(cacheData.Key)) result.UnionWith(cacheData.Value);
            }

            return result;
        }

        /// <summary>
        /// <para>Get the first terminal set that can is induced by the symbol.</para>
        /// <para>symbol로 부터 첫번째로 유도될 수 있는 단말 집합을 가져옵니다.</para>
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public TerminalSet FirstSet(Symbol symbol, HashSet<NonTerminalSingle> seenNT = null)
        {
            if (seenNT == null) seenNT = new HashSet<NonTerminalSingle>();
            if (symbol is Terminal) return new TerminalSet(symbol as Terminal);

            TerminalSet result = new TerminalSet();
            foreach (NonTerminalSingle singleNT in symbol as NonTerminal)
            {
                if (!_cache.ContainsKey(singleNT)) _cache.Add(singleNT, new TerminalSet());

                // in the case the singleNT is seen
                if (seenNT.Contains(singleNT))
                {
                    result.UnionWith(_cache[singleNT]);
                    // A nullable cached result still must NOT recurse into the same rule.
                    // The caller's concatenation uses epsilon to continue with later symbols;
                    // CalculateAllFirst revisits rules until those cached sets reach a fixed point.
                    continue;
                }
                seenNT.Add(singleNT);

                var productionFirst = FirstSet(singleNT, seenNT);
                int beforeCount = _cache[singleNT].Count;
                // Cache this production's FIRST only. The aggregate result also contains
                // earlier alternatives, whose epsilon/tokens must not leak into this rule.
                _cache[singleNT].UnionWith(productionFirst);
                if (_cache[singleNT].Count != beforeCount)
                {
                    _bChanged = true;
                }
                result.UnionWith(_cache[singleNT]);
            }

            return result;
        }

        /// <summary>
        /// <para>Get the first terminal set that can is induced by the singleNT.</para>
        /// <para>비단말 기호로 부터 첫번째로 유도될 수 있는 단말 집합을 가져옵니다. </para>
        /// </summary>
        /// <param name="singleNT"></param>
        /// <returns></returns>
        public TerminalSet FirstSet(NonTerminalConcat singleNT, HashSet<NonTerminalSingle> seenNT = null)
        {
            // 캐시를 계산할 때도 빈 기호열의 FIRST는 {ε}로 표현합니다.
            if (singleNT.Count == 0) return new TerminalSet(new Epsilon());

            if (seenNT == null) seenNT = new HashSet<NonTerminalSingle>();
            TerminalSet result = new TerminalSet();

            // ringsum 연산
            foreach (var symbol in singleNT)
            {
                result = result.RingSum(FirstSet(symbol, seenNT));

                // ringsum 연산은 epsilon이 없다면 더 이상 볼 필요 없다
                if (!result.IsNullAble) break;
            }

            return result;
        }


        public void CalculateAllFirst(HashSet<NonTerminal> nonTerminals)
        {
            do
            {
                _bChanged = false;
                foreach (var nonTerminal in nonTerminals) FirstSet(nonTerminal);
            }
            while (_bChanged);
        }
    }
}
