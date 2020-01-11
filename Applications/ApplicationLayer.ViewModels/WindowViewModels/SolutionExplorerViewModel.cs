using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.ViewModels.Messages;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Serialization;

namespace WpfApp.ViewModels.WindowViewModels
{
    public class SolutionExplorerViewModel : ViewModelBase
    {
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

                Directory.CreateDirectory(child.OPath);

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
            var solutionName = message.SolutionName + SolutionStruct.Extension;

            this.Solutions.Add(SolutionStruct.Create(message.SolutionPath, message.SolutionName, message.IsCreateSolutionFolder,
                                                                            message.Language, message.MachineTarget));
//            this.Solutions.Add(this.solutionManager.Loader.LoadSolution(message.SolutionPath, message.SolutionName));
        }

        public void ReceivedLoadSolutionMessage(LoadSolutionMessage message)
        {
            SolutionStruct solution = new SolutionStruct();
            solution.Load(message.SolutionPath, message.SolutionName);
            this.Solutions.Add(solution);
        }
    }
}
