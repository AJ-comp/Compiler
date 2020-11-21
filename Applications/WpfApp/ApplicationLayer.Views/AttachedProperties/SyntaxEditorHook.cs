using Parse.FrontEnd.Support.EventArgs;
using Parse.WpfControls.SyntaxEditor;
using System.Windows;
using System.Windows.Input;

namespace ApplicationLayer.Views.AttachedProperties
{
    public static class SyntaxEditorHook
    {
        public static bool GetParsingCompletedEventHook(DependencyObject obj)
        {
            return (bool)obj.GetValue(ParsingCompletedEventHookProperty);
        }

        public static void SetParsingCompletedEventHook(DependencyObject obj, bool value)
        {
            obj.SetValue(ParsingCompletedEventHookProperty, value);
        }

        // Using a DependencyProperty as the backing store for ParsingCompletedEventHook.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParsingCompletedEventHookProperty =
            DependencyProperty.RegisterAttached("ParsingCompletedEventHook", typeof(bool), typeof(SyntaxEditorHook), new PropertyMetadata(false, OnParsingCompletedHookChanged));

        private static void OnParsingCompletedHookChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = sender as SyntaxEditor;

            if ((bool)e.NewValue) editor.ParsingCompleted += Editor_ParsingCompleted;
            else editor.ParsingCompleted -= Editor_ParsingCompleted;
        }

        private static void Editor_ParsingCompleted(object sender, ParsingCompletedEventArgs e)
        {
            var command = SyntaxEditorHook.GetParsingCompletedCommand(sender as SyntaxEditor);
            if (command == null) return;

            if (command?.CanExecute(null) == false) return;
            command?.Execute(null);
        }


        public static ICommand GetParsingCompletedCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(ParsingCompletedCommandProperty);
        }

        public static void SetParsingCompletedCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(ParsingCompletedCommandProperty, value);
        }

        // Using a DependencyProperty as the backing store for ParsingCompletedCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParsingCompletedCommandProperty =
            DependencyProperty.RegisterAttached("ParsingCompletedCommand", typeof(ICommand), typeof(SyntaxEditorHook), new PropertyMetadata(null));


    }
}
