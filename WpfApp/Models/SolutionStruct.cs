using System.Collections.ObjectModel;

namespace WpfApp.Models
{
    public class HirStruct
    {
        public string ImageSource { get; set; }
        public string Name { get; set; }
    }

    public class SolutionStruct : HirStruct
    {
        public ObservableCollection<ProjectStruct> Projects { get; set; } = new ObservableCollection<ProjectStruct>();
    }

    public class ProjectStruct : HirStruct
    {
        public ObservableCollection<GroupStruct> Groups { get; set; } = new ObservableCollection<GroupStruct>();
        public ObservableCollection<ItemStruct> Items { get; set; } = new ObservableCollection<ItemStruct>();
    }

    public class GroupStruct : HirStruct
    {
        public ObservableCollection<ItemStruct> Items { get; set; } = new ObservableCollection<ItemStruct>();
    }

    public class ItemStruct : HirStruct
    {

    }
}
