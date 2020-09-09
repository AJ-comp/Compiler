using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Tokenize;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.Logical
{
    public class LLParserSnippet : ParserSnippet
    {
        public LLParserSnippet(Parser parser) : base(parser)
        {
        }

        public override ParsingResult Parsing(IReadOnlyList<TokenData> tokens)
        {
            throw new System.NotImplementedException();
        }

        public override ParsingResult Parsing(IReadOnlyList<TokenData> tokens, ParsingResult prevParsingInfo, TokenizeImpactRanges rangeToParse)
        {
            throw new System.NotImplementedException();
        }
    }
}
