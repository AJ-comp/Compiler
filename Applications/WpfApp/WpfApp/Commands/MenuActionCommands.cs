﻿using ActiproSoftware.Windows.Controls.Docking;
using ActiproSoftware.Windows.Controls.Docking.Serialization;
using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.ViewModels.DialogViewModels;
using ApplicationLayer.ViewModels.DockingItemViewModels;
using ApplicationLayer.ViewModels.DocumentTypeViewModels;
using ApplicationLayer.ViewModels.Messages;
using ApplicationLayer.ViewModels.ToolWindowViewModels;
using ApplicationLayer.WpfApp.ViewModels;
using ApplicationLayer.WpfApp.Views.DialogViews;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using WPFLocalizeExtension.Engine;

using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.WpfApp.Commands
{
    public static class MenuActionCommands
    {
        public static MainWindow RootWindow { get; set; }

        private static string visibleToolItems = "VisibleToolItems.layout";
        private static string dockingLayoutFileName = "DeployInformation.layout";

        /// <summary>
        /// New Solution Command
        /// </summary>
        public static readonly RelayUICommand CreateNewSolution = new RelayUICommand(CommonResource.Project,
            () =>
        {
            NewSolutionDialog dialog = new NewSolutionDialog();
            var vm = dialog.DataContext as NewSolutionViewModel;

            dialog.Owner = RootWindow;
            dialog.ShowInTaskbar = false;
            dialog.ShowDialog();
        }, () =>
        {
            var vm = RootWindow.DataContext as MainViewModel;
            return (vm.IsDebugStatus == false);
        });

        /// <summary>
        /// New Project Command
        /// </summary>
        public static readonly RelayUICommand AddNewProject = new RelayUICommand(CommonResource.Project, 
            () =>
        {
            NewProjectDialog dialog = new NewProjectDialog();
            var vm = dialog.DataContext as NewProjectViewModel;

            dialog.Owner = RootWindow;
            dialog.ShowInTaskbar = false;
            dialog.ShowDialog();
        }, () => 
        {
            var vm = RootWindow.DataContext as MainViewModel;
            return (vm.IsDebugStatus == false);
        });

        /// <summary>
        /// New Item Command
        /// </summary>
        public static readonly RelayUICommand<HierarchicalData> AddNewItem = new RelayUICommand<HierarchicalData>(CommonResource.NewItem,
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

                        if(hier.IsChanged) Messenger.Default.Send<AddChangedFileMessage>(new AddChangedFileMessage(hier));
                        else Messenger.Default.Send<RemoveChangedFileMessage>(new RemoveChangedFileMessage(hier));
                    }
                    else if (hirStruct is FolderHier)
                    {
                        var folderHier = hirStruct as FolderHier;
                        var parent = folderHier.ProjectTypeParent;
                        folderHier.Items.Add(fileStruct);

                        if (parent == null) return;

                        if (parent.IsChanged) Messenger.Default.Send<AddChangedFileMessage>(new AddChangedFileMessage(parent));
                        else Messenger.Default.Send<RemoveChangedFileMessage>(new RemoveChangedFileMessage(parent));
                    }
                };

                dialog.Owner = RootWindow;
                dialog.ShowInTaskbar = false;
                dialog.ShowDialog();

            }, (condition) =>
            {
                var vm = RootWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });

        /// <summary>
        /// Load Existing Item Command
        /// </summary>
        public static readonly RelayUICommand<HierarchicalData> AddExistItem = new RelayUICommand<HierarchicalData>(CommonResource.ExistItem,
            (hirStruct) =>
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Filter = "mini C files (*.mc)|*.mc|All files (*.*)|*.*"
                };
                dialog.ShowDialog();

                foreach(var fileName in dialog.FileNames)
                {
                    // If file is not in the current path then copy it to the current path.
                    if(hirStruct.BaseOPath != Path.GetDirectoryName(fileName))
                    {
                        string destPath = Path.Combine(hirStruct.BaseOPath, Path.GetFileName(fileName));
                        if (System.IO.File.Exists(destPath))
                        {
                            DialogResult dResult = System.Windows.Forms.MessageBox.Show(CommonResource.AlreadyExistFile, string.Empty, MessageBoxButtons.YesNo);

                            if (dResult == DialogResult.Yes) System.IO.File.Copy(fileName, destPath);
                            else return;
                        }
                        else System.IO.File.Copy(fileName, destPath);
                    }

                    var fileStruct = new DefaultFileHier(Path.GetFileName(fileName))
                    {
                        Data = File.ReadAllText(fileName)
                    };

                    if (hirStruct is DefaultProjectHier) (hirStruct as DefaultProjectHier).Items.Add(fileStruct);
                    else if (hirStruct is FolderHier) (hirStruct as FolderHier).Items.Add(fileStruct);
                }
            }, (condition) =>
            {
                var vm = RootWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });


        /// <summary>
        /// Item delete command
        /// </summary>
        public static readonly RelayUICommand<HierarchicalData> DelItem = new RelayUICommand<HierarchicalData>(CommonResource.Delete,
            (selectedStruct) =>
            {
                HierarchicalData parent = selectedStruct.Parent;
                if (parent == null) return;

                DialogResult dResult = System.Windows.Forms.MessageBox.Show(CommonResource.DeleteWarning, string.Empty, MessageBoxButtons.YesNo);

                if (dResult == DialogResult.Yes)
                {
                    try
                    {
                        if (selectedStruct is ProjectHier) Directory.Delete(selectedStruct.BaseOPath, true);
                        else if (selectedStruct is FolderHier) Directory.Delete(selectedStruct.FullPath, true);
                        else File.Delete(selectedStruct.FullPath);
                    }
                    catch { }
                }
                else return;

                if (parent is SolutionHier)
                {
                    var solutionHier = parent as SolutionHier;

                    solutionHier.RemoveChild(selectedStruct);
                    if (solutionHier.IsChanged) Messenger.Default.Send<AddChangedFileMessage>(new AddChangedFileMessage(parent as SolutionHier));
                    else Messenger.Default.Send<RemoveChangedFileMessage>(new RemoveChangedFileMessage(parent as SolutionHier));
                }
                else if (parent is DefaultProjectHier)
                {
                    var projectHier = parent as DefaultProjectHier;
                    projectHier.RemoveChild(selectedStruct);

                    if (projectHier.IsChanged) Messenger.Default.Send<AddChangedFileMessage>(new AddChangedFileMessage(parent as DefaultProjectHier));
                    else Messenger.Default.Send<RemoveChangedFileMessage>(new RemoveChangedFileMessage(parent as DefaultProjectHier));
                }
                else if (parent is FolderHier)
                {
                    var folderHier = parent as FolderHier;
                    folderHier.RemoveChild(selectedStruct);

                    var projectParent = folderHier.ProjectTypeParent;
                    if (projectParent == null) return;

                    if (projectParent.IsChanged) Messenger.Default.Send<AddChangedFileMessage>(new AddChangedFileMessage(projectParent));
                    else Messenger.Default.Send<RemoveChangedFileMessage>(new RemoveChangedFileMessage(projectParent));
                }
            }, (condition) =>
            {
                var vm = RootWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });


        /// <summary>
        /// New Folder Command
        /// </summary>
        public static readonly RelayUICommand<HierarchicalData> AddNewFolder = new RelayUICommand<HierarchicalData>(CommonResource.NewFolder,
            (selHier) =>
            {
                string newFolderName = "New Folder";

                int index = 1;
                while(true)
                {
                    string newFolderFullPath = Path.Combine(selHier.BaseOPath, newFolderName);

                    if (Directory.Exists(newFolderFullPath)) newFolderName = "New Folder" + index++;
                    else
                    {
                        Directory.CreateDirectory(newFolderFullPath);
                        break;
                    }
                }

                if (selHier is DefaultProjectHier)
                {
                    var projectHier = selHier as DefaultProjectHier;
                    projectHier.Folders.Add(new FolderHier(newFolderName));

                    if (projectHier.IsChanged) Messenger.Default.Send<AddChangedFileMessage>(new AddChangedFileMessage(projectHier));
                    else Messenger.Default.Send<RemoveChangedFileMessage>(new RemoveChangedFileMessage(projectHier));
                }
                else if (selHier is FolderHier)
                {
                    var folderHier = selHier as FolderHier;
                    var projectHier = folderHier.ProjectTypeParent;

                    folderHier.Folders.Add(new FolderHier(newFolderName));

                    if (projectHier is null) return;

                    if (projectHier.IsChanged) Messenger.Default.Send<AddChangedFileMessage>(new AddChangedFileMessage(projectHier));
                    else Messenger.Default.Send<RemoveChangedFileMessage>(new RemoveChangedFileMessage(projectHier));
                }
                
            }, (condition) =>
            {
                var vm = RootWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });


        /// <summary>
        /// This command open folder from file explorer
        /// </summary>
        public static readonly RelayUICommand<HierarchicalData> OpenFolder = new RelayUICommand<HierarchicalData>(CommonResource.OpenFolderFromExplorer,
            (hirStruct) =>
            {
                // opens explorer, showing some other folder)
                Process.Start("explorer.exe", hirStruct.BaseOPath);
            }, (condition) =>
            {
                return true;
            });







        /// <summary>
        /// This command shows parsing history for selected document.
        /// </summary>
        public static readonly RelayUICommand ParsingHistory = new RelayUICommand(CommonResource.ParsingHistory,
            () =>
            {
                var mainViewModel = RootWindow.DataContext as MainViewModel;

//                mainViewModel.Documents.Add(new ParsingHistoryViewModel());
            }, () =>
            {
                return true;
            });


        /// <summary>
        /// This command is executed if docking windows were closed.
        /// </summary>
        public static readonly RelayUICommand<Tuple<object, DockingWindowsEventArgs>> DockingWindowClosed = 
            new RelayUICommand<Tuple<object, DockingWindowsEventArgs>>(CommonResource.Close, (eventArgs) =>
            {
                if (eventArgs == null) return;
                if (eventArgs.Item1 == null) return;

                var mainWindow = eventArgs.Item1 as MainWindow;
                var mainViewModel = mainWindow.DataContext as MainViewModel;

                List<DockingItemViewModel> result = new List<DockingItemViewModel>();
                foreach (var window in eventArgs.Item2.Windows)
                {
                    result.Add(window.DataContext as DockingItemViewModel);
                }

                foreach (var item in result)
                {
                    if (item is DocumentViewModel) mainViewModel.SolutionExplorer.Documents.Remove(item as DocumentViewModel);
                    else if (item is ToolWindowViewModel) mainViewModel.VisibleToolItems.Remove(item as ToolWindowViewModel);
                }
            }, (condition) =>
            {
                return true;
            });


        /// <summary>
        /// This command is executed after the main window is loaded.
        /// </summary>
        public static readonly RelayUICommand<Tuple<object, RoutedEventArgs>> MainWindowLoaded = new RelayUICommand<Tuple<object, RoutedEventArgs>>(string.Empty,
            (e) =>
            {
                RootWindow = e.Item1 as MainWindow;
                if (RootWindow == null) return;
                var mainViewModel = RootWindow.DataContext as MainViewModel;

                try
                {
                    var dockSite = LogicalTreeHelper.FindLogicalNode(RootWindow, "dockSite") as DockSite;
                    new DockSiteLayoutSerializer().LoadFromFile(dockingLayoutFileName, dockSite);
                }
                catch { }

                try
                {
                    var toolItems = File.ReadAllText(visibleToolItems);
                    foreach (var toolItemId in toolItems.Split(new string[] { Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries))
                    {
                        foreach (var toolItem in mainViewModel.AllToolItems)
                        {
                            if (toolItem.SerializationId == toolItemId) mainViewModel.VisibleToolItems.Add(toolItem);
                        }
                    }
                }
                catch { }
            }, (condition) =>
            {
                return true;
            });

        /// <summary>
        /// This command is executed before the main window is closed.
        /// </summary>
        public static readonly RelayUICommand<Tuple<object, CancelEventArgs>> MainWindowClosing = 
            new RelayUICommand<Tuple<object, CancelEventArgs>>(CommonResource.Close, (e) =>
            {
                if (RootWindow == null) return;
                var mainViewModel = RootWindow.DataContext as MainViewModel;

                if (mainViewModel.IsDebugStatus)
                {
                    // You have to implement the logic that exit from debug mode.
                }
                else
                {
                    var answer = SolutionExplorerViewModel.CheckChangedFiles();

                    if (answer?.ResultStatus == ShowSaveDialogMessage.Result.Cancel)
                    {
                        e.Item2.Cancel = true;
                        return;
                    }
                }

                string content = string.Empty;
                foreach (var item in mainViewModel.VisibleToolItems)
                    content += item.SerializationId + Environment.NewLine;
                File.WriteAllText(visibleToolItems, content);

                var dockSite = LogicalTreeHelper.FindLogicalNode(RootWindow, "dockSite") as DockSite;
                new DockSiteLayoutSerializer().SaveToFile(dockingLayoutFileName, dockSite);
            }, (condition) =>
            {
                return true;
            });




        /// <summary>
        /// This command open changes character set to Korean.
        /// </summary>
        public static readonly RelayUICommand ChangeToKorean = new RelayUICommand(CommonResource.Korean,
            () =>
            {
                LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
                LocalizeDictionary.Instance.Culture = new CultureInfo("ko-KR");
            }, () =>
            {
                var vm = RootWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });

        /// <summary>
        /// This command open changes character set to English.
        /// </summary>
        public static readonly RelayUICommand ChangeToEnglish = new RelayUICommand(CommonResource.English,
            () =>
            {
                LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
                LocalizeDictionary.Instance.Culture = new CultureInfo("en-US");
            }, () =>
            {
                var vm = RootWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });

        /// <summary>
        /// This command open changes character set to Chinese.
        /// </summary>
        public static readonly RelayUICommand ChangeToChinese = new RelayUICommand(CommonResource.Chinese,
            () =>
            {
                LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
//                LocalizeDictionary.Instance.Culture = new CultureInfo("ko-KR");
            }, () =>
            {
                var vm = RootWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });

        /// <summary>
        /// This command open changes character set to Japanese.
        /// </summary>
        public static readonly RelayUICommand ChangeToJapanese = new RelayUICommand(CommonResource.Japanese,
            () =>
            {
                LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
//                LocalizeDictionary.Instance.Culture = new CultureInfo("ko-KR");
            }, () =>
            {
                var vm = RootWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });
    }
}
