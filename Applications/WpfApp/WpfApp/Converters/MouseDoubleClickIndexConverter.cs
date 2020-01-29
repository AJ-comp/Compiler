﻿using System.Windows.Input;

namespace ApplicationLayer.WpfApp.Converters
{
    public class MouseDoubleClickIndexConverter : EventArgsConverterExtension<MouseDoubleClickIndexConverter>
    {
        public override object Convert(object value, object parameter)
        {
            var index = (int)parameter;

            if (!(value is MouseButtonEventArgs e)) return null;
            if (e.ClickCount != 2) return null;

            return index;
        }
    }
}