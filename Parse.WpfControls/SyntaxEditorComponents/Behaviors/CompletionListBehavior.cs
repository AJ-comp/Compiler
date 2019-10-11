using Parse.WpfControls.SyntaxEditorComponents.ViewModels;
using Parse.WpfControls.SyntaxEditorComponents.Views;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Parse.WpfControls.SyntaxEditorComponents.Behaviors
{
    class CompletionListBehavior : Behavior<TextArea>
    {
        /// <summary> This property gets the caret index when completion list occur. </summary>
        private int caretIndexWhenCLOccur;

        protected override void OnDetaching()
        {
            this.AssociatedObject.Loaded -= AssociatedObject_Loaded;
            this.AssociatedObject.TextChanged -= AssociatedObject_TextChanged;
            this.AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
            this.AssociatedObject.MouseDoubleClick -= AssociatedObject_MouseDoubleClick;

            base.OnDetaching();
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.Loaded += AssociatedObject_Loaded;
            this.AssociatedObject.TextChanged += AssociatedObject_TextChanged;
            this.AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
            this.AssociatedObject.MouseDoubleClick += AssociatedObject_MouseDoubleClick;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            TextArea textArea = sender as TextArea;

            var completionListContext = textArea.completionList.DataContext as CompletionListViewModel;

            textArea.completionList.listBox.SelectionChanged += ((s, le) => textArea.completionList.listBox.ScrollIntoView(textArea.completionList.listBox.SelectedItem));
            textArea.completionList.listBox.SelectionChanged += ((s, le) => textArea.Focus());
            completionListContext.RequestFilterButtonClick += ((s, le) => textArea.Focus());
        }

        private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.GenerateCompletionList(sender as TextArea, e.Changes.First());
        }

        private void AssociatedObject_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextArea textArea = sender as TextArea;

            if (textArea.completionList.IsOpen)
            {
                if (this.InputProcessOnCompletionList(textArea, e.Key)) e.Handled = true;
                return;
            }
        }

        private void AssociatedObject_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextArea textArea = sender as TextArea;

            if (textArea.completionList.listBox.IsMouseOver)
            {
                var src = VisualTreeHelper.GetParent(e.OriginalSource as DependencyObject);
                if (src is VirtualizingStackPanel || src is ContentPresenter)
                {
                    this.InputProcessOnCompletionList(textArea, Key.Enter);
                }
                return;
            }
        }

        private bool IsBackSpace(TextChange changeInfo) => (changeInfo.RemovedLength >= 1 && changeInfo.AddedLength == 0);

        /// <summary>
        /// This function generates completion list.
        /// </summary>
        /// <param name="changeInfo"></param>
        private void GenerateCompletionList(TextArea textArea, TextChange changeInfo)
        {
            var addString = textArea.Text.Substring(changeInfo.Offset, changeInfo.AddedLength);
            if (addString.Length > 1) { textArea.completionList.IsOpen = false; return; }
            if (textArea.DelimiterSet.Contains(addString)) { textArea.completionList.IsOpen = false; return; }

            var context = textArea.completionList.DataContext as CompletionListViewModel;
            if (this.IsBackSpace(changeInfo))
            {
                if (textArea.completionList.IsOpen == false) return;
                if (textArea.CaretIndex <= this.caretIndexWhenCLOccur) { textArea.completionList.IsOpen = false; return; }
            }
            else if (textArea.completionList.IsOpen == false)
            {
                if (addString.Length == 1)
                {
                    context.LoadAvailableCollection();
                    this.caretIndexWhenCLOccur = textArea.CaretIndex - 1;
                }
            }

            var rect = textArea.GetRectFromCharacterIndex(textArea.CaretIndex);

            textArea.completionList.StaysOpen = false;
            textArea.completionList.Placement = PlacementMode.Relative;
            textArea.completionList.PlacementTarget = textArea;
            textArea.completionList.VerticalOffset = rect.Y + textArea.LineHeight;
            textArea.completionList.HorizontalOffset = rect.X;

            context.InputString = textArea.Text.Substring(this.caretIndexWhenCLOccur, textArea.CaretIndex - this.caretIndexWhenCLOccur);
            textArea.completionList.IsOpen = true;
        }

        /// <summary>
        /// This function brings to TextArea a selected text in the completion list.
        /// </summary>
        /// <param name="other"></param>
        private void BringStringFromCompletionList(TextArea textArea, string other = "")
        {
            var context = textArea.completionList.DataContext as CompletionListViewModel;

            int startIndex = this.caretIndexWhenCLOccur;
            int endIndex = textArea.CaretIndex;

            if (context.SelectedIndex >= 0)
            {
                var addString = context.CandidateCollection[context.SelectedIndex].ItemName + other;

                textArea.Select(startIndex, endIndex - startIndex);
                TextCompositionManager.StartComposition(new TextComposition(InputManager.Current, textArea, addString));
            }
            else if (other.Length > 0)
                TextCompositionManager.StartComposition(new TextComposition(InputManager.Current, textArea, other));
        }

        private bool InputProcessOnCompletionList(TextArea textArea, Key keyType)
        {
            bool result = false;

            if (keyType == Key.Up)
            {
                var context = textArea.completionList.DataContext as CompletionListViewModel;
                context.Up();
                result = true;
            }
            else if (keyType == Key.Down)
            {
                var context = textArea.completionList.DataContext as CompletionListViewModel;
                context.Down();
                result = true;
            }
            else if (keyType == Key.Enter || keyType == Key.Tab)
            {
                textArea.completionList.IsOpen = false;
                this.BringStringFromCompletionList(textArea);
                result = true;
            }
            else if (keyType == Key.Space || keyType == Key.OemPeriod)
            {
                textArea.completionList.IsOpen = false;
                if (keyType == Key.Space) this.BringStringFromCompletionList(textArea, " ");
                else if (keyType == Key.OemPeriod) this.BringStringFromCompletionList(textArea, ".");

                result = true;
            }
            else if (keyType == Key.Escape)
            {
                textArea.completionList.IsOpen = false;
                result = true;
            }

            return result;
        }
    }
}
