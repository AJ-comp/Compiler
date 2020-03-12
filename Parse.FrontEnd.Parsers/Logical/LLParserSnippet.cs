using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.RegularGrammar;
using Parse.Tokenize;
using System.Data;

namespace Parse.FrontEnd.Parsers.Logical
{
    public class LLParserSnippet : ParserSnippet
    {
        public LLParserSnippet(Parser parser) : base(parser)
        {
        }

        public override ParsingResult Parsing(TokenCell[] tokenCells)
        {
            throw new System.NotImplementedException();
        }
    }
}
