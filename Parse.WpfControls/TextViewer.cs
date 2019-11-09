using Parse.WpfControls.Common;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Parse.WpfControls
{
    //    [System.Windows.Markup.ContentProperty("Inlines")]
    public class TextViewer : FrameworkElement
    {
        private double currentVerticalOffset = 0;
        private double currentHorizontalOffset = 0;

        private DrawingAlgorithm algorithm = new DrawingAlgorithm();
        private DrawingVisual selectionLineAppearance = new DrawingVisual();
        private VisualCollection lines;

        //        public InlineCollection Inlines { get; }


        public double LineHeight
        {
            get { return (double)GetValue(LineHeightProperty); }
            set { SetValue(LineHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineHeightProperty =
            DependencyProperty.Register("LineHeight", typeof(double), typeof(TextViewer), new PropertyMetadata(null));


        public TextViewer()
        {
            this.lines = new VisualCollection(this);
            this.AddVisualChild(this.selectionLineAppearance);
        }

        public override void OnApplyTemplate()
        {

        }

        protected override int VisualChildrenCount
        {
            // selectionLineAppearance + lines
            get { return 1+lines.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= VisualChildrenCount)
                throw new ArgumentOutOfRangeException();

            if (index == 0) return this.selectionLineAppearance;
            return lines[index-1];
        }

        public void SetDrawStartingPos(double horizontalOffset, double verticalOffset)
        {
            this.currentHorizontalOffset = horizontalOffset;
            this.currentVerticalOffset = verticalOffset;

            this.algorithm.Initial(this.LineHeight, horizontalOffset, verticalOffset);
        }

        public void DrawLines(List<FormattedText> lines)
        {
            this.lines.Clear();

            foreach (var line in lines)
            {
                this.AddLine();
                var dc = this.GetLastLine().RenderOpen();

                this.algorithm.CalculateNextLine();

                double top = 0 - this.currentVerticalOffset;
                if (top >= ActualHeight) return;

                dc.DrawText(line, new Point(this.algorithm.DrawingPointX, this.algorithm.DrawingPointY));

                dc.Close();
            }
        }

        public void DrawAll(List<LineFormattedText> contents, double horizontalOffset, double verticalOffset)
        {
            this.algorithm.Initial(this.LineHeight, horizontalOffset, verticalOffset);

            this.lines.Clear();

            foreach (var line in contents)
            {
                this.AddLine();
                var dc = this.GetLastLine().RenderOpen();

                this.algorithm.CalculateNextLine();

                double top = 0 - verticalOffset;
                if (top >= ActualHeight) break;

                foreach (var token in line)
                {
                    dc.DrawText(token, new Point(this.algorithm.DrawingPointX, this.algorithm.DrawingPointY));

                    this.algorithm.CalculateNextXPoint(token.WidthIncludingTrailingWhitespace);
                }

                dc.Close();
            }
        }

        public void DrawLine(int index, LineFormattedText line)
        {
            for (int i = this.lines.Count - 1; i < index; i++)
            {
                this.AddLine();
                var dc = this.GetLastLine().RenderOpen();

                double x = this.algorithm.DrawingStartingPoint.X;
                double y = this.algorithm.GetDrawingYPosition(i);
                dc.DrawRectangle(Brushes.Transparent, null, new Rect(x, y, 10, this.LineHeight));

                dc.Close();
            }

            if (this.lines.Count <= index) this.AddLine();

            var visual = this.lines[index] as DrawingVisual;
            var dc2 = visual.RenderOpen();
            var yPos = this.algorithm.GetDrawingYPosition(index);

            this.algorithm.HorizontalPosInitial();
            foreach (var token in line)
            {
                dc2.DrawText(token, new Point(this.algorithm.DrawingPointX, yPos));

                this.algorithm.CalculateNextXPoint(token.WidthIncludingTrailingWhitespace);
            }


            dc2.Close();
        }

        /// <summary>
        /// This function draws the appearance of the selected line.
        /// </summary>
        /// <param name="index"></param>
        public void DrawSelectedLineAppearance(int index, AppearanceInfo appearanceInfo)
        {
            if (index < 0) return;
            if (this.lines.Count <= index) return;

            var dc = this.selectionLineAppearance.RenderOpen();
            Pen pen = new Pen(appearanceInfo.BorderBrush, appearanceInfo.BorderThickness);
            Rect rect = new Rect(2, this.algorithm.GetDrawingYPosition(index), this.ActualWidth-2, this.LineHeight);

            dc.DrawRectangle(appearanceInfo.BackGroundBrush, pen, rect);

            dc.Close();
        }

        public void AddLine() => this.lines.Add(new DrawingVisual());
        public DrawingVisual GetLastLine() => this.lines[this.lines.Count - 1] as DrawingVisual;
    }

    public class LineFormattedText : List<FormattedText>
    {
    }

    public class AppearanceInfo
    {
        public Brush BorderBrush { get; set; } = Brushes.Transparent;
        public Brush BackGroundBrush { get; set; } = Brushes.Transparent;

        public int BorderThickness { get; set; } = 1;
    }
}
