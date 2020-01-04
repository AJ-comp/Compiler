using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using WpfApp.Models;

namespace WpfApp.Converters
{
    public class ToImageByThemeConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;

            if (Theme.Instance.ThemeKind == ThemeKind.Dark)
            {
                if (parameter.ToString() == "Solution") result = "/Resources/Images/DarkTheme/solution.png";
                else if (parameter.ToString() == "Project") result = "/Resources/Images/DarkTheme/project.png";

                else if (parameter.ToString() == "logo24") result = "/Resources/Images/DarkTheme/logo_dark_24.png";
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
