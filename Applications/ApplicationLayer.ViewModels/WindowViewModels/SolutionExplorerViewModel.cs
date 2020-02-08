using ApplicationLayer.Common.Helpers;
using ApplicationLayer.Common.Interfaces;
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

        public ObservableCollection<SolutionHier> Solutions { get; } = new ObservableCollection<SolutionHier>();

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
                SolutionHier child = e.NewItems[i] as SolutionHier;

                if (System.IO.File.Exists(child.FullPath)) continue;

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
                }
                else if (saveMessage.ResultStatus == ShowSaveDialogMessage.Result.No)
                {
                    foreach (var item in hirStructs)
                    {
                        item.RollBack();
                        item.Save();
                    }
                }
            }
        }


        private bool LoadSolution(LoadSolutionMessage message)
        {
            this.Solutions.Clear();

            using (StreamReader sr = new StreamReader(message.SolutionFullPath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(SolutionHier));
                SolutionHier solution = xs.Deserialize(sr) as SolutionHier;
                solution.CurOPath = message.SolutionPath;
                solution.FullName = message.SolutionName;

                this.Solutions.Add(solution);
            }

            bool bFiredError = false;
            foreach (var item in this.Solutions[0].ProjectPaths)
            {
                string fullPath = (item.IsAbsolute) ? item.Path : Path.Combine(message.SolutionPath, item.Path);

                try
                {
                    using (StreamReader sr = new StreamReader(fullPath))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(DefaultProjectHier));
                        DefaultProjectHier project = xs.Deserialize(sr) as DefaultProjectHier;

                        project.CurOPath = Path.GetDirectoryName(item.Path);
                        project.FullName = Path.GetFileName(item.Path);

                        this.Solutions[0].Projects.Add(project);    // for connect with the parent node (solution)

                        project.RollBack();
                    }
                }
                catch
                {
                    bFiredError = true;
                    ErrorProjectHier project = new ErrorProjectHier()
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

            if (saveMessage?.ResultStatus == ShowSaveDialogMessage.Result.Cancel) return;

            if (message is null) return;
            if (this.LoadSolution(message)) this.messageBoxService?.ShowWarning(CommonResource.WarningOnLoad, "");
        }

        /// <summary>
        /// This message handler addes new project to the solution.
        /// </summary>
        /// <param name="message"></param>
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
