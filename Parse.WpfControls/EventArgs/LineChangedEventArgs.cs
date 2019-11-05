using System.Windows;

namespace Parse.WpfControls.EventArgs
{
    public class LineChangedEventArgs : RoutedEventArgs
    {
        public int AddedLines;
        public int RemovedLines;
    }
}
