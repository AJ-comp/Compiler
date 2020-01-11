using Parse.BackEnd.Target;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Data;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using WpfApp.Models;

namespace WpfApp.Utilities.GeneratorPackages.ProjectStructs
{
    public class ProjectProperty
    {
        public enum Configure { Debug, Release }

        public Configure Mode { get; set; }
        public Target Target { get; set; }
        public int OptimizeLevel { get; set; }
    }

    public class ProjectStruct : HirStruct, IXmlSerializable
    {
        public double Version { get; set; }
        public ObservableCollection<ProjectProperty> Properties { get; } = new ObservableCollection<ProjectProperty>();

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

        public ProjectStruct()
        {
            this.Folders.CollectionChanged += Folders_CollectionChanged;
            this.Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Folders_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            for (int i = e.NewStartingIndex; i < e.NewItems.Count; i++)
            {
                FileStruct item = e.NewItems[i] as FileStruct;

                if (File.Exists(this.FullPath)) continue;

                Directory.CreateDirectory(this.Path);

                using (StreamWriter wr = new StreamWriter(this.FullPath))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(ProjectStruct));
                    xs.Serialize(wr, item);
                }
            }
        }

        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            throw new System.NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (var property in this.Properties)
            {
                writer.WriteStartElement("PropertyGroup");
                writer.WriteElementString("Mode", property.Mode.ToString());
                writer.WriteElementString("Target", property.Target.ToString());
                writer.WriteElementString("OptimizeLevel", property.OptimizeLevel.ToString());
                writer.WriteEndElement();
            }


        }
    }




    public class ReferenceStruct : HirStruct
    {
        public ObservableCollection<ReferenceFileStruct> ReferenceFiles { get; } = new ObservableCollection<ReferenceFileStruct>();

        public ReferenceStruct()
        {
            this.ReferenceFiles.CollectionChanged += ReferenceFiles_CollectionChanged;
        }

        private void ReferenceFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

        }
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

        public FolderStruct()
        {
            this.Folders.CollectionChanged += Folders_CollectionChanged;
            this.Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Folders_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

        }
    }

    public class FileStruct : HirStruct
    {
    }
}
