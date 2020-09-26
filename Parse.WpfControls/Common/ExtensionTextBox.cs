using Parse.WpfControls.EventArgs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

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
//                this.UpdateCaretInfo();
            };

            this.TextChanged += (s, e) =>
            {
//                TextChange changeInfo = e.Changes.First();
//                this.UpdateLineString(changeInfo);
            };
        }


        /// <summary>
        /// This function returns a line index and a caret index of a line from the current caret index.
        /// </summary>
        /// <param name="caretIndex">current caret index.</param>
        /// <returns>X : caret index of a line, Y : a line index.</returns>
        public System.Drawing.Point GetIndexInfoFromCaretIndex(int caretIndex)
        {
            System.Drawing.Point result = new System.Drawing.Point();

            for (int i = 0; i < this.LineIndexes.Count; i++)
            {
                if (this.LineIndexes[i] > caretIndex)
                {
                    result.Y = i - 1;
                    result.X = caretIndex - this.LineIndexes[i - 1];
                    break;
                }
                else result = new System.Drawing.Point(0, i);
            }

            return result;
        }

        public int GetStartingCaretIndexOfLineIndex(int lineIndex) => (lineIndex >= this.LineIndexes.Count) ? -1 : this.LineIndexes[lineIndex];

        protected void UpdateCaretInfo()
        {
            this.prevLineIndex = this.LineIndex;
            this.prevStartCaretIndexByLine = this.StartCaretIndexByLine;

            var point = this.GetIndexInfoFromCaretIndex(this.CaretIndex);
            this.LineIndex = point.Y;
            this.StartCaretIndexByLine = point.X;
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
            var point = this.GetIndexInfoFromCaretIndex(this.SelectionStart);
            selectionInfo.StartLine = point.Y;
            selectionInfo.StartCaretFromLine = point.X;

            point = this.GetIndexInfoFromCaretIndex(selectionEndIndex);
            selectionInfo.EndLine = point.Y;
            selectionInfo.EndCaretFromLine = point.X;

            this.selectionBlocks.Add(selectionInfo);
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
