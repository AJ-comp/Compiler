using ApplicationLayer.ViewModels.DialogViewModels;
using ApplicationLayer.WpfApp.ViewModels;
using ApplicationLayer.WpfApp.Views.DialogViews;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Command;
using WpfApp.ViewModels.WindowViewModels;

namespace ApplicationLayer.WpfApp.Commands
{
    public static class MenuActionCommands
    {
        public static MainWindow parentWindow { get; set; }

        /// <summary>
        /// New Solution Command
        /// </summary>
        public static readonly RelayUICommand CreateNewSolution = new RelayUICommand(Properties.Resources.Project,
            () =>
        {
            NewSolutionDialog dialog = new NewSolutionDialog();
            var vm = dialog.DataContext as NewSolutionViewModel;

            dialog.Owner = parentWindow;
            dialog.ShowInTaskbar = false;
            dialog.ShowDialog();
        }, () =>
        {
            var vm = parentWindow.DataContext as MainViewModel;
            return (vm.IsDebugStatus == false);
        });

        /// <summary>
        /// New Project Command
        /// </summary>
        public static readonly RelayUICommand AddNewProject = new RelayUICommand(Properties.Resources.Project, 
            () =>
        {
            NewProjectDialog dialog = new NewProjectDialog();
            var vm = dialog.DataContext as NewProjectViewModel;
            var solutionExplorer = ServiceLocator.Current.GetInstance<SolutionExplorerViewModel>();
            if (solutionExplorer.Solutions.Count == 0) return;
            vm.SolutionFullPath = solutionExplorer.Solutions[0].FullPath;

            dialog.Owner = parentWindow;
            dialog.ShowInTaskbar = false;
            dialog.ShowDialog();
        }, () => 
        {
            var vm = parentWindow.DataContext as MainViewModel;
            return (vm.IsDebugStatus == false);
        });
    }
}
