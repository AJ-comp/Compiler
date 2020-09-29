using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.Tokenize
{
    public class LexingResult
    {
        public TokenStorage TokenStorage { get; }
        public TokenizeImpactRanges RangeToLex { get; }

        public LexingResult(TokenStorage tokenStorage, TokenizeImpactRanges impactRanges)
        {
            TokenStorage = tokenStorage;
            RangeToLex = impactRanges;
        }


        //public ParsingData ToParsingData()
        //{
        //    var tokenCells = TokenStorage.TokensToView;

        //    if (tokenCells.Count == 0) return null;
        //    List<TokenData> tokens = new List<TokenData>();
        //    TokenizeImpactRanges rangesToParse = new TokenizeImpactRanges();

        //    int startIndex = 0;
        //    int count = 0;
        //    for (int i = 0; i < tokenCells.Count; i++)
        //    //            Parallel.For(0, tokenCells.Count, (i) =>
        //    {
        //        var tokenCell = tokenCells[i];

        //        //****  this function may creates a key for TokenType (NotDefined) so this function is not thread safe. ****
        //        var tokenData = TokenData.CreateFromTokenCell(tokenCell, (i == tokenCells.Count - 1));
        //        if (tokenData == null) continue;

        //        tokens.Add(tokenData);
        //        if (RangeToLex.Contains())
        //        {
        //            if (count == 0) startIndex = i;
        //            count++;
        //        }
        //        else
        //        {
        //            if(count != 0) rangesToParse.Add(new RangePair())
        //        }
        //    }
        //    var endMarker = new EndMarker();
        //    tokens.Add(new TokenData(endMarker, new TokenCell(-1, endMarker.Value, null)));

        //    return new ParsingData(RangeToLex, tokens);
        //}
    }
}
