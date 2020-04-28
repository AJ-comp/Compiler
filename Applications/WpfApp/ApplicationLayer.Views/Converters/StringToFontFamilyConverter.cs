using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ApplicationLayer.Views.Converters
{
    public class StringToFontFamilyConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var name = value as FontFamily;
                return new FontFamily(name.Name);
            }
            catch(Exception ex)
            {
                return new FontFamily("Arial");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var fontFamily = value as FontFamily;
                return fontFamily.Name;
            }
            catch
            {
                return string.Empty;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
