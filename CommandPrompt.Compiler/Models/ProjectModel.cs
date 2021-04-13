using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace CommandPrompt.Compiler.Models
{
    public class ProjectModel
    {
        public bool IsStarting { get; set; } = false;
        public string Target { get; set; }

        [XmlElement("RemoveFiles")] public Collection<PathModel> RemoveFiles { get; } = new Collection<PathModel>();
        [XmlElement("AddExternalFiles")] public Collection<PathModel> ExternalFiles { get; } = new Collection<PathModel>();


        [XmlIgnore] public static string Extension => ".ajproj";
        [XmlIgnore] public string ProjectPath { get; private set; }
        [XmlIgnore] public string FileName { get; private set; }
        [XmlIgnore] public string FullPath => Path.Combine(ProjectPath, FileName);

        [XmlIgnore] public IEnumerable<string> SourcePaths
        {
            get
            {
                List<string> result = new List<string>();

                string[] filePaths = Directory.GetFiles(ProjectPath, 
                                                                      $"*{SourceModel.Extension}", 
                                                                      SearchOption.TopDirectoryOnly);
                result.AddRange(filePaths);

                return result;
            }
        }


        public static ProjectModel Read(string fullPath)
        {
            ProjectModel result;
            using (Stream reader = new FileStream(fullPath, FileMode.Open))
            {
                XmlSerializer xs = new XmlSerializer(typeof(ProjectModel));
                result = xs.Deserialize(reader) as ProjectModel;
                result.ProjectPath = Path.GetDirectoryName(fullPath);
                result.FileName = Path.GetFileName(fullPath);
            }

            return result;
        }


        public void Write(string fullPath)
        {
            using StreamWriter wr = new StreamWriter(fullPath);
            XmlSerializer xs = new XmlSerializer(typeof(ProjectModel));
            xs.Serialize(wr, this);
        }
    }
}
