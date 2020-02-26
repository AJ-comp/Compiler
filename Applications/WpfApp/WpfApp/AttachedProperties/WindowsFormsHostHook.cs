using System;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;

namespace ApplicationLayer.WpfApp.AttachedProperties
{
    public static class WindowsFormsHostHook
    {
        public static bool GetEnableLoadedHook(DependencyObject obj) => (bool)obj.GetValue(EnableLoadedHookProperty);
        public static void SetEnableLoadedHook(DependencyObject obj, bool value) => obj.SetValue(EnableLoadedHookProperty, value);

        // Using a DependencyProperty as the backing store for EnableLoadedHook.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableLoadedHookProperty =
            DependencyProperty.RegisterAttached("EnableLoadedHook", typeof(bool), typeof(WindowsFormsHostHook), new PropertyMetadata(false, OnEnableLoadedHookChanged));

        private static void OnEnableLoadedHookChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var host = sender as WindowsFormsHost;
            host.Foreground = Brushes.Black;

            if ((bool)e.NewValue) host.Loaded += Host_Loaded;
            else host.Loaded -= Host_Loaded;
        }

        private static void Host_Loaded(object sender, RoutedEventArgs e)
        {
            var command = WindowsFormsHostHook.GetLoadedCommand(sender as WindowsFormsHost);
            if (command == null) return;

            var datasource = WindowsFormsHostHook.GetDataSource(sender as WindowsFormsHost);
            var parameter = WindowsFormsHostHook.GetLoadedCommandParameter(sender as WindowsFormsHost);
            var commandParam = new Tuple<object, object, RoutedEventArgs>(datasource, parameter, e);
            if (command.CanExecute(commandParam) == false) return;

            command.Execute(commandParam);
        }


        public static ICommand GetLoadedCommand(DependencyObject obj) => (ICommand)obj.GetValue(LoadedCommandProperty);
        public static void SetLoadedCommand(DependencyObject obj, ICommand value) => obj.SetValue(LoadedCommandProperty, value);

        // Using a DependencyProperty as the backing store for LoadedCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LoadedCommandProperty =
            DependencyProperty.RegisterAttached("LoadedCommand", typeof(ICommand), typeof(WindowsFormsHostHook), new PropertyMetadata(null));



        public static object GetLoadedCommandParameter(DependencyObject obj) => (object)obj.GetValue(LoadedCommandParameterProperty);
        public static void SetLoadedCommandParameter(DependencyObject obj, object value) => obj.SetValue(LoadedCommandParameterProperty, value);

        // Using a DependencyProperty as the backing store for LoadedCommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LoadedCommandParameterProperty =
            DependencyProperty.RegisterAttached("LoadedCommandParameter", typeof(object), typeof(WindowsFormsHostHook), new PropertyMetadata(null));




        public static object GetDataSource(DependencyObject obj) => (object)obj.GetValue(DataSourceProperty);
        public static void SetDataSource(DependencyObject obj, object value) => obj.SetValue(DataSourceProperty, value);

        // Using a DependencyProperty as the backing store for DataSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.RegisterAttached("DataSource", typeof(object), typeof(WindowsFormsHostHook), new PropertyMetadata(null));
    }
}
