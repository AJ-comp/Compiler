using ApplicationLayer.Views.Converters;
using System.Windows.Navigation;

namespace ApplicationLayer.WpfApp.Converters
{
    public class NavigateUriToStringConverter : EventArgsConverterExtension<NavigateUriToStringConverter>
    {
        public override object Convert(object value, object parameter)
        {
            if (!(value is RequestNavigateEventArgs e)) return null;

            var index = (int)parameter;

            return e.Uri.AbsoluteUri;
        }
    }
}
