using ApplicationLayer.Common.Utilities;
using System.Windows;

namespace ApplicationLayer.WpfApp.Converters
{
    class SelectedItemConverter : EventArgsConverterExtension<SelectedItemConverter>
    {
        public override object Convert(object value, object parameter)
        {

            if (!(value is RoutedPropertyChangedEventArgs<object> arg) || arg.NewValue == null) return null;

            return arg.NewValue as ClassHierarchyData;
        }
    }
}
