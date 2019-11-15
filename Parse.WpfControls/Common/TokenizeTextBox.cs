using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Parse.WpfControls.Common
{
    public class TokenizeTextBox : ExtensionTextBox
    {
        public TokenData TokenData { get; }
        public List<int> TokenIndexes { get; } = new List<int>();
        public List<TokenInfo> Tokens { get; } = new List<TokenInfo>();
        public List<TokenPatternInfo> TokenPatternList = new List<TokenPatternInfo>();


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

        private TokenInfo GetTokenInfo(int offset)
        {
            int prevMaxIndex = 0;
            TokenInfo result = null;

            foreach (var maxIndex in this.TokenIndexes)
            {
                if (offset <= maxIndex)
                {
                    int accum = 0;

                    foreach (var item in this.Tokens)
                    {
                        int startIndex = (prevMaxIndex == 0) ? 0 + accum : (maxIndex - prevMaxIndex) + accum;
                        if (startIndex < offset && maxIndex >= offset)
                        {
                            result = new TokenInfo(startIndex, item.Data, null);
                            break;
                        }
                        accum += item.Data.Length;
                    }
                }

                prevMaxIndex = maxIndex;
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
            this.TokenPatternList.Sort(delegate (TokenPatternInfo t, TokenPatternInfo td)
            {
                if (t.CanDerived || td.CanDerived)
                {
                    if (t.CanDerived == false && td.CanDerived)
                    {
                        // CanDerived > Delimitable
                        return (t.Operator) ? 1 : -1;
                    }
                    else if (t.CanDerived && td.CanDerived == false)
                    {
                        // CanDerived > Delimitable
                        return (td.Operator) ? -1 : 1;
                    }
                    return 0;
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

            TokenInfo tokenInfo = this.GetTokenInfo(changeInfo.Offset);
            if (tokenInfo == null)
            {
                List<TokenInfo> tokens = new List<TokenInfo>();
                this.TokenIndexes.Add(addString.Length);

                this.SortTokenPatternList();

                string patternSum = string.Empty;
                string generateString = string.Empty;
                foreach (var pattern in this.TokenPatternList)
                {
                    generateString += "a";
                    patternSum += string.Format("(?<{1}>{0})|", pattern.Pattern, generateString);
//                    patternSum += string.Format("({0})|", pattern.Pattern);
                }


                patternSum = patternSum.Substring(0, patternSum.Length - 1);
                int prevEI = 0;
                foreach(var data in Regex.Matches(addString, patternSum, RegexOptions.Multiline | RegexOptions.ExplicitCapture))
                {
                    var matchData = data as Match;

                    TokenPatternInfo patternInfo = new TokenPatternInfo(string.Empty, string.Empty);
                    if (prevEI < matchData.Index)
                        tokens.Add(new TokenInfo(prevEI, addString.Substring(prevEI, matchData.Index - prevEI), patternInfo));

                    // The number of elements in the group is 1 more than the number of elements in the TokenPatternList.
                    for (int i=0; i<this.TokenPatternList.Count+1; i++)
                    {
                        var dd = matchData.Groups[8];
                        // The 0 index of the groups is always matched so it doesn't need to check.
                        if (matchData.Groups[i+1].Length > 0)
                        {
                            patternInfo = this.TokenPatternList[i];
                            break;
                        }
                    }

                    tokens.Add(new TokenInfo(matchData.Index, matchData.Value, patternInfo));

                    prevEI = matchData.Index + matchData.Length;
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
                int insertPos = changeInfo.Offset - tokenInfo.StartIndex;
                addString = tokenInfo.Data.Insert(insertPos, addString);

            }

        }

        /// <summary>
        /// This function returns a token index from the caretIndex
        /// </summary>
        /// <param name="caretIndex">The index of the caret</param>
        /// <returns>a token index</returns>
        public int GetTokenIndexFromCaretIndex(int caretIndex)
        {
            int index = -1;

            for(int i=0; i<this.Tokens.Count; i++)
            {
                TokenInfo tokenInfo = this.Tokens[i];

                if (tokenInfo.StartIndex <= caretIndex && caretIndex <= tokenInfo.EndIndex)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }
    }

    public class TokenWithCaretInfo
    {
        public enum PositionFromCaret { Included, FrontOfCaret, BackOfCaret }

        public int TokenIndex { get; }
        public TokenData TokenData { get; }

        public PositionFromCaret Type { get; }

        public TokenWithCaretInfo(int tokenIndex, TokenData tokenData, PositionFromCaret type)
        {
            this.TokenIndex = tokenIndex;
            this.TokenData = TokenData;
            this.Type = type;
        }
    }

    public class TokenPatternInfo
    {
        public object Type { get; }
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

        public TokenPatternInfo(object type, string pattern, bool bCanDerived = false, bool bOperator = false)
        {
            this.Type = type;
            this.OriginalPattern = pattern;
            this.CanDerived = bCanDerived;
            this.Operator = bOperator;
        }

        public override string ToString() => string.Format("{0}, {1}, {2}, {3}", this.Type, this.Pattern, this.CanDerived.ToString().ToLower(), this.Operator.ToString().ToLower());
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

            return string.Format("{0}, \"{1}\", {2}", this.StartIndex, convertedString, PatternInfo.Type);
        }
    }

}
