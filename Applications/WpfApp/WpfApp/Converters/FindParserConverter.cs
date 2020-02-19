using ApplicationLayer.Common.Utilities;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.LR;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ApplicationLayer.WpfApp.Converters
{
    class FindParserConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Grammar grammar)) return null;

            // If parser already exists this function will nothing do.
            ParserFactory.Instance.RegisterParser(ParserFactory.ParserKind.SLR_Parser, grammar);
            return ParserFactory.Instance.GetParser(ParserFactory.ParserKind.SLR_Parser, grammar);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
