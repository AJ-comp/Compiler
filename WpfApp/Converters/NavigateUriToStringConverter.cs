using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace WpfApp.Converters
{
    public class NavigateUriToStringConverter : EventArgsConverterExtension<NavigateUriToStringConverter>
    {
        public override object Convert(object value, object parameter)
        {
            var e = value as RequestNavigateEventArgs;
            var index = (int)parameter;

            if (e == null) return null;

            return e.Uri.AbsoluteUri;
        }
    }
}
