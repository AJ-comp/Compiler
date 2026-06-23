using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ApplicationLayer.WpfApp.AttachedProperties
{
    public static class TreeViewHook
    {
        public static bool GetEnableGotFocusHook(DependencyObject obj)
            => (bool)obj.GetValue(EnableGotFocusHookProperty);
        public static void SetEnableGotFocusHook(DependencyObject obj, bool value)
            => obj.SetValue(EnableGotFocusHookProperty, value);

        // Using a DependencyProperty as the backing store for EnableGotFocusHook.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableGotFocusHookProperty =
            DependencyProperty.RegisterAttached("EnableGotFocusHook", typeof(bool), typeof(TreeViewHook), new PropertyMetadata(false, OnEnableGotFocusChanged));

        private static void OnEnableGotFocusChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var treeView = (sender as TreeView);
            if (treeView == null) return;

            bool on = (bool)e.NewValue;
            if (on) treeView.GotFocus += TreeView_GotFocus;
            else treeView.GotFocus -= TreeView_GotFocus;
        }

        private static void TreeView_GotFocus(object sender, RoutedEventArgs e)
        {
            var command = TreeViewHook.GetGotFocusCommand(sender as TreeView);
            if (command == null) return;

            var parameter = TreeViewHook.GetGotFocusCommandParameter(sender as TreeView);
            var commandParam = new Tuple<object, RoutedEventArgs>(parameter, e);
            if (command.CanExecute(commandParam) == false) return;

            command.Execute(commandParam);
        }


        public static ICommand GetGotFocusCommand(DependencyObject obj)
            => (ICommand)obj.GetValue(GotFocusCommandProperty);
        public static void SetGotFocusCommand(DependencyObject obj, ICommand value)
            => obj.SetValue(GotFocusCommandProperty, value);

        // Using a DependencyProperty as the backing store for GotFocusCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GotFocusCommandProperty =
            DependencyProperty.RegisterAttached("GotFocusCommand", typeof(ICommand), typeof(TreeViewHook), new PropertyMetadata(null));



        public static object GetGotFocusCommandParameter(DependencyObject obj)
            => (object)obj.GetValue(GotFocusCommandParameterProperty);
        public static void SetGotFocusCommandParameter(DependencyObject obj, object value)
            => obj.SetValue(GotFocusCommandParameterProperty, value);

        // Using a DependencyProperty as the backing store for GotFocusCommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GotFocusCommandParameterProperty =
            DependencyProperty.RegisterAttached("GotFocusCommandParameter", typeof(object), typeof(TreeViewHook), new PropertyMetadata(null));






    }
}
