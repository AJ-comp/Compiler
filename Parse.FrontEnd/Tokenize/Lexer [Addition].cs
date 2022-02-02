using Parse.Extensions;
using System.Linq;

namespace Parse.FrontEnd.Tokenize
{
    public partial class Lexer
    {
        /// <summary>
        /// This function returns token list after lexing a string data.
        /// </summary>
        /// <param name="data">The string data to lex</param>
        /// <returns></returns>
        public LexingData Lexing(string data)
        {
            LexingData result = new LexingData(this._tokenPatternList);

            var tokenCells = this._tokenizer.Tokenize(this._tokenizeRule, data);
            result.InitToken(tokenCells);

            return result;
        }

        /// <summary>
        /// This function returns token list after add dataToAdd at the end of prevTokens.
        /// </summary>
        /// <param name="prevTokens">The target tokens</param>
        /// <param name="dataToAdd">The data to add</param>
        /// <see cref="https://www.lucidchart.com/documents/edit/f4366425-61f9-4b4f-9abc-72ce4efe864c/ZYtDEhwbkIBA?beaconFlowId=53B1C199D7307981"/>
        /// <returns></returns>
        public LexingData Lexing(LexingData prevTokens, string dataToAdd)
        {
            if (prevTokens == null) return this.Lexing(dataToAdd);

            int offset = prevTokens.TokensForView.Count - 1;
            return this.Lexing(prevTokens, offset, dataToAdd);
        }

        /// <summary>
        /// This function returns token list after add dataToAdd at the offset of prevTokens.
        /// </summary>
        /// <param name="prevTokens">The target tokens</param>
        /// <param name="offset">The position to add</param>
        /// <param name="dataToAdd">The data to add</param>
        /// <see cref="https://www.lucidchart.com/documents/edit/f4366425-61f9-4b4f-9abc-72ce4efe864c/ZYtDEhwbkIBA?beaconFlowId=53B1C199D7307981"/>
        /// <returns></returns>
        public LexingData Lexing(LexingData prevTokens, int offset, string dataToAdd)
        {
            if (prevTokens == null) return this.Lexing(dataToAdd);
            if (prevTokens.TokensForView.Count == 0) return this.Lexing(dataToAdd);

            //            TokenStorage result = prevTokens.Clone() as TokenStorage;

            RecognitionWay recognitionWay = RecognitionWay.Back;
            int curTokenIndex = prevTokens.TokenIndexForOffset(offset, recognitionWay);

            if (curTokenIndex == -1)
            {
                TokenCell token = prevTokens.TokensForView[0];
                return this.TokenizeAfterReplace(prevTokens, 0, token.MergeString(offset, dataToAdd, RecognitionWay.Front));
            }
            else
            {
                TokenCell token = prevTokens.TokensForView[curTokenIndex];
                return this.TokenizeAfterReplace(prevTokens, curTokenIndex, token.MergeString(offset, dataToAdd, recognitionWay));
            }
        }



        /// <summary>
        /// This function process the following algorithm.
        /// 1. Generate tokens by tokenizing the mergeString argument.
        /// 2. Replace token of the replaceIndex argument to the generated tokens.
        /// 3. Work the RangeMergeAndTokenizeProcess.
        /// </summary>
        /// <param name="targetStorage"></param>
        /// <param name="replaceIndex"></param>
        /// <param name="mergeString"></param>
        /// <returns></returns>
        private LexingData TokenizeAfterReplace(LexingData targetStorage, int replaceIndex, string mergeString)
        {
            var impactRange = targetStorage.FindImpactRange(replaceIndex);

//            TokenStorage cloneStorage = targetStorage.Clone();
            var firstString = targetStorage.GetMergeStringOfRange(new Range(impactRange.StartIndex, replaceIndex - impactRange.StartIndex));
            var lastString = targetStorage.GetMergeStringOfRange(new Range(replaceIndex + 1, impactRange.EndIndex - replaceIndex));
            mergeString = firstString + mergeString + lastString;

            // If a basisIndex is used then it can increase the performance of the ReplaceToken function because of need not arrange.
            // But the logic is not written yet.
            var tokenList = this._tokenizer.Tokenize(this._tokenizeRule, mergeString);
            targetStorage.ReplaceToken(impactRange, tokenList);

            return targetStorage;

            /// The comment can include all tokens therefore if a comment exists the process is performed as below.
            /// ex : void main()\r\n{}\r\n -> void main(/*)\r\n{}\r\n
            ///       process 1 : "void", " ", "main", "(", "/*)", "\r", "\n" -> The tokens in the impact range.
            ///                        "void main(/*)\r\n"                            -> The string that merged.
            ///                        "void", " ", "main", "(", "/*)\r\n"          -> The tokens after splite.
            ///       process 2 : "void", " ", "main", "(", "/*)\r\n", "{", "}", "\r", "\n" -> The tokens in the impact range.
            ///                        "void main(/*)\r\n{}\r\n"                                     -> The string that merged.
            ///                        "void", " ", "main", "(", "/*)\r\n{}\r\n"                   -> The tokens after splite.

            /*
            while (true)
            {
                var impactRangeToParse = cloneStorage.FindImpactRange(rangePairToRegist.Item1.StartIndex, rangePairToRegist.Item1.EndIndex);
                var beforeTokens = cloneStorage.TokensToView.Skip(impactRangeToParse.StartIndex).Take(impactRangeToParse.Count).ToList();
                var basisIndex = (beforeTokens.Count > 0) ? beforeTokens[0].StartIndex : 0;
                var processedTokens = this._tokenizer.Tokenize(this._tokenizeRule, cloneStorage.GetMergeStringOfRange(impactRangeToParse), basisIndex);

                if (beforeTokens.IsEqual(processedTokens)) break;
                cloneStorage.ReplaceToken(impactRangeToParse, processedTokens);
                rangePairToRegist = new RangePair(impactRange, new Range(impactRangeToParse.StartIndex, processedTokens.Count));
            }
            */
        }
    }
}
