using Parse.WpfControls.Common;
using Parse.WpfControls.SyntaxEditorComponents.EventArgs;
using Parse.WpfControls.SyntaxEditorComponents.Models;
using Parse.WpfControls.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Parse.WpfControls.SyntaxEditorComponents
{
    public class TextArea : ExtensionTextBox
    {
        private TextViewer renderCanvas;
        private ScrollViewer scrollViewer;

        private Dictionary<string, TextStyle> textStyleDic = new Dictionary<string, TextStyle>();
        private Dictionary<string, TextStyle> patternStyleDic = new Dictionary<string, TextStyle>();

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
            DependencyProperty.Register("TabSize", typeof(int), typeof(TextArea), new PropertyMetadata(4));
        #endregion

        #region Dependency Properties related with CompletionListBehavior
        public ObservableCollection<CompletionItem> CompletionItems
        {
            get { return (ObservableCollection<CompletionItem>)GetValue(CompletionItemsProperty); }
            set { SetValue(CompletionItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CompletionItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CompletionItemsProperty =
            DependencyProperty.Register("CompletionItems", typeof(ObservableCollection<CompletionItem>), typeof(TextArea), new PropertyMetadata(null));


        public StringCollection DelimiterSet
        {
            get { return (StringCollection)GetValue(DelimiterSetProperty); }
            set { SetValue(DelimiterSetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DelimiterSet.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DelimiterSetProperty =
            DependencyProperty.Register("DelimiterSet", typeof(StringCollection), typeof(TextArea), new PropertyMetadata(null));
        #endregion

        #region Dependency Properties releated with Visual (Render)
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
            SetValue(CompletionItemsProperty, new ObservableCollection<CompletionItem>());
            SetValue(DelimiterSetProperty, new StringCollection());

            this.LineString.Add(string.Empty);

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
            var ViewLineString = this.GetLineStringCollection(startLine, this.maxViewLineOnce);

            List<LineFormattedText> drawnItems = new List<LineFormattedText>();
            foreach (var viewLineData in ViewLineString)
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

            int startNumber = ViewLineString.First().AbsoluteLineIndex + 1;
            int endNumber = ViewLineString.Last().AbsoluteLineIndex + 1;
            this.RaiseEvent(new EditorRenderedEventArgs(TextArea.RenderedEvent, startLine, endNumber, 
                                                                                this.HorizontalOffset, this.VerticalOffset, this.LineHeight));
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
    }
}
