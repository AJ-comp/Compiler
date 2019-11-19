using Parse.WpfControls.Models;
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

        private void UpdateTokenInfos(TextChange changeInfo)
        {
            string addString = this.Text.Substring(changeInfo.Offset, changeInfo.AddedLength);

            int tokenIndex = this.GetTokenIndexFromCaretIndex(changeInfo.Offset);
            if (tokenIndex == -1)
            {
                List<TokenInfo> tokens = new List<TokenInfo>();

                int prevEI = 0;
                var count = Regex.Matches(addString, patternSum, RegexOptions.Multiline | RegexOptions.ExplicitCapture);
                foreach (var data in Regex.Matches(addString, patternSum, RegexOptions.Multiline | RegexOptions.ExplicitCapture))
                {
                    var matchData = data as Match;

                    // Not defined pattern
                    TokenPatternInfo patternInfo = this.notDefinedToken;
                    if (prevEI < matchData.Index)
                        tokens.Add(new TokenInfo(prevEI, addString.Substring(prevEI, matchData.Index - prevEI), patternInfo));

                    // The number of elements in the group is 1 more than the number of elements in the TokenPatternList.
                    for (int i=0; i<this.tokenPatternList.Count+1; i++)
                    {
                        // The 0 index of the groups is always matched so it doesn't need to check.
                        if (matchData.Groups[i+1].Length > 0)
                        {
                            patternInfo = this.tokenPatternList[i];
                            break;
                        }
                    }

                    tokens.Add(new TokenInfo(matchData.Index, matchData.Value, patternInfo));
                    prevEI = matchData.Index + matchData.Length;
                }

                if(prevEI < addString.Length)
                {
                    TokenPatternInfo patternInfo = this.notDefinedToken;
                    tokens.Add(new TokenInfo(prevEI, addString.Substring(prevEI, addString.Length - prevEI), patternInfo));
                }

                tokens.ForEach(i => this.Tokens.Add(i));


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
                int insertPos = changeInfo.Offset - token.StartIndex;
                addString = token.Data.Insert(insertPos, addString);

            }

        }

        /// <summary>
        /// This function returns a token index from the caretIndex.
        /// </summary>
        /// <param name="caretIndex">The index of the caret</param>
        /// <param name="bBackWay">The standard index for recognition a token</param>
        /// <returns>a token index</returns>
        public int GetTokenIndexFromCaretIndex(int caretIndex, bool bBackWay = true)
        {
            int index = -1;

            Parallel.For(0, this.Tokens.Count, (i, loopState) =>
            {
                TokenInfo tokenInfo = this.Tokens[i];

                if(bBackWay)
                {
                    if (tokenInfo.StartIndex < caretIndex && caretIndex <= tokenInfo.EndIndex + 1)
                    {
                        index = i;
                        loopState.Stop();
                    }
                }
                else
                {
                    if (tokenInfo.StartIndex <= caretIndex && caretIndex <= tokenInfo.EndIndex)
                    {
                        index = i;
                        loopState.Stop();
                    }
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

    public class TokenPatternInfo
    {
        public int Key { get; }
        public object OptionData { get; }
        public string Pattern
        {
            get
            {
                if (this.Operator)
                {
                    string convertString = string.Empty;
                    foreach (var c in this.OriginalPattern)
                        convertString += "\\" + c;

                    return convertString;
                }
                else
                {
                    return (this.CanDerived) ? this.OriginalPattern : "\\b" + this.OriginalPattern + "\\b";
                }
            }
        }
        public string OriginalPattern { get; }
        public bool CanDerived { get; }
        public bool Operator { get; }

        public TokenPatternInfo(int key, string pattern, object optionData = null, bool bCanDerived = false, bool bOperator = false)
        {
            this.Key = key;
            this.OptionData = optionData;
            this.OriginalPattern = pattern;
            this.CanDerived = bCanDerived;
            this.Operator = bOperator;
        }

        public override string ToString() => string.Format("{0}, {1}, {2}, {3}", this.Key, this.Pattern, this.CanDerived.ToString().ToLower(), this.Operator.ToString().ToLower());
    }


    public class TokenInfo
    {
        public int StartIndex { get; }
        public int EndIndex { get => this.StartIndex + this.Data.Length - 1; }
        public string Data { get; } = string.Empty;
        public TokenPatternInfo PatternInfo { get; }

        public TokenInfo(int startIndex, string data, TokenPatternInfo patternInfo)
        {
            this.StartIndex = startIndex;
            this.Data = data;

            this.PatternInfo = patternInfo;
        }

        public override string ToString()
        {
            string convertedString = this.Data.Replace("\r", "\\r");
            convertedString = convertedString.Replace("\n", "\\n");
            convertedString = convertedString.Replace("\t", "\\t");

            return string.Format("{0}, \"{1}\", {2}", this.StartIndex, convertedString, PatternInfo.OptionData);
        }
    }

}
