using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.ViewModels.DialogViewModels;
using ApplicationLayer.WpfApp.ViewModels;
using ApplicationLayer.WpfApp.Views.DialogViews;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

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

            dialog.Owner = parentWindow;
            dialog.ShowInTaskbar = false;
            dialog.ShowDialog();
        }, () => 
        {
            var vm = parentWindow.DataContext as MainViewModel;
            return (vm.IsDebugStatus == false);
        });

        /// <summary>
        /// New Item Command
        /// </summary>
        public static readonly RelayUICommand AddNewItem = new RelayUICommand(Properties.Resources.NewItem,
            () =>
            {
                NewProjectDialog dialog = new NewProjectDialog();
                var vm = dialog.DataContext as NewProjectViewModel;

                dialog.Owner = parentWindow;
                dialog.ShowInTaskbar = false;
                dialog.ShowDialog();
            }, () =>
            {
                var vm = parentWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });

        /// <summary>
        /// Load Existing Item Command
        /// </summary>
        public static readonly RelayUICommand<ProjectStruct> AddExistItem = new RelayUICommand<ProjectStruct>(Properties.Resources.ExistItem,
            (projectStruct) =>
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "mini C files (*.mc)|*.mc|All files (*.*)|*.*";
                dialog.ShowDialog();

                foreach(var fileName in dialog.FileNames)
                {
//                    fileName.
                }
            }, (condition) =>
            {
                var vm = parentWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });

        /// <summary>
        /// New Folder Command
        /// </summary>
        public static readonly RelayUICommand<ProjectStruct> AddNewFolder = new RelayUICommand<ProjectStruct>(Properties.Resources.NewFolder,
            (projectStruct) =>
            {
                string newFolderName = "New Folder";

                int index = 1;
                while(true)
                {
                    string newFolderFullPath = Path.Combine(projectStruct.BaseOPath, newFolderName);

                    if (Directory.Exists(newFolderFullPath)) newFolderName = "New Folder" + index++;
                    else
                    {
                        Directory.CreateDirectory(newFolderFullPath);
                        break;
                    }
                }

                projectStruct.Folders.Add(new FolderStruct() { FullName = newFolderName });
                
            }, (condition) =>
            {
                var vm = parentWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });


        /// <summary>
        /// This command open folder from file explorer
        /// </summary>
        public static readonly RelayUICommand<HirStruct> OpenFolder = new RelayUICommand<HirStruct>(Properties.Resources.OpenFolderFromExplorer,
            (hirStruct) =>
            {
                // opens explorer, showing some other folder)
                Process.Start("explorer.exe", hirStruct.BaseOPath);
            }, (condition) =>
            {
                return true;
            });
    }
}
