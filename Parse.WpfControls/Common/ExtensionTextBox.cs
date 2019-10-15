using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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

        #region Dependency Properties releated with TextLogic
        public char BeforeCharFromCursor
        {
            get { return (char)GetValue(BeforeCharFromCursorProperty); }
            set { SetValue(BeforeCharFromCursorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BeforeCharFromCursor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BeforeCharFromCursorProperty =
            DependencyProperty.Register("BeforeCharFromCursor", typeof(char), typeof(ExtensionTextBox), new PropertyMetadata(' '));


        public StringCollection LineString
        {
            get { return (StringCollection)GetValue(LineStringProperty); }
            set { SetValue(LineStringProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineStringProperty =
            DependencyProperty.Register("LineString", typeof(StringCollection), typeof(ExtensionTextBox), new PropertyMetadata(new StringCollection()));


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
            Loaded += (s, e) =>
            {
                this.SelectionChanged += TextArea_SelectionChanged;
            };

            TextChanged += (s, e) =>
            {
                TextChange changeInfo = e.Changes.First();
                this.UpdateLineString(changeInfo);
            };
        }


        protected int GetLineIndexFromCaretIndex(int caretIndex)
        {
            int result = 0;
            int totalLength = 0;
            for (int i = 0; i < this.LineString.Count; i++)
            {
                totalLength += this.LineString[i].Length + Environment.NewLine.Length;
                if (totalLength > caretIndex) { result = i; break; }
            }

            return result;
        }

        protected int GetStartIndexFromLineIndex(int lineIndex, int caretIndex)
        {
            int totalLength = 0;
            for (int i = 0; i < lineIndex; i++) totalLength += this.LineString[i].Length + Environment.NewLine.Length;

            return caretIndex - totalLength;
        }

        protected void UpdateCaretInfo()
        {
            this.prevLineIndex = this.LineIndex;
            this.LineIndex = this.GetLineIndexFromCaretIndex(this.CaretIndex);

            this.prevStartCaretIndexByLine = this.StartCaretIndexByLine;
            this.StartCaretIndexByLine = GetStartIndexFromLineIndex(this.LineIndex, this.CaretIndex);
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
            selectionInfo.StartLine = this.GetLineIndexFromCaretIndex(this.SelectionStart);
            selectionInfo.StartCaretFromLine = this.GetStartIndexFromLineIndex(selectionInfo.StartLine, this.SelectionStart);

            selectionInfo.EndLine = this.GetLineIndexFromCaretIndex(selectionEndIndex);
            selectionInfo.EndCaretFromLine = this.GetStartIndexFromLineIndex(selectionInfo.EndLine, selectionEndIndex);

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

        /// <summary>
        /// This function adds line-string an added string.
        /// </summary>
        /// <see cref=""/>
        /// <param name="offset">Start position of addString</param>
        /// <param name="addString">added string</param>
        private void AddCharToLineString(int offset, string addString)
        {
            if (addString.Length == 0) return;

            int lineIndex = this.GetLineIndexFromCaretIndex(offset);
            int startCaretByLine = this.GetStartIndexFromLineIndex(lineIndex, offset);
            this.LineString[lineIndex] = this.LineString[lineIndex].Insert(startCaretByLine, addString);

            if (addString.Contains(Environment.NewLine) == false) return;

            string[] lines = this.LineString[lineIndex].Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            this.LineString[lineIndex] = lines.First();

            for (int i = 1; i < lines.Length; i++)
                this.LineString.Insert(lineIndex + i, lines[i]);
        }

        /// <summary>
        /// This function removes from the line-string a string.
        /// </summary>
        /// <param name="changeInfo"></param>
        private void DelCharFromLineString(TextChange changeInfo)
        {
            if (changeInfo.RemovedLength == 0) return;

            int sLineIndex = this.GetLineIndexFromCaretIndex(changeInfo.Offset);
            int startCaretByLine = this.GetStartIndexFromLineIndex(sLineIndex, changeInfo.Offset);

            int endIndex = changeInfo.Offset + changeInfo.RemovedLength;
            int eLineIndex = this.GetLineIndexFromCaretIndex(endIndex);
            int endCaretByLine = this.GetStartIndexFromLineIndex(eLineIndex, endIndex);

            string sLineString = this.LineString[sLineIndex];
            string eLineString = this.LineString[eLineIndex];

            if (sLineIndex == eLineIndex)
                this.LineString[sLineIndex] = sLineString.Remove(startCaretByLine, endCaretByLine - startCaretByLine);
            else
            {
                if (startCaretByLine < sLineString.Length) this.LineString[sLineIndex] = sLineString.Remove(startCaretByLine);
                this.LineString[sLineIndex] += eLineString.Substring(endCaretByLine);

                for (int i = sLineIndex + 1; i <= eLineIndex; i++) this.LineString.RemoveAt(sLineIndex + 1);
            }

            /*
            // single character deletion process
            if (this.SelectionBlocks.Count == 0)
            {
                if (this.StartCaretIndexByLine == 0)
                {
                    this.LineString[this.LineIndex - 1] += this.LineString[this.LineIndex];
                    this.LineString.RemoveAt(this.LineIndex);
                }
                else this.LineString[this.LineIndex] = this.LineString[this.LineIndex].Remove(this.StartCaretIndexByLine-1, 1);
            }
            // block deletion process
            else
            {
                foreach(var block in this.SelectionBlocks)
                {
                    string sLineString = this.LineString[block.StartLine];
                    string eLineString = this.LineString[block.EndLine];

                    if (block.StartLine == block.EndLine)
                        this.LineString[block.StartLine] = sLineString.Remove(block.StartCaretFromLine, block.EndCaretFromLine - block.StartCaretFromLine);
                    else
                    {
                        if (block.StartCaretFromLine < sLineString.Length)  this.LineString[block.StartLine] = sLineString.Remove(block.StartCaretFromLine);
                        this.LineString[block.StartLine] += eLineString.Substring(block.EndCaretFromLine);

                        for (int i = block.StartLine + 1; i <= block.EndLine; i++)  this.LineString.RemoveAt(block.StartLine + 1);
                    }
                }
            }
            */
        }
    }
}
