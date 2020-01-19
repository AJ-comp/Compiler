using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.ViewModels.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Serialization;

namespace WpfApp.ViewModels.WindowViewModels
{
    public class SolutionExplorerViewModel : ViewModelBase
    {
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
            if(selectedItem is FileStruct)
            {
                Messenger.Default.Send(new OpenFileMessage(selectedItem as FileStruct));
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


        #region Command related to ContextMenu
        private RelayCommand newItemCommand;
        public RelayCommand NewItemCommand
        {
            get
            {
                if (this.newItemCommand == null)
                    this.newItemCommand = new RelayCommand(this.OnNewItemMenuClick);

                return this.newItemCommand;
            }
        }
        private void OnNewItemMenuClick()
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
        #endregion


        // for test
        public SolutionExplorerViewModel()
        {
            this.Solutions.CollectionChanged += Solutions_CollectionChanged;
        }

        private void Solutions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = e.NewStartingIndex; i < e.NewItems.Count; i++)
            {
                SolutionStruct child = e.NewItems[i] as SolutionStruct;

                if (File.Exists(child.FullPath)) continue;

                Directory.CreateDirectory(child.OPath);
            }
        }

        public void ReceivedCreateSolutionMessage(CreateSolutionMessage message)
        {
            this.Solutions.Add(SolutionStruct.Create(message.SolutionPath, message.SoltionName, message.Language, message.MachineTarget));

            using (StreamWriter wr = new StreamWriter(message.SolutionFullPath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(SolutionStruct));
                xs.Serialize(wr, this.Solutions[0]);
            }

            foreach (var item in this.Solutions[0].Projects)
            {
                using (StreamWriter wr = new StreamWriter(item.FullPath))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(ProjectStruct));
                    xs.Serialize(wr, item);
                }
            }
            //            this.Solutions.Add(this.solutionManager.Loader.LoadSolution(message.SolutionPath, message.SolutionName));
        }

        public void ReceivedLoadSolutionMessage(LoadSolutionMessage message)
        {
            using (StreamReader sr = new StreamReader(message.SolutionFullPath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(SolutionStruct));
                SolutionStruct solution = xs.Deserialize(sr) as SolutionStruct;
                solution.OPath = message.SolutionPath;
                solution.FullName = message.SolutionName;

                this.Solutions.Add(solution);
            }

            foreach(var item in this.Solutions[0].ProjectPaths)
            {
                using (StreamReader sr = new StreamReader(Path.Combine(message.SolutionPath, item)))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(ProjectStruct));
                    ProjectStruct project = xs.Deserialize(sr) as ProjectStruct;
                    project.OPath = Path.GetDirectoryName(item);
                    project.FullName = Path.GetFileName(item);
                    this.Solutions[0].Projects.Add(project);    // for connect with the parent node (solution)

                    project.SyncWithXMLData();
                }
            }
        }
    }
}
