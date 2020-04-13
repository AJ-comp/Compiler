using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.Views.Converters;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ApplicationLayer.WpfApp.Converters
{
    public class FileCheckConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FileTreeNodeModel fileNode = value as FileTreeNodeModel;
            if (fileNode == null) return null;

            return fileNode.IsExistFile;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
