using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Data;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    [XmlInclude(typeof(ProjectProperty))]
    [XmlInclude(typeof(ReferenceStruct))]
    public class ProjectStruct : HirStruct
    {
        [XmlIgnore]
        public string Extension => Path.GetExtension(this.FullName);
        public double Version { get; set; }

        public ObservableCollection<ProjectProperty> Properties { get; } = new ObservableCollection<ProjectProperty>();

        public StringCollection referencePaths { get; } = new StringCollection();

        [XmlIgnore]
        public List<PathInfo> ItemPaths { get; } = new List<PathInfo>();

        [XmlIgnore]
        public ObservableCollection<ReferenceStruct> ReferenceFolder { get; } = new ObservableCollection<ReferenceStruct>();
        [XmlIgnore]
        public ObservableCollection<FolderStruct> Folders { get; } = new ObservableCollection<FolderStruct>();
        [XmlIgnore]
        public ObservableCollection<FileStruct> Items { get; } = new ObservableCollection<FileStruct>();

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

        public ProjectStruct()
        {
            this.ReferenceFolder.Add(new ReferenceStruct() { CurOPath = "C:\\Program Files (x86)\\AJ\\IDE\\Reference Assemblies", FullName = "Reference" });

            this.ReferenceFolder[0].Items.CollectionChanged += ReferenceItems_CollectionChanged;
            this.Folders.CollectionChanged += Folders_CollectionChanged;
            this.Items.CollectionChanged += Items_CollectionChanged;
        }

        private void ReferenceItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems?.Count; i++)
            {
                ReferenceFileStruct referenceFile = e.NewItems[i] as ReferenceFileStruct;

                if (this.referencePaths.Contains(referenceFile.FullPath) == false) this.referencePaths.Add(referenceFile.FullPath);
            }
        }

        private void Folders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems?.Count; i++)
            {
                FolderStruct folder = e.NewItems[i] as FolderStruct;
                folder.Parent = this;

                var pathInfo = (folder.IsAbsolutePath) ? new PathInfo(folder.FullPath, true) : new PathInfo(folder.RelativePath, false);
                if (this.ItemPaths.Contains(pathInfo) == false) this.ItemPaths.Add(pathInfo);

                folder.Folders.CollectionChanged += SubFolders_CollectionChanged;
                folder.Items.CollectionChanged += SubItems_CollectionChanged;
            }
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems?.Count; i++)
            {
                FileStruct item = e.NewItems[i] as FileStruct;
                item.Parent = this;

                var pathInfo = (item.IsAbsolutePath) ? new PathInfo(item.FullPath, true) : new PathInfo(item.RelativePath, false);
                if (this.ItemPaths.Contains(pathInfo) == false) this.ItemPaths.Add(pathInfo);

                if (File.Exists(item.FullPath)) continue;

                Directory.CreateDirectory(item.BaseOPath);
                File.WriteAllText(item.FullPath, item.Data);
            }
        }

        private void SubFolders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems?.Count; i++)
            {
                FolderStruct folder = e.NewItems[i] as FolderStruct;
                var findPath = new PathInfo(folder.AutoPath, true);
                int index = this.ItemPaths.FindIndex(a => (a == findPath));
                if (index >= 0)
                    this.ItemPaths[index].Path = folder.FullPath;

                folder.Folders.CollectionChanged += SubFolders_CollectionChanged;
                folder.Items.CollectionChanged += SubItems_CollectionChanged;
            }
        }

        private void SubItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems?.Count; i++)
            {
                FileStruct item = e.NewItems[i] as FileStruct;
                var findPath = new PathInfo(item.AutoPath, true);
                int index = this.ItemPaths.FindIndex(a => (a == findPath));
                if (index >= 0)
                    this.ItemPaths[index].Path = item.FullPath;
            }
        }

        private string FindExistPathFromItemPaths(HirStruct findTarget)
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
        /// This function syncronize with object based on xml.
        /// </summary>
        public void SyncXmlToObject()
        {
            this.ReferenceFolder[0].Items.Clear();
            foreach (var item in this.referencePaths)
            {
                var directoryName = Path.GetDirectoryName(item);
                var fileName = Path.GetFileName(item);

                this.ReferenceFolder[0].Items.Add(new ReferenceFileStruct() { CurOPath = directoryName, FullName = fileName });
            }

            this.Folders.Clear();
            this.Items.Clear();
            foreach (var item in this.ItemPaths)
            {
                var directoryName = Path.GetDirectoryName(item.Path);
                var fileName = Path.GetFileName(item.Path);

                if (string.IsNullOrEmpty(directoryName))
                    this.Items.Add(new FileStruct() { FullName = fileName });
                else
                {
                    FolderStruct subFolder = FolderStruct.GetFolderSet(this.BaseOPath, directoryName);
                    subFolder.Items.Add(new FileStruct() { FullName = fileName });
                    this.Folders.Add(subFolder);
                }
            }
        }

        /// <summary>
        /// This function syncronize with xml based on object.
        /// </summary>
        public void SyncObjectToXml()
        {
            this.referencePaths.Clear();

            if(this.ReferenceFolder.Count > 0)
            {
                foreach(var item in this.ReferenceFolder[0].Items) this.referencePaths.Add(item.FullPath);
            }


        }
    }


    [XmlInclude(typeof(ReferenceFileStruct))]
    public class ReferenceStruct : HirStruct
    {
        public ObservableCollection<ReferenceFileStruct> Items { get; } = new ObservableCollection<ReferenceFileStruct>();

        public ReferenceStruct()
        {
            this.Items.CollectionChanged += ReferenceFiles_CollectionChanged;
        }

        private void ReferenceFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems?.Count; i++)
            {
                ReferenceFileStruct item = e.NewItems[i] as ReferenceFileStruct;
                item.Parent = this;
            }
        }
    }

    public class ReferenceFileStruct : HirStruct
    {
    }


    public class FileStruct : HirStruct
    {
        public string Data { get; set; }
    }
}
