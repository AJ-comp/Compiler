using ApplicationLayer.ViewModels.DialogViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ApplicationLayer.WpfApp.Converters.LogicConverters
{
    /// <summary>
    /// This converter has the solution path search logic to generate.
    /// </summary>
    class SolutionPathSearchLogicConverter : MarkupExtension, IValueConverter
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
                    NewSolutionViewModel viewModel = window.DataContext as NewSolutionViewModel;
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


    /// <summary>
    /// This converter has the project path search logic to generate.
    /// </summary>
    class ProjectSearchLogicConverter : MarkupExtension, IValueConverter
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
                    viewModel.ProjectPath = selectFolderDialog.FileName + "\\";
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
