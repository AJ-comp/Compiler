using Parse.FrontEnd.RegularGrammar;
using Parse.FrontEnd.Support.Drawing;
using Parse.FrontEnd.Tokenize;
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


        public Brush DefaultTextBrush
        {
            get { return (Brush)GetValue(DefaultTextBrushProperty); }
            set { SetValue(DefaultTextBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultTextBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultTextBrushProperty =
            DependencyProperty.Register("DefaultTextBrush", typeof(Brush), typeof(HighlightTextBox), new PropertyMetadata(Brushes.Black));


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
                this.InvalidateVisual();
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

                this.InvalidateVisual();
            };
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.scrollViewer = (ScrollViewer)Template.FindName("PART_ContentHost", this);
            this.scrollViewer.ScrollChanged += OnScrollChanged;

            this.renderCanvas = (TextCanvas)Template.FindName("PART_RenderCanvas", this);
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
            var point = this.GetIndexInfoFromCaretIndex(CaretIndex);
            int addedLineIndex = point.Y - this.GetStartLineOnViewPos(this.VerticalOffset);
            this.renderCanvas?.DrawSelectedLineAppearance(addedLineIndex, appearance);
        }


        /// <summary>
        /// This function gets the line-string-collection.
        /// </summary>
        /// <param name="startLine">The start line that gets line-string-collection</param>
        /// <param name="cnt">Line count</param>
        /// <returns></returns>
        private IEnumerable<LineHighlightText> GetLineDrawingTokenList(int startLine, int cnt)
        {
            List<LineHighlightText> result = new List<LineHighlightText>();
            //            var tokenStartIndex = this.GetTokenIndexForCaretIndex(this.GetStartingCaretIndexOfLineIndex(startLine), RecognitionWay.Front);

            //            if (tokenStartIndex < 0) return result;
            //            if (startLine >= this.LineIndexes.Count) return result;
            int maxCnt = (startLine + cnt < RecentLexedData.GetLineCount()) ? startLine + cnt : RecentLexedData.GetLineCount();

            for (int i = startLine; i < maxCnt; i++)
            {
                LineHighlightText lineString = new LineHighlightText();
                var tokens = RecentLexedData.GetTokensForLine(i);
                lineString.AbsoluteLineIndex = i;

                foreach (var token in tokens)
                    lineString.Add(this.ConvertToHighlightToken(token));


                result.Add(lineString);
            }

            return result;
        }

        /// <summary>
        /// This function returns a HighlightToken from the TokenInfo.
        /// </summary>
        /// <param name="tokenInfo"></param>
        /// <returns></returns>
        private HighlightToken ConvertToHighlightToken(TokenCell tokenInfo)
        {
            return this.ConvertToHighlightToken(tokenInfo.Data, tokenInfo.PatternInfo.OriginalPattern, (DrawingOption)tokenInfo.ValueOptionData);
        }

        private HighlightToken ConvertToHighlightToken(string text, string pattern, DrawingOption status)
        {
            Brush foreBrush = this.DefaultTextBrush;
            if (this.textStyleDic.ContainsKey(pattern))
                foreBrush = this.textStyleDic[pattern].ForeGround;

            HighlightToken ft = new HighlightToken(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch), this.FontSize, foreBrush, VisualTreeHelper.GetDpi(this).PixelsPerDip)
            {
                Trimming = TextTrimming.None,
                LineHeight = this.LineHeight
            };

            if ((status & DrawingOption.Selected) == DrawingOption.Selected)
                ft.AppearanceInfo.Selected = true;
            else ft.AppearanceInfo.Selected = false;

            if ((status & DrawingOption.Underline) == DrawingOption.Underline)
                ft.AppearanceInfo.UnderLine = true;
            else ft.AppearanceInfo.UnderLine = false;

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

            var ViewLineString = this.GetLineDrawingTokenList(this.LineIndex, 1);
            if (ViewLineString.Count() == 0) return;

            this.renderCanvas.DrawLine(addedLineIndex, ViewLineString.First());
        }

        private void AllRender(DrawingContext drawingContext)
        {
            int startLine = this.GetStartLineOnViewPos(this.VerticalOffset);
            var ViewLineString = this.GetLineDrawingTokenList(startLine, this.maxViewLineOnce);
            int startNumber = 1;
            int endNumber = 1;

            if (ViewLineString.Count() == 0) this.renderCanvas.Clear();
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
            if (!IsLoaded || renderCanvas == null) return;

            /*
            if (this.bSingleCharacterAdded)
            {
                this.SingleLineRender(drawingContext);
                this.bSingleCharacterAdded = false;
                return;
            }
            else
            */
            this.AllRender(drawingContext);

            this.DrawSelectionLineAppearance();
        }

        public void AddCompletionList(CompletionItemType type, string item)
        {
            this.CompletionItems.Add(new CompletionItem() { ItemType = type, ItemName = item });
        }

        /// <summary>
        /// This function adds information which for syntax-highlighting to the editor.
        /// </summary>
        /// <param name="text">This argument means text to highlight. This argument can be a pattern.</param>
        /// <param name="foreBrush">This argument means the foreground color of the text.</param>
        /// <param name="terminal"></param>
        public void AddSyntaxHighLightInfo(Brush foreBrush, Brush backBrush, Terminal terminal)
        {
            this.textStyleDic.Add(terminal.Value, new TextStyle(foreBrush, Brushes.Transparent));
            this.AddTokenPattern(terminal);
        }

        public override void TokenizeRuleClear()
        {
            base.TokenizeRuleClear();

            this.textStyleDic.Clear();
            this.CompletionItems.Clear();
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
