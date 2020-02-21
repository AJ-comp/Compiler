using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Parsers.Collections;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.Datas
{
    public class LRParsingResult : ParsingResult
    {
        public LRParsingResult(bool success, ParsingHistory parsingHistory, Stack<AstSymbol> meaningStack) : base(success, parsingHistory, meaningStack)
        {
        }
    }
}
