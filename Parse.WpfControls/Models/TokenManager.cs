using System.Collections.Generic;

namespace Parse.WpfControls.Models
{
    public class TokenInfo
    {
        public int TokenIndex { get; }
        public string Token { get; }

        public TokenInfo(int tokenIndex, string token)
        {
            this.TokenIndex = tokenIndex;
            this.Token = token;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", this.TokenIndex, this.Token);
        }
    }

    public class TokenList : List<string>
    {
        public TokenInfo GetMergedToken(int caretIndex, string mergeString)
        {
            int sumLength = 0;
            int tokenIndex = 0;

            TokenInfo result = null;

            foreach (var item in this)
            {
                // include condition
                if (sumLength < caretIndex && caretIndex < sumLength + item.Length)
                {
                    result = new TokenInfo(tokenIndex, item.Insert(caretIndex, mergeString));
                    break;
                }
                else if (sumLength == caretIndex)
                {
                    result = new TokenInfo(tokenIndex, item.Insert(caretIndex, mergeString));
                    break;
                }
//                else if (item.StartIndex + item.Token.Length == caretIndex)

                tokenIndex++;
                sumLength += item.Length;
            }

            return result;
        }

        public string GetToken(int tokenIndex)
        {
            return (this.Count <= tokenIndex) ? string.Empty : this[tokenIndex];
        }

        public TokenInfo GetNextToken(int tokenIndex)
        {
            TokenInfo result = null;
            int nextIndex = tokenIndex + 1;

            if (this.Count <= nextIndex) return result;

            return new TokenInfo(nextIndex, this[nextIndex]);
        }
    }


    /*
    /// <summary>
    /// This class combined the data struct with the algorithm, therefore, There is a condition when adding new Match information.
    /// </summary>
    class MatchedList : SortedList<int, Match>
    {
        /// <summary>
        /// This property returns a minimum index in the collection.
        /// </summary>
        public int MinIndex
        {
            get
            {
                if (this.Count == 0) return -1;
                return this.First().Value.Index;
            }
        }

        /// <summary>
        /// This property returns a maximum index in the collection.
        /// </summary>
        public int MaxIndex
        {
            get
            {
                if (this.Count == 0) return -1;
                return this.Last().Value.Index;
            }
        }

        /// <summary>
        /// This function adds when a match parameter is bigger than the token in the collection.
        /// </summary>
        /// <param name="match">A match parameter</param>
        /// <returns>If added true else false.</returns>
        public bool AddWhenBigger(Match match)
        {
            bool result = false;

            if(this.RemoveWhere((m) => this.MinIndex >= match.Index && this.MaxIndex <= match.EndIndex()) >= 1)
            {
                result = true;
                this.Add(match);
            }

            return result;
        }

        /// <summary>
        /// This function adds when a match parameter does not exist in the collection.
        /// </summary>
        /// <param name="match">A match parameter</param>
        /// <returns>If added true else false.</returns>
        public bool AddWhenNew(Match match)
        {
            bool overlap = false;

            foreach(var item in this)
            {
                //                if(item.EndIndex() < match.Index || item.Index > match.EndIndex())
                if (item.EndIndex() >= match.Index && item.Index <= match.EndIndex()) overlap = true;
            }

            bool result = false;
            if (overlap == false) { this.Add(match); result = true; }

            return result;
        }

        /// <summary>
        /// This function adds match information to the list when satisfied special condition (does not exists in the list, bigger data than existing data).
        /// </summary>
        /// <param name="matchInfo">The data that would add.</param>
        /// <returns></returns>
        public new void Add(Match matchInfo)
        {
            if (!this.AddWhenBigger(matchInfo))
                this.AddWhenNew(matchInfo);
        }
    }

    class TokenTable
    {
        Dictionary<int, TokenList> tokenListByLine = new Dictionary<int, TokenList>();
        StringCollection Patterns { get; } = new StringCollection();

        public TokenInfo GetMergedToken(int lineIndex, int startCaretIndexByLine, string addString)
        {
            return (this.tokenListByLine.Count <= lineIndex) ? null : this.tokenListByLine[lineIndex].GetMergedToken(startCaretIndexByLine, addString);
        }

        /// <summary>
        /// This function checks all string of the token is mapped by comparing pattern information.
        /// </summary>
        /// <param name="token">The token that will compare.</param>
        /// <returns>if all be mapped returns true.</returns>
        public bool IsExistAllIncluded(string token)
        {
            foreach (var pattern in this.Patterns)
            {
                if (Regex.Match(token, pattern).Length == token.Length)
                    return true;
            }

            return false;
        }

        public StringCollection GetSubStringByPattern(string token)
        {
            StringCollection result = new StringCollection();

            HashSet<Match> matchedSet = new HashSet<Match>();
            foreach (var pattern in this.Patterns)
            {
                MatchCollection matchCollection = Regex.Matches(token, pattern);

                for (int i = 0; i < matchCollection.Count; i++)
                {
                    var matchInfo = matchCollection[i];

                    matchedSet.Contains()
                }

                if (Regex.Match(token, pattern).Length == token.Length)
                {
                    result.Clear();
                    result.Add(token);

                    return result;
                }

                else if()
            }
        }

        /// <summary>
        /// This function returns a token list of the line.
        /// </summary>
        /// <param name="lineIndex">line index that to bring token list.</param>
        /// <returns>Token list</returns>
        public TokenList GetTokenList(int lineIndex)
        {
            return (this.tokenListByLine.Count <= lineIndex) ? null : this.tokenListByLine[lineIndex];
        }

        /// <summary>
        /// This function returns a token related to line index, token index.
        /// </summary>
        /// <param name="lineIndex">line index that to bring token.</param>
        /// <param name="tokenIndex">In line, token index that to bring token.</param>
        /// <returns>Token</returns>
        public string GetToken(int lineIndex, int tokenIndex)
        {
            var list = (this.tokenListByLine.Count <= lineIndex) ? null : this.tokenListByLine[lineIndex];

            return list.GetToken(tokenIndex);
        }


    }
    */
}
