using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas.LR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Janglim.FrontEnd.Parsers.LR
{
    public class CLRParser : LRParser
    {
        public CLRParser(Grammar grammar, bool bLogging) : base(grammar, CanonicalType.C1, bLogging)
        {

        }

        public override AmbiguityCheckResult CheckAmbiguity()
        {
            throw new NotImplementedException();
        }
    }
}
