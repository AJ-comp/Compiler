using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas.LR;
using Janglim.FrontEnd.RegularGrammar;
using System.Collections.Generic;
using System.Linq;

namespace Janglim.FrontEnd.Parsers.LR
{
    public class LALRParser : LRParser
    {
        public LALRParser(Grammar grammar, bool bLogging) : base(grammar, CanonicalType.LALRC1, bLogging)
        {
        }

        public override AmbiguityCheckResult CheckAmbiguity()
        {
            var result = new AmbiguityCheckResult();

            foreach (var line in Canonical.ToCanonicalLineList())
                result.Add(new AmbiguityCheckItem(line, ReduceParameter.LalrLookAhead));

            return result;
        }
    }
}
