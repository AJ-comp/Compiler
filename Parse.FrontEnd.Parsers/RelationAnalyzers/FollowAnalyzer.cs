using Parse.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.RelationAnalyzers
{
    public class FollowAnalyzer
    {
        internal RelationData Datas { get; private set; } = new RelationData();
        internal TerminalSet Follow(NonTerminal nonTerminal) => this.Datas[nonTerminal];


        public TerminalSet InitFollowSet(NonTerminal nonTerminal, HashSet<NonTerminal> nonTerminalSet)
        {
            TerminalSet result = new TerminalSet();
            if (nonTerminal.IsStartSymbol) result.Add(new EndMarker());

            // First(symbol) - epsilon
            foreach (var symbol in Analyzer.GetFollowSymbols(nonTerminalSet, nonTerminal))
            {
                var firstSet = Analyzer.FirstTerminalSet(symbol);
                firstSet.ExceptWith(new TerminalSet(new Epsilon()));
                result.UnionWith(firstSet);
            }

            return result;
        }

        public void CalculateAllFollow(HashSet<NonTerminal> nonTerminals)
        {
            foreach (var symbol in nonTerminals)
            {
                // 초기 Follow 구하기
                this.Datas.Add(symbol, this.InitFollowSet(symbol, nonTerminals));
            }

            bool bChange = false;
            do
            {
                bChange = false;
                foreach (var followSetDic in this.Datas)
                {
                    // 2차 Follow Update
                    if (this.UpdateFollow(followSetDic.Key, followSetDic.Value)) bChange = true;
                }
            }
            while (bChange);
        }



        /// <summary>
        /// contents(symbol 리스트)의 follow 집합을 update 합니다
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="followSet"></param>
        /// <returns>update 후 follow 집합이 변화했다면 true</returns>
        private bool AltExprUpdateFollow(List<Symbol> contents, TerminalSet followSet)
        {
            bool result = false;

            foreach(var symbol in contents)
            {
                if (symbol is Terminal) continue;

                var prevSetCount = this.Datas[symbol as NonTerminal].Count;
                this.Datas[symbol as NonTerminal].UnionWith(followSet);       // update

                if (prevSetCount != this.Datas[symbol as NonTerminal].Count) result = true;
            }

            return result;
        }

        /// <summary>
        /// contents(symbol 리스트)의 follow 집합을 update 합니다
        /// </summary>
        /// <param name="contents">symbol 리스트</param>
        /// <param name="followSet">symbol 리스트에 업데이트 할 follow 집합</param>
        /// <returns>update 후 follow 집합이 변화했다면 true</returns>
        private bool ConCatExprUpdateFollow(NonTerminalSingle contents, TerminalSet followSet)
        {
            bool result = false;

            for(int i=contents.Count-1; i>=0; i--)
            {
                var symbol = contents[i];

                if (symbol is Terminal) break;

                var prevSetCount = this.Datas[symbol as NonTerminal].Count;
                this.Datas[symbol as NonTerminal].UnionWith(followSet);       // update

                if (prevSetCount != this.Datas[symbol as NonTerminal].Count) result = true;

                if (!Analyzer.FirstTerminalSet(symbol).IsNullAble) break;
            }

            return result;
        }

        private bool UpdateFollow(NonTerminal root, TerminalSet lhsFollowSet)
        {
            bool result = false;

            foreach (NonTerminalSingle singleNT in root)
            {
                if (this.ConCatExprUpdateFollow(singleNT, lhsFollowSet)) result = true;
            }

            return result;
        }
    }
}
