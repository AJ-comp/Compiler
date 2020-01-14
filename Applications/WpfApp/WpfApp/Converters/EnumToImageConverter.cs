using Parse.WpfControls.SyntaxEditor.EventArgs;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ApplicationLayer.WpfApp.Converters
{
    public class EnumToImageConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            AlarmStatus status = (AlarmStatus)value;

            string path = "/Resources/Images/Basic/";

            if (status == AlarmStatus.ParsingError) return path + "error.png";
            else if (status == AlarmStatus.ParsingWarning) return path + "warning.png";

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
