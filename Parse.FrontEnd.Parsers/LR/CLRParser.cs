﻿using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas.LR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.Parsers.LR
{
    public class CLRParser : LRParser
    {
        public CLRParser(Grammar grammar) : base(grammar, CanonicalType.C1)
        {

        }

        public override AmbiguityCheckResult CheckAmbiguity()
        {
            throw new NotImplementedException();
        }
    }
}
