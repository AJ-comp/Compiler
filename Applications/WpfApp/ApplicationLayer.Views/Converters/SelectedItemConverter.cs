using ApplicationLayer.Common.Utilities;
using ApplicationLayer.Views.SubViews;
using System.Windows;

namespace ApplicationLayer.Views.Converters
{
    class SelectedItemConverter : EventArgsConverterExtension<SelectedItemConverter>
    {
        public override object Convert(object value, object parameter)
        {
            if (!(value is RoutedPropertyChangedEventArgs<object> arg) || arg.NewValue == null) return null;

            object result = null;
            if (parameter is ProjectSelectionView) result = arg.NewValue as ClassHierarchyData;

            return result;
        }
    }
}
