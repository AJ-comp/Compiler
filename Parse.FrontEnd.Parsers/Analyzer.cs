using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas.LR;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers
{
    public class Analyzer
    {
        /// <summary>
        /// <para>Get the first terminal set that can is induced by the symbol.</para>
        /// <para>symbol로 부터 첫번째로 유도될 수 있는 단말 집합을 가져옵니다.</para>
        /// </summary>
        /// <param name="symbol"></param>
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
        /// <para>Get the first terminal set that can is induced by the singleNT.</para>
        /// <para>비단말 기호로 부터 첫번째로 유도될 수 있는 단말 집합을 가져옵니다. </para>
        /// </summary>
        /// <param name="singleNT"></param>
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
        /// <para>Get the second terminal set that can is induced by the symbol.</para>
        /// <para></para>
        /// </summary>
        /// <param name="nonTerminal"></param>
        /// <returns></returns>
        public static TerminalSet SecondTerminalSet(NonTerminal nonTerminal)
        {
            return new TerminalSet();
        }


        /// <summary>
        /// <para>Get all symbol set that can come next of nonTerminal from allNonTerminal.</para>
        /// <para>현재 등록된 모든 유도식에서 nonTerminal <b><i>다음에 오는</i></b> 모든 symbol 집합을 찾습니다.</para>
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
        /// nonTerminal의 유도식 에서 findSymbol <b><i>다음에 오는</i></b> 모든 심벌집합을 찾습니다.
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
        public static CanonicalItemSet Closure(CanonicalItemSet iStatus, HashSet<NonTerminal> exploredSet = null)
        {
            if (exploredSet == null) exploredSet = new HashSet<NonTerminal>();

            var result = new CanonicalItemSet();

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
                CanonicalItemSet param = new CanonicalItemSet();
                foreach (NonTerminalSingle single in multipleNT)
                {
                    param.Add(new CanonicalItem(single));
                }

                result.UnionWith(Analyzer.Closure(param, exploredSet));
            }

            return result;
        }

        public static CanonicalItemSet Goto(CanonicalItemSet iStatus, Symbol toSeeSymbol)
        {
            if (toSeeSymbol == null) return null;
            var param = new CanonicalItemSet();

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
