using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Parse.WpfControls.Converters
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
