using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using WpfApp.Models;

namespace WpfApp.ViewModels.WindowViewModels
{
    public class SolutionExplorerViewModel : ViewModelBase
    {
        public ObservableCollection<SolutionStruct> Solutions { get; set; } = new ObservableCollection<SolutionStruct>();

        // for test
        public SolutionExplorerViewModel()
        {
            SolutionStruct root = new SolutionStruct() { Name = "Solution" };
            this.Solutions.Add(root);

            var project1 = new ProjectStruct() { Name = "project1" };
            project1.Items.Add(new FileStruct() { Name = "item1" });
            project1.Items.Add(new FileStruct() { Name = "item2" });
            project1.Items.Add(new FileStruct() { Name = "item3" });
            root.Projects.Add(project1);

            var project2 = new ProjectStruct() { Name = "project2" };
            project2.Folders.Add(new FolderStruct() { Name = "Group1" });
            var group = new FolderStruct() { Name = "Group2" };
            group.Items.Add(new FileStruct() { Name = "item1" });

            var folder = new FolderStruct() { Name = "FolderTest" };
            folder.Items.Add(new FileStruct() { Name = "Item3" });

            group.Folders.Add(folder);

            project2.Folders.Add(group);
            root.Projects.Add(project2);
        }
    }
}
