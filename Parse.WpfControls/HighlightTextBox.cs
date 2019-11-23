using Parse.WpfControls.Common;
using Parse.WpfControls.EventArgs;
using Parse.WpfControls.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Parse.WpfControls
{
    public class HighlightTextBox : TokenizeTextBox
    {
        private TextCanvas renderCanvas;
        private ScrollViewer scrollViewer;

        private Dictionary<string, TextStyle> textStyleDic = new Dictionary<string, TextStyle>();
        private Dictionary<string, TextStyle> patternStyleDic = new Dictionary<string, TextStyle>();

        /// <summary>For cache</summary>
        private bool bSingleCharacterAdded = false;

        /// <summary>This member means maximum showable line count at one go.</summary>
        private int maxViewLineOnce = 100;

        #region Dependency Property releated with TabSize Property
        public int TabSize
        {
            get { return (int)GetValue(TabSizeProperty); }
            set { SetValue(TabSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TabSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TabSizeProperty =
            DependencyProperty.Register("TabSize", typeof(int), typeof(HighlightTextBox), new PropertyMetadata(4));
        #endregion

        #region Dependency Properties related with CompletionListBehavior
        public ObservableCollection<CompletionItem> CompletionItems
        {
            get { return (ObservableCollection<CompletionItem>)GetValue(CompletionItemsProperty); }
            set { SetValue(CompletionItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CompletionItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CompletionItemsProperty =
            DependencyProperty.Register("CompletionItems", typeof(ObservableCollection<CompletionItem>), typeof(HighlightTextBox), new PropertyMetadata(null));


        public StringCollection DelimiterSet
        {
            get { return (StringCollection)GetValue(DelimiterSetProperty); }
            set { SetValue(DelimiterSetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DelimiterSet.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DelimiterSetProperty =
            DependencyProperty.Register("DelimiterSet", typeof(StringCollection), typeof(HighlightTextBox), new PropertyMetadata(null));
        #endregion

        #region Dependency Properties releated with Visual (Render)
        public double LineHeight
        {
            get { return (double)GetValue(LineHeightProperty); }
            internal set { SetValue(LineHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineHeightProperty =
            DependencyProperty.Register("LineHeight", typeof(double), typeof(HighlightTextBox), new PropertyMetadata(new PropertyChangedCallback(LineHeightChanged)));


        public static void LineHeightChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            HighlightTextBox area = dp as HighlightTextBox;

            TextBlock.SetLineStackingStrategy(area, LineStackingStrategy.BlockLineHeight);
            TextBlock.SetLineHeight(area, (double)args.NewValue);
        }

        public Brush SelectionLineBrush
        {
            get { return (Brush)GetValue(SelectionLineBrushProperty); }
            set { SetValue(SelectionLineBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectionLineBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionLineBrushProperty =
            DependencyProperty.Register("SelectionLineBrush", typeof(Brush), typeof(HighlightTextBox), new PropertyMetadata(Brushes.Transparent));


        public Brush SelectionLineBorderBrush
        {
            get { return (Brush)GetValue(SelectionLineBorderBrushProperty); }
            set { SetValue(SelectionLineBorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectionBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionLineBorderBrushProperty =
            DependencyProperty.Register("SelectionLineBorderBrush", typeof(Brush), typeof(HighlightTextBox), new PropertyMetadata(Brushes.Transparent));


        public int SelectionLineBorderThickness
        {
            get { return (int)GetValue(SelectionLineBorderThicknessProperty); }
            set { SetValue(SelectionLineBorderThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectionBorderThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionLineBorderThicknessProperty =
            DependencyProperty.Register("SelectionLineBorderThickness", typeof(int), typeof(HighlightTextBox), new PropertyMetadata(1));
        #endregion

        #region Routed Events
        public static readonly RoutedEvent ScrollChangedEvent = EventManager.RegisterRoutedEvent("ScrollChanged", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(HighlightTextBox));

        // .NET wrapper
        public event RoutedEventHandler ScrollChanged
        {
            add { AddHandler(ScrollChangedEvent, value); }
            remove { RemoveHandler(ScrollChangedEvent, value); }
        }

        public static readonly RoutedEvent RenderedEvent = EventManager.RegisterRoutedEvent("Rendered", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(HighlightTextBox));

        // .NET wrapper
        public event RoutedEventHandler Rendered
        {
            add { AddHandler(RenderedEvent, value); }
            remove { RemoveHandler(RenderedEvent, value); }
        }
        #endregion


        static HighlightTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HighlightTextBox), new FrameworkPropertyMetadata(typeof(HighlightTextBox)));
        }

        public HighlightTextBox()
        {
            SetValue(CompletionItemsProperty, new ObservableCollection<CompletionItem>());
            SetValue(DelimiterSetProperty, new StringCollection());

//            this.AddHandler(ListBox.MouseLeftButtonDownEvent, new RoutedEventHandler(this.OnMouseLeftClick), true);

            Loaded += (s, e) =>
            {
                this.scrollViewer.ScrollChanged += OnScrollChanged;

                InvalidateVisual();
            };

            SizeChanged += (s, e) =>
            {
                if (e.HeightChanged == false) return;

                InvalidateVisual();
            };

            SelectionChanged += (s, e) =>
            {
                this.DrawSelectionLineAppearance();
            };

            TextChanged += (s, e) =>
            {
                TextChange changeInfo = e.Changes.First();
                string addString = this.Text.Substring(changeInfo.Offset, changeInfo.AddedLength);
                if (changeInfo.RemovedLength == 0 && addString.Length == 1) this.bSingleCharacterAdded = true;

                InvalidateVisual();
            };
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.renderCanvas = (TextCanvas)Template.FindName("PART_RenderCanvas", this);
            this.scrollViewer = (ScrollViewer)Template.FindName("PART_ContentHost", this);
        }

        private int GetStartLineOnViewPos(double topViewPos) => (int)(topViewPos / this.LineHeight);

        private void DrawSelectionLineAppearance()
        {
            var appearance = new AppearanceInfo()
            {
                BorderBrush = this.SelectionLineBorderBrush,
                BorderThickness = this.SelectionLineBorderThickness,
                BackGroundBrush = Brushes.Transparent
            };
            int addedLineIndex = this.LineIndex - this.GetStartLineOnViewPos(this.VerticalOffset);
            this.renderCanvas.DrawSelectedLineAppearance(addedLineIndex, appearance);
        }

        private void AddLineString(ref LineHighlightText from, List<LineHighlightText> to, int lineIndex)
        {
            from.AbsoluteLineIndex = lineIndex - 1;
            to.Add(from);
            from = new LineHighlightText();
        }


        /// <summary>
        /// This function stores to List after split line by line.
        /// </summary>
        /// <param name="lineString">string list in the line</param>
        /// <param name="result">line list</param>
        /// <param name="lineIndex">line index</param>
        /// <param name="tokenIndex">The index of the token that has "\n" string</param>
        private void SplitMultipleLineFromToken(ref LineHighlightText lineString, List<LineHighlightText> result, ref int lineIndex, int tokenIndex)
        {
            // Found the line break syntax.
            int startPos = 0;
            string searchString = this.Tokens[tokenIndex].Data;
            while (true)
            {
                if (searchString.Length == 0)
                {
                    // If searchString is empty then the last string is "n" so must add one line.
                    lineString.Add(this.GetHighlightToken("", this.Tokens[tokenIndex].PatternInfo.OriginalPattern));
                    break;
                }

                int endPos = searchString.IndexOf("\n");

                // The "\n" was not in the searchString.
                if (endPos < startPos)
                {
                    lineString.Add(this.GetHighlightToken(searchString, this.Tokens[tokenIndex].PatternInfo.OriginalPattern));
                    break;
                }

                // Found the "\n"
                lineString.Add(this.GetHighlightToken(searchString.Substring(0, endPos + 1), this.Tokens[tokenIndex].PatternInfo.OriginalPattern));
                this.AddLineString(ref lineString, result, lineIndex);
                lineIndex++;
                searchString = searchString.Substring(endPos + 1);
            }
        }

        /// <summary>
        /// This function gets the line-string-collection.
        /// </summary>
        /// <param name="startLine">The start line that gets line-string-collection</param>
        /// <param name="cnt">Line count</param>
        /// <returns></returns>
        private List<LineHighlightText> GetLineStringCollection(int startLine, int cnt)
        {
            List<LineHighlightText> result = new List<LineHighlightText>();
            var tokenStartIndex = this.GetTokenIndexFromCaretIndex(this.GetStartingCaretIndexOfLineIndex(startLine), RecognitionWay.Front);

            if (tokenStartIndex < 0) return result;
            if (startLine >= this.LineIndexes.Count) return result;

            int maxCnt = (startLine + cnt < this.LineIndexes.Count) ? startLine + cnt : this.LineIndexes.Count;

            int lineIndex = startLine + 1;
            LineHighlightText lineString = new LineHighlightText();
            for (int i = tokenStartIndex; i < this.Tokens.Count; i++)
            {
                int lbPos = (lineIndex == this.LineIndexes.Count) ? -1 : this.LineIndexes[lineIndex] - 1;

                // Not found the line break syntax.
                if (this.Tokens[i].StartIndex > lbPos || lbPos > this.Tokens[i].EndIndex)
                {
                    lineString.Add(this.GetHighlightToken(this.Tokens[i]));
                    continue;
                }

                this.SplitMultipleLineFromToken(ref lineString, result, ref lineIndex, i);

                if (lineIndex > maxCnt) break;
            }

            if (lineString.Count > 0) this.AddLineString(ref lineString, result, lineIndex);

            return result;
        }

        /// <summary>
        /// This function returns a HighlightToken from the TokenInfo.
        /// </summary>
        /// <param name="tokenInfo"></param>
        /// <returns></returns>
        private HighlightToken GetHighlightToken(TokenCell tokenInfo)
        {
            return this.GetHighlightToken(tokenInfo.Data, tokenInfo.PatternInfo.OriginalPattern);
        }

        private HighlightToken GetHighlightToken(string text, string pattern)
        {
            Brush foreBrush = Brushes.Black;
            if (this.textStyleDic.ContainsKey(pattern))
                foreBrush = this.textStyleDic[pattern].ForeGround;

            HighlightToken ft = new HighlightToken(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch), this.FontSize, foreBrush, VisualTreeHelper.GetDpi(this).PixelsPerDip);

            ft.Trimming = TextTrimming.None;
            ft.LineHeight = this.LineHeight;

            return ft;
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0 || e.HorizontalChange != 0)
            {
                InvalidateVisual();

                e.RoutedEvent = HighlightTextBox.ScrollChangedEvent;
                this.RaiseEvent(e);
            }
        }

        private void SingleLineRender(DrawingContext drawingContext)
        {
            this.bSingleCharacterAdded = false;
            int addedLineIndex = this.LineIndex - this.GetStartLineOnViewPos(this.VerticalOffset);
            if (addedLineIndex < 0) return;

            var ViewLineString = this.GetLineStringCollection(this.LineIndex, 1);
            if (ViewLineString.Count == 0) return;

            this.renderCanvas.DrawLine(addedLineIndex, ViewLineString.First());
        }

        private void AllRender(DrawingContext drawingContext)
        {
            int startLine = this.GetStartLineOnViewPos(this.VerticalOffset);
            var ViewLineString = this.GetLineStringCollection(startLine, this.maxViewLineOnce);
            int startNumber = 1;
            int endNumber = 1;

            if (ViewLineString.Count == 0) this.renderCanvas.Clear();
            else
            {
                startNumber += ViewLineString.First().AbsoluteLineIndex;
                endNumber += ViewLineString.Last().AbsoluteLineIndex;

                this.renderCanvas.DrawAll(ViewLineString, this.HorizontalOffset, this.VerticalOffset);
            }

            base.OnRender(drawingContext);

            this.RaiseEvent(new EditorRenderedEventArgs(HighlightTextBox.RenderedEvent, startLine, endNumber,
                                                                                this.HorizontalOffset, this.VerticalOffset, this.LineHeight));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            //            if (!IsLoaded || renderCanvas == null || lineNumbersCanvas == null) return;
            if (!IsLoaded || renderCanvas == null) return;

            if (this.bSingleCharacterAdded)
            {
                this.SingleLineRender(drawingContext);
                this.bSingleCharacterAdded = false;
                return;
            }
            else
                this.AllRender(drawingContext);

            this.DrawSelectionLineAppearance();
        }

        public void AddCompletionList(CompletionItemType type, string item)
        {
            this.CompletionItems.Add(new CompletionItem() { ItemType = type, ItemName = item });
        }

        /// <summary>
        /// This function adds information which to syntax-highlight to the editor.
        /// </summary>
        /// <param name="text">This argument means text to highlight. This argument can be a pattern.</param>
        /// <param name="foreBrush">This argument means the foreground color of the text.</param>
        /// <param name="bCanDerived">This argument means whether a text argument can create a derived text.</param>
        /// <param name="bOperator">This argument means whether text is operator.</param>
        public void AddSyntaxHighLightInfo(string text, object optionData, Brush foreBrush, bool bCanDerived, bool bOperator = false)
        {
            this.textStyleDic.Add(text, new TextStyle(foreBrush, Brushes.Transparent));
            this.AddTokenPattern(text, optionData, bCanDerived, bOperator);
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

        public override string ToString() => string.Format("{0}, {1}", this.AbsoluteLineIndex, this.Data);
    }
}
