using ActiproSoftware.Windows.Controls.Docking;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ApplicationLayer.WpfApp.AttachedProperties
{
    public static class DockSiteHook
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // Attached Property for a menu items hook
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        public static bool GetEnableMenuItemHook(DependencyObject obj) => (bool)obj.GetValue(EnableMenuItemHookProperty);
        public static void SetEnableMenuItemHook(DependencyObject obj, bool value) => obj.SetValue(EnableMenuItemHookProperty, value);

        // Using a DependencyProperty as the backing store for EnableMenuItemHook.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableMenuItemHookProperty =
            DependencyProperty.RegisterAttached("EnableMenuItemHook", typeof(bool), typeof(DockSiteHook), new PropertyMetadata(false, OnDocumentMenuChanged));

        private static void OnDocumentMenuChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var dockSite = (sender as DockSite);
            if (dockSite == null) return;

            bool on = (bool)e.NewValue;
            if (on) dockSite.MenuOpening += DockSite_MenuOpening;
            else dockSite.MenuOpening -= DockSite_MenuOpening;
        }

        private static void DockSite_MenuOpening(object sender, DockingMenuEventArgs e)
        {
            MenuItem menuItem = null;

            if (e.Window is DocumentWindow)
            {
                var docFilter = DockSiteHook.GetFilterDocumentMenuItemToAdd(sender as DockSite) as Type;

                // if docFilter is null then filtering off
                // if docFilter is not null then filtering on 
                if (docFilter != null)
                {
                    // filtering
                    if (docFilter == e.Window.GetType() || docFilter == e.Window.DataContext.GetType())
                        menuItem = DockSiteHook.GetDocumentMenuItemToAdd(sender as DockSite) as MenuItem;
                }
                else menuItem = DockSiteHook.GetDocumentMenuItemToAdd(sender as DockSite) as MenuItem;
            }
            else if(e.Window is ToolWindow)
            {
                var toolWindowFilter = DockSiteHook.GetFilterToolWindowMenuItemToAdd(sender as DockSite) as Type;

                // if toolWindowFilter is null then filtering off
                // if toolWindowFilter is not null then filtering on 
                if (toolWindowFilter != null)
                {
                    // filtering
                    if (toolWindowFilter == e.Window.GetType() || toolWindowFilter == e.Window.DataContext.GetType())
                        menuItem = DockSiteHook.GetToolWindowMenuItemToAdd(sender as DockSite) as MenuItem;
                }
                else menuItem = DockSiteHook.GetToolWindowMenuItemToAdd(sender as DockSite) as MenuItem;
            }

            if (menuItem == null) return;

            e.Menu.Items.Add(new Separator());
            var addItem = new MenuItem() { Header = menuItem.Header, Command = menuItem.Command, CommandParameter = menuItem.CommandParameter };
            e.Menu.Items.Add(addItem);
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // This AP registers a menu item to add into the Document.
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        public static MenuItem GetDocumentMenuItemToAdd(DependencyObject obj) => (MenuItem)obj.GetValue(DocumentMenuItemToAddProperty);
        public static void SetDocumentMenuItemToAdd(DependencyObject obj, MenuItem value) => obj.SetValue(DocumentMenuItemToAddProperty, value);

        // Using a DependencyProperty as the backing store for DocumentMenuItemToAdd.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentMenuItemToAddProperty =
            DependencyProperty.RegisterAttached("DocumentMenuItemToAdd", typeof(MenuItem), typeof(DockSiteHook), new PropertyMetadata(null));



        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // This AP registers a filter for menu item to add into the Document.
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        public static Type GetFilterDocumentMenuItemToAdd(DependencyObject obj) => (Type)obj.GetValue(FilterDocumentMenuItemToAddProperty);
        public static void SetFilterDocumentMenuItemToAdd(DependencyObject obj, Type value) => obj.SetValue(FilterDocumentMenuItemToAddProperty, value);

        // Using a DependencyProperty as the backing store for FilterDocumentMenuItemToAdd.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterDocumentMenuItemToAddProperty =
            DependencyProperty.RegisterAttached("FilterDocumentMenuItemToAdd", typeof(Type), typeof(DockSiteHook), new PropertyMetadata(null));



        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // This AP registers a menu item to add into the ToolWindow.
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        public static MenuItem GetToolWindowMenuItemToAdd(DependencyObject obj) => (MenuItem)obj.GetValue(ToolWindowMenuItemToAddProperty);
        public static void SetToolWindowMenuItemToAdd(DependencyObject obj, MenuItem value) => obj.SetValue(ToolWindowMenuItemToAddProperty, value);

        // Using a DependencyProperty as the backing store for ToolWindowMenuItemToAdd.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToolWindowMenuItemToAddProperty =
            DependencyProperty.RegisterAttached("ToolWindowMenuItemToAdd", typeof(MenuItem), typeof(DockSiteHook), new PropertyMetadata(null));



        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // This AP registers a filter for menu item to add into the ToolWindow.
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        public static Type GetFilterToolWindowMenuItemToAdd(DependencyObject obj) => (Type)obj.GetValue(FilterToolWindowMenuItemToAddProperty);
        public static void SetFilterToolWindowMenuItemToAdd(DependencyObject obj, Type value) => obj.SetValue(FilterToolWindowMenuItemToAddProperty, value);

        // Using a DependencyProperty as the backing store for FilterToolWindowMenuItemToAdd.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterToolWindowMenuItemToAddProperty =
            DependencyProperty.RegisterAttached("FilterToolWindowMenuItemToAdd", typeof(Type), typeof(DockSiteHook), new PropertyMetadata(null));

        




        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // This AP set up whether hooks a primary document changed event.
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        public static bool GetEnablePrimaryDocumentChangedHook(DependencyObject obj) => (bool)obj.GetValue(EnablePrimaryDocumentChangedHookProperty);
        public static void SetEnablePrimaryDocumentChangedHook(DependencyObject obj, bool value) => obj.SetValue(EnablePrimaryDocumentChangedHookProperty, value);

        // Using a DependencyProperty as the backing store for EnablePrimaryDocumentChangedHook.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnablePrimaryDocumentChangedHookProperty =
            DependencyProperty.RegisterAttached("EnablePrimaryDocumentChangedHook", typeof(bool), typeof(DockSiteHook), new PropertyMetadata(OnPrimaryDocumentChangedEventHookChanged));

        private static void OnPrimaryDocumentChangedEventHookChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var dockSite = (sender as DockSite);
            if (dockSite == null) return;

            bool on = (bool)e.NewValue;
            if (on) dockSite.PrimaryDocumentChanged += DockSite_PrimaryDocumentChanged;
            else dockSite.PrimaryDocumentChanged -= DockSite_PrimaryDocumentChanged;
        }

        private static void DockSite_PrimaryDocumentChanged(object sender, DockingWindowEventArgs e)
        {
            var command = DockSiteHook.GetPrimaryDocumentChangedCommand(sender as DockSite);
            if (command == null) return;

            var parameter = DockSiteHook.GetPrimaryDocumentChangedCommandParameter(sender as DockSite);
            var commandParam = new Tuple<object, DockingWindowEventArgs>(parameter, e);
            if (command.CanExecute(commandParam) == false) return;

            command.Execute(commandParam);
        }



        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // This AP has command to execute when a primary document changed event fired.
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        public static ICommand GetPrimaryDocumentChangedCommand(DependencyObject obj) => (ICommand)obj.GetValue(PrimaryDocumentChangedCommandProperty);
        public static void SetPrimaryDocumentChangedCommand(DependencyObject obj, ICommand value) => obj.SetValue(PrimaryDocumentChangedCommandProperty, value);

        // Using a DependencyProperty as the backing store for PrimaryDocumentChangedCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrimaryDocumentChangedCommandProperty =
            DependencyProperty.RegisterAttached("PrimaryDocumentChangedCommand", typeof(ICommand), typeof(DockSiteHook), new PropertyMetadata(null));



        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // This AP has command parameter to pass to the command when a primary document changed event fired.
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static object GetPrimaryDocumentChangedCommandParameter(DependencyObject obj) => (object)obj.GetValue(PrimaryDocumentChangedCommandParameterProperty);
        public static void SetPrimaryDocumentChangedCommandParameter(DependencyObject obj, object value) => obj.SetValue(PrimaryDocumentChangedCommandParameterProperty, value);

        // Using a DependencyProperty as the backing store for PrimaryDocumentChangedCommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrimaryDocumentChangedCommandParameterProperty =
            DependencyProperty.RegisterAttached("PrimaryDocumentChangedCommandParameter", typeof(object), typeof(DockSiteHook), new PropertyMetadata(null));






        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // This AP set up whether hooks a window closed event.
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        public static bool GetEnableWindowClosedHook(DependencyObject obj) => (bool)obj.GetValue(EnableWindowClosedHookProperty);
        public static void SetEnableWindowClosedHook(DependencyObject obj, bool value) => obj.SetValue(EnableWindowClosedHookProperty, value);

        // Using a DependencyProperty as the backing store for EnableWindowClosedHook.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableWindowClosedHookProperty =
            DependencyProperty.RegisterAttached("EnableWindowClosedHook", typeof(bool), typeof(DockSiteHook), new PropertyMetadata(false, OnWindowClosedEventHookChanged));


        private static void OnWindowClosedEventHookChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var dockSite = (sender as DockSite);
            if (dockSite == null) return;

            bool on = (bool)e.NewValue;
            if (on) dockSite.WindowsClosed += DockSite_WindowsClosed;
            else dockSite.WindowsClosed -= DockSite_WindowsClosed;
        }

        private static void DockSite_WindowsClosed(object sender, DockingWindowsEventArgs e)
        {
            var command = DockSiteHook.GetWindowClosedCommand(sender as DockSite);
            if (command == null) return;

            var parameter = DockSiteHook.GetWindowClosedCommandParameter(sender as DockSite);
            var commandParam = new Tuple<object, DockingWindowsEventArgs>(parameter, e);
            if (command.CanExecute(commandParam) == false) return;

            command.Execute(commandParam);
        }



        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // This AP has command to execute when a window closed event fired.
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        public static ICommand GetWindowClosedCommand(DependencyObject obj) => (ICommand)obj.GetValue(WindowClosedCommandProperty);
        public static void SetWindowClosedCommand(DependencyObject obj, ICommand value) => obj.SetValue(WindowClosedCommandProperty, value);

        // Using a DependencyProperty as the backing store for WindowClosedCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WindowClosedCommandProperty =
            DependencyProperty.RegisterAttached("WindowClosedCommand", typeof(ICommand), typeof(DockSiteHook), new PropertyMetadata(null));



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // This AP has command parameter to pass to the command when a window closed event fired.
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static object GetWindowClosedCommandParameter(DependencyObject obj) => (object)obj.GetValue(WindowClosedCommandParameterProperty);
        public static void SetWindowClosedCommandParameter(DependencyObject obj, object value) => obj.SetValue(WindowClosedCommandParameterProperty, value);

        // Using a DependencyProperty as the backing store for WindowClosedCommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WindowClosedCommandParameterProperty =
            DependencyProperty.RegisterAttached("WindowClosedCommandParameter", typeof(object), typeof(DockSiteHook), new PropertyMetadata(null));



    }
}
