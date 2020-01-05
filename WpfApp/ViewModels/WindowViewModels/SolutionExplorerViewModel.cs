using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using WpfApp.Models;

namespace WpfApp.ViewModels.WindowViewModels
{
    public class SolutionExplorerViewModel : ViewModelBase
    {
        public ObservableCollection<SolutionStruct> Solutions { get; } = new ObservableCollection<SolutionStruct>();

        // for test
        public SolutionExplorerViewModel()
        {
        }
    }
}
