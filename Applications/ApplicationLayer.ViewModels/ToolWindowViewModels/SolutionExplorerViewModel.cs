using ApplicationLayer.Common;
using ApplicationLayer.Common.Helpers;
using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.Models.SolutionPackage.MiniCPackage;
using ApplicationLayer.ViewModels.DocumentTypeViewModels;
using ApplicationLayer.ViewModels.Messages;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Serialization;
using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.ViewModels.ToolWindowViewModels
{
    public class SolutionExplorerViewModel : ToolWindowViewModel
    {
        /********************************************************************************************
         * private field section
         ********************************************************************************************/
        private SolutionTreeNodeModel solution;
        private TreeNodeModel selectedItem;
        private ObservableCollection<DocumentViewModel> _documents;
        private DocumentViewModel selectedDocument;
        private RelayCommand doubleClickCommand;
        private RelayCommand existItemCommand;
        private RelayCommand<TreeNodeModel> selectedCommand;
        private RelayCommand<TreeNodeModel> newFolderCommand;



        /********************************************************************************************
         * property section
         ********************************************************************************************/
        public SolutionTreeNodeModel Solution
        {
            get => solution;
            private set
            {
                solution = value;
                RaisePropertyChanged(nameof(Solution));
            }
        }

        public ObservableCollection<DocumentViewModel> Documents
        {
            get
            {
                if (this._documents == null)
                {
                    this._documents = new ObservableCollection<DocumentViewModel>();
                    this._documents.CollectionChanged += Documents_CollectionChanged;
                }

                return this._documents;
            }
        }

        public DocumentViewModel SelectedDocument
        {
            get => this.selectedDocument;
            set
            {
                this.selectedDocument = value;
                this.RaisePropertyChanged(nameof(SelectedDocument));
            }
        }

        public TreeNodeModel SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged(nameof(SelectedItem));
            }
        }



        /********************************************************************************************
         * command property section
         ********************************************************************************************/
        public RelayCommand<TreeNodeModel> SelectedCommand
        {
            get
            {
                if (this.selectedCommand == null)
                    this.selectedCommand = new RelayCommand<TreeNodeModel>(this.OnSelected);

                return this.selectedCommand;
            }
        }

        public RelayCommand DoubleClickCommand
        {
            get
            {
                if (this.doubleClickCommand == null)
                    this.doubleClickCommand = new RelayCommand(this.OnMouseDoubleClick);

                return this.doubleClickCommand;
            }
        }

        public RelayCommand ExistItemCommand
        {
            get
            {
                if (this.existItemCommand == null)
                    this.existItemCommand = new RelayCommand(this.OnExistItemMenuClick);

                return this.existItemCommand;
            }
        }

        public RelayCommand<TreeNodeModel> NewFolderCommand
        {
            get
            {
                if (this.newFolderCommand == null)
                    this.newFolderCommand = new RelayCommand<TreeNodeModel>(this.OnNewFolderMenuClick);

                return this.newFolderCommand;
            }
        }



        /********************************************************************************************
         * constructor section
         ********************************************************************************************/
        public SolutionExplorerViewModel()
        {
            this.SerializationId = "SE";
            this.DefaultDockSide = Models.ToolWindowStatus.ToolItemDockSide.Right;
            this.WindowState = Models.ToolWindowStatus.ToolItemState.Docked;
            this.Title = CommonResource.SolutionExplorer;
        }



        /********************************************************************************************
         * public method section
         ********************************************************************************************/
        public void Save()
        {
            if (Solution == null) return;

            Directory.CreateDirectory(Solution.Path);
            using (StreamWriter wr = new StreamWriter(Solution.PathWithFileName))
            {
                XmlSerializer xs = new XmlSerializer(typeof(SolutionTreeNodeModel));
                xs.Serialize(wr, Solution);
            }

            foreach (var item in Solution.Children)
            {
                var project = item as ProjectTreeNodeModel;
                string projPath = Path.Combine(Solution.Path, project.Path);
                Directory.CreateDirectory(projPath);
                string fullPath = Path.Combine(projPath, project.FileName);

                Type type = typeof(ProjectTreeNodeModel);
                if (Path.GetExtension(project.FileName) == string.Format(".{0}proj", LanguageExtensions.MiniC))
                    type = typeof(MiniCProjectTreeNodeModel);

                using (StreamWriter wr = new StreamWriter(fullPath))
                {
                    XmlSerializer xs = new XmlSerializer(type);
                    xs.Serialize(wr, project);
                }
            }
        }

        public void Load(string path, string solutionFileName)
        {
            SolutionTreeNodeModel result = new SolutionTreeNodeModel(path, solutionFileName);

            using (StreamReader sr = new StreamReader(Path.Combine(path, solutionFileName)))
            {
                XmlSerializer xs = new XmlSerializer(typeof(SolutionTreeNodeModel));
                result = xs.Deserialize(sr) as SolutionTreeNodeModel;
                result.Path = path;
                result.SolutionName = solutionFileName;
            }

            this.Solution = result;
            if(this.Solution.LoadProject() == false)
                Messenger.Default.Send<DisplayMessage>(new DisplayMessage(new Models.MessageData(Models.MessageKind.Information, CommonResource.WarningOnLoad), string.Empty));

            this.Solution.IsExpanded = true;

            this.ConnectEventHandler();
        }

        private void ConnectEventHandler()
        {
            this.Solution.ChildrenChanged += Solution_ChildrenChanged;
        }



        /********************************************************************************************
         * event handler section
         ********************************************************************************************/
        /// <summary>
        /// This function is called when a child (project) self is changed.
        /// (ex project rename, delete, unload, add)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Solution_ChildrenChanged(object sender, FileChangedEventArgs e)
        {
            if (this.solution.IsChanged) Messenger.Default.Send<AddChangedFileMessage>(new AddChangedFileMessage(this.solution));
            else Messenger.Default.Send<RemoveChangedFileMessage>(new RemoveChangedFileMessage(this.solution));
        }

        private void Project_ChildrenChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Message handler for CreateSolutionMessage
        /// </summary>
        /// <param name="message"></param>
        public void ReceivedCreateSolutionMessage(CreateSolutionMessage message)
        {
            if (message is null) return;

            this.Solution = SolutionTreeNodeModel.Create(message.SolutionPath, message.SoltionName, message.Language, message.MachineTarget);
            this.Solution.IsExpanded = true;

            this.Save();
            this.ConnectEventHandler();
        }

        private void Documents_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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



        /********************************************************************************************
         * static method section
         ********************************************************************************************/
        /// <summary>
        /// This function checks whether changed files are.
        /// </summary>
        /// <returns>If a changed files not exist return null, if a changed files exist return a user's answer after process for answer.</returns>
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
                        item.SyncWithCurrentValue();
                        item.Save();
                    }

                    Messenger.Default.Send<RemoveChangedFileMessage>(null);
                }
                else if (result.ResultStatus == ShowSaveDialogMessage.Result.No)
                {
                    foreach (var item in hirStructs)
                    {
                        item.SyncWithLoadValue();
                        item.Save();
                    }

                    Messenger.Default.Send<RemoveChangedFileMessage>(null);
                }
            });

            Messenger.Default.Send<GetChangedListMessage>(process);

            return result;
        }



        /********************************************************************************************
         * message action method section
         ********************************************************************************************/
        /// <summary>
        /// This message handler loads the Solution.
        /// </summary>
        /// <param name="message">Information about the solution to load</param>
        public void ReceivedLoadSolutionMessage(LoadSolutionMessage message)
        {
            var answer = SolutionExplorerViewModel.CheckChangedFiles();

            if (answer?.ResultStatus == ShowSaveDialogMessage.Result.Cancel) return;

            if (message is null) return;
            this.Load(message.SolutionPath, message.SolutionName);

            //            if (this.LoadSolution(message)) Messenger.Default.Send<DisplayMessage>(new DisplayMessage(CommonResource.WarningOnLoad, ""));
        }

        /// <summary>
        /// This message handler addes new project to the solution.
        /// </summary>
        /// <param name="message">Information about the project to add</param>
        public void ReceivedAddNewProjectMessage(AddProjectMessage message)
        {
            if (message is null) return;
            if (this.Solution is null) return;

            // if project path is in the solution path.
            var solutionPath = this.Solution.Path;
            int matchedPos = message.ProjectPath.IndexOf(solutionPath) + solutionPath.Length;
            bool isAbsolutePath = (PathHelper.ComparePath(solutionPath, message.ProjectPath) == false);

            ProjectGenerator projectGenerator = ProjectGenerator.CreateProjectGenerator(message.Language);
            if (projectGenerator == null) return;

            string projectPath = (isAbsolutePath) ? message.ProjectPath : message.ProjectPath.Substring(matchedPos);
            if (projectPath[0] == '\\') projectPath = projectPath.Remove(0, 1);

            ProjectTreeNodeModel newProject = projectGenerator.CreateDefaultProject(projectPath, isAbsolutePath, message.ProjectName, message.MachineTarget);
//            newProject.Save();

            this.Solution.AddProject(newProject);

//            var changedInfo = new AddChangedFileMessage(this.Solution);
//            Messenger.Default.Send<AddChangedFileMessage>(changedInfo);

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
            if (this.Solution == null) return;

            /*
            if (this.Solutions[0].IsChanged) Messenger.Default.Send<AddChangedFileMessage>(new AddChangedFileMessage(this.Solutions[0]));
            else Messenger.Default.Send<RemoveChangedFileMessage>(new RemoveChangedFileMessage(this.Solutions[0]));

            foreach(var project in this.Solutions[0].Projects)
            {
                if (project.IsChanged) Messenger.Default.Send<AddChangedFileMessage>(new AddChangedFileMessage(project));
                else Messenger.Default.Send<RemoveChangedFileMessage>(new RemoveChangedFileMessage(project));
            }
            */
        }



        /********************************************************************************************
         * command action method section
         ********************************************************************************************/
        private void OnSelected(TreeNodeModel selectedItem)
        {
            this.SelectedItem = selectedItem;
        }

        private void OnMouseDoubleClick()
        {
            if (SelectedItem is FileTreeNodeModel)
            {
                string fileName = (SelectedItem as FileTreeNodeModel).PathWithFileName;

                if (File.Exists(fileName) == false) return;
                string content = File.ReadAllText(fileName);

                var editor = new EditorTypeViewModel(fileName, content);
                if (this.Documents.Contains(editor))
                {
                    this.SelectedDocument = editor;
                    return;
                }

                this.Documents.Add(editor);
                this.SelectedDocument = editor;

                Messenger.Default.Send<AddEditorMessage>(new AddEditorMessage(editor));
            }
        }

        private void OnExistItemMenuClick()
        {
        }

        private void OnNewFolderMenuClick(TreeNodeModel selectedItem)
        {

        }
    }
}
