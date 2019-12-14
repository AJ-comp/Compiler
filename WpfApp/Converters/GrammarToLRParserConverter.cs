using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.LR;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace WpfApp.Converters
{
    class GrammarToLRParserConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Grammar grammar = value as Grammar;
            if (grammar == null) return null;

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
