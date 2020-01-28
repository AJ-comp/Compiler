using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Markup;

namespace ApplicationLayer.WpfApp.Converters.LogicConverters
{
    class OpenSolutionLogicConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Func<string>(() =>
            {
                string result = string.Empty;
                OpenFileDialog selectFolderDialog = new OpenFileDialog
                {
                    Filter = "AJ Solution files (*.ajn)|*.ajn",
                    InitialDirectory = "C:\\Users",
                };

                if (selectFolderDialog.ShowDialog() == DialogResult.OK)
                    result = selectFolderDialog.FileName;

                return result;
            });
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
