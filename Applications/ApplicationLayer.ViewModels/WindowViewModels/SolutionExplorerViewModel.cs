using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.ViewModels.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Serialization;

namespace WpfApp.ViewModels.WindowViewModels
{
    public class SolutionExplorerViewModel : ViewModelBase
    {
        public ObservableCollection<SolutionStruct> Solutions { get; } = new ObservableCollection<SolutionStruct>();

        public RelayCommand doubleClickCommand;
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
            //this.solutionManager.Generator.GenerateSolution(message.SolutionPath, message.SolutionName, message.IsCreateSolutionFolder,
            //                                                                message.Language, message.MachineTarget);
            this.Solutions.Add(SolutionStruct.Create(message.SolutionPath, message.SoltionName, message.Language, message.MachineTarget));

            using (StreamWriter wr = new StreamWriter(message.FullPath))
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
                this.Solutions.Add(xs.Deserialize(sr) as SolutionStruct);
            }

            foreach(var item in this.Solutions[0].ProjectPaths)
            {
                using (StreamReader sr = new StreamReader(Path.Combine(message.SolutionFullPath, item)))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(ProjectStruct));
                    ProjectStruct project = xs.Deserialize(sr) as ProjectStruct;

                    project.SyncWithXMLData();
                    this.Solutions[0].Projects.Add(project);
                }
            }
        }
    }
}
