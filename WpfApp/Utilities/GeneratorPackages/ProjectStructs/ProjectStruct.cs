using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Data;
using WpfApp.Models;

namespace WpfApp.Utilities.GeneratorPackages.ProjectStructs
{
    public class ProjectStruct : HirStruct
    {
        public ReferenceStruct ReferenceFolder { get; } = new ReferenceStruct();
        public ObservableCollection<FolderStruct> Folders { get; } = new ObservableCollection<FolderStruct>();
        public ObservableCollection<FileStruct> Items { get; } = new ObservableCollection<FileStruct>();

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

    public class ReferenceStruct : HirStruct
    {
        public ObservableCollection<ReferenceFileStruct> ReferenceFiles { get; } = new ObservableCollection<ReferenceFileStruct>();
    }

    public class ReferenceFileStruct : HirStruct
    {
    }

    public class FolderStruct : HirStruct
    {
        public ObservableCollection<FolderStruct> Folders { get; } = new ObservableCollection<FolderStruct>();
        public ObservableCollection<FileStruct> Items { get; } = new ObservableCollection<FileStruct>();

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
