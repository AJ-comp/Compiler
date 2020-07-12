using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ApplicationLayer.WpfApp.AttachedProperties
{
    public static class MainWindowHook
    {
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
            var command = MainWindowHook.GetLoadedCommand(sender as MainWindow);
            if (command == null) return;

            var parameter = MainWindowHook.GetLoadedCommandParameter(sender as MainWindow);
            var commandParam = new Tuple<object, RoutedEventArgs>(parameter, e);
            if (command.CanExecute(commandParam) == false) return;

            command.Execute(commandParam);
        }


        public static ICommand GetLoadedCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(LoadedCommandProperty);
        }

        public static void SetLoadedCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(LoadedCommandProperty, value);
        }

        // Using a DependencyProperty as the backing store for LoadedCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LoadedCommandProperty =
            DependencyProperty.RegisterAttached("LoadedCommand", typeof(ICommand), typeof(MainWindowHook), new PropertyMetadata(null));



        public static object GetLoadedCommandParameter(DependencyObject obj)
        {
            return (object)obj.GetValue(LoadedCommandParameterProperty);
        }

        public static void SetLoadedCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(LoadedCommandParameterProperty, value);
        }

        // Using a DependencyProperty as the backing store for LoadedCommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LoadedCommandParameterProperty =
            DependencyProperty.RegisterAttached("LoadedCommandParameter", typeof(object), typeof(MainWindowHook), new PropertyMetadata(null));

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

        private static void Window_Closing(object sender, CancelEventArgs e)
        {
            var command = MainWindowHook.GetClosingCommand(sender as MainWindow);
            if (command == null) return;

            var parameter = MainWindowHook.GetLoadedCommandParameter(sender as MainWindow);
            var commandParam = new Tuple<object, CancelEventArgs>(parameter, e);
            if (command.CanExecute(commandParam) == false) return;

            command.Execute(commandParam);
        }


        public static ICommand GetClosingCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(ClosingCommandProperty);
        }

        public static void SetClosingCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(ClosingCommandProperty, value);
        }

        // Using a DependencyProperty as the backing store for ClosingCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClosingCommandProperty =
            DependencyProperty.RegisterAttached("ClosingCommand", typeof(ICommand), typeof(MainWindowHook), new PropertyMetadata(null));



        public static object GetClosingCommandParameter(DependencyObject obj)
        {
            return (object)obj.GetValue(ClosingCommandParameterProperty);
        }

        public static void SetClosingCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(ClosingCommandParameterProperty, value);
        }

        // Using a DependencyProperty as the backing store for ClosingCommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClosingCommandParameterProperty =
            DependencyProperty.RegisterAttached("ClosingCommandParameter", typeof(object), typeof(MainWindowHook), new PropertyMetadata(null));

        #endregion
    }
}
