using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Parse.WpfControls
{
    //    [System.Windows.Markup.ContentProperty("Inlines")]
    public class TextViewer : DrawingControl
    {
        private double currentVerticalOffset = 0;
        private double currentHorizontalOffset = 0;
        private double currentLineHeight = 0;

        private DrawingAlgorithm algorithm = new DrawingAlgorithm();

        //        public InlineCollection Inlines { get; }

        public TextViewer()
        {
        }

        public override void OnApplyTemplate()
        {

        }

        public void SetDrawStartingPos(double horizontalOffset, double verticalOffset, double lineHeight)
        {
            this.currentHorizontalOffset = horizontalOffset;
            this.currentVerticalOffset = verticalOffset;
            this.currentLineHeight = lineHeight;

            this.algorithm.Initial(lineHeight, horizontalOffset, verticalOffset);
        }

        /// <summary>
        /// This function draws the appearance of the selected line.
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="lineData"></param>
        /// <param name="lineHeight"></param>
        public void DrawSelectedLineAppearance(DrawingContext dc, LineFormattedText lineData, double lineHeight)
        {
            if (lineData.BackGroundBrush != Brushes.Transparent || lineData.BorderBrush != Brushes.Transparent)
            {
                Pen pen = new Pen(lineData.BorderBrush, lineData.BorderThickness);
                Rect rect = new Rect(this.algorithm.DrawingPointX, this.algorithm.DrawingPointY, this.ActualWidth, lineHeight);

                dc.DrawRectangle(lineData.BackGroundBrush, pen, rect);
            }
        }

        public void DrawLines(List<FormattedText> lines)
        {
            this.Clear();

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

        public void DrawAll(List<LineFormattedText> contents, double horizontalOffset, double verticalOffset, double lineHeight)
        {
            this.algorithm.Initial(lineHeight, horizontalOffset, verticalOffset);

            this.Clear();

            foreach (var line in contents)
            {
                this.AddLine();
                var dc = this.GetLastLine().RenderOpen();

                this.algorithm.CalculateNextLine();

                double top = 0 - verticalOffset;
                if (top >= ActualHeight) break;

                this.DrawSelectedLineAppearance(dc, line, lineHeight);

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
            for (int i = this.VisualChildrenCount - 1; i < index; i++)
            {
                this.AddLine();
                var dc = this.GetLastLine().RenderOpen();

                double x = this.algorithm.DrawingStartingPoint.X;
                double y = this.algorithm.GetDrawingYPosition(i);
                dc.DrawRectangle(Brushes.Transparent, null, new Rect(x, y, 10, this.currentLineHeight));

                dc.Close();
            }

            if (this.VisualChildrenCount <= index) this.AddLine();

            var visual = this.GetVisualChild(index) as DrawingVisual;
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

        public void AddLine()
        {
            DrawingVisual visual = new DrawingVisual();
            var dc = visual.RenderOpen();
            this.Add(visual);
        }

        public DrawingVisual GetLastLine() => this.GetVisualChild(this.VisualChildrenCount - 1) as DrawingVisual;
    }

    public class LineFormattedText : List<FormattedText>
    {
        public Brush BorderBrush { get; set; } = Brushes.Transparent;
        public Brush BackGroundBrush { get; set; } = Brushes.Transparent;

        public int BorderThickness { get; set; } = 1;
    }
}
