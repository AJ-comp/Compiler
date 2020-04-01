using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.Views.WindowViews;
using System.Windows;

namespace ApplicationLayer.Views.Converters
{
    class ToHierarchicalDataConverter : EventArgsConverterExtension<ToHierarchicalDataConverter>
    {
        public override object Convert(object value, object parameter)
        {
            if (!(value is RoutedPropertyChangedEventArgs<object> arg) || arg.NewValue == null) return null;

            object result = null;
            if (parameter is SolutionExplorer) result = arg.NewValue as HierarchicalData;

            return result;
        }
    }
}
