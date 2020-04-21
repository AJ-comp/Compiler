using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Parse.WpfControls.Converters
{
    public class UIntListToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;
            List<uint> src = value as List<uint>;
            if (src == null) return result;

            foreach(var item in src)
                result += item.ToString() + ",";

            return (result.Length > 0) ? result.Substring(0, result.Length - 1) : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
