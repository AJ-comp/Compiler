using ApplicationLayer.ViewModels.Interfaces;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ApplicationLayer.WpfApp.Converters.LogicConverters
{
    /// <summary>
    /// This converter has the project path search logic to generate.
    /// </summary>
    class PathSearchLogicConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Window window = value as Window;

            return new Action<string>((initDirectory) =>
            {
                CommonOpenFileDialog selectFolderDialog = new CommonOpenFileDialog
                {
                    InitialDirectory = initDirectory,
                    IsFolderPicker = true
                };

                if (selectFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    IPathSearchable viewModel = window.DataContext as IPathSearchable;
                    viewModel.Path = selectFolderDialog.FileName + "\\";
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
