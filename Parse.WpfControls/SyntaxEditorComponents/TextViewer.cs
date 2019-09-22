using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace Parse.WpfControls.SyntaxEditorComponents
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
            var dc = this.GetContext();

            foreach(var line in lines)
            {
                this.algorithm.CalculateNextLine();

                double top = 0 - this.currentVerticalOffset;
                if (top >= ActualHeight) return;

                dc.DrawText(line, new Point(this.algorithm.DrawingPointX, this.algorithm.DrawingPointY));
            }

            dc.Close();
        }

        public void DrawAll(List<LineFormattedText> contents, double horizontalOffset, double verticalOffset, double lineHeight)
        {
            this.algorithm.Initial(lineHeight, horizontalOffset, verticalOffset);

            var dc = this.GetContext();

            foreach(var line in contents)
            {
                this.algorithm.CalculateNextLine();

                double top = 0 - verticalOffset;
                if (top >= ActualHeight) break;

                this.DrawSelectedLineAppearance(dc, line, lineHeight);

                foreach (var token in line)
                {
                    dc.DrawText(token, new Point(this.algorithm.DrawingPointX, this.algorithm.DrawingPointY));

                    this.algorithm.CalculateNextXPoint(token.WidthIncludingTrailingWhitespace);
                }
            }

            dc.Close();
        }
    }

    public class LineFormattedText : List<FormattedText>
    {
        public Brush BorderBrush { get; set; } = Brushes.Transparent;
        public Brush BackGroundBrush { get; set; } = Brushes.Transparent;

        public int BorderThickness { get; set; } = 1;
    }
}
