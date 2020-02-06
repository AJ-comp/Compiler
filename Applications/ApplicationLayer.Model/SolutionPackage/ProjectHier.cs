using ApplicationLayer.Common.Helpers;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Data;
using System.Xml.Serialization;

using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.Models.SolutionPackage
{
    public abstract class ProjectHier : HierarchicalData, IChangeTrackable
    {
        public abstract bool IsChanged { get; }

        public abstract void Commit();
        public abstract void RollBack();
    }


    [XmlInclude(typeof(ProjectProperty))]
    [XmlInclude(typeof(ReferenceHier))]
    public class DefaultProjectHier : ProjectHier
    {
        [XmlIgnore]
        public string Extension => Path.GetExtension(this.FullName);

        private double originalVersion = 0.0;
        public double Version { get; set; }

        private Collection<ProjectProperty> originalProperties = new Collection<ProjectProperty>();
        public ObservableCollection<ProjectProperty> Properties { get; } = new ObservableCollection<ProjectProperty>();

        public StringCollection ReferencePaths { get; } = new StringCollection();
        public StringCollection HasNotItemPaths { get; } = new StringCollection();
        public StringCollection ItemPaths { get; } = new StringCollection();

        [XmlIgnore]
        public ObservableCollection<ReferenceHier> ReferenceFolder { get; } = new ObservableCollection<ReferenceHier>();
        [XmlIgnore]
        public ObservableCollection<FolderHier> Folders { get; } = new ObservableCollection<FolderHier>();
        [XmlIgnore]
        public ObservableCollection<FileHier> Items { get; } = new ObservableCollection<FileHier>();

        [XmlIgnore]
        public IList Children
        {
            get
            {
                return new CompositeCollection()
                {
                    new CollectionContainer() { Collection = ReferenceFolder },
                    new CollectionContainer() { Collection = Folders },
                    new CollectionContainer() { Collection = Items }
                };
            }
        }

        public override bool IsChanged
        {
            get
            {
                // compare with Version whether equals.
                if (originalVersion != Version) return true;

                // compare with Properties whether equals.
                if (originalProperties.Count != Properties.Count) return true;
                foreach(var property in Properties)
                {
                    if (originalProperties.Contains(property) == false) return true;
                }

                // compare with ReferencePath whether equals.
                StringCollection referencePaths = new StringCollection();
                foreach (var item in ReferenceFolder[0].Items) referencePaths.Add(item.FullPath);
                if (ReferenceFolder[0].Items.Equals(referencePaths) == false) return true;

                // compare with HasNotItemPaths whether equals.
                StringCollection hasNotItemPaths = new StringCollection();
                foreach (var folder in Folders) hasNotItemPaths.AddRange(folder.HasNotItemAllPaths.ToArray());
                if (HasNotItemPaths.Equals(hasNotItemPaths) == false) return true;

                // compare with Items whether equals.
                StringCollection itemPaths = new StringCollection();
                foreach (var folder in Folders) itemPaths.AddRange(folder.HasItemAllPaths.ToArray());
                foreach (var item in Items) ItemPaths.Add(item.AutoPath);
                if (itemPaths.Equals(hasNotItemPaths) == false) return true;

                return false;
            }
        }

        public DefaultProjectHier()
        {
            this.ReferenceFolder.Add(new ReferenceHier() { CurOPath = "C:\\Program Files (x86)\\AJ\\IDE\\Reference Assemblies", FullName = "Reference" });

            this.ReferenceFolder[0].Items.CollectionChanged += ReferenceItems_CollectionChanged;
            this.Folders.CollectionChanged += Folders_CollectionChanged;
            this.Items.CollectionChanged += Items_CollectionChanged;
        }

        private void ReferenceItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems?.Count; i++)
            {
            }
        }

        private void Folders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems?.Count; i++)
            {
                FolderHier folder = e.NewItems[i] as FolderHier;
                folder.Parent = this;

                folder.Folders.CollectionChanged += SubFolders_CollectionChanged;
                folder.Items.CollectionChanged += SubItems_CollectionChanged;
            }
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems?.Count; i++)
            {
                FileHier item = e.NewItems[i] as FileHier;
                item.Parent = this;
            }
        }

        private void SubFolders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems?.Count; i++)
            {
                FolderHier folder = e.NewItems[i] as FolderHier;

                folder.Folders.CollectionChanged += SubFolders_CollectionChanged;
                folder.Items.CollectionChanged += SubItems_CollectionChanged;
            }
        }

        private void SubItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

        private string FindExistPathFromItemPaths(HierarchicalData findTarget)
        {
            string result = string.Empty;

            if(findTarget.IsAbsolutePath)
            {
//                if(findTarget.FullPath)
            }
            else
            {

            }

            return result;
        }

        /// <summary>
        /// This function removes the child after find child type.
        /// </summary>
        /// <param name="child">child to remove</param>
        public void RemoveChild(HierarchicalData child)
        {
            if (child is FolderHier) this.Folders.Remove(child as FolderHier);
            else if (child is FileHier) this.Items.Remove(child as FileHier);
            else if (child is ReferenceFileStruct) this.ReferenceFolder[0].Items.Remove(child as ReferenceFileStruct);
        }

        public override void RollBack()
        {
            this.ReferenceFolder[0].Items.Clear();
            foreach (var item in this.ReferencePaths)
            {
                var directoryName = Path.GetDirectoryName(item);
                var fileName = Path.GetFileName(item);

                this.ReferenceFolder[0].Items.Add(new ReferenceFileStruct() { CurOPath = directoryName, FullName = fileName });
            }

            this.Folders.Clear();
            this.Items.Clear();
            foreach (var item in this.HasNotItemPaths)
            {
                var directoryName = Path.GetFileName(item);

                this.Folders.Add(FolderHier.GetFolderSet(this.BaseOPath, directoryName));
            }

            foreach (var item in this.ItemPaths)
            {
                var directoryName = Path.GetDirectoryName(item);
                var fileName = Path.GetFileName(item);

                if (string.IsNullOrEmpty(directoryName))
                {
                    if (System.IO.File.Exists(Path.Combine(this.BaseOPath, fileName))) this.Items.Add(new DefaultFileHier() { FullName = fileName });
                    else this.Items.Add(new ErrorFileHier() { FullName = fileName });
                }
                else
                {
                    FolderHier folderStruct = FolderHier.GetFolderSet(this.BaseOPath, directoryName);
                    DefaultFileHier fileStruct = new DefaultFileHier() { Parent = folderStruct, FullName = fileName };
                    if (System.IO.File.Exists(fileStruct.FullPath)) folderStruct.Items.Add(fileStruct);
                    this.Folders.Add(folderStruct);
                }
            }
        }

        public override void Commit()
        {
            this.ReferencePaths.Clear();
            if (this.ReferenceFolder.Count > 0)
            {
                foreach (var item in this.ReferenceFolder[0].Items) this.ReferencePaths.Add(item.FullPath);
            }

            this.ItemPaths.Clear();
            foreach (var folder in this.Folders)
            {
                // The folders or files in the ProjectStruct uses only relative path.
                foreach (var item in folder.HasItemAllPaths) this.ItemPaths.Add(item);
                foreach (var item in folder.HasNotItemAllPaths) this.HasNotItemPaths.Add(item);
            }

            foreach (var item in this.Items)
            {
                // The folders or files in the ProjectStruct uses only relative path.
                this.ItemPaths.Add(item.FullName);
            }
        }
    }


    public class ErrorProjectHier : ProjectHier
    {
        public string DisplayName => NameWithoutExtension + string.Format(" ({0})", CommonResource.NotLoad);

        public override bool IsChanged => false;

        public override void Commit()
        {
        }

        public override void RollBack()
        {
        }
    }
}
