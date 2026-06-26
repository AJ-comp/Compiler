using GalaSoft.MvvmLight.Command;
using System.Windows.Controls;

namespace Janglim.WpfControls.Converters
{
    public class SingleSelectionChangedEventArgsConverter : IEventArgsConverter
    {
        public object Convert(object value, object parameter)
        {
            SelectionChangedEventArgs e = value as SelectionChangedEventArgs;

            return e.AddedItems[0];
        }
    }
}
