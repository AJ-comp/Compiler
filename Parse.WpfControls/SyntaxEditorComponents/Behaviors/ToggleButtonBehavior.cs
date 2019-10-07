using Parse.WpfControls.SyntaxEditorComponents.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Parse.WpfControls.SyntaxEditorComponents.Behaviors
{
    class ToggleButtonBehavior : Behavior<ToggleButton>
    {
        protected override void OnDetaching()
        {

            base.OnDetaching();
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.GotFocus += AssociatedObject_GotFocus;
            this.AssociatedObject.Click += AssociatedObject_Click;
        }

        //private T FindParent<T>(DependencyObject child) where T : DependencyObject
        //{
        //    var parent = VisualTreeHelper.GetParent(child);
        //    if (parent is T) return parent as T;
        //    else return FindParent<T>(parent);
        //}

        private void AssociatedObject_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        }

        private void AssociatedObject_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
        }
    }
}
