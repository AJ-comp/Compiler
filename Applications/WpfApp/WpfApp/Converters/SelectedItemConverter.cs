using ApplicationLayer.Common.Utilities;
using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.WpfApp.Views.SubViews;
using ApplicationLayer.WpfApp.Views.WindowViews;
using System.Windows;

namespace ApplicationLayer.WpfApp.Converters
{
    class SelectedItemConverter : EventArgsConverterExtension<SelectedItemConverter>
    {
        public override object Convert(object value, object parameter)
        {
            if (!(value is RoutedPropertyChangedEventArgs<object> arg) || arg.NewValue == null) return null;

            object result = null;
            if (parameter is ProjectSelectionView) result = arg.NewValue as ClassHierarchyData;
            else if (parameter is SolutionExplorer) result = arg.NewValue as HierarchicalData;

            return result;
        }
    }

    class ItemRightClickConverter : EventArgsConverterExtension<ItemRightClickConverter>
    {
        public override object Convert(object value, object parameter)
        {
            //MouseButtonEventArgs
            throw new System.NotImplementedException();
        }
    }
}
