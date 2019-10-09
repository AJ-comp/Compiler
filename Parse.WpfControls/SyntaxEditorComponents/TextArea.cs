using Parse.WpfControls.SyntaxEditorComponents.EventArgs;
using Parse.WpfControls.SyntaxEditorComponents.Models;
using Parse.WpfControls.SyntaxEditorComponents.ViewModels;
using Parse.WpfControls.SyntaxEditorComponents.Views;
using Parse.WpfControls.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Parse.WpfControls.SyntaxEditorComponents
{
    public enum CompletionItemType { Field, Keyword, Property, Enum, Namespace, CodeSnipp, Function, Event, Delegate, Class, Struct, Interface };

    public class TextArea : TextBox
    {
        private TextViewer renderCanvas;
        private ScrollViewer scrollViewer;
        private CompletionList completionList;

        private HashSet<SelectionInfo> selectionBlocks = new HashSet<SelectionInfo>();
        private Dictionary<string, TextStyle> textStyleDic = new Dictionary<string, TextStyle>();
        private Dictionary<string, TextStyle> patternStyleDic = new Dictionary<string, TextStyle>();

        private int prevLineIndex = 0;
        private int prevStartCaretIndexByLine = 0;
        private HashSet<string> DelimiterSet = new HashSet<string>();
        /// <summary>This member means maximum showable line count at one go.</summary>
        private int maxViewLineOnce = 100;

        /// <summary> This property gets or sets the tab size. </summary>
        public uint TabSize { get; set; } = 4;
        /// <summary> This property gets the caret index when completion list occur. </summary>
        public int CaretIndexWhenCLOccur { get; private set; }


        #region Dependency Properties
        public double LineHeight
        {
            get { return (double)GetValue(LineHeightProperty); }
            internal set { SetValue(LineHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineHeightProperty =
            DependencyProperty.Register("LineHeight", typeof(double), typeof(TextArea), new PropertyMetadata(new PropertyChangedCallback(LineHeightChanged)));


        public static void LineHeightChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            TextArea area = dp as TextArea;

            TextBlock.SetLineStackingStrategy(area, LineStackingStrategy.BlockLineHeight);
            TextBlock.SetLineHeight(area, (double)args.NewValue);
        }

        public char BeforeCharFromCursor
        {
            get { return (char)GetValue(BeforeCharFromCursorProperty); }
            set { SetValue(BeforeCharFromCursorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BeforeCharFromCursor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BeforeCharFromCursorProperty =
            DependencyProperty.Register("BeforeCharFromCursor", typeof(char), typeof(TextArea), new PropertyMetadata(' '));


        public Brush SelectionLineBrush
        {
            get { return (Brush)GetValue(SelectionLineBrushProperty); }
            set { SetValue(SelectionLineBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectionLineBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionLineBrushProperty =
            DependencyProperty.Register("SelectionLineBrush", typeof(Brush), typeof(TextArea), new PropertyMetadata(Brushes.Transparent));


        public Brush SelectionLineBorderBrush
        {
            get { return (Brush)GetValue(SelectionLineBorderBrushProperty); }
            set { SetValue(SelectionLineBorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectionBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionLineBorderBrushProperty =
            DependencyProperty.Register("SelectionLineBorderBrush", typeof(Brush), typeof(TextArea), new PropertyMetadata(Brushes.Transparent));


        public int SelectionLineBorderThickness
        {
            get { return (int)GetValue(SelectionLineBorderThicknessProperty); }
            set { SetValue(SelectionLineBorderThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectionBorderThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionLineBorderThicknessProperty =
            DependencyProperty.Register("SelectionLineBorderThickness", typeof(int), typeof(TextArea), new PropertyMetadata(1));


        public StringCollection LineString
        {
            get { return (StringCollection)GetValue(LineStringProperty); }
            set { SetValue(LineStringProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineStringProperty =
            DependencyProperty.Register("LineString", typeof(StringCollection), typeof(TextArea), new PropertyMetadata(new StringCollection()));


        public List<ViewStringInfo> ViewLineString
        {
            get { return (List<ViewStringInfo>)GetValue(ViewLineStringProperty); }
            set { SetValue(ViewLineStringProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewLineStringProperty =
            DependencyProperty.Register("ViewLineString", typeof(List<ViewStringInfo>), typeof(TextArea), new PropertyMetadata(new List<ViewStringInfo>()));


        public int LineIndex
        {
            get { return (int)GetValue(LineIndexProperty); }
            set { SetValue(LineIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineIndexProperty =
            DependencyProperty.Register("LineIndex", typeof(int), typeof(TextArea), new PropertyMetadata(0));


        public int StartCaretIndexByLine
        {
            get { return (int)GetValue(StartCaretIndexByLineProperty); }
            set { SetValue(StartCaretIndexByLineProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CaretIndexBD.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartCaretIndexByLineProperty =
            DependencyProperty.Register("StartCaretIndexByLine", typeof(int), typeof(TextArea), new PropertyMetadata(0));

        #endregion

        #region Routed Events
        public static readonly RoutedEvent ScrollChangedEvent = EventManager.RegisterRoutedEvent("ScrollChanged", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(TextArea));

        // .NET wrapper
        public event RoutedEventHandler ScrollChanged
        {
            add { AddHandler(ScrollChangedEvent, value); }
            remove { RemoveHandler(ScrollChangedEvent, value); }
        }

        public static readonly RoutedEvent RenderedEvent = EventManager.RegisterRoutedEvent("Rendered", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(TextArea));

        // .NET wrapper
        public event RoutedEventHandler Rendered
        {
            add { AddHandler(RenderedEvent, value); }
            remove { RemoveHandler(RenderedEvent, value); }
        }
        #endregion


        static TextArea()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextArea), new FrameworkPropertyMetadata(typeof(TextArea)));
        }

        public TextArea()
        {
            this.LineString.Add(string.Empty);

//            this.AddHandler(ListBox.MouseLeftButtonDownEvent, new RoutedEventHandler(this.OnMouseLeftClick), true);

            Loaded += (s, e) =>
            {
                this.scrollViewer.ScrollChanged += OnScrollChanged;
                this.SelectionChanged += TextArea_SelectionChanged;
                this.completionList.listBox.SelectionChanged += ((ls, le) => this.completionList.listBox.ScrollIntoView(this.completionList.listBox.SelectedItem));

                InvalidateVisual();
            };

            SizeChanged += (s, e) =>
            {
                if (e.HeightChanged == false) return;

                InvalidateVisual();
            };

            TextChanged += (s, e) =>
            {
                TextChange changeInfo = e.Changes.First();
                this.UpdateLineString(changeInfo);
                this.GenerateCompletionList(changeInfo);

                InvalidateVisual();
            };
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.renderCanvas = (TextViewer)Template.FindName("PART_RenderCanvas", this);
            this.scrollViewer = (ScrollViewer)Template.FindName("PART_ContentHost", this);
            this.completionList = (CompletionList)Template.FindName("PART_CompletionList", this);

            var completionListContext = this.completionList.DataContext as CompletionListViewModel;
            this.completionList.listBox.SelectionChanged += ((s, e) => this.Focus());
            completionListContext.RequestFilterButtonClick += ((s, e) => this.Focus());
        }

        private bool IsBackSpace(TextChange changeInfo) => (changeInfo.RemovedLength >= 1 && changeInfo.AddedLength == 0);

        /// <summary>
        /// This function generates completion list.
        /// </summary>
        /// <param name="changeInfo"></param>
        private void GenerateCompletionList(TextChange changeInfo)
        {
            var addString = this.Text.Substring(changeInfo.Offset, changeInfo.AddedLength);
            if (addString.Length > 1) { this.completionList.IsOpen = false; return; }
            if (this.DelimiterSet.Contains(addString)) { this.completionList.IsOpen = false; return; }

            var context = this.completionList.DataContext as CompletionListViewModel;
            if(this.IsBackSpace(changeInfo))
            {
                if (this.completionList.IsOpen == false) return;
                if (this.CaretIndex <= this.CaretIndexWhenCLOccur) { this.completionList.IsOpen = false; return; }
            }
            else if(this.completionList.IsOpen == false)
            {
                if(addString.Length == 1)
                {
                    context.LoadAvailableCollection();
                    this.CaretIndexWhenCLOccur = this.CaretIndex - 1;
                }
            }

            var rect = this.GetRectFromCharacterIndex(this.CaretIndex);

            this.completionList.StaysOpen = false;
            this.completionList.Placement = PlacementMode.Relative;
            this.completionList.PlacementTarget = this;
            this.completionList.VerticalOffset = rect.Y + this.LineHeight;
            this.completionList.HorizontalOffset = rect.X;

            context.InputString = this.Text.Substring(this.CaretIndexWhenCLOccur, this.CaretIndex - this.CaretIndexWhenCLOccur);
            this.completionList.IsOpen = true;
        }

        private int GetLineIndexFromCaretIndex(int caretIndex)
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

        private int GetStartIndexFromLineIndex(int lineIndex, int caretIndex)
        {
            int totalLength = 0;
            for (int i = 0; i < lineIndex; i++) totalLength += this.LineString[i].Length + Environment.NewLine.Length;

            return caretIndex - totalLength;
        }

        private int GetStartLineOnViewPos(double topViewPos) => (int)(topViewPos / this.LineHeight);


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

        /// <summary>
        /// This function gets the line-string-collection
        /// </summary>
        /// <param name="startLine">The start line that gets line-string-collection</param>
        /// <param name="cnt">Line count</param>
        /// <returns></returns>
        private List<ViewStringInfo> GetLineStringCollection(int startLine, int cnt)
        {
            List<ViewStringInfo> result = new List<ViewStringInfo>();

            if (startLine >= this.LineString.Count) return result;

            int maxCnt = (startLine + cnt < this.LineString.Count) ? startLine + cnt : this.LineString.Count;
            for (int i = startLine; i < maxCnt; i++) result.Add(new ViewStringInfo(this.LineString[i], i));

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
                foreach (var item in this.patternStyleDic)
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
            ft.LineHeight = this.LineHeight;

            return ft;
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

        private void UpdateCaretInfo()
        {
            this.prevLineIndex = this.LineIndex;
            this.LineIndex = this.GetLineIndexFromCaretIndex(this.CaretIndex);

            this.prevStartCaretIndexByLine = this.StartCaretIndexByLine;
            this.StartCaretIndexByLine = GetStartIndexFromLineIndex(this.LineIndex, this.CaretIndex);
        }

        private LineFormattedText GetLineFormattedText(string line)
        {
            LineFormattedText result = new LineFormattedText();

            foreach (var token in StringUtility.SplitAndKeep(line, this.DelimiterSet.ToArray()).ToList())
                result.Add(this.GetFormattedText(token));

            return result;
        }

        private void DrawCore(double itemDrawStartX, double itemDrawStartY, ViewStringInfo viewLineData, DrawingContext dc)
        {
            /*
            // Draw current line rectangle.
            if (viewLineData.AbsoluteLineIndex == this.LineIndex)
            {
                var currentLineRect = new Rect(itemDrawStartX, itemDrawStartY, this.RenderSize.Width, this.lineHeight);
                dc.DrawRectangle(this.CurrentLineBrush, null, currentLineRect);
            }
            */
        }

        /// <summary>
        /// This function brings to TextArea a selected text in the completion list.
        /// </summary>
        /// <param name="other"></param>
        private void BringStringFromCompletionList(string other = "")
        {
            var context = this.completionList.DataContext as CompletionListViewModel;

            int startIndex = this.CaretIndexWhenCLOccur;
            int endIndex = this.CaretIndex;

            if (context.SelectedIndex >= 0)
            {
                var addString = context.CandidateCollection[context.SelectedIndex].ItemName + other;

                this.Select(startIndex, endIndex - startIndex);
                TextCompositionManager.StartComposition(new TextComposition(InputManager.Current, this, addString));
            }
            else if(other.Length > 0)
                TextCompositionManager.StartComposition(new TextComposition(InputManager.Current, this, other));
        }

        private bool InputProcessOnCompletionList(Key keyType)
        {
            bool result = false;

            if (keyType == Key.Up)
            {
                var context = this.completionList.DataContext as CompletionListViewModel;
                context.Up();
                result = true;
            }
            else if(keyType == Key.Down)
            {
                var context = this.completionList.DataContext as CompletionListViewModel;
                context.Down();
                result = true;
            }
            else if (keyType == Key.Enter || keyType == Key.Tab)
            {
                this.completionList.IsOpen = false;
                this.BringStringFromCompletionList();
                result = true;
            }
            else if(keyType == Key.Space || keyType == Key.OemPeriod)
            {
                this.completionList.IsOpen = false;
                if(keyType == Key.Space)  this.BringStringFromCompletionList(" ");
                else if(keyType == Key.OemPeriod) this.BringStringFromCompletionList(".");

                result = true;
            }
            else if(keyType == Key.Escape)
            {
                this.completionList.IsOpen = false;
                result = true;
            }

            return result;
        }


        private void TextArea_SelectionChanged(object sender, RoutedEventArgs e)
        {
            this.UpdateCaretInfo();
            this.selectionBlocks.Clear();

            this.BeforeCharFromCursor = (this.CaretIndex == 0) ? ' ' : this.Text[this.CaretIndex - 1];
            // For current line hightlight effect
//            if (this.prevLineIndex != this.LineIndex) this.InvalidateVisual();
            if (this.SelectionLength == 0)  return;

            SelectionInfo selectionInfo = new SelectionInfo();

            var selectionEndIndex = this.SelectionStart + this.SelectionLength;
            selectionInfo.StartLine = this.GetLineIndexFromCaretIndex(this.SelectionStart);
            selectionInfo.StartCaretFromLine = this.GetStartIndexFromLineIndex(selectionInfo.StartLine, this.SelectionStart);

            selectionInfo.EndLine = this.GetLineIndexFromCaretIndex(selectionEndIndex);
            selectionInfo.EndCaretFromLine = this.GetStartIndexFromLineIndex(selectionInfo.EndLine, selectionEndIndex);

            this.selectionBlocks.Add(selectionInfo);
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0 || e.HorizontalChange != 0)
            {
                InvalidateVisual();

                e.RoutedEvent = TextArea.ScrollChangedEvent;
                this.RaiseEvent(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            //            if (!IsLoaded || renderCanvas == null || lineNumbersCanvas == null) return;
            if (!IsLoaded || renderCanvas == null) return;

            int startLine = this.GetStartLineOnViewPos(this.VerticalOffset);
            this.ViewLineString = this.GetLineStringCollection(startLine, this.maxViewLineOnce);

            List<LineFormattedText> drawnItems = new List<LineFormattedText>();
            foreach (var viewLineData in this.ViewLineString)
            {
                var item = this.GetLineFormattedText(viewLineData.Data);

                if (viewLineData.AbsoluteLineIndex == this.LineIndex)
                {
                    item.BackGroundBrush = this.SelectionLineBrush;
                    item.BorderBrush = this.SelectionLineBorderBrush;
                    item.BorderThickness = this.SelectionLineBorderThickness;
                }

                drawnItems.Add(item);
            }

            this.renderCanvas.DrawAll(drawnItems, this.HorizontalOffset, this.VerticalOffset, this.LineHeight);

            base.OnRender(drawingContext);

            int startNumber = startLine + 1;
            int endNumber = startLine + 1 + this.ViewLineString.Count;
            this.RaiseEvent(new EditorRenderedEventArgs(TextArea.RenderedEvent, startLine+1, startNumber + endNumber, 
                                                                                this.HorizontalOffset, this.VerticalOffset, this.LineHeight));
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (this.completionList.IsOpen)
            {
                if (this.InputProcessOnCompletionList(e.Key)) e.Handled = true;
                return;
            }

            if (e.Key == Key.Tab)
            {
                string tab = new string(' ', (int)TabSize);
                TextCompositionManager.StartComposition(new TextComposition(InputManager.Current, this, tab));

                e.Handled = true;
            }
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            if(this.completionList.listBox.IsMouseOver)
            {
                var src = VisualTreeHelper.GetParent(e.OriginalSource as DependencyObject);
                if(src is VirtualizingStackPanel || src is ContentPresenter)
                {
                    this.InputProcessOnCompletionList(Key.Enter);
                }
                return;
            }

            base.OnMouseDoubleClick(e);
        }

        /// <summary>
        /// This function adds to the editor a delimiter that is used to separate.
        /// </summary>
        /// <param name="delimiter"></param>
        public void AddDelimiter(string delimiter)
        {
            this.DelimiterSet.Add(delimiter);
        }

        public void AddCompletionList(CompletionItemType type, string item)
        {
            var context = this.completionList.DataContext as CompletionListViewModel;

            context.AddCollection(type, item);
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
    }


    public class ViewStringInfo
    {
        public string Data { get; }
        public int AbsoluteLineIndex { get; }

        public ViewStringInfo(string data, int absoluteLineIndex)
        {
            this.Data = data;
            this.AbsoluteLineIndex = absoluteLineIndex;
        }
    }
}
