using System;
using System.Windows;
using System.Windows.Media;

namespace Parse.WpfControls
{
    public class DrawingControl : FrameworkElement
    {
        private VisualCollection visuals;

        public DrawingControl()
        {
            visuals = new VisualCollection(this);
        }

        protected override int VisualChildrenCount
        {
            get { return visuals.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= visuals.Count)
                throw new ArgumentOutOfRangeException();
            return visuals[index];
        }

        public void Add(DrawingVisual visual) => this.visuals.Add(visual);

        public void Clear() => this.visuals.Clear();
    }
}
