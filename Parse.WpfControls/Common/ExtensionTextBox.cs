using Parse.WpfControls.EventArgs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Parse.WpfControls.Common
{
    /// <summary>
    /// This class extens a feature of TextBox control.
    /// </summary>
    public class ExtensionTextBox : TextBox
    {
        private int prevLineIndex = 0;
        private int prevStartCaretIndexByLine = 0;
        private HashSet<SelectionInfo> selectionBlocks = new HashSet<SelectionInfo>();

        public delegate void LineChangedEventHandler(object sender, LineChangedEventArgs e);
        //        public new LineChangedEventHandler TextChanged;

        public List<int> LineIndexes { get; } = new List<int>();
//        public Dictionary<int, TokenDataList> TokenDataByLine { get; } = new Dictionary<int, TokenDataList>();
//        public BlockManager BlockManager { get; } = new BlockManager();

        #region Dependency Properties releated with TextLogic
        public char BeforeCharFromCursor
        {
            get { return (char)GetValue(BeforeCharFromCursorProperty); }
            set { SetValue(BeforeCharFromCursorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BeforeCharFromCursor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BeforeCharFromCursorProperty =
            DependencyProperty.Register("BeforeCharFromCursor", typeof(char), typeof(ExtensionTextBox), new PropertyMetadata(' '));


        public ObservableCollection<string> TokenPatternList
        {
            get { return (ObservableCollection<string>)GetValue(TokenPatternListProperty); }
            set { SetValue(TokenPatternListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TokenPatternList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TokenPatternListProperty =
            DependencyProperty.Register("TokenPatternList", typeof(ObservableCollection<string>), typeof(ExtensionTextBox), new PropertyMetadata(new ObservableCollection<string>()));




        public ObservableCollection<PatternInfo> TokenPattern
        {
            get { return (ObservableCollection<PatternInfo>)GetValue(TokenPatternProperty); }
            set { SetValue(TokenPatternProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TokenPattern.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TokenPatternProperty =
            DependencyProperty.Register("TokenPattern", typeof(ObservableCollection<PatternInfo>), typeof(ExtensionTextBox), new PropertyMetadata(new ObservableCollection<PatternInfo>()));




        public int LineIndex
        {
            get { return (int)GetValue(LineIndexProperty); }
            set { SetValue(LineIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineIndexProperty =
            DependencyProperty.Register("LineIndex", typeof(int), typeof(ExtensionTextBox), new PropertyMetadata(0));


        public int StartCaretIndexByLine
        {
            get { return (int)GetValue(StartCaretIndexByLineProperty); }
            set { SetValue(StartCaretIndexByLineProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CaretIndexBD.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartCaretIndexByLineProperty =
            DependencyProperty.Register("StartCaretIndexByLine", typeof(int), typeof(ExtensionTextBox), new PropertyMetadata(0));

        #endregion

        public ExtensionTextBox()
        {
            this.LineIndexes.Add(0);

            this.Loaded += (s, e) =>
            {
                this.SelectionChanged += TextArea_SelectionChanged;
            };

            this.SelectionChanged += (s, e) =>
            {
                this.UpdateCaretInfo();
            };

            this.TextChanged += (s, e) =>
            {
                TextChange changeInfo = e.Changes.First();
                this.UpdateLineString(changeInfo);
            };
        }


        /// <summary>
        /// This function returns a line index and a caret index of a line from the current caret index.
        /// </summary>
        /// <param name="caretIndex">current caret index.</param>
        /// <returns>X : caret index of a line, Y : a line index.</returns>
        public Point GetIndexInfoFromCaretIndex(int caretIndex)
        {
            Point result = new Point();

            for (int i = 0; i < this.LineIndexes.Count; i++)
            {
                if (this.LineIndexes[i] > caretIndex)
                {
                    result.Y = i - 1;
                    result.X = caretIndex - this.LineIndexes[i - 1];
                    break;
                }
                else result = new Point(0, i);
            }

            return result;
        }


        protected void UpdateCaretInfo()
        {
            this.prevLineIndex = this.LineIndex;
            this.prevStartCaretIndexByLine = this.StartCaretIndexByLine;

            var point = this.GetIndexInfoFromCaretIndex(this.CaretIndex);
            this.LineIndex = (int)point.Y;
            this.StartCaretIndexByLine = (int)point.X;
        }

        private void TextArea_SelectionChanged(object sender, RoutedEventArgs e)
        {
            this.UpdateCaretInfo();
            this.selectionBlocks.Clear();

            this.BeforeCharFromCursor = (this.CaretIndex == 0) ? ' ' : this.Text[this.CaretIndex - 1];
            // For current line hightlight effect
            //            if (this.prevLineIndex != this.LineIndex) this.InvalidateVisual();
            if (this.SelectionLength == 0) return;

            SelectionInfo selectionInfo = new SelectionInfo();

            var selectionEndIndex = this.SelectionStart + this.SelectionLength;
            Point point = this.GetIndexInfoFromCaretIndex(this.SelectionStart);
            selectionInfo.StartLine = (int)point.Y;
            selectionInfo.StartCaretFromLine = (int)point.X;

            point = this.GetIndexInfoFromCaretIndex(selectionEndIndex);
            selectionInfo.EndLine = (int)point.Y;
            selectionInfo.EndCaretFromLine = (int)point.X;

            this.selectionBlocks.Add(selectionInfo);
        }

        /// <summary>
        /// This function updates a line-string when is changed string of editor.
        /// </summary>
        /// <param name="changeInfo">update information</param>
        private void UpdateLineString(TextChange changeInfo)
        {
            string addString = this.Text.Substring(changeInfo.Offset, changeInfo.AddedLength);

            this.DelCharFromLineString(changeInfo);
            this.AddCharToLineString(changeInfo.Offset, addString);
        }

        /*
        private void EditTokenFromChar(int lineIndex, int startCaretByLine, string addString)
        {

            else
            {
                

                int minIndex = 0xff;
                PatternInfo selectedPattern = null;
                foreach (var pattern in this.TokenPattern)
                {
                    Match matchInfo = Regex.Match(token, pattern.StartPattern);
                    if (matchInfo.Success == false) continue;

                    int curIndex = matchInfo.Index;
                    if (minIndex > curIndex)
                    {
                        minIndex = curIndex;
                        selectedPattern = pattern;
                    }
                }

                if (selectedPattern != null)
                {
                    selectedPattern.EndPattern
                }
            }
        }

        /// <summary>
        /// This function merge addString with existing token and returns merged token.
        /// </summary>
        /// <param name="lineIndex"></param>
        /// <param name="startCaretByLine"></param>
        /// <param name="addString">a string that tries to merge.</param>
        /// <returns>first : token index, second : TokenData</returns>
        private Tuple<int, TokenData> TokenMerge(int lineIndex, int startCaretByLine, string addString)
        {
            if (this.TokenDataByLine.ContainsKey(lineIndex) == false)
            {
                var tokenList = new TokenDataList();
                var tokenData = new TokenData() { Token = addString, StartIndex = startCaretByLine, bEnd = false };
                tokenList.Add(tokenData);

                this.TokenDataByLine.Add(lineIndex, tokenList);
                return new Tuple<int, TokenData>(0, tokenData);
            }
            else
            {
                var tokenDataList = this.TokenDataByLine[lineIndex].GetCandidateTokens(startCaretByLine);
                if(tokenDataList != null)
                {

                    tokenData.Item2.Token.Insert(startCaretByLine, addString);
                    return new Tuple<int, TokenData>(tokenData.Item1, tokenData.Item2);
                }

                tokenData = 
                {

                }
            }
        }

        private void EditToken(int lineIndex, int startCaretByLine, string addString)
        {
            if (addString.Length == 1) this.EditTokenFromChar(lineIndex, startCaretByLine, addString);


        }
        */

        /// <summary>
        /// This function adds line-string an added string.
        /// </summary>
        /// <see cref=""/>
        /// <param name="offset">Start position of addString</param>
        /// <param name="addString">added string</param>
        private void AddCharToLineString(int offset, string addString)
        {
            if (addString.Length == 0) return;

            int index = this.LineIndexes.FindIndex(p => p > offset);
            if (index >= 0)
            {
                for (int i = index; i < this.LineIndexes.Count; i++)
                    this.LineIndexes[i] += addString.Length;
            }

            int startIndex = 0;
            while(true)
            {
                var newLineIndex = addString.IndexOf(Environment.NewLine, startIndex);
                if (newLineIndex < 0) break;

                startIndex = newLineIndex + Environment.NewLine.Length;

                this.LineIndexes.Add(offset + startIndex);
            }

            this.LineIndexes.Sort();

//            this.EditToken(lineIndex, startCaretByLine, addString);
        }

        /// <summary>
        /// This function removes from the line-string a string.
        /// </summary>
        /// <param name="changeInfo"></param>
        private void DelCharFromLineString(TextChange changeInfo)
        {
            if (changeInfo.RemovedLength == 0) return;

            int findIndex = 0;
            List<int> removeIndexes = new List<int>();
            for (int i = changeInfo.Offset+1; i <= changeInfo.Offset + changeInfo.RemovedLength; i++)
            {
                for(int j=findIndex; j<this.LineIndexes.Count; j++)
                {
                    if (this.LineIndexes[j] <= i) findIndex = j;
                    else break;

                    if (this.LineIndexes[j] == i)
                    {
                        removeIndexes.Add(j);
                        break;
                    }
                }
            }
            
            if(removeIndexes.Count > 0)
                this.LineIndexes.RemoveRange(removeIndexes.First(), removeIndexes.Count);

            int index = this.LineIndexes.FindIndex(p => p > changeInfo.Offset + changeInfo.RemovedLength);
            if (index >= 0)
            {
                for (int i = index; i < this.LineIndexes.Count; i++)
                    this.LineIndexes[i] -= changeInfo.RemovedLength;
            }

        }
    }


    public class SelectionInfo
    {
        public int Length { get; internal set; }

        public int StartLine { get; internal set; }
        public int StartCaretFromLine { get; internal set; }

        public int EndLine { get; internal set; }
        public int EndCaretFromLine { get; internal set; }
    }

    public class PatternInfo
    {
        public string StartPattern { get; set; }
        public string EndPattern { get; set; }
    }
}
