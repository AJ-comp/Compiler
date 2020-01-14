using Parse.BackEnd.Target;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class HirStruct
    {
        [XmlIgnore]
        public string OPath { get; set; } = string.Empty;
        [XmlIgnore]
        public string FullName { get; set; } = string.Empty;
        [XmlIgnore]
        public string ImageSource { get; set; }
        [XmlIgnore]
        public HirStruct Parent { get; internal set; } = null;
        [XmlIgnore]
        public string NameWithoutExtension => Path.GetFileNameWithoutExtension(this.FullName);
        [XmlIgnore]
        public string FullPath => Path.Combine(this.BasePath, this.FullName);

        [XmlIgnore]
        public string BasePath
        {
            get
            {
                string result = this.OPath;

                HirStruct current = this;
                while(current.Parent != null)
                {
                    result = Path.Combine(current.Parent.OPath, result);
                    current = current.Parent;
                }

                return result;
            }
        }
    }

    [XmlInclude(typeof(ProjectStruct))]
    public class SolutionStruct : HirStruct, IXmlSerializable
    {
        [XmlIgnore]
        public static string Extension => "ajn";
        public double Version { get; set; }

        private StringCollection projectPaths = new StringCollection();
        public ObservableCollection<ProjectStruct> Projects { get; set; } = new ObservableCollection<ProjectStruct>();

        public SolutionStruct()
        {
            this.Projects.CollectionChanged += Projects_CollectionChanged;
        }

        private void Projects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for(int i=0; i<e.NewItems.Count; i++)
            {
                ProjectStruct child = e.NewItems[i] as ProjectStruct;
                child.Parent = this;

                if (File.Exists(child.FullPath)) continue;

                Directory.CreateDirectory(child.BasePath);

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
            this.Version = double.Parse(reader.GetAttribute("Version"));

            while(reader.MoveToAttribute("Project"))
                this.projectPaths.Add(reader.GetAttribute("Include"));

            foreach(var path in projectPaths)
            {
                ProjectStruct loadProject = new ProjectStruct();

                var fullPath = Path.Combine(this.BasePath, path);
                using (StreamReader wr= new StreamReader(fullPath))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(ProjectStruct));
                    loadProject = xs.Deserialize(wr) as ProjectStruct;

                    this.Projects.Add(loadProject);
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Version", this.Version.ToString());

            foreach (var project in this.Projects)
            {
                writer.WriteStartElement("Project");
                var relatePath = Path.Combine(project.OPath, project.FullName);
                writer.WriteAttributeString("Include", relatePath);
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

            result.Projects.Add(projectGenerator.CreateDefaultProject(solutionName, solutionName, target, result));
            result.Projects.Add(projectGenerator.CreateDefaultProject(solutionName, solutionName+"abc", target, result));

            return result;
        }
    }
}
