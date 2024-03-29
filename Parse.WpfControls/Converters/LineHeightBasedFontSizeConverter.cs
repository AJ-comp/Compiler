﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Parse.WpfControls.Converters
{
    public class LineHeightBasedFontSizeConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double fontSize = System.Convert.ToDouble(value.ToString());

            return fontSize * 1.3;
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
