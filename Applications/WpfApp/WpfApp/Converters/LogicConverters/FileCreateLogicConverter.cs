using ApplicationLayer.Models;
using ApplicationLayer.ViewModels.DialogViewModels;
using ApplicationLayer.WpfApp.Views.DialogViews;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ApplicationLayer.WpfApp.Converters.LogicConverters
{
    class FileCreateLogicConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Window parentWindow = value as Window;

            return new Func<Document>(() =>
            {
                NewFileDialog dialog = new NewFileDialog();
                var vm = dialog.DataContext as NewFileDialogViewModel;

                dialog.Owner = parentWindow;
                dialog.ShowInTaskbar = false;
                dialog.ShowDialog();

                return vm.SelectedItem;
            });
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
