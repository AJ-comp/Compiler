using ApplicationLayer.Views.Converters;
using Janglim.FrontEnd.Support.EventArgs;

namespace ApplicationLayer.WpfApp.Converters
{
    public class RemoveObjectConverter : EventArgsConverterExtension<RemoveObjectConverter>
    {
        public override object Convert(object value, object parameter)
        {
            var param = value as AlarmCollection;

            return param;
        }
    }
}
