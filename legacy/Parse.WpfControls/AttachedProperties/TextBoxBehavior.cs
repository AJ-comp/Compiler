using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Parse.WpfControls.AttachedProperties
{
    public class TextBoxBehavior : DependencyObject
    {
        private static bool backupAcceptTab = false;

        #region TabSizeEnable
        public static bool GetTabSizeEnable(DependencyObject item)
        {
            return (bool)item.GetValue(TabSizeEnableProperty);
        }

        public static void SetTabSizeEnable(DependencyObject obj, bool value)
        {
            obj.SetValue(TabSizeEnableProperty, value);
        }

        // Using a DependencyProperty as the backing store for CanFocusOnLoad.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TabSizeEnableProperty =
            DependencyProperty.RegisterAttached("TabSizeEnable", typeof(bool), typeof(TextBoxBehavior), new PropertyMetadata(TabSizeEnableChanged));

        private static void TabSizeEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = d as TextBox;

            if ((bool)e.NewValue == true)
            {
                backupAcceptTab = textBox.AcceptsTab;
                textBox.AcceptsTab = true;
                textBox.PreviewKeyDown += TextBox_TabSizeHandler;
            }
            else
            {
                textBox.AcceptsTab = backupAcceptTab;
                textBox.PreviewKeyDown -= TextBox_TabSizeHandler;
            }
        }
        #endregion

        #region TabSize
        private static int tabSize = 4;
        public static int GetTabSize(DependencyObject item)
        {
            return (int)item.GetValue(TabSizeProperty);
        }

        public static void SetTabSize(DependencyObject obj, int value)
        {
            obj.SetValue(TabSizeProperty, value);
        }

        // Using a DependencyProperty as the backing store for CanFocusOnLoad.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TabSizeProperty =
            DependencyProperty.RegisterAttached("TabSize", typeof(int), typeof(TextBoxBehavior), new PropertyMetadata(TabSizeChanged));

        private static void TabSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            tabSize = (int)e.NewValue;
        }
        #endregion

        #region TabSize Handler
        private static void TextBox_TabSizeHandler(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;

            if (e.Key == Key.Tab)
            {
                string tab = new string(' ', (int)tabSize);
                TextCompositionManager.StartComposition(new TextComposition(InputManager.Current, textBox, tab));

                e.Handled = true;
            }
        }
        #endregion
    }
}
