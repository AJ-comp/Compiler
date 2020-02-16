using ApplicationLayer.Common.Helpers;
using ApplicationLayer.Common.Interfaces;
using ApplicationLayer.Models;
using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.ViewModels.CommandArgs;
using ApplicationLayer.ViewModels.Messages;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;

using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.ViewModels.ToolWindowViewModels
{
    public class SolutionExplorerViewModel : ToolWindowViewModel
    {
        private ShowSaveDialogMessage saveMessage;
        private IMessageBoxService messageBoxService;

        public ObservableCollection<SolutionHier> Solutions { get; } = new ObservableCollection<SolutionHier>();

        public HierarchicalData SelectedItem { get; private set; }

        private RelayCommand<HierarchicalData> selectedCommand;
        public RelayCommand<HierarchicalData> SelectedCommand
        {
            get
            {
                if (this.selectedCommand == null)
                    this.selectedCommand = new RelayCommand<HierarchicalData>(this.OnSelected);

                return this.selectedCommand;
            }
        }
        private void OnSelected(HierarchicalData selectedItem)
        {
            this.SelectedItem = selectedItem;

            if(selectedItem is DefaultFileHier)
            {
                Messenger.Default.Send(new OpenFileMessage(selectedItem as DefaultFileHier));
            }
        }

        private RelayCommand doubleClickCommand;
        public RelayCommand DoubleClickCommand
        {
            get
            {
                if (this.doubleClickCommand == null)
                    this.doubleClickCommand = new RelayCommand(this.OnMouseDoubleClick);

                return this.doubleClickCommand;
            }
        }
        private void OnMouseDoubleClick()
        {

        }

        private RelayCommand existItemCommand;
        public RelayCommand ExistItemCommand
        {
            get
            {
                if (this.existItemCommand == null)
                    this.existItemCommand = new RelayCommand(this.OnExistItemMenuClick);

                return this.existItemCommand;
            }
        }
        private void OnExistItemMenuClick()
        {
        }

        private RelayCommand<HierarchicalData> newFolderCommand;
        public RelayCommand<HierarchicalData> NewFolderCommand
        {
            get
            {
                if (this.newFolderCommand == null)
                    this.newFolderCommand = new RelayCommand<HierarchicalData>(this.OnNewFolderMenuClick);

                return this.newFolderCommand;
            }
        }
        private void OnNewFolderMenuClick(HierarchicalData selectedItem)
        {

        }

        private RelayCommand<SolutionExplorerKeyDownArgs> keydownCommand;
        public RelayCommand<SolutionExplorerKeyDownArgs> KeyDownCommand
        {
            get
            {
                if (this.keydownCommand == null)
                    this.keydownCommand = new RelayCommand<SolutionExplorerKeyDownArgs>(this.OnRename);

                return this.keydownCommand;
            }
        }
        private void OnRename(SolutionExplorerKeyDownArgs target)
        {
            if (target == null) return;

            if (target.Key == SolutionExplorerKeyDownArgs.PressedKey.F2) target.Item.IsEditMode = true;
            else if (target.Key == SolutionExplorerKeyDownArgs.PressedKey.Esc)
            {
                target.Item.IsEditMode = false;

                target.Item.CancelChangeDisplayName();
            }
            else if (target.Key == SolutionExplorerKeyDownArgs.PressedKey.Enter)
            {
                ExceptionData exceptData = target.Item.IsValidToChange();
                if(exceptData == null)
                {
                    target.Item.IsEditMode = false;
                    target.Item.ChangeDisplayName();
                }
                else if(exceptData.Kind == ExceptionKind.Error)
                    messageBoxService.ShowError(exceptData.Message, string.Empty);
                else if(exceptData.Kind == ExceptionKind.Warning)
                    messageBoxService.ShowWarning(exceptData.Message, string.Empty);
            }
        }

        public SolutionExplorerViewModel(IMessageBoxService messageBoxService)
        {
            this.messageBoxService = messageBoxService;
            this.Solutions.CollectionChanged += Solutions_CollectionChanged;

            this.DefaultDockSide = Models.ToolWindowStatus.ToolItemDockSide.Right;
            this.State = Models.ToolWindowStatus.ToolItemState.Docked;
            this.Title = CommonResource.SolutionExplorer;
        }

        private void Solutions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null) return;

            for (int i = e.NewStartingIndex; i < e.NewItems.Count; i++)
            {
                SolutionHier child = e.NewItems[i] as SolutionHier;

                if (File.Exists(child.FullPath)) continue;

                Directory.CreateDirectory(child.CurOPath);
            }
        }

        /// <summary>
        /// Message handler for CreateSolutionMessage
        /// </summary>
        /// <param name="message"></param>
        public void ReceivedCreateSolutionMessage(CreateSolutionMessage message)
        {
            if (message is null) return;

            var solution = SolutionHier.Create(message.SolutionPath, message.SoltionName, message.Language, message.MachineTarget);

            solution.Save();
            foreach (var project in solution.Projects) project.Save();

            this.Solutions.Add(solution);
        }

        private void PreprocessChangedFileList(Collection<ISaveAndChangeTrackable> hirStructs)
        {
            if(hirStructs.Count > 0)
            {
                saveMessage = new ShowSaveDialogMessage();
                Messenger.Default.Send<ShowSaveDialogMessage>(saveMessage);

                if (saveMessage.ResultStatus == ShowSaveDialogMessage.Result.Yes)
                {
                    foreach (var item in hirStructs)
                    {
                        item.Commit();
                        item.Save();
                    }

                    Messenger.Default.Send<RemoveChangedFileMessage>(null);
                }
                else if (saveMessage.ResultStatus == ShowSaveDialogMessage.Result.No)
                {
                    foreach (var item in hirStructs)
                    {
                        item.RollBack();
                        item.Save();
                    }

                    Messenger.Default.Send<RemoveChangedFileMessage>(null);
                }
            }
        }


        private bool LoadSolution(LoadSolutionMessage message)
        {
            this.Solutions.Clear();
            this.Solutions.Add(SolutionHier.Load(message.SolutionPath, message.SolutionName, message.SolutionFullPath));

            bool bFiredError = false;
            foreach (var item in this.Solutions[0].ProjectPaths)
            {
                string fullPath = (item.IsAbsolute) ? item.Path : Path.Combine(message.SolutionPath, item.Path);

                try
                {
                    var project = DefaultProjectHier.Load(Path.GetDirectoryName(item.Path), Path.GetFileName(item.Path), fullPath, this.Solutions[0]);

                    this.Solutions[0].Projects.Add(project);    // for connect with the parent node (solution)
                }
                catch
                {
                    bFiredError = true;
                    ErrorProjectHier project = new ErrorProjectHier(Path.GetDirectoryName(item.Path), item.Path);

                    this.Solutions[0].Projects.Add(project);
                }
            }

            return bFiredError;
        }

        /// <summary>
        /// This message handler loads the Solution.
        /// </summary>
        /// <param name="message">Information about the solution to load</param>
        public void ReceivedLoadSolutionMessage(LoadSolutionMessage message)
        {
            saveMessage = null;
            Messenger.Default.Send<AddMissedChangedFiles>(new AddMissedChangedFiles());

            var process = new GetChangedListMessage(string.Empty, PreprocessChangedFileList);
            Messenger.Default.Send<GetChangedListMessage>(process);

            if (saveMessage?.ResultStatus == ShowSaveDialogMessage.Result.Cancel) return;

            if (message is null) return;
            if (this.LoadSolution(message)) this.messageBoxService?.ShowWarning(CommonResource.WarningOnLoad, "");
        }

        /// <summary>
        /// This message handler addes new project to the solution.
        /// </summary>
        /// <param name="message">Information about the project to add</param>
        public void ReceivedAddNewProjectMessage(AddProjectMessage message)
        {
            if (message is null) return;
            if (this.Solutions.Count == 0) return;

            // if project path is in the solution path.
            var solutionPath = this.Solutions[0].CurOPath;
            int matchedPos = message.ProjectPath.IndexOf(solutionPath) + solutionPath.Length;
            bool isAbsolutePath = (PathHelper.ComparePath(solutionPath, message.ProjectPath) == false);

            ProjectGenerator projectGenerator = ProjectGenerator.CreateProjectGenerator(message.Language);
            if (projectGenerator == null) return;

            string projectPath = (isAbsolutePath) ? message.ProjectPath : message.ProjectPath.Substring(matchedPos);
            if (projectPath[0] == '\\') projectPath = projectPath.Remove(0, 1);

            DefaultProjectHier newProject = projectGenerator.CreateDefaultProject(projectPath, isAbsolutePath, message.ProjectName, message.MachineTarget, this.Solutions[0]);
            newProject.Save();

            this.Solutions[0].Projects.Add(newProject);

            var changedInfo = new AddChangedFileMessage(this.Solutions[0]);
            Messenger.Default.Send<AddChangedFileMessage>(changedInfo);

            //// Notify the changed data to the out.
            //var changedData = new ChangedFileListMessage.ChangedFile(this.Solutions[0], ChangedFileListMessage.ChangedStatus.Changed);
            //var changedFileListMessage = new ChangedFileListMessage();
            //changedFileListMessage.AddFile(changedData);
            //Messenger.Default.Send<ChangedFileListMessage>(changedFileListMessage);
        }

        /// <summary>
        /// This message handler addes a changed files that missed.
        /// </summary>
        /// <param name="message">not use (to match the shape)</param>
        public void ReceivedAddMissedChangedFilesMessage(AddMissedChangedFiles message)
        {
            if (message == null) return;
            if (this.Solutions.Count == 0) return;

            if (this.Solutions[0].IsChanged) Messenger.Default.Send<AddChangedFileMessage>(new AddChangedFileMessage(this.Solutions[0]));
            else Messenger.Default.Send<RemoveChangedFileMessage>(new RemoveChangedFileMessage(this.Solutions[0]));

            foreach(var project in this.Solutions[0].Projects)
            {
                if (project.IsChanged) Messenger.Default.Send<AddChangedFileMessage>(new AddChangedFileMessage(project));
                else Messenger.Default.Send<RemoveChangedFileMessage>(new RemoveChangedFileMessage(project));
            }
        }
    }
}
