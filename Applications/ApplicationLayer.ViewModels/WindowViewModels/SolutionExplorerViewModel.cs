using ApplicationLayer.Common.Helpers;
using ApplicationLayer.Models;
using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.ViewModels.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Serialization;

using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace WpfApp.ViewModels.WindowViewModels
{
    public class SolutionExplorerViewModel : ViewModelBase
    {
        private ShowSaveDialogMessage saveMessage;
        private IMessageBoxService messageBoxService;

        public ObservableCollection<SolutionStruct> Solutions { get; } = new ObservableCollection<SolutionStruct>();

        private RelayCommand<HirStruct> selectedCommand;
        public RelayCommand<HirStruct> SelectedCommand
        {
            get
            {
                if (this.selectedCommand == null)
                    this.selectedCommand = new RelayCommand<HirStruct>(this.OnSelected);

                return this.selectedCommand;
            }
        }
        private void OnSelected(HirStruct selectedItem)
        {
            if(selectedItem is DefaultFileStruct)
            {
                Messenger.Default.Send(new OpenFileMessage(selectedItem as DefaultFileStruct));
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

        private RelayCommand newFolderCommand;
        public RelayCommand NewFolderCommand
        {
            get
            {
                if (this.newFolderCommand == null)
                    this.newFolderCommand = new RelayCommand(this.OnNewFolderMenuClick);

                return this.newFolderCommand;
            }
        }
        private void OnNewFolderMenuClick()
        {

        }


        public SolutionExplorerViewModel(IMessageBoxService messageBoxService)
        {
            this.messageBoxService = messageBoxService;
            this.Solutions.CollectionChanged += Solutions_CollectionChanged;
        }

        private void Solutions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null) return;

            for (int i = e.NewStartingIndex; i < e.NewItems.Count; i++)
            {
                SolutionStruct child = e.NewItems[i] as SolutionStruct;

                if (System.IO.File.Exists(child.FullPath)) continue;

                Directory.CreateDirectory(child.CurOPath);
            }
        }

        private void ToXML(SolutionStruct solution)
        {
            using (StreamWriter wr = new StreamWriter(solution.FullPath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(SolutionStruct));
                xs.Serialize(wr, this.Solutions[0]);
            }

            foreach (var item in this.Solutions[0].Projects)
            {
                using (StreamWriter wr = new StreamWriter(item.FullPath))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(DefaultProjectStruct));
                    xs.Serialize(wr, item);
                }
            }
        }

        /// <summary>
        /// Message handler for CreateSolutionMessage
        /// </summary>
        /// <param name="message"></param>
        public void ReceivedCreateSolutionMessage(CreateSolutionMessage message)
        {
            this.Solutions.Add(SolutionStruct.Create(message.SolutionPath, message.SoltionName, message.Language, message.MachineTarget));

            this.ToXML(this.Solutions[0]);
        }

        private void PreprocessChangedFileList(Collection<HirStruct> hirStructs)
        {
            if(hirStructs.Count > 0)
            {
                saveMessage = new ShowSaveDialogMessage();
                Messenger.Default.Send<ShowSaveDialogMessage>(saveMessage);
            }
        }


        private bool LoadSolution(LoadSolutionMessage message)
        {
            this.Solutions.Clear();

            using (StreamReader sr = new StreamReader(message.SolutionFullPath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(SolutionStruct));
                SolutionStruct solution = xs.Deserialize(sr) as SolutionStruct;
                solution.CurOPath = message.SolutionPath;
                solution.FullName = message.SolutionName;
                foreach (var path in solution.SyncWithXMLProjectPaths) solution.CurrentProjectPath.Add(path);

                this.Solutions.Add(solution);
            }

            bool bFiredError = false;
            foreach (var item in this.Solutions[0].SyncWithXMLProjectPaths)
            {
                string fullPath = (item.IsAbsolute) ? item.Path : Path.Combine(message.SolutionPath, item.Path);

                try
                {
                    using (StreamReader sr = new StreamReader(fullPath))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(DefaultProjectStruct));
                        DefaultProjectStruct project = xs.Deserialize(sr) as DefaultProjectStruct;

                        project.CurOPath = Path.GetDirectoryName(item.Path);
                        project.FullName = Path.GetFileName(item.Path);
                        this.Solutions[0].Projects.Add(project);    // for connect with the parent node (solution)

                        project.SyncXmlToObject();
                    }
                }
                catch
                {
                    bFiredError = true;
                    ErrorProjectStruct project = new ErrorProjectStruct()
                    {
                        FullName = item.Path,
                        CurOPath = Path.GetDirectoryName(item.Path)
                    };

                    this.Solutions[0].Projects.Add(project);
                }
            }

            return bFiredError;
        }

        /// <summary>
        /// Message handler for LoadSolutionMessage
        /// </summary>
        /// <param name="message"></param>
        public void ReceivedLoadSolutionMessage(LoadSolutionMessage message)
        {
            saveMessage = null;
            var process = new GetChangedListMessage(string.Empty, PreprocessChangedFileList);
            Messenger.Default.Send<GetChangedListMessage>(process);

            if(saveMessage != null)
            {
                if (saveMessage.ResultStatus == ShowSaveDialogMessage.Result.Yes)
                {

                }
                else if (saveMessage.ResultStatus == ShowSaveDialogMessage.Result.Cancel) return;
            }

            if (this.LoadSolution(message)) this.messageBoxService?.ShowWarning(CommonResource.WarningOnLoad, "");
        }

        /// <summary>
        /// This message handler addes new project to the solution.
        /// </summary>
        /// <param name="message"></param>
        public void ReceivedAddNewProjectMessage(AddProjectMessage message)
        {
            if (this.Solutions.Count == 0) return;

            // if project path is in the solution path.
            var solutionPath = this.Solutions[0].CurOPath;
            int matchedPos = message.ProjectPath.IndexOf(solutionPath) + solutionPath.Length;
            bool isAbsolutePath = (PathHelper.ComparePath(solutionPath, message.ProjectPath) == false);

            ProjectGenerator projectGenerator = ProjectGenerator.CreateProjectGenerator(message.Language);
            if (projectGenerator == null) return;

            string projectPath = (isAbsolutePath) ? message.ProjectPath : message.ProjectPath.Substring(matchedPos);
            if (projectPath[0] == '\\') projectPath = projectPath.Remove(0, 1);

            DefaultProjectStruct newProject = projectGenerator.CreateDefaultProject(projectPath, isAbsolutePath, message.ProjectName, message.MachineTarget, this.Solutions[0]);
            this.Solutions[0].Projects.Add(newProject);

            // create project files into the folder.
            using (StreamWriter wr = new StreamWriter(newProject.FullPath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(DefaultProjectStruct));
                xs.Serialize(wr, newProject);
            }

            var changedInfo = new ChangedFileMessage(this.Solutions[0], ChangedFileMessage.ChangedStatus.Changed);
            Messenger.Default.Send<ChangedFileMessage>(changedInfo);

            //// Notify the changed data to the out.
            //var changedData = new ChangedFileListMessage.ChangedFile(this.Solutions[0], ChangedFileListMessage.ChangedStatus.Changed);
            //var changedFileListMessage = new ChangedFileListMessage();
            //changedFileListMessage.AddFile(changedData);
            //Messenger.Default.Send<ChangedFileListMessage>(changedFileListMessage);
        }
    }
}
