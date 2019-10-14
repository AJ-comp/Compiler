using Parse.WpfControls.SyntaxEditorComponents.Models;
using Parse.WpfControls.SyntaxEditorComponents.ViewModels;
using Parse.WpfControls.SyntaxEditorComponents.Views;
using System.Windows;
using System.Windows.Interactivity;

namespace Parse.WpfControls.SyntaxEditorComponents.Behaviors
{
    class DynamicToggleButtonCreateBehavior : Behavior<CompletionList>
    {
        protected override void OnDetaching()
        {
            this.AssociatedObject.Opened -= AssociatedObject_Opened;
            this.AssociatedObject.Closed -= AssociatedObject_Closed;

            base.OnDetaching();
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.Opened += AssociatedObject_Opened;
            this.AssociatedObject.Closed += AssociatedObject_Closed;
        }

        private void AssociatedObject_Closed(object sender, System.EventArgs e)
        {
            CompletionList completionList = sender as CompletionList;
            var context = completionList.DataContext as CompletionListViewModel;

            context.Init();
        }

        private void AssociatedObject_Opened(object sender, System.EventArgs e)
        {
            CompletionList completionList = sender as CompletionList;

            foreach (CompletionItem item in completionList.listBox.Items)
            {
                if (item.ItemType == CompletionItemType.Field) completionList.fieldBtn.Visibility = Visibility.Visible;
                else if (item.ItemType == CompletionItemType.Property) completionList.propertyBtn.Visibility = Visibility.Visible;
                else if (item.ItemType == CompletionItemType.Keyword) completionList.keywordBtn.Visibility = Visibility.Visible;
                else if (item.ItemType == CompletionItemType.Enum) completionList.enumBtn.Visibility = Visibility.Visible;
                else if (item.ItemType == CompletionItemType.CodeSnipp) completionList.codeSnippBtn.Visibility = Visibility.Visible;
                else if (item.ItemType == CompletionItemType.Function) completionList.functionBtn.Visibility = Visibility.Visible;
                else if (item.ItemType == CompletionItemType.Namespace) completionList.namespaceBtn.Visibility = Visibility.Visible;
                else if (item.ItemType == CompletionItemType.Event) completionList.eventBtn.Visibility = Visibility.Visible;
                else if (item.ItemType == CompletionItemType.Delegate) completionList.delegateBtn.Visibility = Visibility.Visible;
                else if (item.ItemType == CompletionItemType.Class) completionList.classBtn.Visibility = Visibility.Visible;
                else if (item.ItemType == CompletionItemType.Struct) completionList.structBtn.Visibility = Visibility.Visible;
                else if (item.ItemType == CompletionItemType.Interface) completionList.interfaceBtn.Visibility = Visibility.Visible;
            }
            
        }
    }
}
