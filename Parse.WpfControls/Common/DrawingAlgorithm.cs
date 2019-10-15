namespace Parse.WpfControls
{
    public class DrawingAlgorithm
    {
        private double horizontalOffset = 0;
        private double verticalOffset = 0;
        private double lineHeight = 0;
        private double yPos = 0;

        public double DrawingPointX { get; private set; } = 0;
        public double DrawingPointY { get; private set; } = 0;

        public void Initial(double lineHeight, double horizontalOffset, double verticalOffset)
        {
            this.horizontalOffset = horizontalOffset;
            this.verticalOffset = verticalOffset;
            this.lineHeight = lineHeight;

            this.VerticalPosInitial();
            this.HorizontalPosInitial();
        }

        public void VerticalPosInitial()
        {
            this.yPos = this.lineHeight * (int)(this.verticalOffset / this.lineHeight);
        }

        public void HorizontalPosInitial()
        {
            this.DrawingPointX = 2 - this.horizontalOffset;
        }

        /// <summary>
        /// This function calculates the next line text position to be drawn.
        /// </summary>
        /// <see cref="https://www.lucidchart.com/documents/edit/cdd4b7ff-4807-4df0-8360-bc75d3f9dd2b/0_0"/>
        public void CalculateNextLine()
        {
            this.HorizontalPosInitial();

            this.DrawingPointY = this.yPos - this.verticalOffset;
            this.yPos += this.lineHeight;
        }

        /// <summary>
        /// This function calculates the next horizontal text position to be drawn.
        /// </summary>
        /// <param name="drawnTextWidth"></param>
        public void CalculateNextXPoint(double drawnTextWidth)
        {
            this.DrawingPointX += drawnTextWidth;
        }
    }
}
