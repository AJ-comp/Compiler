using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.Views.Converters;
using ApplicationLayer.Views.WindowViews;
using ApplicationLayer.WpfApp.Views.WindowViews;
using System.Windows;

namespace ApplicationLayer.WpfApp.Converters
{
    class ToHierarchicalDataConverter : EventArgsConverterExtension<ToHierarchicalDataConverter>
    {
        public override object Convert(object value, object parameter)
        {
            if (!(value is RoutedPropertyChangedEventArgs<object> arg) || arg.NewValue == null) return null;

            object result = null;
            if (parameter is SolutionExplorer) result = arg.NewValue as TreeNodeModel;

            return result;
        }
    }
}
