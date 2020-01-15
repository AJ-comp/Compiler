using ApplicationLayer.ViewModels.DialogViewModels;
using ApplicationLayer.WpfApp.Views.DialogViews;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ApplicationLayer.WpfApp.Converters.LogicConverters
{
    class ProjectCreateLogicConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MainWindow parentWindow = value as MainWindow;

            return new Action(() =>
            {
                NewProjectDialog dialog = new NewProjectDialog();
                var vm = dialog.DataContext as NewProjectViewModel;

                dialog.Owner = parentWindow;
                dialog.ShowInTaskbar = false;
                dialog.ShowDialog();
            });
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
