using Parse.WpfControls.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Parse.WpfControls
{
    //    [System.Windows.Markup.ContentProperty("Inlines")]
    public class TextCanvas : FrameworkElement
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
            DependencyProperty.Register("LineHeight", typeof(double), typeof(TextCanvas), new PropertyMetadata(null));


        public TextCanvas()
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

        private void SetDecoration(HighlightToken token)
        {
            if(token.AppearanceInfo.UnderLine == true)
            {
                TextDecorationCollection textDecorations = new TextDecorationCollection();

                TextDecoration underline = new TextDecoration
                {
                    Location = TextDecorationLocation.Underline,
                    Pen = new Pen(Brushes.Red, 1),
                    PenThicknessUnit = TextDecorationUnit.FontRecommended
                };

                textDecorations.Add(underline);

                token.SetTextDecorations(textDecorations);
            }

            if(token.AppearanceInfo.Selected == true)
            {

            }
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

        public void DrawAll(List<LineHighlightText> contents, double horizontalOffset, double verticalOffset)
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
                    this.SetDecoration(token);
                    dc.DrawText(token, new Point(this.algorithm.DrawingPointX, this.algorithm.DrawingPointY));

                    this.algorithm.CalculateNextXPoint(token.WidthIncludingTrailingWhitespace);
                }

                dc.Close();
            }
        }

        public void DrawLine(int index, LineHighlightText line)
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
                this.SetDecoration(token);
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

        public void Clear()
        {
            this.lines.Clear();
        }

        public void AddLine() => this.lines.Add(new DrawingVisual());
        public DrawingVisual GetLastLine() => this.lines[this.lines.Count - 1] as DrawingVisual;
    }
}
