using ActiproSoftware.Windows.Controls.Grids;
using System.Windows;
using System.Windows.Input;

namespace ApplicationLayer.Views.AttachedProperties
{
    public static class TreeListBoxHook
    {
        public static bool GetDefaultActionHook(DependencyObject obj) => (bool)obj.GetValue(DefaultActionHookProperty);
        public static void SetDefaultActionHook(DependencyObject obj, bool value) => obj.SetValue(DefaultActionHookProperty, value);

        // Using a DependencyProperty as the backing store for DefaultActionHook.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultActionHookProperty =
            DependencyProperty.RegisterAttached("DefaultActionHook", typeof(bool), typeof(TreeListBoxHook), new PropertyMetadata(false, OnDefaultActionHookChanged));

        private static void OnDefaultActionHookChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var treeView = (sender as TreeListBox);
            if (treeView == null) return;

            bool on = (bool)e.NewValue;
            if (on) treeView.ItemDefaultActionExecuting += TreeView_ItemDefaultActionExecuting;
            else treeView.ItemDefaultActionExecuting -= TreeView_ItemDefaultActionExecuting;
        }

        private static void TreeView_ItemDefaultActionExecuting(object sender, TreeListBoxItemEventArgs e)
        {
            e.Cancel = TreeListBoxHook.GetCancelCondition(sender as TreeListBox);
            if (e.Cancel == false) return;

            var command = TreeListBoxHook.GetDefaultActionCommand(sender as TreeListBox);

            if (command == null) return;
            if (command.CanExecute(e.Item)) command.Execute(e.Item);
        }



        public static bool GetCancelCondition(DependencyObject obj) => (bool)obj.GetValue(CancelConditionProperty);
        public static void SetCancelCondition(DependencyObject obj, bool value) => obj.SetValue(CancelConditionProperty, value);

        // Using a DependencyProperty as the backing store for CancelCondition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CancelConditionProperty =
            DependencyProperty.RegisterAttached("CancelCondition", typeof(bool), typeof(TreeListBoxHook), new PropertyMetadata(false));




        public static ICommand GetDefaultActionCommand(DependencyObject obj) => (ICommand)obj.GetValue(DefaultActionCommandProperty);
        public static void SetDefaultActionCommand(DependencyObject obj, ICommand value) => obj.SetValue(DefaultActionCommandProperty, value);

        // Using a DependencyProperty as the backing store for DefaultActionCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultActionCommandProperty =
            DependencyProperty.RegisterAttached("DefaultActionCommand", typeof(ICommand), typeof(TreeListBoxHook), new PropertyMetadata(null));
    }
}
