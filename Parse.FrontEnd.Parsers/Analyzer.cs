using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers
{
    public class Analyzer
    {
        /// <summary>
        /// Get the first terminal set that can is induced by the symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="explorSet"></param>
        /// <returns></returns>
        public static TerminalSet FirstTerminalSet(Symbol symbol)
        {
            if (symbol is Terminal) return new TerminalSet(symbol as Terminal);

            TerminalSet result = new TerminalSet();
            foreach (NonTerminalSingle singleNT in (symbol as NonTerminal))
            {
                // S -> S ~ 인 경우
                if (singleNT[0] == symbol) continue;

                result.UnionWith(Analyzer.FirstTerminalSet(singleNT));
            }

            return result;
        }

        /// <summary>
        /// Get the first terminal set that can is induced by the symbolList
        /// </summary>
        /// <param name="symbolList"></param>
        /// <param name="explorSet"></param>
        /// <returns></returns>
        public static TerminalSet FirstTerminalSet(NonTerminalSingle singleNT)
        {
            TerminalSet result = new TerminalSet();

            // ringsum 연산
            foreach (var symbol in singleNT)
            {
                result = result.RingSum(Analyzer.FirstTerminalSet(symbol));

                // ringsum 연산은 epsilon이 없다면 더 이상 볼 필요 없다
                if (!result.IsNullAble) break;
            }

            return result;
        }

        /// <summary>
        /// Get the second terminal set that can is induced by the symbol
        /// </summary>
        /// <param name="nonTerminal"></param>
        /// <returns></returns>
        public static TerminalSet SecondTerminalSet(NonTerminal nonTerminal)
        {
            return new TerminalSet();
        }


        /// <summary>
        /// 현재 등록된 모든 유도식에서 nonTerminal "다음에 오는" 모든 symbol 집합을 찾는다
        /// </summary>
        /// <param name="nonTerminal"></param>
        /// <returns></returns>
        public static SymbolSet GetFollowSymbols(HashSet<NonTerminal> allNonTerminal,  NonTerminal nonTerminal)
        {
            SymbolSet result = new SymbolSet();

            foreach (var section in allNonTerminal)
                result.UnionWith(Analyzer.FindNextSymbolSet(section, nonTerminal));

            return result;
        }


        /// <summary>
        /// nonTerminal의 유도식 에서 findSymbol "다음에 오는" 모든 심벌집합을 찾는다
        /// </summary>
        /// <param name="nonTerminal">유도식의 최상단</param>
        /// <param name="findSymbol">찾고자 하는 심벌집합의 기준이 되는 심벌</param>
        /// <returns>찾은 심벌집합</returns>
        private static SymbolSet FindNextSymbolSet(NonTerminal nonTerminal, NonTerminal findSymbol)
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
                        if (!Analyzer.FirstTerminalSet(symbol).IsNullAble) break;
                    }
                    else if (symbol == findSymbol) bFind = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Closure calculation
        /// </summary>
        /// <param name="iStatus">current status</param>
        /// <param name="exploredSet">explored node until now</param>
        /// <returns></returns>
        /// <see cref="https://www.lucidchart.com/documents/edit/515ff26b-2649-4150-86ec-80288ef51570/0?beaconFlowId=67B120E7DA8E4FC0"/>
        public static Canonical Closure(Canonical iStatus, HashSet<NonTerminal> exploredSet = null)
        {
            if (exploredSet == null) exploredSet = new HashSet<NonTerminal>();

            var result = new Canonical();

            foreach(var item in iStatus)
            {
                result.Add(item);

                // if marker symbol does not exist.
                if (item.MarkSymbol == null) continue;
                // if marker symbol is terminal
                if (item.MarkSymbol is Terminal) continue;

                // if marker symbol is nonterminal
                NonTerminal multipleNT = item.MarkSymbol as NonTerminal;
                if (exploredSet.Contains(multipleNT)) continue;

                exploredSet.Add(multipleNT);
                Canonical param = new Canonical();
                foreach (NonTerminalSingle single in multipleNT)
                {
                    param.Add(new CanonicalItem(single));
                }

                result.UnionWith(Analyzer.Closure(param, exploredSet));
            }

            return result;
        }

        public static Canonical Goto(Canonical iStatus, Symbol toSeeSymbol)
        {
            if (toSeeSymbol == null) return null;
            var param = new Canonical();

            foreach(var item in iStatus)
            {
                if (item.MarkSymbol == toSeeSymbol)
                {
                    var cloneNT = item.Clone() as CanonicalItem;

                    cloneNT.MoveMarkSymbol();

                    param.Add(cloneNT);
                }
            }

            return Analyzer.Closure(param);
        }
    }
}
