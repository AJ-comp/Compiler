using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas.LR;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.Parsers
{
    public class Analyzer
    {
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
        /// Closure calculation
        /// </summary>
        /// <param name="iStatus">current status</param>
        /// <param name="exploredSet">explored node until now</param>
        /// <returns></returns>
        /// <see cref="https://www.lucidchart.com/documents/edit/515ff26b-2649-4150-86ec-80288ef51570/0?beaconFlowId=67B120E7DA8E4FC0"/>
        public static CanonicalState Closure(CanonicalState iStatus, HashSet<NonTerminal> exploredSet = null)
        {
            if (exploredSet == null) exploredSet = new HashSet<NonTerminal>();

            var result = new CanonicalState();

            foreach (var item in iStatus)
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
                CanonicalState param = new CanonicalState();
                foreach (NonTerminalSingle single in multipleNT)
                {
                    param.Add(new LRItem(single));
                }

                result.UnionWith(Analyzer.Closure(param, exploredSet));
            }

            return result;
        }

        public static CanonicalState Goto(CanonicalState iStatus, Symbol toSeeSymbol)
        {
            if (toSeeSymbol == null) return null;
            var param = new CanonicalState();

            foreach (var item in iStatus)
            {
                if (item.MarkSymbol == toSeeSymbol)
                {
                    var cloneNT = item.Clone() as LRItem;

                    cloneNT.MoveMarkSymbol();

                    param.Add(cloneNT);
                }
            }

            return Analyzer.Closure(param);
        }
    }
}
