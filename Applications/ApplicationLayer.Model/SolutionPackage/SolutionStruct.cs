using Parse.BackEnd.Target;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class HirStruct
    {
        public string OPath { get; set; }
        public string FullName { get; set; }
        public string ImageSource { get; set; }

        public string FullPath => Path.Combine(this.OPath, this.FullName);
        public string NameWithoutExtension => Path.GetFileNameWithoutExtension(this.FullName);
    }

    public class SolutionStruct : HirStruct, IXmlSerializable
    {
        public static string Extension => "ajn";
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

                Directory.CreateDirectory(child.OPath);

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
                writer.WriteElementString("Path", project.OPath);
                writer.WriteElementString("Name", project.FullName);
                writer.WriteEndElement();
            }
        }

        public void Load(string path, string name)
        {

        }

        public static SolutionStruct Create(string solutionPath, string solutionName, bool bCreateSolutionFolder, Grammar grammar, Target target)
        {
            SolutionStruct result = new SolutionStruct();

            if (bCreateSolutionFolder)
                solutionPath = Path.Combine(solutionPath, solutionName);

            result.OPath = solutionPath;
            result.FullName = string.Format("{0}.{1}", solutionName, SolutionStruct.Extension);
            result.Version = 1.0;

            ProjectGenerator projectGenerator = null;
            if (grammar is MiniCGrammar)
                projectGenerator = new MiniCGenerator();

            if (projectGenerator == null) return result;

            var projectPath = solutionPath + "\\" + solutionName;
            result.Projects.Add(projectGenerator.Generator(projectPath, solutionName, target));

            return result;
        }
    }
}
