using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas.LR;

namespace Janglim.FrontEnd.Parsers.LR
{
    public class SLRParser : LRParser
    {
        public override AmbiguityCheckResult CheckAmbiguity()
        {
            var result = new AmbiguityCheckResult();

            foreach (var line in Canonical.ToCanonicalLineList())
                result.Add(new AmbiguityCheckItem(line, ReduceParameter.Follow));

            return result;
        }


        public SLRParser(Grammar grammar, bool bLogging) : base(grammar, CanonicalType.C0, bLogging)
        {
        }
    }
}
