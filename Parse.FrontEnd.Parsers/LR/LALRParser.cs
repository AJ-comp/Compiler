using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas.LR;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.Parsers.LR
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
