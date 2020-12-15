using Parse;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ApplicationLayer.Views.Converters
{
    public class EnumToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return (parameter.ToString() == "true") ? Helper.GetDescription(value)
                                                                           : Enum.GetName((value.GetType()), value);
            }
            catch
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
