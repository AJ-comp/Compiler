using System.Windows;
using System.Windows.Controls;

namespace ApplicationLayer.WpfApp.AttachedProperties
{
    public static class EditableTextBox
    {
        public static bool GetFocus(DependencyObject obj)
        {
            return (bool)obj.GetValue(FocusProperty);
        }

        public static void SetFocus(DependencyObject obj, bool value)
        {
            obj.SetValue(FocusProperty, value);
        }

        // Using a DependencyProperty as the backing store for Focus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusProperty =
            DependencyProperty.RegisterAttached("Focus", typeof(bool), typeof(EditableTextBox), new PropertyMetadata(OnPropertyChanged));


        private static void OnPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (sender as TextBox);
            if (control == null) return;

            bool on = (bool)e.NewValue;
            if (on) control.Focus();
        }
    }
}
