using ApplicationLayer.Common.Helpers;
using ApplicationLayer.Common.Interfaces;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Data;
using System.Xml.Serialization;

using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.Models.SolutionPackage
{
    public abstract class ProjectHier : HierarchicalData, ISaveAndChangeTrackable
    {
        public abstract bool IsChanged { get; }

        public abstract void Commit();
        public abstract void RollBack();

        public override string DisplayName => this.NameWithoutExtension;

        private ProjectHier() : base(string.Empty, string.Empty)
        {
        }

        public ProjectHier(string curOpath, string fullName) : base(curOpath, fullName)
        {
            this.ToChangeDisplayName = this.DisplayName;
        }

        public override void ChangeDisplayName()
        {
            string extension = Path.GetExtension(this.FullName);

            string destFullPath = Path.Combine(this.BaseOPath, this.ToChangeDisplayName + extension);
            File.Move(this.FullPath, destFullPath);

            this.FullName = this.ToChangeDisplayName + extension;
        }

        public override void CancelChangeDisplayName() => this.ToChangeDisplayName = this.NameWithoutExtension;
    }


    [XmlInclude(typeof(ProjectProperty))]
    [XmlInclude(typeof(ReferenceHier))]
    [XmlRoot ("Project")]
    public class DefaultProjectHier : ProjectHier
    {
        [XmlIgnore]
        public string Extension => Path.GetExtension(this.FullName);

        [XmlIgnore]
        public double CurrentVersion { get; set; }
        [XmlElement("Version")]
        public double OriginalVersion { get; set; } = 0.0;

        [XmlIgnore]
        public ObservableCollection<ProjectProperty> CurrentProperties { get; } = new ObservableCollection<ProjectProperty>();
        [XmlElement("Properties")]
        public Collection<ProjectProperty> OriginalProperties { get; } = new Collection<ProjectProperty>();

        [XmlArrayItem("IncludePath")]
        public StringCollection ReferencePaths { get; } = new StringCollection();
        [XmlArrayItem("IncludePath")]
        public StringCollection HasNotItemPaths { get; } = new StringCollection();
        [XmlArrayItem("IncludePath")]
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
                if (OriginalVersion != CurrentVersion) return true;

                // compare with Properties whether equals.
                if (OriginalProperties.Count != CurrentProperties.Count) return true;
                foreach(var property in CurrentProperties)
                {
                    if (OriginalProperties.Contains(property) == false) return true;
                }

                // compare with ReferencePath whether equals.
                StringCollection referencePaths = new StringCollection();
                foreach (var item in ReferenceFolder[0].Items) referencePaths.Add(item.FullPath);
                if (ReferencePaths.Compare(referencePaths) == false) return true;

                // compare with HasNotItemPaths whether equals.
                StringCollection hasNotItemPaths = new StringCollection();
                foreach (var folder in Folders) hasNotItemPaths.AddRange(folder.HasNotItemAllPaths.ToArray());
                if (HasNotItemPaths.Compare(hasNotItemPaths) == false) return true;

                // compare with Items whether equals.
                StringCollection itemPaths = new StringCollection();
                foreach (var folder in Folders) itemPaths.AddRange(folder.HasItemAllPaths.ToArray());
                foreach (var item in Items) itemPaths.Add(item.AutoPath);
                if (ItemPaths.Compare(itemPaths) == false) return true;

                return false;
            }
        }

        private DefaultProjectHier() : this(string.Empty, string.Empty)
        { }

        public DefaultProjectHier(string curOpath, string fullName) : base(curOpath, fullName)
        {
            this.ReferenceFolder.Add(new ReferenceHier("C:\\Program Files (x86)\\AJ\\IDE\\Reference Assemblies", "Reference"));

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
            this.CurrentVersion = this.OriginalVersion;

            this.CurrentProperties.Clear();
            foreach (var item in this.OriginalProperties) this.CurrentProperties.Add(item);

            this.ReferenceFolder[0].Items.Clear();
            foreach (var item in this.ReferencePaths)
            {
                var directoryName = Path.GetDirectoryName(item);
                var fileName = Path.GetFileName(item);

                this.ReferenceFolder[0].Items.Add(new ReferenceFileStruct(directoryName, fileName));
            }

            this.Folders.Clear();
            this.Items.Clear();
            foreach (var item in this.HasNotItemPaths)
            {
                var pathChain = PathChain.CreateChainCheckItem(this.BaseOPath, item);
                if (pathChain == null) continue;

                FolderHier.AddPathChainToFolderHiers(this.Folders, pathChain);
            }

            foreach (var item in this.ItemPaths)
            {
                var directoryName = Path.GetDirectoryName(item);
                var fileName = Path.GetFileName(item);

                if (string.IsNullOrEmpty(directoryName))
                {
                    if (File.Exists(Path.Combine(this.BaseOPath, fileName))) this.Items.Add(new DefaultFileHier(fileName));
                    else this.Items.Add(new ErrorFileHier(fileName));
                }
                else
                {
                    var pathChain = PathChain.CreateChainCheckItem(this.BaseOPath, directoryName);
                    FolderHier.AddPathChainToFolderHiers(this.Folders, pathChain);
                    FolderHier leafFolderHier = FolderHier.GetLeafFolderHier(this.Folders, pathChain);
                    DefaultFileHier fileStruct = new DefaultFileHier(fileName) { Parent = leafFolderHier };

                    if (File.Exists(fileStruct.FullPath)) leafFolderHier.Items.Add(fileStruct);
                }
            }
        }

        public override void Commit()
        {
            this.OriginalVersion = this.CurrentVersion;

            this.OriginalProperties.Clear();
            foreach (var item in this.CurrentProperties) this.OriginalProperties.Add(item);

            this.ReferencePaths.Clear();
            if (this.ReferenceFolder.Count > 0)
            {
                foreach (var item in this.ReferenceFolder[0].Items) this.ReferencePaths.Add(item.FullPath);
            }

            this.HasNotItemPaths.Clear();
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

        public override void Save()
        {
            string fullPath = this.FullPath;
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            using (StreamWriter wr = new StreamWriter(fullPath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(DefaultProjectHier));
                xs.Serialize(wr, this);
            }
        }

        public static DefaultProjectHier Load(string curOpath, string fullName, string fullPath, HierarchicalData parent)
        {
            DefaultProjectHier result = new DefaultProjectHier(curOpath, fullName);

            using (StreamReader sr = new StreamReader(fullPath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(DefaultProjectHier));
                result = xs.Deserialize(sr) as DefaultProjectHier;
                result.CurOPath = curOpath;
                result.FullName = fullName;
                result.ToChangeDisplayName = result.DisplayName;
                result.Parent = parent;

                result.RollBack();
            }

            return result;
        }
    }


    public class ErrorProjectHier : ProjectHier
    {
        public override string DisplayName => base.DisplayName + string.Format(" ({0})", CommonResource.NotLoad);

        public override bool IsChanged => false;

        public override void Commit()
        {
        }

        public override void RollBack()
        {
        }

        public override void Save()
        {
        }

        private ErrorProjectHier() : this(string.Empty, string.Empty)
        {
        }

        public ErrorProjectHier(string curOpath, string fullName) : base(curOpath, fullName)
        {
            this.ToChangeDisplayName = this.DisplayName;
        }
    }
}
