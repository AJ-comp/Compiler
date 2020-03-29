using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.Views.Converters;
using ApplicationLayer.WpfApp.Views.WindowViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ApplicationLayer.WpfApp.Converters
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
