using Parse.WpfControls.Common;
using Parse.WpfControls.EventArgs;
using Parse.WpfControls.Models;
using Parse.WpfControls.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Parse.WpfControls
{
    public class HighlightTextBox : TokenizeTextBox
    {
        private TextViewer renderCanvas;
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

            this.renderCanvas = (TextViewer)Template.FindName("PART_RenderCanvas", this);
            this.scrollViewer = (ScrollViewer)Template.FindName("PART_ContentHost", this);
        }

        private int GetStartLineOnViewPos(double topViewPos) => (int)(topViewPos / this.LineHeight);


        /// <summary>
        /// This function gets the line-string-collection
        /// </summary>
        /// <param name="startLine">The start line that gets line-string-collection</param>
        /// <param name="cnt">Line count</param>
        /// <returns></returns>
        private List<ViewStringInfo> GetLineStringCollection(int startLine, int cnt)
        {
            List<ViewStringInfo> result = new List<ViewStringInfo>();

            if (startLine >= this.LineIndexes.Count) return result;

            int maxCnt = (startLine + cnt < this.LineIndexes.Count) ? startLine + cnt : this.LineIndexes.Count;

            for (int i = startLine; i < maxCnt; i++)
            {
                if (i == this.LineIndexes.Count - 1)
                    result.Add(new ViewStringInfo(this.Text.Substring(this.LineIndexes[i], this.Text.Length - this.LineIndexes[i]), i));
                else
                    result.Add(new ViewStringInfo(this.Text.Substring(this.LineIndexes[i], this.LineIndexes[i+1] - this.LineIndexes[i] - Environment.NewLine.Length), i));

                
            }

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

        private LineFormattedText GetLineFormattedText(string line)
        {
            LineFormattedText result = new LineFormattedText();

            foreach (var token in StringUtility.SplitAndKeep(line, this.DelimiterSet.Cast<string>().ToArray()).ToList())
                result.Add(this.GetFormattedText(token));

            return result;
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
            var startLineIndex = this.GetStartLineOnViewPos(this.VerticalOffset);
            if (this.LineIndex >= startLineIndex && this.LineIndex <= startLineIndex + this.maxViewLineOnce)
            {
                var ViewLineString = this.GetLineStringCollection(this.LineIndex, 1);
                if (ViewLineString.Count == 0) return;

                var item = this.GetLineFormattedText(ViewLineString[0].Data);

                int addIndex = this.LineIndex - startLineIndex;
                this.renderCanvas.DrawLine(addIndex, item);
            }
        }

        private void AllRender(DrawingContext drawingContext)
        {
            int startLine = this.GetStartLineOnViewPos(this.VerticalOffset);
            var ViewLineString = this.GetLineStringCollection(startLine, this.maxViewLineOnce);
            if (ViewLineString.Count == 0) return;

            List<LineFormattedText> drawnItems = new List<LineFormattedText>();
            foreach (var viewLineData in ViewLineString)
            {
                var item = this.GetLineFormattedText(viewLineData.Data);

                drawnItems.Add(item);
            }

            /*
            if (ViewLineString.First().AbsoluteLineIndex <= this.LineIndex && ViewLineString.Last().AbsoluteLineIndex >= this.LineIndex)
            {
                drawnItems[this.LineIndex].BackGroundBrush = this.SelectionLineBrush;
                drawnItems[this.LineIndex].BorderBrush = this.SelectionLineBorderBrush;
                drawnItems[this.LineIndex].BorderThickness = this.SelectionLineBorderThickness;
            }
            */

            this.renderCanvas.DrawAll(drawnItems, this.HorizontalOffset, this.VerticalOffset, this.LineHeight);

            base.OnRender(drawingContext);

            int startNumber = ViewLineString.First().AbsoluteLineIndex + 1;
            int endNumber = ViewLineString.Last().AbsoluteLineIndex + 1;
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
        }

        public void AddCompletionList(CompletionItemType type, string item)
        {
            this.CompletionItems.Add(new CompletionItem() { ItemType = type, ItemName = item });
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

        public override string ToString() => string.Format("{0}, {1}", this.AbsoluteLineIndex, this.Data);
    }
}
