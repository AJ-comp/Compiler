using Parse.WpfControls.SyntaxEditorComponents.Views;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Parse.WpfControls.SyntaxEditorComponents
{
    public class CompletionWindow : Window
    {
        TextBox parent;
        CompletionList completionList;

        static CompletionWindow()
        {
            WindowStyleProperty.OverrideMetadata(typeof(CompletionWindow), new FrameworkPropertyMetadata(WindowStyle.None));
            ShowActivatedProperty.OverrideMetadata(typeof(CompletionWindow), new FrameworkPropertyMetadata(false));
            ShowInTaskbarProperty.OverrideMetadata(typeof(CompletionWindow), new FrameworkPropertyMetadata(false));
        }

        /*
        public CompletionWindow(TextBox parent)
        {
            this.parent = parent;
            this.parent.KeyDown += OnKeyDown;
        }
        */

        public CompletionWindow()
        {

        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!e.Handled && e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        /*
        /// <inheritdoc/>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            this.parent.KeyDown -= OnKeyDown;
        }
        */

        /*
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
        */
    }
}
