﻿using Parse.WpfControls.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Parse.WpfControls.Common
{
    public class TokenizeTextBox : ExtensionTextBox
    {
        private int key = 1;
        private string patternSum = string.Empty;
        private TokenPatternInfo notDefinedToken = new TokenPatternInfo(0, string.Empty);
        private List<Tuple<int, int>> scopeSyntaxes = new List<Tuple<int, int>>();
        private List<TokenPatternInfo> tokenPatternList = new List<TokenPatternInfo>();
        private Dictionary<int, List<int>> tokenIndexesTable = new Dictionary<int, List<int>>();

        public List<TokenInfo> Tokens { get; } = new List<TokenInfo>();
        public SyntaxPairCollection syntaxPairs = new SyntaxPairCollection();


        public int TokenIndex
        {
            get { return (int)GetValue(TokenIndexProperty); }
            set { SetValue(TokenIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TokenIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TokenIndexProperty =
            DependencyProperty.Register("TokenIndex", typeof(int), typeof(TokenizeTextBox), new PropertyMetadata(0));


        public TokenizeTextBox()
        {
            this.Loaded += (s, e) =>
            {
            };

            this.SelectionChanged += (s, e) =>
            {
                this.UpdateTokenIndex();
            };

            this.TextChanged += (s, e) =>
            {
                this.UpdateTokenInfos(e.Changes.First());
            };
        }

        private int GetTokenKey(string pattern)
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
                if(t.Operator != td.Operator)
                {
                    return (t.Operator) ? 1 : -1;
                }
                else if(t.CanDerived != td.CanDerived)
                {
                    return (t.CanDerived) ? 1 : -1;
                }

                if (t.OriginalPattern.Length > td.OriginalPattern.Length) return -1;
                else if (t.OriginalPattern.Length <= td.OriginalPattern.Length) return 1;
                return 0;
            });
        }

        private void UpdateTokenIndex()
        {
            this.TokenIndex = this.GetTokenIndexFromCaretIndex(this.CaretIndex);
        }

        /// <summary>
        /// This function register after tokenizing string into the multiple tokens.
        /// </summary>
        /// <param name="addString">The string for tokenizing.</param>
        /// <param name="basisIndex">The basis index that uses to set up a starting index of a token.</param>
        private List<TokenInfo> Tokenize(string addString, int basisIndex)
        {
            List<TokenInfo> result = new List<TokenInfo>();

            int prevEI = 0;
            foreach (var data in Regex.Matches(addString, patternSum, RegexOptions.Multiline | RegexOptions.ExplicitCapture))
            {
                var matchData = data as Match;

                // Not defined pattern
                TokenPatternInfo patternInfo = this.notDefinedToken;
                if (prevEI < matchData.Index)
                    result.Add(new TokenInfo(basisIndex + prevEI, addString.Substring(prevEI, matchData.Index - prevEI), patternInfo));

                // The number of elements in the group is 1 more than the number of elements in the TokenPatternList.
                for (int i = 0; i < this.tokenPatternList.Count + 1; i++)
                {
                    // The 0 index of the groups is always matched so it doesn't need to check.
                    if (matchData.Groups[i + 1].Length > 0)
                    {
                        patternInfo = this.tokenPatternList[i];
                        break;
                    }
                }

                result.Add(new TokenInfo(basisIndex + matchData.Index, matchData.Value, patternInfo));
                prevEI = matchData.Index + matchData.Length;
            }

            // if a string is remained then add to the token.
            if (prevEI < addString.Length)
                result.Add(new TokenInfo(basisIndex + prevEI, addString.Substring(prevEI, addString.Length - prevEI), this.notDefinedToken));

            return result;
        }

        private void DelTokens(TextChange changeInfo)
        {
            if (changeInfo.RemovedLength == 0) return;


        }

        private void AddTokens(TextChange changeInfo)
        {

        }

        private void UpdateTokenInfos(TextChange changeInfo)
        {
            RecognitionWay recognitionWay = RecognitionWay.Back;
            string addString = this.Text.Substring(changeInfo.Offset, changeInfo.AddedLength);

            int tokenIndex = this.GetTokenIndexFromCaretIndex(changeInfo.Offset, recognitionWay);
            if (tokenIndex == -1)
            {
                this.Tokenize(addString, 0).ForEach(i => this.Tokens.Add(i));

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
                TokenInfo token = this.Tokens[tokenIndex];
                string mergeString = token.MergeString(changeInfo.Offset, addString, recognitionWay);

                int addLength = mergeString.Length - token.Data.Length;

                Parallel.For(tokenIndex + 1, this.Tokens.Count, i =>
                {
                    this.Tokens[i].StartIndex += addLength;
                });

                int prevTokenCnt = this.Tokens.Count;
                int basisIndex = (tokenIndex == 0) ? 0 : this.Tokens[tokenIndex - 1].EndIndex + 1;

                this.Tokens.RemoveAt(tokenIndex);
                this.Tokenize(mergeString, basisIndex).ForEach(i => this.Tokens.Insert(tokenIndex++, i));

                while(tokenIndex < this.Tokens.Count)
                {
                    // Check next token
                    var nextToken = this.Tokens[tokenIndex];
                    mergeString = nextToken.MergeStringToFront(this.Tokens[tokenIndex-1].Data);

                    basisIndex = (tokenIndex == 0) ? 0 : this.Tokens[tokenIndex - 1].StartIndex;
                    List<TokenInfo> result = this.Tokenize(mergeString, basisIndex);
                    if (result[0].Data == this.Tokens[tokenIndex - 1].Data) break;

                    this.Tokens.RemoveAt(tokenIndex);
                    this.Tokens.RemoveAt(tokenIndex--);

                    result.ForEach(i => this.Tokens.Insert(tokenIndex++, i));
                }
            }

        }

        /// <summary>
        /// This function returns a token index from the caretIndex.
        /// </summary>
        /// <param name="caretIndex">The index of the caret</param>
        /// <param name="recognWay">The standard index for recognition a token</param>
        /// <returns>a token index</returns>
        public int GetTokenIndexFromCaretIndex(int caretIndex, RecognitionWay recognWay = RecognitionWay.Back)
        {
            int index = -1;

            Parallel.For(0, this.Tokens.Count, (i, loopState) =>
            {
                TokenInfo tokenInfo = this.Tokens[i];

                if (tokenInfo.Contains(caretIndex, recognWay))
                {
                    index = i;
                    loopState.Stop();
                }
            });

            return index;
        }

        /// <summary>
        /// This function regist a pattern for tokenizing.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="optionData"></param>
        /// <param name="bCanDerived"></param>
        /// <param name="bOperator"></param>
        public void AddTokenPattern(string text, object optionData = null, bool bCanDerived = false, bool bOperator = false)
        {
            foreach(var item in this.tokenPatternList)
            {
                if (item.OriginalPattern == text) return;
            }

            this.tokenIndexesTable.Add(this.key, new List<int>());
            this.tokenPatternList.Add(new TokenPatternInfo(this.key++, text, optionData, bCanDerived, bOperator));

            this.SortTokenPatternList();

            this.patternSum = string.Empty;
            string generateString = string.Empty;
            foreach (var pattern in this.tokenPatternList)
            {
                if (pattern.OriginalPattern == string.Empty) continue;

                generateString += "a";
                this.patternSum += string.Format("(?<{1}>{0})|", pattern.Pattern, generateString);
                //                    patternSum += string.Format("({0})|", pattern.Pattern);
            }

            this.patternSum = this.patternSum.Substring(0, patternSum.Length - 1);
        }

        public void AddScopeGroup(string startScopeSymbol, string endScopeSymbol)
        {
            int startScopeKey = this.GetTokenKey(startScopeSymbol);
            int endScopeKey = this.GetTokenKey(endScopeSymbol);

            if (startScopeKey > 0 || endScopeKey > 0) return;
            this.scopeSyntaxes.Add(new Tuple<int, int>(startScopeKey, endScopeKey));
        }
    }
}
