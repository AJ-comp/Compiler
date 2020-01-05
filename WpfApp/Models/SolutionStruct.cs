using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Data;

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

    public class ProjectStruct : HirStruct
    {
        public ObservableCollection<FolderStruct> Folders { get; set; } = new ObservableCollection<FolderStruct>();
        public ObservableCollection<FileStruct> Items { get; set; } = new ObservableCollection<FileStruct>();

        public IList Children
        {
            get
            {
                return new CompositeCollection()
                {
                    new CollectionContainer() { Collection = Folders },
                    new CollectionContainer() { Collection = Items }
                };
            }
        }
    }

    public class FolderStruct : HirStruct
    {
        public ObservableCollection<FolderStruct> Folders { get; set; } = new ObservableCollection<FolderStruct>();
        public ObservableCollection<FileStruct> Items { get; set; } = new ObservableCollection<FileStruct>();

        public IList Children
        {
            get
            {
                return new CompositeCollection()
                {
                    new CollectionContainer() { Collection = Folders },
                    new CollectionContainer() { Collection = Items }
                };
            }
        }
    }

    public class FileStruct : HirStruct
    {
    }
}
