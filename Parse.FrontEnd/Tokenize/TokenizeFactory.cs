using System.Collections.Generic;

namespace Parse.Tokenize
{
    public class TokenizeFactory
    {
        int key = 1;
        List<TokenPatternInfo> tokenPatternList = new List<TokenPatternInfo>();

        private Tokenizer tokenizeTeam = new Tokenizer();
        public TokenStorage StorageTeam { get; } = new TokenStorage();


        /// <summary>
        /// This function sorts the TokenPatterList accoding to the following principle.
        /// 1st : Delimitable == false, CanDerived == false 
        /// 2nd : Delimitable == false, CanDerived == true
        /// 3rd : Delimitable == true, CanDerived == false
        /// </summary>
        private void SortTokenPatternList()
        {
            this.tokenPatternList.Sort(delegate (TokenPatternInfo t, TokenPatternInfo td)
            {
                if (t.Operator != td.Operator)
                {
                    return (t.Operator) ? 1 : -1;
                }
                else if (t.CanDerived != td.CanDerived)
                {
                    return (t.CanDerived) ? 1 : -1;
                }

                if (t.OriginalPattern.Length > td.OriginalPattern.Length) return -1;
                else if (t.OriginalPattern.Length <= td.OriginalPattern.Length) return 1;
                return 0;
            });
        }

        public int GetPatternKey(string pattern)
        {
            int result = -1;

            foreach (var item in this.tokenPatternList)
            {
                if (item.OriginalPattern == pattern)
                {
                    result = item.Key;
                    break;
                }
            }

            return result;
        }

        public void AddTokenRule(string text, object optionData = null, bool bCanDerived = false, bool bOperator = false)
        {
            foreach (var item in this.tokenPatternList)
            {
                if (item.OriginalPattern == text) return;
            }

            this.tokenPatternList.Add(new TokenPatternInfo(this.key++, text, optionData, bCanDerived, bOperator));
            this.SortTokenPatternList();

            this.tokenizeTeam.RegistTokenizeRule(this.tokenPatternList);
        }

        /// <summary>
        /// This function adds a token from the offset. The token is generated after tokenize the rawString.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="rawString"></param>
        public void ReceiveOrder(int offset, string rawString)
        {
            RecognitionWay recognitionWay = RecognitionWay.Back;

            int curTokenIndex = this.StorageTeam.TokenIndexForOffset(offset, recognitionWay);
            int nextTokenIndex = curTokenIndex + 1;
            if (this.StorageTeam.AllTokens.Count == 0)
            {
                this.tokenizeTeam.Tokenize(rawString, nextTokenIndex).ForEach(i => this.StorageTeam.AllTokens.Add(i));

                #region The method of the second tokenize. (delete duplicate element after all match)
                /*
                foreach(var item in data)
                {
                    if (item.Length == 0) continue;
                    tokens.Add(item);
                }*/

                /*
                 *                 List<Match> tokens = new List<Match>();
                foreach(var pattern in this.TokenPatternList)
                {
                    foreach(var match in Regex.Matches(addString, pattern))
                    {
                        var matchData = match as Match;
                        bool bExist = tokens.Any((i) => (i.Index < matchData.Index && matchData.Index <= i.Length - 1 + i.Index));
                        if (bExist) continue;
                        tokens.Add(matchData);
                    }
                }

                tokens.Sort(delegate (Match a, Match b)
                {
                    if (a.Index > b.Index) return 1;
                    else if (a.Index < b.Index) return -1;
                    return 0;
                });
                */
                #endregion

                #region The method of the Third tokenize. (sequentially match from the first element)
                /*
                while(addString.Length > 0)
                {
                    string topCandidate = string.Empty;

                    foreach (var pattern in this.TokenPatternList)
                    {
                        var matchResult = Regex.Match(addString, pattern.Pattern);
                        if (matchResult.Index != 0) continue;

                        if (matchResult.Length > topCandidate.Length) topCandidate = matchResult.Value;
                    }

                    if (topCandidate == string.Empty)
                    {
                        this.Tokens.Add(new TokenInfo(0, addString[0].ToString());
                        addString = addString.Substring(1);
                    }
                    else
                    {
                        this.Tokens.Add(topCandidate);
                        addString = addString.Substring(topCandidate.Length);
                    }
                }
                */
                #endregion

            }
            else
            {
                if (curTokenIndex == -1)
                {
                    TokenCell nextToken = this.StorageTeam.AllTokens[nextTokenIndex];

                    int lastTokenIndex = this.tokenizeTeam.ReplaceToken(this.StorageTeam.AllTokens, nextTokenIndex, nextToken.MergeStringToFront(rawString));
                    this.tokenizeTeam.ContinousTokenize(this.StorageTeam.AllTokens, lastTokenIndex);
                }
                else
                {
                    TokenCell token = this.StorageTeam.AllTokens[curTokenIndex];
                    string mergeString = token.MergeString(offset, rawString, recognitionWay);

                    var lastTokenIndex = this.tokenizeTeam.ReplaceToken(this.StorageTeam.AllTokens, curTokenIndex, mergeString);
                    this.tokenizeTeam.ContinousTokenize(this.StorageTeam.AllTokens, lastTokenIndex);
                }
            }
        }

        public void ReceiveOrder(int offset, int delLen)
        {

        }

    }
}
