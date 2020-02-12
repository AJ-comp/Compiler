using ApplicationLayer.ViewModels.CommandArgs;
using ApplicationLayer.WpfApp.Views.WindowViews;
using System.Windows.Input;
using WpfApp.ViewModels.WindowViewModels;

namespace ApplicationLayer.WpfApp.Converters
{
    class RenameHotKeyConverter : EventArgsConverterExtension<RenameHotKeyConverter>
    {
        public override object Convert(object value, object parameter)
        {
            if (!(value is KeyEventArgs arg) || arg == null) return null;

            SolutionExplorerKeyDownArgs result = null;
            if ((parameter is SolutionExplorer) == false) return result;

            var solutionExplorer = parameter as SolutionExplorer;

            if (arg.Key == Key.F2)
            {
                var viewModel = solutionExplorer.DataContext as SolutionExplorerViewModel;
                result = new SolutionExplorerKeyDownArgs(viewModel.SelectedItem, SolutionExplorerKeyDownArgs.PressedKey.F2);
            }
            else if(arg.Key == Key.Escape)
            {
                var viewModel = solutionExplorer.DataContext as SolutionExplorerViewModel;
                result = new SolutionExplorerKeyDownArgs(viewModel.SelectedItem, SolutionExplorerKeyDownArgs.PressedKey.Esc);
            }
            else if (arg.Key == Key.Enter)
            {
                var viewModel = solutionExplorer.DataContext as SolutionExplorerViewModel;
                result = new SolutionExplorerKeyDownArgs(viewModel.SelectedItem, SolutionExplorerKeyDownArgs.PressedKey.Enter);
            }

            return result;
        }
    }
}
