using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace ApplicationLayer.Views.Converters
{
    class DataContextTestConverter : EventArgsConverterExtension<DataContextTestConverter>
    {
        public override object Convert(object value, object parameter)
        {
            Control val = value as Control;

            object p = val.DataContext;

            return null;
        }
    }
}
