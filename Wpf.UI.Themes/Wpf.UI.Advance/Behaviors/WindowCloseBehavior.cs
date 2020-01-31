using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.UI.Advance.Behaviors
{
    public class WindowCloseBehavior : DependencyObject
    {
        public static bool GetEnableWindowClose(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableWindowCloseProperty);
        }

        public static void SetEnableWindowClose(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableWindowCloseProperty, value);
        }

        // Using a DependencyProperty as the backing store for EnableWindowClose.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableWindowCloseProperty =
            DependencyProperty.RegisterAttached("EnableWindowClose", typeof(bool), typeof(WindowCloseBehavior), new PropertyMetadata(WindowCloseChanged));


        private static void WindowCloseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = d as Button;
            bool value = (bool)e.NewValue;

            if (value) button.Click += Button_Click;
            else button.Click -= Button_Click;
        }

        private static void Button_Click(object sender, RoutedEventArgs e)
        {
            DependencyObject current = sender as Button;

            while(true)
            {
                current = VisualTreeHelper.GetParent(current);
                if (current == null) return;

                if (current is Window)
                {
                    (current as Window).Close();
                    return;
                }
            }
        }
    }
}
