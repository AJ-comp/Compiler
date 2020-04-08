using ActiproSoftware.Windows.Controls.Grids;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ApplicationLayer.Views.AttachedProperties
{
    public static class TreeListBoxHook
    {
        public static bool GetItemMenuRequestedHook(DependencyObject obj) => (bool)obj.GetValue(ItemMenuRequestedHookProperty);
        public static void SetItemMenuRequestedHook(DependencyObject obj, bool value) => obj.SetValue(ItemMenuRequestedHookProperty, value);

        // Using a DependencyProperty as the backing store for ItemMenuRequestedHook.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemMenuRequestedHookProperty =
            DependencyProperty.RegisterAttached("ItemMenuRequestedHook", typeof(bool), typeof(TreeListBoxHook), new PropertyMetadata(false, OnItemMenuRequestedChanged));

        private static void OnItemMenuRequestedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (sender as TreeListBox);
            if (control == null) return;

            control.ContextMenu = new ContextMenu() { DataContext = control.DataContext };
            bool value = (bool)e.NewValue;
            if(value) control.ItemMenuRequested += Control_ItemMenuRequested;
            else control.ItemMenuRequested -= Control_ItemMenuRequested;
        }

        private static void Control_ItemMenuRequested(object sender, TreeListBoxItemMenuEventArgs e)
        {
            var treeListBox = sender as TreeListBox;

            e.Menu = new ContextMenu();
            e.Menu.DataContext = treeListBox.DataContext;
            var menusForTargets = TreeListBoxHook.GetMenusForTargetCollection(sender as TreeListBox);

            /*
            foreach (var item in menusForTargets)
            {
                if (treeListBox.SelectedItem.GetType() != item.TargetType) continue;

                foreach(var menuItem in item.Menu)
                {
                    if (menuItem is MenuItem)
                    {
                        var typeItem = menuItem as MenuItem;

                        var addItem = new MenuItem()
                        {
                            DataContext = treeListBox.DataContext,
                            Header = typeItem.Header, 
                            Command = typeItem.Command, 
                            CommandParameter = typeItem.CommandParameter 
                        };
                        e.Menu.Items.Add(addItem);
                    }
                    else if (menuItem is Separator)
                    {
                        e.Menu.Items.Add(new Separator());
                    }
                }
            }
            */
        }


        public static MenusForTargetCollection GetMenusForTargetCollection(DependencyObject obj)
        {
            return (MenusForTargetCollection)obj.GetValue(MenusForTargetCollectionProperty);
        }

        public static void SetMenusForTargetCollection(DependencyObject obj, MenusForTargetCollection value)
        {
            obj.SetValue(MenusForTargetCollectionProperty, value);
        }

        // Using a DependencyProperty as the backing store for MenusForTargetCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MenusForTargetCollectionProperty =
            DependencyProperty.RegisterAttached("MenusForTargetCollection", typeof(MenusForTargetCollection), typeof(TreeListBoxHook), new PropertyMetadata(default(MenusForTargetCollection)));




    }
}
