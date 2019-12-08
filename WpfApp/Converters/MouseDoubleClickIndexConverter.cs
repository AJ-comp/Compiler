using System.Windows.Input;

namespace WpfApp.Converters
{
    public class MouseDoubleClickIndexConverter : EventArgsConverterExtension<MouseDoubleClickIndexConverter>
    {
        public override object Convert(object value, object parameter)
        {
            var e = value as MouseButtonEventArgs;
            var index = (int)parameter;

            if (e == null) return null;
            if (e.ClickCount != 2) return null;

            return index;
        }
    }
}
