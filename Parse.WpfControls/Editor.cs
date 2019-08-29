using Parse.WpfControls.SyntaxEditorComponents;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Parse.WpfControls
{
    /// <summary>
    /// XAML 파일에서 이 사용자 지정 컨트롤을 사용하려면 1a 또는 1b단계를 수행한 다음 2단계를 수행하십시오.
    ///
    /// 1a단계) 현재 프로젝트에 있는 XAML 파일에서 이 사용자 지정 컨트롤 사용.
    /// 이 XmlNamespace 특성을 사용할 마크업 파일의 루트 요소에 이 특성을 
    /// 추가합니다.
    ///
    ///     xmlns:MyNamespace="clr-namespace:Parse.WpfControls"
    ///
    ///
    /// 1b단계) 다른 프로젝트에 있는 XAML 파일에서 이 사용자 지정 컨트롤 사용.
    /// 이 XmlNamespace 특성을 사용할 마크업 파일의 루트 요소에 이 특성을 
    /// 추가합니다.
    ///
    ///     xmlns:MyNamespace="clr-namespace:Parse.WpfControls;assembly=Parse.WpfControls"
    ///
    /// 또한 XAML 파일이 있는 프로젝트의 프로젝트 참조를 이 프로젝트에 추가하고
    /// 다시 빌드하여 컴파일 오류를 방지해야 합니다.
    ///
    ///     솔루션 탐색기에서 대상 프로젝트를 마우스 오른쪽 단추로 클릭하고
    ///     [참조 추가]->[프로젝트]를 차례로 클릭한 다음 이 프로젝트를 찾아서 선택합니다.
    ///
    ///
    /// 2단계)
    /// 계속 진행하여 XAML 파일에서 컨트롤을 사용합니다.
    ///
    ///     <MyNamespace:Editor/>
    ///
    /// </summary>
    public class Editor : TextBox
    {
        private DrawingControl renderCanvas;
        private DrawingControl lineNumbersCanvas;
        private ScrollViewer scrollViewer;
        private HashSet<SelectionInfo> selectionBlocks = new HashSet<SelectionInfo>();
        private Dictionary<string, TextStyle> textStyleDic = new Dictionary<string, TextStyle>();
        private Dictionary<string, TextStyle> patternStyleDic = new Dictionary<string, TextStyle>();
        private int prevLineIndex = 0;
        private int prevStartCaretIndexByLine = 0;
        private double lineHeight;
        private int maxLineCountInBlock;
        private HashSet<string> DelimiterSet = new HashSet<string>();

        /// <summary>
        /// This member is used to improve speed when collecting line string.
        /// </summary>
        private List<int> cacheLineFeedIndex = new List<int>();

        /// <summary>
        /// This member means maximum showable line count at one go.
        /// </summary>
        private int maxViewLineOnce = 100;

        public List<LineTextInfo> LineInfos { get; internal set; } = new List<LineTextInfo>();

        public uint TabSize { get; set; } = 4;

        public double LineHeight
        {
            get { return lineHeight; }
            set
            {
                if (value != lineHeight)
                {
                    lineHeight = value;
                    TextBlock.SetLineStackingStrategy(this, LineStackingStrategy.BlockLineHeight);
                    TextBlock.SetLineHeight(this, lineHeight);
                }
            }
        }

        public int MaxLineCountInBlock
        {
            get { return maxLineCountInBlock; }
            set
            {
                maxLineCountInBlock = value > 0 ? value : 0;
            }
        }




        public char BeforeCharFromCursor
        {
            get { return (char)GetValue(BeforeCharFromCursorProperty); }
            set { SetValue(BeforeCharFromCursorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BeforeCharFromCursor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BeforeCharFromCursorProperty =
            DependencyProperty.Register("BeforeCharFromCursor", typeof(char), typeof(Editor), new PropertyMetadata(' '));




        public StringCollection LineString
        {
            get { return (StringCollection)GetValue(LineStringProperty); }
            set { SetValue(LineStringProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineStringProperty =
            DependencyProperty.Register("LineString", typeof(StringCollection), typeof(Editor), new PropertyMetadata(new StringCollection()));


        public StringCollection ViewLineString
        {
            get { return (StringCollection)GetValue(ViewLineStringProperty); }
            set { SetValue(ViewLineStringProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewLineStringProperty =
            DependencyProperty.Register("ViewLineString", typeof(StringCollection), typeof(Editor), new PropertyMetadata(new StringCollection()));


        public int LineIndex
        {
            get { return (int)GetValue(LineIndexProperty); }
            set { SetValue(LineIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineIndexProperty =
            DependencyProperty.Register("LineIndex", typeof(int), typeof(Editor), new PropertyMetadata(0));


        public int StartCaretIndexByLine
        {
            get { return (int)GetValue(StartCaretIndexByLineProperty); }
            set { SetValue(StartCaretIndexByLineProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CaretIndexBD.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartCaretIndexByLineProperty =
            DependencyProperty.Register("StartCaretIndexByLine", typeof(int), typeof(Editor), new PropertyMetadata(0));



        static Editor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Editor), new FrameworkPropertyMetadata(typeof(Editor)));
            
//            TextProperty.OverrideMetadata(typeof(Editor), new FrameworkPropertyMetadata(new PropertyChangedCallback(TextPropertyChanged)));
        }

        public Editor()
        {
            this.DelimiterSet.Add(" ");
            this.DelimiterSet.Add("\r\n");

            this.LineString.Add(string.Empty);

            LineHeight = FontSize * 1.3;

            Loaded += (s, e) =>
            {
                renderCanvas = (DrawingControl)Template.FindName("PART_RenderCanvas", this);
                lineNumbersCanvas = (DrawingControl)Template.FindName("PART_LineNumbersCanvas", this);
                scrollViewer = (ScrollViewer)Template.FindName("PART_ContentHost", this);

                //                lineNumbersCanvas.Width = GetFormattedTextWidth(string.Format("{0:0000}", totalLineCount)) + 5;
                scrollViewer.ScrollChanged += OnScrollChanged;
                this.SelectionChanged += Editor_SelectionChanged;

                //                InvalidateBlocks(0);
                InvalidateVisual();
            };

            SizeChanged += (s, e) =>
            {
                if (e.HeightChanged == false)
                    return;
//                UpdateBlocks();
                InvalidateVisual();
            };

            TextChanged += (s, e) =>
            {
                this.UpdateLineString(e.Changes.First());
                InvalidateVisual();
            };
        }

        private int GetLineIndexFromCaretIndex(int caretIndex)
        {
            int result = 0;
            int totalLength = 0;
            for(int i=0; i<this.LineString.Count; i++)
            {
                totalLength += this.LineString[i].Length + Environment.NewLine.Length;
                if (totalLength > caretIndex) { result = i; break; }
            }

            return result;
        }

        private int GetStartIndexFromLineIndex(int lineIndex, int caretIndex)
        {
            int totalLength = 0;
            for (int i = 0; i < lineIndex; i++) totalLength += this.LineString[i].Length + Environment.NewLine.Length;

            return caretIndex - totalLength;
        }

        private int GetStartLineOnViewPos(double topViewPos) => (int)(topViewPos / this.lineHeight);

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

            for(int i=1; i<lines.Length; i++)
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
        /// This function gets the line-string-collection
        /// </summary>
        /// <param name="startLine">The start line that gets line-string-collection</param>
        /// <param name="cnt">Line count</param>
        /// <returns></returns>
        private StringCollection GetLineStringCollection(int startLine, int cnt)
        {
            StringCollection result = new StringCollection();

            if (startLine >= this.LineString.Count) return result;

            int maxCnt = (startLine + cnt < this.LineString.Count) ? startLine + cnt : this.LineString.Count;
            for (int i = startLine; i < maxCnt; i++) result.Add(this.LineString[i]);

            return result;
        }

        /// <summary>
        /// This function returns a string that is applied a style.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private FormattedText GetFormattedText(string text)
        {
            Brush foreBrush = Brushes.Black;
            if (this.textStyleDic.ContainsKey(text)) foreBrush = this.textStyleDic[text].ForeGround;
            else
            {
                foreach(var item in this.patternStyleDic)
                {
                    // all match
                    if (Regex.Match(text, item.Key).Length == text.Length)
                    {
                        foreBrush = item.Value.ForeGround;
                        break;
                    }
                }
            }

            FormattedText ft = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize,
                foreBrush);

            ft.Trimming = TextTrimming.None;
            ft.LineHeight = lineHeight;

            return ft;
        }

        private void UpdateCaretInfo()
        {
            this.prevLineIndex = this.LineIndex;
            this.LineIndex = this.GetLineIndexFromCaretIndex(this.CaretIndex);

            this.prevStartCaretIndexByLine = this.StartCaretIndexByLine;
            this.StartCaretIndexByLine = GetStartIndexFromLineIndex(this.LineIndex, this.CaretIndex);
        }

        private void Editor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            this.UpdateCaretInfo();
            this.selectionBlocks.Clear();

            this.BeforeCharFromCursor = (this.CaretIndex == 0) ? ' ' : this.Text[this.CaretIndex - 1];

            if (this.SelectionLength == 0)  return;


            SelectionInfo selectionInfo = new SelectionInfo();

            var selectionEndIndex = this.SelectionStart + this.SelectionLength;
            selectionInfo.StartLine = this.GetLineIndexFromCaretIndex(this.SelectionStart);
            selectionInfo.StartCaretFromLine = this.GetStartIndexFromLineIndex(selectionInfo.StartLine, this.SelectionStart);

            selectionInfo.EndLine = this.GetLineIndexFromCaretIndex(selectionEndIndex);
            selectionInfo.EndCaretFromLine = this.GetStartIndexFromLineIndex(selectionInfo.EndLine, selectionEndIndex);

            this.selectionBlocks.Add(selectionInfo);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                string tab = new string(' ', (int)TabSize);
                int caretPosition = base.CaretIndex;
                base.Text = base.Text.Insert(caretPosition, tab);
                base.CaretIndex = caretPosition + (int)TabSize;

                e.Handled = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <see cref="https://www.lucidchart.com/documents/edit/cdd4b7ff-4807-4df0-8360-bc75d3f9dd2b/0_0"/>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
//            if (!IsLoaded || renderCanvas == null || lineNumbersCanvas == null) return;
            if (!IsLoaded || renderCanvas == null) return;

            var dc = renderCanvas.GetContext();

            this.ViewLineString = this.GetLineStringCollection(this.GetStartLineOnViewPos(this.VerticalOffset), this.maxViewLineOnce);

            double yPos = this.lineHeight * (int)(this.VerticalOffset / this.lineHeight);
            foreach(var line in this.ViewLineString)
            {
                List<string> tokenList = SplitAndKeep(line, this.DelimiterSet.ToArray()).ToList();

                double itemDrawStartY = yPos;
                double itemDrawEndY = yPos + this.lineHeight;
                yPos = itemDrawEndY;
                double top = 0 - this.VerticalOffset;

                double itemDrawStartX = 2 - this.HorizontalOffset;

                foreach (var token in tokenList)
                {
                    FormattedText item = this.GetFormattedText(token);

                    if (top < ActualHeight)
                        dc.DrawText(item, new Point(itemDrawStartX, itemDrawStartY - this.VerticalOffset));

                    itemDrawStartX += item.WidthIncludingTrailingWhitespace;
                }
            }

            dc.Close();

            base.OnRender(drawingContext);
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0 || e.HorizontalChange != 0) InvalidateVisual();
        }

        /// <summary>
        /// This function adds to the editor a delimiter that is used to separate.
        /// </summary>
        /// <param name="delimiter"></param>
        public void AddDelimiterList(string delimiter)
        {
            this.DelimiterSet.Add(delimiter);
        }

        /// <summary>
        /// This function adds information which to syntax-highlight to the editor.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="foreBrush"></param>
        /// <param name="bPattern"></param>
        public void AddSyntaxHighLightInfo(string text, Brush foreBrush, bool bPattern = false)
        {
            if (bPattern)
                this.patternStyleDic.Add(text, new TextStyle(foreBrush, Brushes.Transparent));
            else
                this.textStyleDic.Add(text, new TextStyle(foreBrush, Brushes.Transparent));
        }

        static void TextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
        }


        /// <summary>
        /// This function split a string on the basis of delimiters.
        /// </summary>
        /// <see cref="https://www.lucidchart.com/documents/edit/41d20574-d843-41ce-ae44-2d3e29fbc716/0_0?beaconFlowId=E9C38964142A86C1"/>
        /// <param name="s"></param>
        /// <param name="delimiters"></param>
        /// <returns></returns>
        public static IEnumerable<string> SplitAndKeep(string s, params string[] delimiters)
        {
            List<string> result = new List<string>();

            string data = string.Empty;
            string delimiteStr = string.Empty;

            foreach(var ch in s)
            {
                if (delimiters.Contains(ch + delimiteStr))
                {
                    if(data.Length > 0) result.Add(data);
                    data = string.Empty;
                    delimiteStr += ch;
                }
                else
                {
                    if(delimiteStr.Length > 0) result.Add(delimiteStr);
                    delimiteStr = string.Empty;
                    data += ch;
                }
            }

            if (delimiteStr.Length > 0) result.Add(delimiteStr);
            if (data.Length > 0) result.Add(data);

            return result;
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

    public class LineTextInfo
    {
        public List<TextInfo> TokenInfos { get; internal set; }

    }
}
