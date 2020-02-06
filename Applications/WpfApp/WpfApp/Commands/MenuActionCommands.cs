using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.ViewModels.DialogViewModels;
using ApplicationLayer.ViewModels.Messages;
using ApplicationLayer.WpfApp.ViewModels;
using ApplicationLayer.WpfApp.Views.DialogViews;
using GalaSoft.MvvmLight.Messaging;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using WPFLocalizeExtension.Engine;

using static ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.WpfApp.Commands
{
    public static class MenuActionCommands
    {
        public static MainWindow parentWindow { get; set; }

        /// <summary>
        /// New Solution Command
        /// </summary>
        public static readonly RelayUICommand CreateNewSolution = new RelayUICommand(Project,
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
        public static readonly RelayUICommand AddNewProject = new RelayUICommand(Project, 
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
        public static readonly RelayUICommand<HierarchicalData> AddNewItem = new RelayUICommand<HierarchicalData>(NewItem,
            (hirStruct) =>
            {
                NewItemDialog dialog = new NewItemDialog();
                var vm = dialog.DataContext as NewItemViewModel;
                vm.CreateRequest += (s, e) =>
                {
                    var fileStruct = vm.SelectedItem.FileStruct(hirStruct);
                    fileStruct.CreateFile();

                    if (hirStruct is DefaultProjectHier)
                    {
                        var hier = hirStruct as DefaultProjectHier;
                        hier.Items.Add(fileStruct);

                        if(hier.IsChanged) Messenger.Default.Send<ChangedFileMessage>(new ChangedFileMessage(hier, ChangedFileMessage.ChangedStatus.Changed));
                    }
                    else if (hirStruct is FolderHier)
                    {
                        var folderHier = hirStruct as FolderHier;
                        var parent = folderHier.ProjectTypeParent;
                        folderHier.Items.Add(fileStruct);

                        if (parent == null) return;
                        if (parent.IsChanged) Messenger.Default.Send<ChangedFileMessage>(new ChangedFileMessage(parent, ChangedFileMessage.ChangedStatus.Changed));
                    }
                };

                dialog.Owner = parentWindow;
                dialog.ShowInTaskbar = false;
                dialog.ShowDialog();

            }, (condition) =>
            {
                var vm = parentWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });

        /// <summary>
        /// Load Existing Item Command
        /// </summary>
        public static readonly RelayUICommand<HierarchicalData> AddExistItem = new RelayUICommand<HierarchicalData>(ExistItem,
            (hirStruct) =>
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "mini C files (*.mc)|*.mc|All files (*.*)|*.*";
                dialog.ShowDialog();

                foreach(var fileName in dialog.FileNames)
                {
                    // If file is not in the current path then copy it to the current path.
                    if(hirStruct.BaseOPath != Path.GetDirectoryName(fileName))
                    {
                        string destPath = Path.Combine(hirStruct.BaseOPath, Path.GetFileName(fileName));
                        if (System.IO.File.Exists(destPath))
                        {
                            DialogResult dResult = MessageBox.Show(AlreadyExistFile, string.Empty, MessageBoxButtons.YesNo);

                            if (dResult == DialogResult.Yes) System.IO.File.Copy(fileName, destPath);
                            else return;
                        }
                        else System.IO.File.Copy(fileName, destPath);
                    }

                    var fileStruct = new DefaultFileHier()
                    {
                        FullName = Path.GetFileName(fileName),
                        Data = System.IO.File.ReadAllText(fileName)
                    };

                    if (hirStruct is DefaultProjectHier) (hirStruct as DefaultProjectHier).Items.Add(fileStruct);
                    else if (hirStruct is FolderHier) (hirStruct as FolderHier).Items.Add(fileStruct);
                }
            }, (condition) =>
            {
                var vm = parentWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });


        /// <summary>
        /// Item delete command
        /// </summary>
        public static readonly RelayUICommand<HierarchicalData> DelItem = new RelayUICommand<HierarchicalData>(Delete,
            (selectedStruct) =>
            {
                HierarchicalData parent = selectedStruct.Parent;
                if (parent == null) return;

                DialogResult dResult = MessageBox.Show(DeleteWarning, string.Empty, MessageBoxButtons.YesNo);

                if (dResult == DialogResult.Yes)
                {
                    try
                    {
                        if (selectedStruct is ProjectHier) Directory.Delete(selectedStruct.BaseOPath, true);
                        else if (selectedStruct is FolderHier) Directory.Delete(selectedStruct.FullPath, true);
                        else System.IO.File.Delete(selectedStruct.FullPath);
                    }
                    catch { }
                }
                else return;

                if (parent is SolutionHier) (parent as SolutionHier).RemoveChild(selectedStruct);
                else if (parent is DefaultProjectHier) (parent as DefaultProjectHier).RemoveChild(selectedStruct);
                else if (parent is FolderHier) (parent as FolderHier).RemoveChild(selectedStruct);

                Messenger.Default.Send<ChangedFileMessage>(new ChangedFileMessage(parent, ChangedFileMessage.ChangedStatus.Changed));
            }, (condition) =>
            {
                var vm = parentWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });


        /// <summary>
        /// New Folder Command
        /// </summary>
        public static readonly RelayUICommand<DefaultProjectHier> AddNewFolder = new RelayUICommand<DefaultProjectHier>(NewFolder,
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

                projectStruct.Folders.Add(new FolderHier() { CurOPath = newFolderName, FullName = newFolderName });
                
            }, (condition) =>
            {
                var vm = parentWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });


        /// <summary>
        /// This command open folder from file explorer
        /// </summary>
        public static readonly RelayUICommand<HierarchicalData> OpenFolder = new RelayUICommand<HierarchicalData>(OpenFolderFromExplorer,
            (hirStruct) =>
            {
                // opens explorer, showing some other folder)
                Process.Start("explorer.exe", hirStruct.BaseOPath);
            }, (condition) =>
            {
                return true;
            });







        /// <summary>
        /// This command open changes character set to Korean.
        /// </summary>
        public static readonly RelayUICommand<HierarchicalData> ChangeToKorean = new RelayUICommand<HierarchicalData>(Korean,
            (hirStruct) =>
            {
                LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
                LocalizeDictionary.Instance.Culture = new CultureInfo("ko-KR");
            }, (condition) =>
            {
                var vm = parentWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });

        /// <summary>
        /// This command open changes character set to English.
        /// </summary>
        public static readonly RelayUICommand<HierarchicalData> ChangeToEnglish = new RelayUICommand<HierarchicalData>(English,
            (hirStruct) =>
            {
                LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
                LocalizeDictionary.Instance.Culture = new CultureInfo("en-US");
            }, (condition) =>
            {
                var vm = parentWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });

        /// <summary>
        /// This command open changes character set to Chinese.
        /// </summary>
        public static readonly RelayUICommand<HierarchicalData> ChangeToChinese = new RelayUICommand<HierarchicalData>(Chinese,
            (hirStruct) =>
            {
                LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
//                LocalizeDictionary.Instance.Culture = new CultureInfo("ko-KR");
            }, (condition) =>
            {
                var vm = parentWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });

        /// <summary>
        /// This command open changes character set to Japanese.
        /// </summary>
        public static readonly RelayUICommand<HierarchicalData> ChangeToJapanese = new RelayUICommand<HierarchicalData>(Japanese,
            (hirStruct) =>
            {
                LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
//                LocalizeDictionary.Instance.Culture = new CultureInfo("ko-KR");
            }, (condition) =>
            {
                var vm = parentWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });
    }
}
