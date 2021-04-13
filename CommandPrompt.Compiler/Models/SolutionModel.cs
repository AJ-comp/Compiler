using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace CommandPrompt.Compiler.Models
{
    [Serializable]
    [XmlRoot("Solution")]
    public class SolutionModel
    {
        public string Version { get; set; }
        [XmlElement("Project")] public Collection<PathModel> ProjectPaths { get; private set; } = new Collection<PathModel>();


        [XmlIgnore] public static string Extension => ".ajn";
        [XmlIgnore] public string SolutionPath { get; private set; }
        [XmlIgnore] public string FileName { get; private set; }
        [XmlIgnore]
        public IEnumerable<ProjectModel> Projects
        {
            get
            {
                List<ProjectModel> result = new List<ProjectModel>();

                foreach (var projectPath in ProjectPaths)
                {
                    var projFullPath = (projectPath.IsAbsolute) ? projectPath.FullPath
                                                                            : Path.Combine(SolutionPath, projectPath.FullPath);

                    result.Add(ProjectModel.Read(projFullPath));
                }

                return result;
            }
        }


        public static SolutionModel Read(string fullPath)
        {
            SolutionModel result;
            using (Stream reader = new FileStream(fullPath, FileMode.Open))
            {
                XmlSerializer xs = new XmlSerializer(typeof(SolutionModel));
                result = xs.Deserialize(reader) as SolutionModel;
                result.SolutionPath = Path.GetDirectoryName(fullPath);
                result.FileName = Path.GetFileName(fullPath);
            }

            return result;
        }


        public void Write(string fullPath)
        {
            using StreamWriter wr = new StreamWriter(fullPath);
            XmlSerializer xs = new XmlSerializer(typeof(SolutionModel));
            xs.Serialize(wr, this);
        }
    }
}
