using ApplicationLayer.ViewModels.DocumentTypeViewModels;
using ApplicationLayer.Views.DocumentTypeViews;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ApplicationLayer.Views.AttachedProperties
{
    public static class FrameworkElementHook
    {
        public static bool GetMouseDownHookEnable(DependencyObject obj)
        {
            return (bool)obj.GetValue(MouseDownHookEnableProperty);
        }

        public static void SetMouseDownHookEnable(DependencyObject obj, bool value)
        {
            obj.SetValue(MouseDownHookEnableProperty, value);
        }

        // Using a DependencyProperty as the backing store for MouseDownHookEnable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseDownHookEnableProperty =
            DependencyProperty.RegisterAttached("MouseDownHookEnable", typeof(bool), typeof(FrameworkElementHook), new PropertyMetadata(false, OnMouseDownHookEnableChanged));

        private static void OnMouseDownHookEnableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = (sender as FrameworkElement);
            if (frameworkElement == null) return;

            bool on = (bool)e.NewValue;
            if (on) frameworkElement.MouseDown += VertexControl_MouseDown;
            else frameworkElement.MouseDown -= VertexControl_MouseDown;
        }

        private static void VertexControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var frameworkElement = (sender as FrameworkElement);

            DependencyObject currentNode = frameworkElement;
            while(true)
            {
                currentNode = VisualTreeHelper.GetParent(currentNode);
                if (currentNode == null) break;
                if (currentNode.GetType() == typeof(ParseTreeView))
                {
                    var view = currentNode as ParseTreeView;
                    var viewModel = view.DataContext as ParseTreeViewModel;

                    viewModel.MouseDownCommand.Execute(frameworkElement.DataContext);
                }
            }
        }

    }
}
