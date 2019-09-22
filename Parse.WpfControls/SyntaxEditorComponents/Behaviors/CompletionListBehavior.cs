using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace Parse.WpfControls.SyntaxEditorComponents.Behaviors
{
    class CompletionListBehavior : Behavior<Popup>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
            base.OnDetaching();
        }

        private void AssociatedObject_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Popup popup = sender as Popup;
            ListBox parent = popup.PlacementTarget as ListBox;


        }
    }
}
