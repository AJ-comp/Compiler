using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Parse.WpfControls.Behaviors
{
    class HighlightBehavior : Behavior<TextBox>
    {
        protected override void OnDetaching()
        {
            this.AssociatedObject.Loaded -= AssociatedObject_Loaded;

            base.OnDetaching();
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox textbox = sender as TextBox;

            textbox.TextWrapping = TextWrapping.NoWrap;
            textbox.Foreground = Brushes.Transparent;
        }
    }
}
