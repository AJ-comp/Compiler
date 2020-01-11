using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using WpfApp.Utilities.GeneratorPackages.ProjectStructs;

namespace WpfApp.Models
{
    public class HirStruct
    {
        public string Path { get; set; }
        public string FullName { get; set; }
        public string ImageSource { get; set; }

        public string FullPath => System.IO.Path.Combine(this.Path, this.FullName);
        public string NameWithoutExtension => System.IO.Path.GetFileNameWithoutExtension(this.FullName);
    }

    public class SolutionStruct : HirStruct, IXmlSerializable
    {
        public double Version { get; set; }

        public ObservableCollection<ProjectStruct> Projects { get; set; } = new ObservableCollection<ProjectStruct>();

        public SolutionStruct()
        {
            this.Projects.CollectionChanged += Projects_CollectionChanged;
        }

        private void Projects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            for(int i=e.NewStartingIndex; i<e.NewItems.Count; i++)
            {
                ProjectStruct child = e.NewItems[i] as ProjectStruct;

                if (File.Exists(child.FullPath)) continue;

                Directory.CreateDirectory(child.Path);

                using (StreamWriter wr = new StreamWriter(child.FullPath))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(ProjectStruct));
                    xs.Serialize(wr, child);
                }
            }
        }

        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            this.Version = double.Parse(reader.GetAttribute("Ver"));

            //reader.Read();
            //this.Name = reader.ReadElementContentAsString("Name", "");
            //this.Dept = reader.ReadElementContentAsString("Dept", "");
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Ver", this.Version.ToString());

            foreach(var project in this.Projects)
            {
                writer.WriteStartElement("Project");
                writer.WriteElementString("Path", project.Path);
                writer.WriteElementString("Name", project.FullName);
                writer.WriteEndElement();
            }
        }
    }
}
