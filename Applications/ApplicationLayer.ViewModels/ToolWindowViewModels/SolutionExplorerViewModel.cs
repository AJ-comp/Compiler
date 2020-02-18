using ApplicationLayer.Common.Helpers;
using ApplicationLayer.Models;
using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.ViewModels.CommandArgs;
using ApplicationLayer.ViewModels.DocumentTypeViewModels;
using ApplicationLayer.ViewModels.Messages;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;

using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.ViewModels.ToolWindowViewModels
{
    public class SolutionExplorerViewModel : ToolWindowViewModel
    {
        public ObservableCollection<SolutionHier> Solutions { get; } = new ObservableCollection<SolutionHier>();

        #region Property related to Document
        private ObservableCollection<DocumentViewModel> _documents;
        public ObservableCollection<DocumentViewModel> Documents
        {
            get
            {
                if (this._documents == null)
                {
                    this._documents = new ObservableCollection<DocumentViewModel>();
                    this._documents.CollectionChanged += _documents_CollectionChanged;
                }

                return this._documents;
            }
        }

        private void _documents_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (DocumentViewModel document in e.NewItems)
                {
                    document.CloseRequest += Document_RequestClose;
                    document.AllCloseExceptThisRequest += Document_AllCloseExceptThisRequest;
                }
            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (DocumentViewModel document in e.OldItems)
                {
                    document.CloseRequest -= Document_RequestClose;
                    document.AllCloseExceptThisRequest -= Document_AllCloseExceptThisRequest;
                }
        }

        private void Document_AllCloseExceptThisRequest(object sender, EventArgs e)
        {
            this._documents.Clear();
            this._documents.Add(sender as DocumentViewModel);
        }

        private void Document_RequestClose(object sender, EventArgs e)
        {
            this._documents.Remove(sender as DocumentViewModel);
        }

        private DocumentViewModel selectedDocument;
        public DocumentViewModel SelectedDocument
        {
            get => this.selectedDocument;
            set
            {
                this.selectedDocument = value;
                this.RaisePropertyChanged(nameof(SelectedDocument));
            }
        }
        #endregion


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
                string fileName = selectedItem.FullPath;

                if (File.Exists(fileName) == false) return;
                string content = File.ReadAllText(fileName);

                var editor = new EditorTypeViewModel(fileName, content);
                if (this.Documents.Contains(editor)) return;

                this.Documents.Add(editor);
                this.SelectedDocument = editor;

                Messenger.Default.Send<AddEditorMessage>(new AddEditorMessage(editor));
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
                MessageData exceptData = target.Item.IsValidToChange();
                if (exceptData == null)
                {
                    target.Item.IsEditMode = false;
                    target.Item.ChangeDisplayName();
                }
                else Messenger.Default.Send<DisplayMessage>(new DisplayMessage(exceptData, string.Empty));
            }
        }

        public SolutionExplorerViewModel()
        {
            this.Solutions.CollectionChanged += Solutions_CollectionChanged;

            this.SerializationId = "SE";
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
        /// This function checks whether changed files are.
        /// </summary>
        /// <returns>If changed files not exist return null, if changed files exist return a user's answer after process for answer.</returns>
        public static ShowSaveDialogMessage CheckChangedFiles()
        {
            ShowSaveDialogMessage result = null;

            Messenger.Default.Send<AddMissedChangedFilesMessage>(new AddMissedChangedFilesMessage());

            var process = new GetChangedListMessage(string.Empty, (hirStructs) => 
            {
                if (hirStructs.Count <= 0) return;

                result = new ShowSaveDialogMessage();
                Messenger.Default.Send<ShowSaveDialogMessage>(result);

                if (result.ResultStatus == ShowSaveDialogMessage.Result.Yes)
                {
                    foreach (var item in hirStructs)
                    {
                        item.Commit();
                        item.Save();
                    }

                    Messenger.Default.Send<RemoveChangedFileMessage>(null);
                }
                else if (result.ResultStatus == ShowSaveDialogMessage.Result.No)
                {
                    foreach (var item in hirStructs)
                    {
                        item.RollBack();
                        item.Save();
                    }

                    Messenger.Default.Send<RemoveChangedFileMessage>(null);
                }
            });

            Messenger.Default.Send<GetChangedListMessage>(process);

            return result;
        }

        /// <summary>
        /// This message handler loads the Solution.
        /// </summary>
        /// <param name="message">Information about the solution to load</param>
        public void ReceivedLoadSolutionMessage(LoadSolutionMessage message)
        {
            var answer = SolutionExplorerViewModel.CheckChangedFiles();

            if (answer?.ResultStatus == ShowSaveDialogMessage.Result.Cancel) return;

            if (message is null) return;
            if (this.LoadSolution(message)) Messenger.Default.Send<DisplayMessage>(new DisplayMessage(CommonResource.WarningOnLoad, ""));
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
        public void ReceivedAddMissedChangedFilesMessage(AddMissedChangedFilesMessage message)
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
