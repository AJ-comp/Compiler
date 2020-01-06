using Parse.BackEnd.Target;
using Parse.BackEnd.Target.ARMv7.MSeries.CortexM3Models.Stm32L1Series;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using WpfApp.Models.MicroControllerModels.ArmModels;

namespace WpfApp.Converters
{
    public class GroupByType : IValueConverter
    {
        public string type { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (type == "root")
            {
                if (value is ITarget) return "Target";

                return null;
            }

            if (type == "subs")
            {
                if (value is Parse.BackEnd.Target.ARM) return "ARM";
            }

            if (type == "main")
            {
                if (value is Stm32L152RBT6) return "PBook";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
