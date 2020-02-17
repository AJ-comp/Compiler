using ActiproSoftware.Windows.Controls.Docking;
using ActiproSoftware.Windows.Controls.Docking.Serialization;
using ApplicationLayer.WpfApp.ViewModels;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace ApplicationLayer.WpfApp.AttachedProperties
{
    public static class MainWindowHook
    {
        private static string mainFormLayoutFileName = "FormInformation.layout";
        private static string dockingLayoutFileName = "DeployInformation.layout";

        #region EnableHookLoaded
        public static bool GetEnableHookLoaded(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableHookLoadedProperty);
        }

        public static void SetEnableHookLoaded(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableHookLoadedProperty, value);
        }

        // Using a DependencyProperty as the backing store for EnableHookLoaded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableHookLoadedProperty =
            DependencyProperty.RegisterAttached("EnableHookLoaded", typeof(bool), typeof(MainWindowHook), new PropertyMetadata(OnHookLoadedChanged));

        private static void OnHookLoadedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var window = (sender as MainWindow);
            if (window == null) return;

            bool on = (bool)e.NewValue;
            if (on) window.Loaded += Window_Loaded;
            else window.Loaded -= Window_Loaded;
        }

        private static void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var mainWindow = (sender as Window);

            var dockSite = LogicalTreeHelper.FindLogicalNode(mainWindow, "dockSite") as DockSite;

            try
            {
                new DockSiteLayoutSerializer().LoadFromFile(dockingLayoutFileName, dockSite);
            }
            catch { }
        }
        #endregion

        #region EnableHookClosing
        public static bool GetEnableHookClosing(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableHookClosingProperty);
        }

        public static void SetEnableHookClosing(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableHookClosingProperty, value);
        }

        // Using a DependencyProperty as the backing store for EnableHookClosing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableHookClosingProperty =
            DependencyProperty.RegisterAttached("EnableHookClosing", typeof(bool), typeof(MainWindowHook), new PropertyMetadata(OnHookClosingChanged));


        private static void OnHookClosingChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var window = (sender as MainWindow);
            if (window == null) return;

            bool on = (bool)e.NewValue;
            if (on) window.Closing += Window_Closing;
            else window.Closing -= Window_Closing;
        }

        private static void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var mainWindow = (sender as Window);

            var mainViewModel = mainWindow.DataContext as MainViewModel;

            var dockSite = LogicalTreeHelper.FindLogicalNode(mainWindow, "dockSite") as DockSite;
            new DockSiteLayoutSerializer().SaveToFile(dockingLayoutFileName, dockSite);
        }

        #endregion
    }
}
