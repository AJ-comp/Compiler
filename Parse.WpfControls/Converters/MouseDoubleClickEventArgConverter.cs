using GalaSoft.MvvmLight.Command;
using System;
using System.Windows.Forms;
using System.Windows.Input;

namespace Parse.WpfControls.Converters
{
    public class MouseDoubleClickEventArgConverter : IEventArgsConverter
    {
        public object Convert(object value, object parameter)
        {
            MouseButtonEventArgs e = value as MouseButtonEventArgs;

            return (e.ClickCount >= 2);
        }
    }
}
