using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Data;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    [XmlInclude(typeof(FolderStruct))]
    [XmlInclude(typeof(FileStruct))]
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

        public StringCollection AllItemPaths
        {
            get
            {
                StringCollection result = new StringCollection();

                if (this.Items.Count == 0 && this.Folders.Count == 0) result.Add(this.CurOPath);

                foreach(var item in this.Items)
                {
                    result.Add(Path.Combine(this.CurOPath, item.AutoPath));
                }

                foreach (var folder in this.Folders)
                {
                    foreach(var item in folder.AllItemPaths)
                        result.Add(Path.Combine(this.CurOPath, item));
                }

                return result;
            }
        }

        public FolderStruct()
        {
            this.Folders.CollectionChanged += Folders_CollectionChanged;
            this.Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Folders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems?.Count; i++)
            {
                FolderStruct item = e.NewItems[i] as FolderStruct;
                item.Parent = this;
            }
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems?.Count; i++)
            {
                FileStruct item = e.NewItems[i] as FileStruct;
                item.Parent = this;
            }
        }

        /// <summary>
        /// This function removes the child after find child type.
        /// </summary>
        /// <param name="child">child to remove</param>
        public void RemoveChild(HirStruct child)
        {
            if (child is FolderStruct) this.Folders.Remove(child as FolderStruct);
            else if (child is FileStruct) this.Items.Remove(child as FileStruct);
        }

        public static FolderStruct GetFolderSet(string path)
        {
            FolderStruct result = new FolderStruct();
            path = Path.GetDirectoryName(path);

            FolderStruct current = result;
            while(true)
            {
                var root = Path.GetPathRoot(path);
                if (root.Length == 0) break;

                path = path.Substring(0, root.Length);
                FolderStruct child = new FolderStruct() { CurOPath = root };
                current.Folders.Add(child);

                current = child;
            }

            return result;
        }

        public static FolderStruct GetFolderSet(string basePath, string path)
        {
            FolderStruct result = new FolderStruct();
            path = Path.GetDirectoryName(path);

            string accumDirectory = string.Empty;
            FolderStruct current = result;
            while (true)
            {
                var root = Path.GetPathRoot(path);
                if (root.Length == 0) break;

                // If real folder not exist then ignore.
                accumDirectory = Path.Combine(accumDirectory, root);
                if (Directory.Exists(Path.Combine(basePath, accumDirectory)) == false) continue;

                path = path.Substring(0, root.Length);
                FolderStruct child = new FolderStruct() { CurOPath = root };
                current.Folders.Add(child);

                current = child;
            }

            return result;
        }

    }
}
