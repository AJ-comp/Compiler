using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace ApplicationLayer.WpfApp.Behaviors
{
    class SolutionTreeViewBehavior : Behavior<TreeView>
    {
        protected override void OnDetaching()
        {
            base.OnDetaching();
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.MouseRightButtonDown += AssociatedObject_MouseRightButtonDown;
        }

        private void AssociatedObject_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void AssociatedObject_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            throw new System.NotImplementedException();
        }
    }
}
