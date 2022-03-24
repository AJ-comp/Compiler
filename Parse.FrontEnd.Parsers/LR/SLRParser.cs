using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas.LR;

namespace Parse.FrontEnd.Parsers.LR
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
