using ApplicationLayer.ViewModels.DialogViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ApplicationLayer.WpfApp.Converters
{
    class NewProjectSearchLogicConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Window window = value as Window;

            return new Action(() =>
            {
                CommonOpenFileDialog selectFolderDialog = new CommonOpenFileDialog
                {
                    InitialDirectory = "C:\\Users",
                    IsFolderPicker = true
                };

                if (selectFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    NewProjectViewModel viewModel = window.DataContext as NewProjectViewModel;
                    viewModel.SolutionPath = selectFolderDialog.FileName + "\\";
                }
            });
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
