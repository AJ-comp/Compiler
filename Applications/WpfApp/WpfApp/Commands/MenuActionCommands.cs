﻿using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.ViewModels.DialogViewModels;
using ApplicationLayer.ViewModels.Messages;
using ApplicationLayer.WpfApp.ViewModels;
using ApplicationLayer.WpfApp.Views.DialogViews;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using WPFLocalizeExtension.Engine;

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
        public static readonly RelayUICommand<HirStruct> AddNewItem = new RelayUICommand<HirStruct>(Properties.Resources.NewItem,
            (hirStruct) =>
            {
                NewItemDialog dialog = new NewItemDialog();
                var vm = dialog.DataContext as NewItemViewModel;
                vm.CreateRequest += (s, e) =>
                {
                    var fileStruct = vm.SelectedItem.FileStruct(hirStruct);
                    fileStruct.CreateFile();

                    if (hirStruct is DefaultProjectStruct) (hirStruct as DefaultProjectStruct).Items.Add(fileStruct);
                    else if (hirStruct is FolderStruct) (hirStruct as FolderStruct).Items.Add(fileStruct);
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
        public static readonly RelayUICommand<HirStruct> AddExistItem = new RelayUICommand<HirStruct>(Properties.Resources.ExistItem,
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
                        if (File.Exists(destPath))
                        {
                            DialogResult dResult = MessageBox.Show(Properties.Resources.AlreadyExistFile, string.Empty, MessageBoxButtons.YesNo);

                            if (dResult == DialogResult.Yes) File.Copy(fileName, destPath);
                            else return;
                        }
                        else File.Copy(fileName, destPath);
                    }

                    var fileStruct = new DefaultFileStruct()
                    {
                        FullName = Path.GetFileName(fileName),
                        Data = File.ReadAllText(fileName)
                    };

                    if (hirStruct is DefaultProjectStruct) (hirStruct as DefaultProjectStruct).Items.Add(fileStruct);
                    else if (hirStruct is FolderStruct) (hirStruct as FolderStruct).Items.Add(fileStruct);
                }
            }, (condition) =>
            {
                var vm = parentWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });


        /// <summary>
        /// Item delete command
        /// </summary>
        public static readonly RelayUICommand<HirStruct> DelItem = new RelayUICommand<HirStruct>(Properties.Resources.Delete,
            (selectedStruct) =>
            {
                HirStruct parent = selectedStruct.Parent;
                if (parent == null) return;

                DialogResult dResult = MessageBox.Show(Properties.Resources.DeleteWarning, string.Empty, MessageBoxButtons.YesNo);

                if (dResult == DialogResult.Yes)
                {
                    try
                    {
                        if (selectedStruct is ProjectStruct) Directory.Delete(selectedStruct.BaseOPath, true);
                        else if (selectedStruct is FolderStruct) Directory.Delete(selectedStruct.FullPath, true);
                        else File.Delete(selectedStruct.FullPath);
                    }
                    catch { }
                }
                else return;

                if (parent is SolutionStruct) (parent as SolutionStruct).RemoveChild(selectedStruct);
                else if (parent is DefaultProjectStruct) (parent as DefaultProjectStruct).RemoveChild(selectedStruct);
                else if (parent is FolderStruct) (parent as FolderStruct).RemoveChild(selectedStruct);

                Messenger.Default.Send<ChangedFileMessage>(new ChangedFileMessage(parent, ChangedFileMessage.ChangedStatus.Changed));
            }, (condition) =>
            {
                var vm = parentWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });


        /// <summary>
        /// New Folder Command
        /// </summary>
        public static readonly RelayUICommand<DefaultProjectStruct> AddNewFolder = new RelayUICommand<DefaultProjectStruct>(Properties.Resources.NewFolder,
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

                projectStruct.Folders.Add(new FolderStruct() { CurOPath = newFolderName, FullName = newFolderName });
                
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







        /// <summary>
        /// This command open changes character set to Korean.
        /// </summary>
        public static readonly RelayUICommand<HirStruct> ChangeToKorean = new RelayUICommand<HirStruct>(Properties.Resources.OpenFolderFromExplorer,
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
        public static readonly RelayUICommand<HirStruct> ChangeToEnglish = new RelayUICommand<HirStruct>(Properties.Resources.OpenFolderFromExplorer,
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
        public static readonly RelayUICommand<HirStruct> ChangeToChinese = new RelayUICommand<HirStruct>(Properties.Resources.OpenFolderFromExplorer,
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
        public static readonly RelayUICommand<HirStruct> ChangeToJapanese = new RelayUICommand<HirStruct>(Properties.Resources.OpenFolderFromExplorer,
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
