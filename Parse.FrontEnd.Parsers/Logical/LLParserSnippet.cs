using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.RegularGrammar;
using Parse.Tokenize;
using System.Collections.Generic;
using System.Data;

namespace Parse.FrontEnd.Parsers.Logical
{
    public class LLParserSnippet : ParserSnippet
    {
        public LLParserSnippet(Parser parser) : base(parser)
        {
        }

        public override ParsingResult Parsing(IReadOnlyList<TokenCell> tokenCells)
        {
            throw new System.NotImplementedException();
        }

        public override ParsingResult Parsing(IReadOnlyList<TokenCell> tokenCells, ParsingResult prevParsingInfo, TokenizeImpactRanges rangeToParse)
        {
            throw new System.NotImplementedException();
        }
    }
}
