using Parse.WpfControls.Utilities;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace ApplicationLayer.Views.Converters
{
    internal class ColorToBrushConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Drawing.Color color)
                return ColorUtility.ToMediaBrush(color);

            return DefaultValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        public SolidColorBrush DefaultValue => System.Windows.Media.Brushes.Fuchsia;
    }
}
