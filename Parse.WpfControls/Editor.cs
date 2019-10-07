using Parse.WpfControls.SyntaxEditorComponents;
using Parse.WpfControls.SyntaxEditorComponents.EventArgs;
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
    [TemplatePart(Name = "TextArea", Type = typeof(TextArea))]
    public class Editor : Control
    {
        private double recentVerticalOffset = 0;
        private double recentHorizontalOffset = 0;

        private TextViewer lineNumbersCanvas;

        #region Dependency Properties
        public TextArea TextArea
        {
            get { return (TextArea)GetValue(TextAreaProperty); }
            private set { SetValue(TextAreaProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextArea.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextAreaProperty =
            DependencyProperty.Register("TextArea", typeof(TextArea), typeof(Editor), new PropertyMetadata(null));

        public Brush LineNumberBackColor
        {
            get { return (Brush)GetValue(LineNumberBackColorProperty); }
            set { SetValue(LineNumberBackColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineNumberBackColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineNumberBackColorProperty =
            DependencyProperty.Register("LineNumberBackColor", typeof(Brush), typeof(Editor), new PropertyMetadata(Brushes.Transparent));


        public Brush LineNumberForeColor
        {
            get { return (Brush)GetValue(LineNumberForeColorProperty); }
            set { SetValue(LineNumberForeColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineNumberForeColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineNumberForeColorProperty =
            DependencyProperty.Register("LineNumberForeColor", typeof(Brush), typeof(Editor), new PropertyMetadata(Brushes.Black));


        public bool IsLineNumberingVisible
        {
            get { return (bool)GetValue(IsLineNumberingVisibleProperty); }
            set { SetValue(IsLineNumberingVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsLineNumberingVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLineNumberingVisibleProperty =
            DependencyProperty.Register("IsLineNumberingVisible", typeof(bool), typeof(Editor), new PropertyMetadata(false));


        public double LineHeight
        {
            get { return (double)GetValue(LineHeightProperty); }
            set { SetValue(LineHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineHeightProperty =
            DependencyProperty.Register("LineHeight", typeof(double), typeof(Editor), new PropertyMetadata(null));


        public Brush SelectedLineBrush
        {
            get { return (Brush)GetValue(SelectedLineBrushProperty); }
            set { SetValue(SelectedLineBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedLineBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedLineBrushProperty =
            DependencyProperty.Register("SelectedLineBrush", typeof(Brush), typeof(Editor), new PropertyMetadata(Brushes.Transparent));


        public Brush SelectedLineBorderBrush
        {
            get { return (Brush)GetValue(SelectedLineBorderBrushProperty); }
            set { SetValue(SelectedLineBorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedLineBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedLineBorderBrushProperty =
            DependencyProperty.Register("SelectedLineBorderBrush", typeof(Brush), typeof(Editor), new PropertyMetadata(Brushes.Transparent));


        public int SelectedLineBorder
        {
            get { return (int)GetValue(SelectedLineBorderProperty); }
            set { SetValue(SelectedLineBorderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedLineBorder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedLineBorderProperty =
            DependencyProperty.Register("SelectedLineBorder", typeof(int), typeof(Editor), new PropertyMetadata(1));

        #endregion

        #region Routed Events
        public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent("TextChanged", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(Editor));

        // .NET wrapper
        public event RoutedEventHandler TextChanged
        {
            add { AddHandler(TextChangedEvent, value); }
            remove { RemoveHandler(TextChangedEvent, value); }
        }

        #endregion

        static Editor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Editor), new FrameworkPropertyMetadata(typeof(Editor)));

            //            TextProperty.OverrideMetadata(typeof(Editor), new FrameworkPropertyMetadata(new PropertyChangedCallback(TextPropertyChanged)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.lineNumbersCanvas = (TextViewer)Template.FindName("PART_LineNumbersCanvas", this);
            this.TextArea = (TextArea)Template.FindName("PART_TextArea", this);
        }

        public Editor()
        {
            Loaded += (s, e) =>
            {
                //                lineNumbersCanvas.Width = GetFormattedTextWidth(string.Format("{0:0000}", totalLineCount)) + 5;
                //                scrollViewer.ScrollChanged += OnScrollChanged;
                this.TextArea.Rendered += TextArea_Rendered;
                this.TextArea.TextChanged += TextArea_TextChanged;

                InvalidateVisual();
            };
        }

        private void TextArea_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void ShowPrompt(TextChange changeInfo)
        {
            if (changeInfo.AddedLength != 1) return;

            IntelliPrompt prompt = new IntelliPrompt();
        }

        private void TextArea_Rendered(object sender, RoutedEventArgs e)
        {
            EditorRenderedEventArgs arg = e as EditorRenderedEventArgs;

            this.recentHorizontalOffset = arg.HorizontalOffset;
            this.recentVerticalOffset = arg.VerticalOffset;

            this.InvalidateVisual();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <see cref="https://www.lucidchart.com/documents/edit/cdd4b7ff-4807-4df0-8360-bc75d3f9dd2b/0_0"/>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (this.IsLoaded == false || this.lineNumbersCanvas == null || this.TextArea == null) return;
            if (this.TextArea.ViewLineString.Count == 0) return;

            this.lineNumbersCanvas.SetDrawStartingPos(0, this.recentVerticalOffset, this.LineHeight);

            List<FormattedText> lines = new List<FormattedText>();

            int maxNumberOfDigit = this.TextArea.ViewLineString[this.TextArea.ViewLineString.Count - 1].AbsoluteLineIndex.ToString().Length;
            this.lineNumbersCanvas.Width = maxNumberOfDigit * this.FontSize;
            for (int i=0; i<this.TextArea.ViewLineString.Count; i++)
            {
                FormattedText lineNumberingText = new FormattedText((this.TextArea.ViewLineString[i].AbsoluteLineIndex + 1).ToString(), 
                    CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                    this.FontSize, this.LineNumberForeColor);

                lines.Add(lineNumberingText);
            }

            this.lineNumbersCanvas.DrawLines(lines);

            base.OnRender(drawingContext);
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0 || e.HorizontalChange != 0) InvalidateVisual();
        }

        static void TextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
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
}
