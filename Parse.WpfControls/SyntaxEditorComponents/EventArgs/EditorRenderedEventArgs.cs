using System.Windows;

namespace Parse.WpfControls.SyntaxEditorComponents.EventArgs
{
    public class EditorRenderedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// This property means a index where it started drawing.
        /// </summary>
        public int ViewStartLineIndex { get; } = 0;
        public int ViewEndLineIndex { get; } = 0;

        public double HorizontalOffset { get; } = 0;
        public double VerticalOffset { get; } = 0;
        public double LineHeight { get; } = 0;

        public EditorRenderedEventArgs(RoutedEvent routedEvent) : base(routedEvent)
        {

        }

        public EditorRenderedEventArgs(RoutedEvent routedEvent, int viewStartLineIndex, int viewEndLineIndex, 
                                                        double horizontalOffset, double verticalOffset, double lineHeight) : base(routedEvent)
        {
            this.ViewStartLineIndex = viewStartLineIndex;
            this.ViewEndLineIndex = viewEndLineIndex;
            this.HorizontalOffset = horizontalOffset;
            this.VerticalOffset = verticalOffset;
            this.LineHeight = lineHeight;
        }
    }
}
