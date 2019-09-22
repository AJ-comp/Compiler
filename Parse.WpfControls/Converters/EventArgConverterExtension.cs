using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Parse.WpfControls.Converters
{
    public abstract class EventArgConverterExtension<T> : MarkupExtension, IEventArgsConverter where T : class, new()
    {
        private static Lazy<T> _converter = new Lazy<T>(() => new T());

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _converter.Value;
        }

        public abstract object Convert(object value, object parameter);
    }
}
