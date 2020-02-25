using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.EventArgs;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.Datas
{
    public class LRParsingResult : ParsingResult
    {
        public LRParsingResult(bool success) : base(success)
        {
        }

        public LRParsingResult() : base(true)
        {
        }
    }
}
