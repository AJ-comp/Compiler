using ActiproSoftware.Windows.Controls.Docking;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ApplicationLayer.WpfApp.AttachedProperties
{
    public static class DockSiteHook
    {
        //public static ContextMenu GetAddDocumentMenu(DependencyObject obj)
        //{
        //    return (ContextMenu)obj.GetValue(AddDocumentMenuProperty);
        //}

        //public static void SetAddDocumentMenu(DependencyObject obj, ContextMenu value)
        //{
        //    obj.SetValue(AddDocumentMenuProperty, value);
        //}

        //// Using a DependencyProperty as the backing store for AddDocumentMenu.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty AddDocumentMenuProperty =
        //    DependencyProperty.RegisterAttached("AddDocumentMenu", typeof(ContextMenu), typeof(DockSiteHook), new PropertyMetadata(new ContextMenu()));

        //private static void OnDocumentMenuChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    var dockSite = (sender as DockSite);
        //    if (dockSite == null) return;

        //    ContextMenu on = (ContextMenu)e.NewValue;
        //    if (on is null) dockSite.MenuOpening -= DockSite_MenuOpening;
        //    else dockSite.MenuOpening += DockSite_MenuOpening;
        //}

        //private static void DockSite_MenuOpening(object sender, DockingMenuEventArgs e)
        //{
        //    e.Menu.Items.Add(DockSiteHook.GetAddDocumentMenu(sender as DockSite));
        //}



        public static bool GetWindowClosedHook(DependencyObject obj)
        {
            return (bool)obj.GetValue(WindowClosedHookProperty);
        }

        public static void SetWindowClosedHook(DependencyObject obj, bool value)
        {
            obj.SetValue(WindowClosedHookProperty, value);
        }

        // Using a DependencyProperty as the backing store for WindowClosedHook.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WindowClosedHookProperty =
            DependencyProperty.RegisterAttached("WindowClosedHook", typeof(bool), typeof(DockSiteHook), new PropertyMetadata(OnWindowClosedEventHookChanged));


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


        public static ICommand GetWindowClosedCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(WindowClosedCommandProperty);
        }

        public static void SetWindowClosedCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(WindowClosedCommandProperty, value);
        }

        // Using a DependencyProperty as the backing store for WindowClosedCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WindowClosedCommandProperty =
            DependencyProperty.RegisterAttached("WindowClosedCommand", typeof(ICommand), typeof(DockSiteHook), new PropertyMetadata(null));



        public static object GetWindowClosedCommandParameter(DependencyObject obj)
        {
            return (object)obj.GetValue(WindowClosedCommandParameterProperty);
        }

        public static void SetWindowClosedCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(WindowClosedCommandParameterProperty, value);
        }

        // Using a DependencyProperty as the backing store for WindowClosedCommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WindowClosedCommandParameterProperty =
            DependencyProperty.RegisterAttached("WindowClosedCommandParameter", typeof(object), typeof(DockSiteHook), new PropertyMetadata(null));



    }
}
