using ActiproSoftware.Windows.Controls.Docking;
using ActiproSoftware.Windows.Controls.Docking.Serialization;
using ApplicationLayer.Common;
using ApplicationLayer.Common.Helpers;
using ApplicationLayer.Common.Interfaces;
using ApplicationLayer.Common.Utilities;
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
        /// Add New Project Command
        /// </summary>
        public static readonly RelayUICommand<SolutionTreeNodeModel> AddNewProject = new RelayUICommand<SolutionTreeNodeModel>(CommonResource.NewProject, 
            (solutionNode) =>
        {
            NewProjectDialog dialog = new NewProjectDialog();
            var vm = dialog.DataContext as NewProjectViewModel;
            vm.Path = solutionNode.FullOnlyPath;

            dialog.Owner = RootWindow;
            dialog.ShowInTaskbar = false;
            dialog.ShowDialog();
        }, (condition) => 
        {
            var vm = RootWindow.DataContext as MainViewModel;
            return (vm.IsDebugStatus == false);
        });

        /// <summary>
        /// Add Exist Project Command
        /// </summary>
        public static readonly RelayUICommand<SolutionTreeNodeModel>AddExistProject = new RelayUICommand<SolutionTreeNodeModel>(CommonResource.ExistProject, 
            (solutionNode) =>
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Filter = string.Format("{0} {1}", CommonResource.AllProjectFiles, "(*.mcproj;*.ajproj;)|*.mcproj;.ajproj;")
                };
                dialog.ShowDialog();

                foreach(var fullPath in dialog.FileNames)
                {
                    // If file is not in the solution path then the project path is absolute.
                    var projPath = Path.GetDirectoryName(fullPath);
                    var fileName = Path.GetFileName(fullPath);
                    bool isAbsolute = (PathHelper.ComparePath(solutionNode.Path, projPath) == false);
                    var path = (isAbsolute) ? projPath : projPath.Substring(projPath.IndexOf(solutionNode.Path) + solutionNode.Path.Length + 1);

                    var projectTreeNode = ProjectTreeNodeModel.CreateProjectTreeNodeModel(solutionNode.Path, new PathInfo(path, fileName));
                    if (projectTreeNode == null)
                        projectTreeNode = new ErrorProjectTreeNodeModel(path, fileName);

                    solutionNode.AddProject(projectTreeNode);

                    if(solutionNode.IsChanged) Messenger.Default.Send<AddChangedFileMessage>(new AddChangedFileMessage(solutionNode));
                    else Messenger.Default.Send<RemoveChangedFileMessage>(new RemoveChangedFileMessage(solutionNode));
                }
            }, (condition) =>
            {
                var vm = RootWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });


        /// <summary>
        /// New Item Command
        /// </summary>
        public static readonly RelayUICommand<TreeNodeModel> AddNewItem = new RelayUICommand<TreeNodeModel>(CommonResource.NewItem,
            (selectedNode) =>
            {
                if ((selectedNode is IManagableElements) == false) return;

                NewFileDialog dialog = new NewFileDialog();
                var vm = dialog.DataContext as NewFileDialogViewModel;
                vm.Path = selectedNode.FullOnlyPath;

                vm.CreateRequest += (s, e) =>
                {
                    FileExtend.CreateFile(vm.Path, vm.FileName);
                    var newFileNode = TreeNodeCreator.CreateFileTreeNodeModel(vm.Path, selectedNode.FullOnlyPath, vm.FileName);

                    selectedNode.AddChildren(newFileNode);
                    var managableNode = selectedNode as IManagableElements;
                    if (managableNode.IsChanged) Messenger.Default.Send<AddChangedFileMessage>(new AddChangedFileMessage(managableNode));
                    else Messenger.Default.Send<RemoveChangedFileMessage>(new RemoveChangedFileMessage(managableNode));
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
        public static readonly RelayUICommand<TreeNodeModel> AddExistItem = new RelayUICommand<TreeNodeModel>(CommonResource.ExistItem,
            (selectedNode) =>
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Filter = string.Format("{0} {1}|{2} {3}", CommonResource.MiniCFile, "(*.mc)|*.mc", 
                                                                                CommonResource.AllFiles, "(*.*)|*.*")
                };
                dialog.ShowDialog();


                foreach (var fullPath in dialog.FileNames)
                {
                    var filePath = Path.GetDirectoryName(fullPath);
                    var fileName = Path.GetFileName(fullPath);
                    bool isAbsolute = (PathHelper.ComparePath(selectedNode.FullOnlyPath, filePath) == false);

                    // If file is not in the current path then copy it into the current path.
                    if (isAbsolute)
                    {
                        string destPath = Path.Combine(selectedNode.FullOnlyPath, fileName);
                        if (File.Exists(destPath))
                        {
                            DialogResult dResult = System.Windows.Forms.MessageBox.Show(CommonResource.AlreadyExistFile, string.Empty, MessageBoxButtons.YesNo);

                            if (dResult == DialogResult.Yes) File.Copy(fullPath, destPath);
                            else return;
                        }
                        else File.Copy(fullPath, destPath);
                    }

                    var fileNode = TreeNodeCreator.CreateFileTreeNodeModel(filePath, selectedNode.FullOnlyPath, fileName);
                    fileNode.Data = File.ReadAllText(fullPath);

                    selectedNode.AddChildren(fileNode);

                    ChangedFileMessage(selectedNode);
                }

            }, (condition) =>
            {
                var vm = RootWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });


        private static void ChangedFileMessage(TreeNodeModel node)
        {
            var manager = TreeNodeHelper.GetNearestManager(node);
            if (manager == null) return;

            if (manager.IsChanged) Messenger.Default.Send<AddChangedFileMessage>(new AddChangedFileMessage(manager));
            else Messenger.Default.Send<RemoveChangedFileMessage>(new RemoveChangedFileMessage(manager));
        }


        private static void RemoveItem(TreeNodeModel target)
        {
            TreeNodeModel parent = target.Parent;
            if (parent == null) return;

            // disconnect a connection of the tree node.
            parent.RemoveChild(target);

            ChangedFileMessage(parent);
        }

        /// <summary>
        /// Item Unload command
        /// </summary>
        public static readonly RelayUICommand<TreeNodeModel> Remove = new RelayUICommand<TreeNodeModel>(CommonResource.Remove,
            (selectedNode) =>
            {
                TreeNodeModel parent = selectedNode.Parent;
                if (parent == null) return;

                DialogResult dResult = System.Windows.Forms.MessageBox.Show(CommonResource.RemoveWarning, string.Empty, MessageBoxButtons.YesNo);

                if (dResult == DialogResult.Yes)
                {
                    RemoveItem(selectedNode);
                }
                else return;

            }, (condition) =>
            {
                var vm = RootWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });

        /// <summary>
        /// Item delete command
        /// </summary>
        public static readonly RelayUICommand<TreeNodeModel> DelItem = new RelayUICommand<TreeNodeModel>(CommonResource.Delete,
            (selectedNode) =>
            {
                TreeNodeModel parent = selectedNode.Parent;
                if (parent == null) return;

                DialogResult dResult = System.Windows.Forms.MessageBox.Show(CommonResource.DeleteWarning, string.Empty, MessageBoxButtons.YesNo);

                if (dResult == DialogResult.Yes)
                {
                    // remove the real file.
                    try
                    {
                        if (selectedNode is ProjectTreeNodeModel)
                        {
                            var nodeToDel = selectedNode as ProjectTreeNodeModel;
                            Directory.Delete(Path.Combine(nodeToDel.FullOnlyPath, nodeToDel.FileName), true);
                        }
                        else if (selectedNode is FolderTreeNodeModel)
                        {
                            var nodeToDel = selectedNode as FolderTreeNodeModel;
                            Directory.Delete(Path.Combine(nodeToDel.FullOnlyPath, nodeToDel.FileName), true);
                        }
                        else if (selectedNode is FileTreeNodeModel)
                        {
                            var nodeToDel = selectedNode as FileTreeNodeModel;
                            File.Delete(Path.Combine(nodeToDel.FullOnlyPath, nodeToDel.FileName));
                        }
                    }
                    catch { }
                }
                else return;

                RemoveItem(selectedNode);

            }, (condition) =>
            {
                var vm = RootWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });


        /// <summary>
        /// Item rename command
        /// </summary>
        public static readonly RelayUICommand<TreeNodeModel> Rename = new RelayUICommand<TreeNodeModel>(CommonResource.Rename,
            (selectedNode) =>
            {
                if (selectedNode.IsEditable) selectedNode.IsEditing = true;
            }, (condition) =>
            {
                var vm = RootWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });


        /// <summary>
        /// Add a new filter command
        /// </summary>
        public static readonly RelayUICommand<TreeNodeModel> AddNewFilter = new RelayUICommand<TreeNodeModel>(CommonResource.NewFilter,
            (selectedNode) =>
            {
                var filterName = TreeNodeCreator.CreateFilterTreeNodeModel(selectedNode, "Filter");
                selectedNode.Children.Add(filterName);

                ChangedFileMessage(selectedNode);
            }, (condition) =>
            {
                var vm = RootWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });


        /// <summary>
        /// Add a new folder command
        /// </summary>
        public static readonly RelayUICommand<TreeNodeModel> AddNewFolder = new RelayUICommand<TreeNodeModel>(CommonResource.NewFolder,
            (selHier) =>
            {
                string newFolderName = "New Folder";

                /*
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
                */
                
            }, (condition) =>
            {
                var vm = RootWindow.DataContext as MainViewModel;
                return (vm.IsDebugStatus == false);
            });


        /// <summary>
        /// This command open folder from file explorer
        /// </summary>
        public static readonly RelayUICommand<TreeNodeModel> OpenFolder = new RelayUICommand<TreeNodeModel>(CommonResource.OpenFolderFromExplorer,
            (treeNode) =>
            {
                // opens explorer, showing some other folder)
                Process.Start("explorer.exe", treeNode.FullOnlyPath);
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
//                if ((docViewModel is EditorTypeViewModel) == false) return;
                var mainViewModel = RootWindow.DataContext as MainViewModel;

                var selDoc = mainViewModel.SolutionExplorer.SelectedDocument;
                if ((selDoc is EditorTypeViewModel) == false) return;
                var editorViewModel = mainViewModel.SolutionExplorer.SelectedDocument as EditorTypeViewModel;
                var parsingHistoryDoc = new ParsingHistoryViewModel(editorViewModel.ParsingHistory, selDoc.ToolTipText);
                mainViewModel.SolutionExplorer.Documents.Add(parsingHistoryDoc);
                mainViewModel.SolutionExplorer.SelectedDocument = parsingHistoryDoc;
            }, () =>
            {
                return true;
            });


        /// This command shows parse tree for selected document.
        public static readonly RelayUICommand ShowParseTreeCommand = new RelayUICommand(CommonResource.ParseTree, 
            () =>
            {
                //                if ((docViewModel is EditorTypeViewModel) == false) return;
                var mainViewModel = RootWindow.DataContext as MainViewModel;

                var selDoc = mainViewModel.SolutionExplorer.SelectedDocument;
                if ((selDoc is EditorTypeViewModel) == false) return;
                var editorViewModel = mainViewModel.SolutionExplorer.SelectedDocument as EditorTypeViewModel;
                var parseTreeDoc = new ParseTreeViewModel(editorViewModel.ParseTree, selDoc.ToolTipText);
                mainViewModel.SolutionExplorer.Documents.Add(parseTreeDoc);
                mainViewModel.SolutionExplorer.SelectedDocument = parseTreeDoc;
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
                    var content = File.ReadAllText(dockingLayoutFileName);
                    var serializer = new DockSiteLayoutSerializer();
                    serializer.LoadFromString(content, dockSite);

                    var data = dockSite.ToolItemsSource;
                }
                catch { }

                try
                {
                    var toolItems = File.ReadAllText(visibleToolItems);
                    foreach (var toolItemId in toolItems.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
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
                var serializer = new DockSiteLayoutSerializer
                {
                    SerializationBehavior = DockSiteSerializationBehavior.ToolWindowsOnly
                };
                serializer.SaveToFile(dockingLayoutFileName, dockSite);
            }, (condition) =>
            {
                return true;
            });



        public static readonly RelayUICommand<Tuple<object, DockingWindowEventArgs>> PrimaryDocumentCommand =
            new RelayUICommand<Tuple<object, DockingWindowEventArgs>>(string.Empty, (eventArgs) =>
            {
                if (eventArgs == null) return;
                if (eventArgs.Item1 == null) return;
                if (eventArgs.Item2.Window == null) return;

                var mainVm = eventArgs.Item1 as MainViewModel;
                mainVm.SolutionExplorer.SelectedDocument = eventArgs.Item2.Window.DataContext as DocumentViewModel;

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
