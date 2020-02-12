using ApplicationLayer.Models.SolutionPackage;
using System.Windows;
using System.Windows.Controls;

namespace ApplicationLayer.WpfApp.AttachedProperties
{
    public static class EditableTextBox
    {
        public static bool GetChangeEditMode(DependencyObject obj)
        {
            return (bool)obj.GetValue(ChangeEditModeProperty);
        }

        public static void SetChangeEditMode(DependencyObject obj, bool value)
        {
            obj.SetValue(ChangeEditModeProperty, value);
        }

        // Using a DependencyProperty as the backing store for ChangeEditMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChangeEditModeProperty =
            DependencyProperty.RegisterAttached("ChangeEditMode", typeof(bool), typeof(EditableTextBox), new PropertyMetadata(OnPropertyChanged));


        private static void OnPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (sender as TextBlock);
            if (control == null) return;

            bool on = (bool)e.NewValue;
            if (on)
            {
                control.KeyDown += TextBox_KeyDown;
                control.GotFocus += Control_GotFocus;
                control.LostFocus += Control_LostFocus;
            }
            else
            {
                control.KeyDown -= TextBox_KeyDown;
                control.GotFocus -= Control_GotFocus;
                control.LostFocus -= Control_LostFocus;
            }
        }

        private static void Control_LostFocus(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private static void Control_GotFocus(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private static void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var textBox = (sender as Control);
            if (textBox == null) return;

            if(e.Key == System.Windows.Input.Key.F2)
            {
                var dataContext = textBox.DataContext as HierarchicalData;
                dataContext.IsEditMode = true;
            }
        }
    }
}
