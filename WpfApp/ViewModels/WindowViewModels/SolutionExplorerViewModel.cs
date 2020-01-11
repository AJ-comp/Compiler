using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Serialization;
using WpfApp.Messages;
using WpfApp.Models;
using WpfApp.Utilities;

namespace WpfApp.ViewModels.WindowViewModels
{
    public class SolutionExplorerViewModel : ViewModelBase
    {
        private SolutionManager solutionManager = new SolutionManager();

        public ObservableCollection<SolutionStruct> Solutions { get; } = new ObservableCollection<SolutionStruct>();

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

                Directory.CreateDirectory(child.Path);

                using (StreamWriter wr = new StreamWriter(child.FullPath))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(SolutionStruct));
                    xs.Serialize(wr, child);
                }
            }
        }

        public void ReceivedCreateSolutionMessage(CreateSolutionMessage message)
        {
            //this.solutionManager.Generator.GenerateSolution(message.SolutionPath, message.SolutionName, message.IsCreateSolutionFolder,
            //                                                                message.Language, message.MachineTarget);

            var solutionPath = (message.IsCreateSolutionFolder) ? message.SolutionPath + message.SolutionName : message.SolutionPath;
            var solutionName = message.SolutionName + this.solutionManager.Generator.SolutionExtension;

            this.Solutions.Add(this.solutionManager.Generator.Generate(message.SolutionPath, message.SolutionName, message.IsCreateSolutionFolder,
                                                                            message.Language, message.MachineTarget));
//            this.Solutions.Add(this.solutionManager.Loader.LoadSolution(message.SolutionPath, message.SolutionName));
        }

        public void ReceivedLoadSolutionMessage(LoadSolutionMessage message)
        {
            this.Solutions.Add(this.solutionManager.Loader.LoadSolution(message.SolutionPath, message.SolutionName));
        }
    }
}
