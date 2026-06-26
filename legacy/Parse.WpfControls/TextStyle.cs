using System.Windows.Media;

namespace Janglim.WpfControls
{
    public class TextStyle
    {
        public Brush ForeGround { get; set; }
        public Brush BackGround { get; set; }

        public TextStyle(Brush foreBrush, Brush backBrush)
        {
            this.ForeGround = foreBrush;
            this.BackGround = backBrush;
        }
    }
}

