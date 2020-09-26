using System;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Tokenize
{
    public partial class Lexer
    {
        public LexingData Lexing(TokenStorage targetStorage, SelectionTokensContainer delInfos, string replaceString)
        {
//            if (delInfos.IsEmpty()) return targetStorage;
//            if (replaceString.Length == 0) return targetStorage;

            TokenStorage result = targetStorage.Clone() as TokenStorage;

            var indexInfo = delInfos.Range;
            var impactRange = result.FindImpactRange(indexInfo.StartIndex, indexInfo.EndIndex);
            string mergeString = this.GetImpactedStringFromDelInfo(result, delInfos, replaceString);

            var toInsertTokens = this._tokenizer.Tokenize(this._tokenizeRule, mergeString);
            result.ReplaceToken(impactRange, toInsertTokens);

            TokenizeImpactRanges ranges = new TokenizeImpactRanges
            {
                new RangePair(impactRange, new Range(impactRange.StartIndex, toInsertTokens.Count))
            };

            return new LexingData(result, ranges);
        }


        public LexingData Lexing(TokenStorage targetStorage, int offset, int len)
        {
            var delInfos = GetSelectionTokenInfos(targetStorage, offset, len);

            //            if (delInfos.IsEmpty()) return targetStorage;

            return Lexing(targetStorage, delInfos);
        }



        /// <summary>
        /// This function processes the deletion order.
        /// </summary>
        /// <param name="targetStorage"></param>
        /// <param name="delInfos"></param>
        /// /// <returns></returns>
        public LexingData Lexing(TokenStorage targetStorage, SelectionTokensContainer delInfos)
        {
            TokenStorage result = targetStorage.Clone() as TokenStorage;

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // The following process needs because of the following.
            // Ex 1 : void main()A
            //       1. It deletes () tokens.
            //       2. It it has to merge "main" token with "A" token.
            //       3. The merge result "mainA" is another token.
            //       4. The deletion result is "void", " ", "mainA".
            // Ex 2 : void main(){}
            //       1. It deletes () tokens.
            //       2. It has to merge "main" token with "{}" token.            ( "}" token also be included because impactRange algorithm, see the impactRange algorithm)
            //       3. The merge result "main{}" is not token therefore it will be separate to the multiple tokens.
            //       4. The separate result is "main", "{", "}".
            //       5. The separated tokens replace the "main{}" token.
            //       6. The deletion result is "void", " ", "main", "{", "}".

            var indexInfo = delInfos.Range;
            var impactRange = result.FindImpactRange(indexInfo.StartIndex, indexInfo.EndIndex);
            string mergeString = this.GetImpactedStringFromDelInfo(result, delInfos);

            var toInsertTokens = this._tokenizer.Tokenize(this._tokenizeRule, mergeString);
            result.ReplaceToken(impactRange, toInsertTokens);

            TokenizeImpactRanges tokenizeRanges = new TokenizeImpactRanges
            {
                new RangePair(impactRange, new Range(impactRange.StartIndex, toInsertTokens.Count))
            };
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            return new LexingData(result, tokenizeRanges);

            // The Rectangle Deletion operation need to write other algorithm also the algorithm will very complicate so I don't write it yet.
            // (The above data struct can be used on the Rectangle Deletion operation.)
        }


        /// <summary>
        /// This function returns a selected information.
        /// </summary>
        /// <returns></returns>
        private SelectionTokensContainer GetSelectionTokenInfos(TokenStorage targetStorage, int offset, int len)
        {
            SelectionTokensContainer result = new SelectionTokensContainer();
            if (len <= 0) return result;

            int endOffset = offset + len;

            Parallel.For(0, targetStorage.TokensToView.Count, (i, loopOption) =>
            {
                var token = targetStorage.TokensToView[i];
                // If whole of the token is contained -> reserve delete
                if (token.MoreRange(offset, endOffset))
                {
                    lock (result.WholeSelectionBag) result.WholeSelectionBag.Add(i);
                }
                // If overlap in part of the first token
                else if (token.Contains(offset, RecognitionWay.Front))
                {
                    int cIndex = offset - token.StartIndex;
                    int length = token.Data.Length - cIndex;
                    length = (len > length) ? length : len;

                    lock (result.PartSelectionBag) result.PartSelectionBag.Add(new Tuple<int, int, int>(i, cIndex, length));
                }
                // If overlap in part of the last token
                else if (token.Contains(endOffset, RecognitionWay.Back))
                {
                    int cIndex = endOffset - token.StartIndex;

                    lock (result.PartSelectionBag) result.PartSelectionBag.Add(new Tuple<int, int, int>(i, 0, cIndex));
                }
            });

            result.SortAll();

            return result;
        }
    }
}
