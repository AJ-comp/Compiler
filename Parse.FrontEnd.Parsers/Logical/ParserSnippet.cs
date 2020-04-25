using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.RegularGrammar;
using Parse.Tokenize;
using System;
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

        /// <summary>
        /// This function returns TokenData list converted from TokenCell list.
        /// </summary>
        /// <param name="tokenCells">The token cell list to convert</param>
        /// <returns>The TokenData list converted</returns>
        public IReadOnlyList<TokenData> ToTokenDataList(IReadOnlyList<TokenCell> tokenCells)
        {
            if (tokenCells.Count == 0) return null;
            var result = new TokenData[tokenCells.Count + 1];

            for (int i = 0; i < tokenCells.Count; i++)
//            Parallel.For(0, tokenCells.Count, (i) =>
            {
                var tokenCell = tokenCells[i];

                //****  this function may creates a key for TokenType (NotDefined) so this function is not thread safe. ****
                result[i] = TokenData.CreateFromTokenCell(tokenCell, (i == tokenCells.Count - 1));
            }

            var endMarker = new EndMarker();
            result[result.Length - 1] = new TokenData(endMarker, new TokenCell(-1, endMarker.Value, null));

            return result;
        }

        /// <summary>
        /// This function returns TokenData list converted from TokenCell list (as much as changedRanges range).
        /// </summary>
        /// <param name="tokenCells"></param>
        /// <param name="changedRanges"></param>
        /// <returns>The TokenData list converted</returns>
        public IReadOnlyList<TokenData> ToTokenDataList(IReadOnlyList<TokenCell> tokenCells, TokenizeImpactRanges changedRanges)
        {
            if (tokenCells.Count == 0) return null;
            var result = new List<TokenData>();

            foreach(var range in changedRanges)
            {
                var curRange = range.Item2;
                for(int i= curRange.StartIndex; i< curRange.EndIndex + 1; i++)
                {
                    var tokenCell = tokenCells[i];
                    result.Add(TokenData.CreateFromTokenCell(tokenCell, (i == tokenCells.Count - 1)));
                }
            }

            return result;
        }

        /// <summary>
        /// This function performs whole parsing for tokenCells
        /// </summary>
        /// <param name="tokens">The tokenCells to parsing</param>
        /// <returns>The parsing result</returns>
        public abstract ParsingResult Parsing(IReadOnlyList<TokenData> tokens);
        public abstract ParsingResult Parsing(IReadOnlyList<TokenData> tokens, ParsingResult prevParsingInfo, TokenizeImpactRanges rangeToParse);
    }
}
