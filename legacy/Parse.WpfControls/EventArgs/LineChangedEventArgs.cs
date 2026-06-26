using System.Windows;

namespace Janglim.WpfControls.EventArgs
{
    public class LineChangedEventArgs : RoutedEventArgs
    {
        public int AddedLines;
        public int RemovedLines;
    }
}
