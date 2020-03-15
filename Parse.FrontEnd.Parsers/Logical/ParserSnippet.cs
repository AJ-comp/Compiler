using Parse.FrontEnd.Parsers.Datas;
using Parse.Tokenize;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Parsers.Logical
{
    public abstract class ParserSnippet
    {
        /// <summary>
        /// Critical Section.
        /// </summary>
        public Parser Parser { get; }

        protected ParserSnippet(Parser parser)
        {
            this.Parser = parser;
        }

        public IReadOnlyList<TokenData> ToTokenDataList(IReadOnlyList<TokenCell> tokenCells)
        {
            if (tokenCells.Count == 0) return null;
            var result = new TokenData[tokenCells.Count];

            Parallel.For(0, tokenCells.Count, (i) =>
            {
                var tokenCell = tokenCells[i];
                result[i] = TokenData.CreateFromTokenCell(tokenCell, (i == tokenCells.Count - 1));
            });

            return result;
        }

        /// <summary>
        /// This function performs whole parsing for tokenCells
        /// </summary>
        /// <param name="tokenCells">The tokenCells to parsing</param>
        /// <param name="bFullSource">If this param is true checks the end marker.</param>
        /// <returns>The parsing result</returns>
        public abstract ParsingResult Parsing(IReadOnlyList<TokenCell> tokenCells);
        public abstract ParsingResult Parsing(IReadOnlyList<TokenCell> tokenCells, ParsingResult prevParsingInfo, TokenizeImpactRanges rangeToParse);
    }
}
