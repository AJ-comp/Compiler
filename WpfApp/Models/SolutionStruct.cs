using System.Collections.ObjectModel;
using WpfApp.Utilities.GeneratorPackages.ProjectStructs;

namespace WpfApp.Models
{
    public class HirStruct
    {
        public string ImageSource { get; set; }
        public string Name { get; set; }
    }


    public enum FileConfigure { Debug, Release }
    public class Configure
    {

    }

    public class SolutionStruct : HirStruct
    {
        public ObservableCollection<ProjectStruct> Projects { get; set; } = new ObservableCollection<ProjectStruct>();
    }
}
