using System.Drawing;

namespace Parse.WpfControls.SyntaxEditor
{
    public class HighlightingMapItem
    {
        public string Text { get; }
        public object OptionData { get; }
        public bool CanDerived { get; }
        public bool Operator { get; }

        public Brush ForegroundBrush { get; }
        public Brush BackgroundBrush { get; }

        public HighlightingMapItem(string text, object optionData, bool canDerived, bool @operator, Brush foregroundBrush, Brush backgroundBrush)
        {
            Text = text;
            OptionData = optionData;
            CanDerived = canDerived;
            Operator = @operator;
            ForegroundBrush = foregroundBrush;
            BackgroundBrush = backgroundBrush;
        }
    }
}
