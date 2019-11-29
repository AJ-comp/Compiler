using Parse.Tokenize;
using Parse.WpfControls.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Parse.WpfControls.Common
{
    public class TokenizeTextBox : ExtensionTextBox
    {
        private List<Tuple<int, int>> scopeSyntaxes = new List<Tuple<int, int>>();
        private Dictionary<int, List<int>> tokenIndexesTable = new Dictionary<int, List<int>>();
        private SelectionTokensContainer selectionBlocks = new SelectionTokensContainer();
        private TokenizeFactory tokenizeFactory = new TokenizeFactory();

        public List<TokenCell> Tokens { get => this.tokenizeFactory.StorageTeam.AllTokens; }
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

        private void UpdateTokenIndex()
        {
            this.TokenIndex = this.tokenizeFactory.StorageTeam.TokenIndexForOffset(this.CaretIndex);

            this.selectionBlocks = this.GetSelectionTokenInfos(this.SelectionStart, this.SelectionLength);
        }

        /// <summary>
        /// This function returns a selected information.
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

            this.tokenizeFactory.ReceiveOrder(delInfos);
        }

        private void UpdateTokenInfos(TextChange changeInfo)
        {
            this.DelTokens(changeInfo);

            if (changeInfo.AddedLength == 0) return;

            string addString = this.Text.Substring(changeInfo.Offset, changeInfo.AddedLength);
            this.tokenizeFactory.ReceiveOrder(changeInfo.Offset, addString);
        }

        public int GetTokenIndexForCaretIndex(int caretIndex, RecognitionWay recognitionWay) => this.tokenizeFactory.StorageTeam.TokenIndexForOffset(caretIndex, recognitionWay);

        /// <summary>
        /// This function regist a pattern for tokenizing.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="optionData"></param>
        /// <param name="bCanDerived"></param>
        /// <param name="bOperator"></param>
        public void AddTokenPattern(string text, object optionData = null, bool bCanDerived = false, bool bOperator = false)
        {
            this.tokenizeFactory.AddTokenRule(text, optionData, bCanDerived, bOperator);
        }

        public void AddScopeGroup(string startScopeSymbol, string endScopeSymbol)
        {
            /*
            int startScopeKey = this.tokenizeFactory.GetPatternInfo(startScopeSymbol);
            int endScopeKey = this.tokenizeFactory.GetPatternInfo(endScopeSymbol);

            if (startScopeKey > 0 || endScopeKey > 0) return;
            this.scopeSyntaxes.Add(new Tuple<int, int>(startScopeKey, endScopeKey));
            */
        }
    }
}
