using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd
{
    public partial class FirstFollowAnalyzer
    {
        public RelationData Datas { get; private set; } = new RelationData();
        public TerminalSet Follow(NonTerminal nonTerminal) => this.Datas[nonTerminal];


        public TerminalSet InitFollowSet(NonTerminal nonTerminal, HashSet<NonTerminal> nonTerminalSet)
        {
            TerminalSet result = new TerminalSet();
            if (nonTerminal.IsStartSymbol) result.Add(new EndMarker());

            // First(symbol) - epsilon
            foreach (var symbol in GetFollowSymbols(nonTerminalSet, nonTerminal))
            {
                var firstSet = FirstSet(symbol);
                firstSet.ExceptWith(new TerminalSet(new Epsilon()));
                result.UnionWith(firstSet);
            }

            return result;
        }

        public void CalculateAllFollow(HashSet<NonTerminal> nonTerminals)
        {
            CalculateAllFirst(nonTerminals);

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
        /// contents(symbol 리스트)의 follow 집합을 update 합니다.
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

                if (!FirstSet(symbol).IsNullAble) break;
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



        /// <summary>
        /// <para>Get all symbol set that can come next of nonTerminal from allNonTerminal.</para>
        /// <para>현재 등록된 모든 유도식에서 nonTerminal <b><i>다음에 오는</i></b> 모든 symbol 집합을 찾습니다.</para>
        /// </summary>
        /// <param name="nonTerminal"></param>
        /// <returns></returns>
        public SymbolSet GetFollowSymbols(HashSet<NonTerminal> allNonTerminal, NonTerminal nonTerminal)
        {
            SymbolSet result = new SymbolSet();

            foreach (var section in allNonTerminal)
                result.UnionWith(FindNextSymbolSet(section, nonTerminal));

            return result;
        }


        /// <summary>
        /// nonTerminal의 유도식 에서 findSymbol <b><i>다음에 오는</i></b> 모든 심벌집합을 찾습니다.
        /// </summary>
        /// <param name="nonTerminal">유도식의 최상단</param>
        /// <param name="findSymbol">찾고자 하는 심벌집합의 기준이 되는 심벌</param>
        /// <returns>찾은 심벌집합</returns>
        private SymbolSet FindNextSymbolSet(NonTerminal nonTerminal, NonTerminal findSymbol)
        {
            SymbolSet result = new SymbolSet();

            foreach (NonTerminalSingle singleNT in nonTerminal)
            {
                bool bFind = false;

                foreach (var symbol in singleNT)
                {
                    if (bFind)
                    {
                        result.Add(symbol);
                        if (!FirstSet(symbol).IsNullAble) break;
                    }
                    else if (symbol == findSymbol) bFind = true;
                }
            }

            return result;
        }
    }
}
