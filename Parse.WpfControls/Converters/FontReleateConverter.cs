﻿using System;
using System.Globalization;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Parse.WpfControls.Converters
{
    public class FontReleateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TextBoxBase obj = value as TextBoxBase;

            

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
