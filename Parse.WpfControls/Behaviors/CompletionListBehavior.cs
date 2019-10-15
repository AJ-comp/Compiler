using Parse.WpfControls.SyntaxEditorComponents.Models;
using Parse.WpfControls.SyntaxEditorComponents.ViewModels;
using Parse.WpfControls.SyntaxEditorComponents.Views;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Parse.WpfControls.Behaviors
{
    /// <summary>
    /// This behavior class gives to TextBox an ability that can interaction CompletionList Module.
    /// </summary>
    class CompletionListBehavior : Behavior<TextBoxBase>
    {
        private int caretIndexWhenCLOccur;
        private static CompletionList completionList = new CompletionList();

        #region CloseCharacters DP
        public StringCollection CloseCharacters
        {
            get { return (StringCollection)GetValue(CloseCharactersProperty); }
            set { SetValue(CloseCharactersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CloseCharacters.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseCharactersProperty =
            DependencyProperty.Register("CloseCharacters", typeof(StringCollection), typeof(CompletionListBehavior), new PropertyMetadata(CloseCharactersChanged));

        private static void CloseCharactersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        #region Items DP
        public ObservableCollection<CompletionItem> Items
        {
            get { return (ObservableCollection<CompletionItem>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<CompletionItem>), typeof(CompletionListBehavior), new PropertyMetadata(ItemsChanged));

        private static void ItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var col = (ObservableCollection<CompletionItem>)e.NewValue;
            if (col != null) { col.CollectionChanged += OnCollectionChanged; ; }

            col = (ObservableCollection<CompletionItem>)e.OldValue;
            if (col != null) { col.CollectionChanged -= OnCollectionChanged; }


        }

        private static void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null) return;
            var context = completionList?.DataContext as CompletionListViewModel;

//            context.ClearCollection();
            foreach (var newItem in e.NewItems)
            {
                var item = newItem as CompletionItem;
                //Add listener for each item on PropertyChanged event
                if (e.Action == NotifyCollectionChangedAction.Add)
                    context.AddCollection(item.ItemType, item.ItemName);
            }
        }
        #endregion

        #region LineHeight DP
        public double LineHeight
        {
            get { return (double)GetValue(LineHeightProperty); }
            set { SetValue(LineHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineHeightProperty =
            DependencyProperty.Register("LineHeight", typeof(double), typeof(CompletionListBehavior), new PropertyMetadata((double)0));
        #endregion


        public CompletionListBehavior()
        {
            SetValue(CloseCharactersProperty, new StringCollection() { " ", Environment.NewLine });
            SetValue(ItemsProperty, new ObservableCollection<CompletionItem>());
        }

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
            TextBox textbox = sender as TextBox;

            var completionListContext = completionList.DataContext as CompletionListViewModel;

            completionList.listBox.SelectionChanged += ((s, le) => completionList.listBox.ScrollIntoView(completionList.listBox.SelectedItem));
            completionList.listBox.SelectionChanged += ((s, le) => textbox.Focus());
            completionListContext.RequestFilterButtonClick += ((s, le) => textbox.Focus());
        }

        private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.GenerateCompletionList(sender as TextBox, e.Changes.First());
        }

        private void AssociatedObject_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox textbox = sender as TextBox;

            if (completionList.IsOpen)
            {
                if (this.InputProcessOnCompletionList(textbox, e.Key)) e.Handled = true;
                return;
            }
        }

        private void AssociatedObject_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var textbox = sender as TextBox;

            if (completionList.listBox.IsMouseOver)
            {
                var src = VisualTreeHelper.GetParent(e.OriginalSource as DependencyObject);
                if (src is VirtualizingStackPanel || src is ContentPresenter)
                {
                    this.InputProcessOnCompletionList(textbox, Key.Enter);
                }
                return;
            }
        }

        private bool IsBackSpace(TextChange changeInfo) => (changeInfo.RemovedLength >= 1 && changeInfo.AddedLength == 0);

        /// <summary>
        /// This function generates completion list.
        /// </summary>
        /// <param name="changeInfo"></param>
        private void GenerateCompletionList(TextBox textbox, TextChange changeInfo)
        {
            var addString = textbox.Text.Substring(changeInfo.Offset, changeInfo.AddedLength);
            if (addString.Length > 1) { completionList.IsOpen = false; return; }
            if (this.CloseCharacters.Contains(addString)) { completionList.IsOpen = false; return; }

            var context = completionList.DataContext as CompletionListViewModel;
            if (IsBackSpace(changeInfo))
            {
                if (completionList.IsOpen == false) return;
                if (textbox.CaretIndex <= caretIndexWhenCLOccur) { completionList.IsOpen = false; return; }
            }
            else if (completionList.IsOpen == false)
            {
                if (addString.Length == 1)
                {
                    context.LoadAvailableCollection();
                    if (context.IsExistCollection() == false) return;
                    caretIndexWhenCLOccur = textbox.CaretIndex - 1;
                }
            }

            var rect = textbox.GetRectFromCharacterIndex(textbox.CaretIndex);

            completionList.StaysOpen = false;
            completionList.Placement = PlacementMode.Relative;
            completionList.PlacementTarget = textbox;
            completionList.VerticalOffset = (this.LineHeight > 0) ? rect.Y + this.LineHeight : rect.Y + textbox.FontSize;
            completionList.HorizontalOffset = rect.X;

            context.InputString = textbox.Text.Substring(caretIndexWhenCLOccur, textbox.CaretIndex - caretIndexWhenCLOccur);
            completionList.IsOpen = true;
        }

        private bool InputProcessOnCompletionList(TextBox textbox, Key keyType)
        {
            bool result = false;

            if (keyType == Key.Up)
            {
                var context = completionList.DataContext as CompletionListViewModel;
                context.Up();
                result = true;
            }
            else if (keyType == Key.Down)
            {
                var context = completionList.DataContext as CompletionListViewModel;
                context.Down();
                result = true;
            }
            else if (keyType == Key.Enter || keyType == Key.Tab)
            {
                completionList.IsOpen = false;
                BringStringFromCompletionList(textbox);
                result = true;
            }
            else if (keyType == Key.Space || keyType == Key.OemPeriod)
            {
                completionList.IsOpen = false;
                if (keyType == Key.Space) BringStringFromCompletionList(textbox, " ");
                else if (keyType == Key.OemPeriod) BringStringFromCompletionList(textbox, ".");

                result = true;
            }
            else if (keyType == Key.Escape)
            {
                completionList.IsOpen = false;
                result = true;
            }

            return result;
        }

        /// <summary>
        /// This function brings to TextArea a selected text in the completion list.
        /// </summary>
        /// <param name="other"></param>
        private void BringStringFromCompletionList(TextBox textbox, string other = "")
        {
            var context = completionList.DataContext as CompletionListViewModel;

            int startIndex = caretIndexWhenCLOccur;
            int endIndex = textbox.CaretIndex;

            if (context.SelectedIndex >= 0)
            {
                var addString = context.CandidateCollection[context.SelectedIndex].ItemName + other;

                textbox.Select(startIndex, endIndex - startIndex);
                TextCompositionManager.StartComposition(new TextComposition(InputManager.Current, textbox, addString));
            }
            else if (other.Length > 0)
                TextCompositionManager.StartComposition(new TextComposition(InputManager.Current, textbox, other));
        }
    }
}
