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
    public class ProjectStruct : HirStruct
    {
        public double Version { get; set; }
    }


    [XmlInclude(typeof(ProjectProperty))]
    [XmlInclude(typeof(ReferenceStruct))]
    public class DefaultProjectStruct : ProjectStruct
    {
        [XmlIgnore]
        public string Extension => Path.GetExtension(this.FullName);

        public ObservableCollection<ProjectProperty> Properties { get; } = new ObservableCollection<ProjectProperty>();

        public StringCollection ReferencePaths { get; } = new StringCollection();
        public StringCollection HasNotItemPaths { get; } = new StringCollection();
        public StringCollection ItemPaths { get; } = new StringCollection();

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

        public DefaultProjectStruct()
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

                if (this.ReferencePaths.Contains(referenceFile.FullPath) == false) this.ReferencePaths.Add(referenceFile.FullPath);
            }
        }

        private void Folders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems?.Count; i++)
            {
                FolderStruct folder = e.NewItems[i] as FolderStruct;
                folder.Parent = this;

                // The folder or file path always uses only relative path.
                var pathInfo = folder.RelativePath;
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

                // The folder or file path always uses only relative path.
                var pathInfo = item.RelativePath;
                if (this.ItemPaths.Contains(pathInfo) == false) this.ItemPaths.Add(pathInfo);
            }
        }

        private void SubFolders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems?.Count; i++)
            {
                FolderStruct folder = e.NewItems[i] as FolderStruct;

                // The folder or file path always uses only relative path.
                var findPath = folder.RelativePath;
                int index = this.ItemPaths.FindIndex(findPath);
                if (index >= 0)
                    this.ItemPaths[index] = folder.FullPath;

                folder.Folders.CollectionChanged += SubFolders_CollectionChanged;
                folder.Items.CollectionChanged += SubItems_CollectionChanged;
            }
        }

        private void SubItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems?.Count; i++)
            {
                DefaultFileStruct item = e.NewItems[i] as DefaultFileStruct;

                // The folder or file path always uses only relative path.
                var findPath = item.RelativePath;
                int index = this.ItemPaths.FindIndex(findPath);
                if (index >= 0)
                    this.ItemPaths[index] = item.FullPath;
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
        /// This function removes the child after find child type.
        /// </summary>
        /// <param name="child">child to remove</param>
        public void RemoveChild(HirStruct child)
        {
            if (child is FolderStruct) this.Folders.Remove(child as FolderStruct);
            else if (child is FileStruct) this.Items.Remove(child as FileStruct);
            else if (child is ReferenceFileStruct) this.ReferenceFolder[0].Items.Remove(child as ReferenceFileStruct);
        }

        /// <summary>
        /// This function syncronize with object based on xml.
        /// </summary>
        public void SyncXmlToObject()
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
            foreach(var item in this.HasNotItemPaths)
            {
                var directoryName = Path.GetFileName(item);

                this.Folders.Add(FolderStruct.GetFolderSet(this.BaseOPath, directoryName));
            }

            foreach (var item in this.ItemPaths)
            {
                var directoryName = Path.GetDirectoryName(item);
                var fileName = Path.GetFileName(item);

                if (string.IsNullOrEmpty(directoryName))
                {
                    if (System.IO.File.Exists(Path.Combine(this.BaseOPath, fileName))) this.Items.Add(new DefaultFileStruct() { FullName = fileName });
                    else this.Items.Add(new ErrorFileStruct() { FullName = fileName });
                }
                else
                {
                    FolderStruct folderStruct = FolderStruct.GetFolderSet(this.BaseOPath, directoryName);
                    DefaultFileStruct fileStruct = new DefaultFileStruct() { Parent = folderStruct, FullName = fileName };
                    if(System.IO.File.Exists(fileStruct.FullPath)) folderStruct.Items.Add(fileStruct);
                    this.Folders.Add(folderStruct);
                }
            }
        }

        /// <summary>
        /// This function syncronize with xml based on object.
        /// </summary>
        public void SyncObjectToXml()
        {
            this.ReferencePaths.Clear();
            if(this.ReferenceFolder.Count > 0)
            {
                foreach(var item in this.ReferenceFolder[0].Items) this.ReferencePaths.Add(item.FullPath);
            }

            this.ItemPaths.Clear();
            foreach(var folder in this.Folders)
            {
                // The folders or files in the ProjectStruct uses only relative path.
                foreach (var item in folder.HasItemAllPaths) this.ItemPaths.Add(item);
                foreach (var item in folder.HasNotItemAllPaths) this.HasNotItemPaths.Add(item);
            }

            foreach(var item in this.Items)
            {
                // The folders or files in the ProjectStruct uses only relative path.
                this.ItemPaths.Add(item.FullName);
            }
        }
    }


    public class ErrorProjectStruct : ProjectStruct
    {
        public string DisplayName => NameWithoutExtension + string.Format(" ({0})", CommonResource.NotLoad);
    }
}
