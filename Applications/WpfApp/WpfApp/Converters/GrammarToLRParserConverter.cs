using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.LR;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ApplicationLayer.WpfApp.Converters
{
    class GrammarToLRParserConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Grammar grammar)) return null;

            SLRParser result = new SLRParser(grammar);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
