﻿using ApplicationLayer.Common.Utilities;
using System.Windows;

namespace WpfApp.Converters
{
    class SelectedItemConverter : EventArgsConverterExtension<SelectedItemConverter>
    {
        public override object Convert(object value, object parameter)
        {
            var arg = value as RoutedPropertyChangedEventArgs<object>;

            if (arg == null || arg.NewValue == null) return null;

            return arg.NewValue as ClassHierarchyData;
        }
    }
}
