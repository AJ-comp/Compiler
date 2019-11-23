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
        private List<Tuple<int, int>> scopeSyntaxes = new List<Tuple<int, int>>();
        private List<TokenPatternInfo> tokenPatternList = new List<TokenPatternInfo>();
        private Dictionary<int, List<int>> tokenIndexesTable = new Dictionary<int, List<int>>();
        private SelectionTokensContainer selectionBlocks = new SelectionTokensContainer();
        private Tokenizer tokenizer = new Tokenizer();

        public List<TokenCell> Tokens { get; } = new List<TokenCell>();
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

            this.selectionBlocks = this.GetSelectionTokenInfos(this.SelectionStart, this.SelectionLength);
        }

        /// <summary>
        /// This function returns an information that selected.
        /// </summary>
        /// <returns></returns>
        private SelectionTokensContainer GetSelectionTokenInfos(int offset, int len)
        {
            SelectionTokensContainer result = new SelectionTokensContainer();
            int prevOffset = offset + len;
            RecognitionWay recognitionWay = RecognitionWay.Front;

            Parallel.For(0, this.Tokens.Count, (i, loopOption) =>
            {
                var token = this.Tokens[i];
                // If whole of the token is contained -> reserve delete
                if (token.MoreRange(offset, prevOffset))
                {
                    lock(result.WholeSelectionBag) result.WholeSelectionBag.Add(i);
                }
                // If overlap in part of the first token
                else if (token.Contains(offset, recognitionWay))
                {
                    int cIndex = offset - token.StartIndex;
                    int length = token.Data.Length - cIndex;
                    length = (len > length) ? length : len;

                    lock(result.PartSelectionBag) result.PartSelectionBag.Add(new Tuple<int, int, int>(i, cIndex, length));
                }
                // If overlap in part of the last token
                else if (token.Contains(prevOffset, recognitionWay))
                {
                    int cIndex = prevOffset - token.StartIndex;

                    lock(result.PartSelectionBag) result.PartSelectionBag.Add(new Tuple<int, int, int>(i, 0, cIndex));
                }
            });

            result.SortAll();

            return result;
        }

        private void DelTokens(TextChange changeInfo)
        {
            if (changeInfo.RemovedLength == 0) return;

            var delInfos = (changeInfo.RemovedLength == 1) ? this.GetSelectionTokenInfos(changeInfo.Offset, 1) : this.selectionBlocks;

            if(delInfos.WholeSelectionBag.Count != 0)
            {
                int delIndex = delInfos.WholeSelectionBag.First();

                // The calculation that influences the length of the array never use a parallel calculation without synchronization. (ex : insert, delete)
                Parallel.For(0, delInfos.WholeSelectionBag.Count, i =>
                {
                    // for synchronization
                    lock (this.Tokens) this.Tokens.RemoveAt(delIndex);
                });



                // The Rectangle Deletion operation need to write other algorithm also the algorithm will very complicate so I don't write it yet.
                // (Can use the above data struct on the Rectangle Deletion operation.)
            }
        }

        private void AddTokens(TextChange changeInfo)
        {
            if (changeInfo.AddedLength == 0) return;

            RecognitionWay recognitionWay = RecognitionWay.Back;
            string addString = this.Text.Substring(changeInfo.Offset, changeInfo.AddedLength);

            int curTokenIndex = this.GetTokenIndexFromCaretIndex(changeInfo.Offset, recognitionWay);
            int nextTokenIndex = curTokenIndex + 1;
            if (this.Tokens.Count == 0)
            {
                this.tokenizer.Tokenize(addString, nextTokenIndex).ForEach(i => this.Tokens.Add(i));

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
                    TokenCell nextToken = this.Tokens[nextTokenIndex];

                    int lastTokenIndex = this.ReplaceToken(nextTokenIndex, nextToken.MergeStringToFront(addString));
                    this.ContinousTokenize(lastTokenIndex);
                }
                else
                {
                    TokenCell token = this.Tokens[curTokenIndex];
                    string mergeString = token.MergeString(changeInfo.Offset, addString, recognitionWay);

                    this.ContinousTokenize(this.ReplaceToken(curTokenIndex, mergeString));
                }
            }
        }

        /// <summary>
        /// This function replaces exist token to new tokens that are generated after tokenize the replaceString.
        /// </summary>
        /// <param name="tokenIndex">The index of the exist token</param>
        /// <param name="replaceString"> </param>
        /// <returns>The last index of the replaced token.</returns>
        private int ReplaceToken(int tokenIndex, string replaceString)
        {
            TokenCell token = this.Tokens[tokenIndex];
            int addLength = replaceString.Length - token.Data.Length;

            Parallel.For(tokenIndex + 1, this.Tokens.Count, i =>
            {
                this.Tokens[i].StartIndex += addLength;
            });

            int prevTokenCnt = this.Tokens.Count;
            int basisIndex = (tokenIndex == 0) ? 0 : this.Tokens[tokenIndex - 1].EndIndex + 1;

            this.Tokens.RemoveAt(tokenIndex);
            this.tokenizer.Tokenize(replaceString, basisIndex).ForEach(i => this.Tokens.Insert(tokenIndex++, i));

            return tokenIndex - 1;
        }

        /// <summary>
        /// This function tokenize after merges the token of the token index-1 with the token of the token index until there's no change.
        /// </summary>
        /// <param name="tokenIndex"></param>
        private void ContinousTokenize(int tokenIndex)
        {
            while (true)
            {
                // If tokenIndex is the last of the token then break.
                if (tokenIndex == this.Tokens.Count - 1) break;

                // Check next token
                var nextToken = this.Tokens[tokenIndex];
                string mergeString = nextToken.MergeStringToEnd(this.Tokens[tokenIndex + 1].Data);

                int basisIndex = (tokenIndex == 0) ? 0 : this.Tokens[tokenIndex].StartIndex;
                List<TokenCell> result = this.tokenizer.Tokenize(mergeString, basisIndex);
                if (result[0].Data == this.Tokens[tokenIndex].Data) break;

                this.Tokens.RemoveAt(tokenIndex);
                this.Tokens.RemoveAt(tokenIndex);

                result.ForEach(i => this.Tokens.Insert(tokenIndex++, i));
                tokenIndex--;
            }
        }

        private void UpdateTokenInfos(TextChange changeInfo)
        {
            this.DelTokens(changeInfo);
            this.AddTokens(changeInfo);
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
                TokenCell tokenInfo = this.Tokens[i];

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
            this.tokenizer.RegistTokenizeRule(this.tokenPatternList);
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
