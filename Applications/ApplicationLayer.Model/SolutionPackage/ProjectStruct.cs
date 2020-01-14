using Parse.BackEnd.Target;
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
    [XmlInclude(typeof(ProjectProperty))]
    [XmlInclude(typeof(FolderStruct))]
    [XmlInclude(typeof(FileStruct))]
    public class ProjectStruct : HirStruct, IXmlSerializable
    {
        public double Version { get; set; }

        public ObservableCollection<ProjectProperty> Properties { get; } = new ObservableCollection<ProjectProperty>();
        public ObservableCollection<ReferenceStruct> ReferenceFolder { get; } = new ObservableCollection<ReferenceStruct>();
        public ObservableCollection<FolderStruct> Folders { get; } = new ObservableCollection<FolderStruct>();
        public ObservableCollection<FileStruct> Items { get; } = new ObservableCollection<FileStruct>();

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
            this.ReferenceFolder.Add(new ReferenceStruct() { OPath = "C:\\Program Files (x86)\\AJ\\IDE\\Reference Assemblies", FullName = "Reference" });

            this.Folders.CollectionChanged += Folders_CollectionChanged;
            this.Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Folders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems.Count; i++)
            {
                FolderStruct folder = e.NewItems[i] as FolderStruct;
                folder.Parent = this;
            }
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems.Count; i++)
            {
                FileStruct item = e.NewItems[i] as FileStruct;
                item.Parent = this;

                if (File.Exists(item.FullPath)) continue;

                Directory.CreateDirectory(item.BasePath);
                File.WriteAllText(item.FullPath, item.Data);
            }
        }

        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            this.Version = double.Parse(reader.GetAttribute("Version"));

            StringCollection importedFiles = new StringCollection();
            while (reader.MoveToAttribute("ItemGroup"))
                importedFiles.Add(reader.GetAttribute("Include"));

            // If files have not extension how?
            foreach (var path in importedFiles)
            {
                ProjectStruct loadProject = new ProjectStruct();
                FolderStruct folderStruct = FolderStruct.GetFolderSet(this.BasePath, path);
                loadProject.Folders.Add(folderStruct);

                if (File.Exists(Path.Combine(this.BasePath, path)) == false) continue;
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("Version", this.Version.ToString());

            foreach (var property in this.Properties)
            {
                writer.WriteStartElement("PropertyGroup");
                writer.WriteElementString("Mode", property.Mode.ToString());
                writer.WriteElementString("Target", property.Target.ToString());
                writer.WriteElementString("OptimizeLevel", property.OptimizeLevel.ToString());
                writer.WriteEndElement();
            }

            var referenceFileStructs = this.ReferenceFolder[0].Items;
            foreach (var item in referenceFileStructs)
            {
                writer.WriteStartElement("Reference");
                writer.WriteAttributeString("Include", item.FullPath);
                writer.WriteEndElement();
            }

            foreach (var item in this.Items)
            {
                writer.WriteStartElement("ItemGroup");
                writer.WriteAttributeString("Include", Path.Combine(item.OPath, item.FullName));
                writer.WriteEndElement();
            }
        }
    }




    public class ReferenceStruct : HirStruct
    {
        public ObservableCollection<ReferenceFileStruct> Items { get; } = new ObservableCollection<ReferenceFileStruct>();

        public ReferenceStruct()
        {
            this.Items.CollectionChanged += ReferenceFiles_CollectionChanged;
        }

        private void ReferenceFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems.Count; i++)
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
