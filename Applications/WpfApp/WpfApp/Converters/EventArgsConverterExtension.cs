using GalaSoft.MvvmLight.Command;
using System;
using System.Windows.Markup;

namespace ApplicationLayer.WpfApp.Converters
{
    public abstract class EventArgsConverterExtension<T> : MarkupExtension, IEventArgsConverter where T : class, new()
    {
        private static Lazy<T> _converter = new Lazy<T>(() => new T());

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _converter.Value;
        }

        public abstract object Convert(object value, object parameter);
    }
}
