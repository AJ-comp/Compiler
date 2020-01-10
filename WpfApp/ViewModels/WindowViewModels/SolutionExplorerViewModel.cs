using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using WpfApp.Messages;
using WpfApp.Models;
using WpfApp.Utilities;

namespace WpfApp.ViewModels.WindowViewModels
{
    public class SolutionExplorerViewModel : ViewModelBase
    {
        public ObservableCollection<SolutionStruct> Solutions { get; } = new ObservableCollection<SolutionStruct>();

        // for test
        public SolutionExplorerViewModel()
        {
        }

        public void ReceivedLoadSolutionMessage(LoadSolutionMessage message)
        {
            SolutionLoader loader = new SolutionLoader();
            this.Solutions.Add(loader.LoadSolution(message.SolutionPath, message.SolutionName));
        }
    }
}
