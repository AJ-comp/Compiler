using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Parse.WpfControls.Behaviors
{
    /// <summary>
    /// This class defines behavior pattern related to scrolling including zoom feature.
    /// </summary>
    public sealed class ZoomScrollBehavior : Behavior<UIElement>
    {
        public double MinFontSize { get; set; } = 9;
        private double MaxFontSize { get; set; } = 50;
        /// <summary>This member switch to 'true' value while the user pressed Ctrl key.</summary>
        private bool zoomingReady = false;

        public Key ZoomingStateEntryKey
        {
            get { return (Key)GetValue(ZoomingStateEntryKeyProperty); }
            set { SetValue(ZoomingStateEntryKeyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ZoomingStateEntryKey.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZoomingStateEntryKeyProperty =
            DependencyProperty.Register("ZoomingStateEntryKey", typeof(Key), typeof(ZoomScrollBehavior), new PropertyMetadata(Key.LeftCtrl));



        protected override void OnDetaching()
        {
            this.AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
            this.AssociatedObject.PreviewKeyUp -= AssociatedObject_PreviewKeyUp;
            this.AssociatedObject.PreviewMouseWheel -= AssociatedObject_PreviewMouseWheel;

            base.OnDetaching();
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
            this.AssociatedObject.PreviewKeyUp += AssociatedObject_PreviewKeyUp;
            this.AssociatedObject.PreviewMouseWheel += AssociatedObject_PreviewMouseWheel;
        }

        private void AssociatedObject_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == this.ZoomingStateEntryKey) this.zoomingReady = true;
        }

        private void AssociatedObject_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == this.ZoomingStateEntryKey) this.zoomingReady = false;
        }

        private void AssociatedObject_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (this.zoomingReady)
            {
                e.Handled = true;
                Control control = sender as Control;

                if(e.Delta > 0)
                {
                    if (control.FontSize < this.MaxFontSize) control.FontSize += 1;
                }
                else
                {
                    if (control.FontSize > this.MinFontSize) control.FontSize -= 1;
                }
            }
            else
            {
                /*
                var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                e2.RoutedEvent = UIElement.MouseWheelEvent;
                AssociatedObject.RaiseEvent(e2);
                */
            }
        }
    }
}
