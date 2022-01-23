using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.Parsers.Collections
{
    public class CanonicalStateSet : HashSet<CanonicalState>
    {
        /// <summary>
        /// <para>Get the state set has the markSymbol.</para>
        /// </summary>
        /// <param name="markSymbol"></param>
        /// <returns></returns>
        public CanonicalStateSet GetStateSetHasSpecifiedMarkSymbol(Symbol markSymbol)
        {
            CanonicalStateSet result = new CanonicalStateSet();

            foreach (var fromState in this)
            {
                if (fromState.GetNTSingleHasMarkSymbol(markSymbol).Count == 0) continue;

                result.Add(fromState);
            }

            return result;
        }
    }
}
