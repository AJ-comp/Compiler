using ApplicationLayer.Common.Helpers;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Data;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    [XmlInclude(typeof(FolderHier))]
    [XmlInclude(typeof(DefaultFileHier))]
    public class FolderHier : HierarchicalData
    {
        public ObservableCollection<FolderHier> Folders { get; } = new ObservableCollection<FolderHier>();
        public ObservableCollection<DefaultFileHier> Items { get; } = new ObservableCollection<DefaultFileHier>();

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

        /// <summary>
        /// This property returns all paths that have an item.
        /// </summary>
        public StringCollection HasItemAllPaths
        {
            get
            {
                StringCollection result = new StringCollection();

                foreach(var item in this.Items)
                {
                    result.Add(Path.Combine(this.CurOPath, item.AutoPath));
                }

                foreach (var folder in this.Folders)
                {
                    foreach(var item in folder.HasItemAllPaths)
                        result.Add(Path.Combine(this.CurOPath, item));
                }

                return result;
            }
        }

        /// <summary>
        /// This property returns all paths that have not an item.
        /// </summary>
        public StringCollection HasNotItemAllPaths
        {
            get
            {
                StringCollection result = new StringCollection();

                if (this.Items.Count == 0 && this.Folders.Count == 0) result.Add(this.CurOPath);

                foreach (var folder in this.Folders)
                {
                    foreach (var item in folder.HasNotItemAllPaths)
                        result.Add(Path.Combine(this.CurOPath, item));
                }

                return result;
            }
        }

        public FolderHier()
        {
            this.Folders.CollectionChanged += Folders_CollectionChanged;
            this.Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Folders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems?.Count; i++)
            {
                FolderHier item = e.NewItems[i] as FolderHier;
                item.Parent = this;
            }
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems?.Count; i++)
            {
                DefaultFileHier item = e.NewItems[i] as DefaultFileHier;
                item.Parent = this;
            }
        }

        public FolderHier RootFolder
        {
            get
            {
                FolderHier result = this;
                while(true)
                {
                    if (result.Parent is FolderHier)
                        result = result.Parent as FolderHier;
                    else break;
                }

                return result;
            }
        }

        public ProjectHier ProjectTypeParent
        {
            get
            {
                HierarchicalData current = this;

                while(true)
                {
                    if (current == null) return null;
                    else if (current is ProjectHier) return current as ProjectHier;

                    current = current.Parent;
                }
            }
        }

        public SolutionHier SolutionTypeParent
        {
            get
            {
                HierarchicalData current = this;

                while (true)
                {
                    if (current == null) return null;
                    else if (current is SolutionHier) return current as SolutionHier;

                    current = current.Parent;
                }
            }
        }

        /// <summary>
        /// This function removes the child after find child type.
        /// </summary>
        /// <param name="child">child to remove</param>
        public void RemoveChild(HierarchicalData child)
        {
            if (child is FolderHier) this.Folders.Remove(child as FolderHier);
            else if (child is DefaultFileHier) this.Items.Remove(child as DefaultFileHier);
        }

        public static FolderHier GetFolderSet(string path)
        {
            FolderHier result = new FolderHier();
            path = Path.GetDirectoryName(path);

            FolderHier current = result;
            while(true)
            {
                var root = Path.GetPathRoot(path);
                if (root.Length == 0) break;

                path = path.Substring(0, root.Length);
                FolderHier child = new FolderHier() { CurOPath = root };
                current.Folders.Add(child);

                current = child;
            }

            return result;
        }

        public static FolderHier GetFolderSet(string basePath, string dirPath, bool bGetLastHierRef = false)
        {
            FolderHier result = new FolderHier();

            string accumDirectory = string.Empty;
            FolderHier current = result;

            bool bFirst = true;
            while (true)
            {
                var root = PathHelper.GetRootDirectory(dirPath);
                if (root.Length == 0) break;

                // If real folder not exist then ignore.
                accumDirectory = Path.Combine(accumDirectory, root);
                if (Directory.Exists(Path.Combine(basePath, accumDirectory)) == false) break;

                dirPath = PathHelper.GetDirectoryPathExceptRoot(dirPath);

                if (bFirst)
                {
                    result = new FolderHier() { CurOPath = root };
                    current = result;

                    bFirst = false;
                }
                else
                {
                    FolderHier child = new FolderHier() { CurOPath = root };
                    current.Folders.Add(child);
                    current = child;

                    if (bGetLastHierRef) result = current;
                }
            }

            return result;
        }

        public override void Save()
        {
            // nothing need do
        }
    }
}
