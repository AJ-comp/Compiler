using Parse.WpfControls.SyntaxEditor.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp.Converters
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
