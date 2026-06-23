using Compile.AJ;
using Parse.FrontEnd;
using Parse.FrontEnd.AJ.Sdts.AstNodes;
using Parse.MiddleEnd.IR.LLVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace CommandPrompt.Builder.Models
{
    [Serializable]
    [XmlRoot("Solution")]
    public class Solution
    {
        public string Version { get; set; }
        [XmlElement("Project")] public Collection<PathModel> ProjectPaths { get; private set; } = new Collection<PathModel>();


        [XmlIgnore] public static string Extension => ".sln";
        [XmlIgnore] public string SolutionPath { get; private set; }
        [XmlIgnore] public string FileName { get; private set; }
        [XmlIgnore] public bool PrintParsingHistory { get; set; }
        [XmlIgnore]
        public IEnumerable<AJProject> Projects
        {
            get
            {
                List<AJProject> result = new List<AJProject>();

                foreach (var projectPath in ProjectPaths)
                {
                    var projFullPath = projectPath.IsAbsolute ? projectPath.FullPath
                                                                            : Path.Combine(SolutionPath, projectPath.FullPath);

                    result.Add(AJProject.Read(projFullPath));
                }

                return result;
            }
        }


        public ProjectBuildResult Build(AJCompiler compiler)
        {
            // it has to set the build order through the reference information.

            var result = new ProjectBuildResult();
            foreach (var project in Projects)
            {
                result = project.Build(compiler, PrintParsingHistory);
            }

            return result;
        }



        public static Solution Read(string fullPath)
        {
            Solution result;
            using (Stream reader = new FileStream(fullPath, FileMode.Open))
            {
                XmlSerializer xs = new XmlSerializer(typeof(Solution));
                result = xs.Deserialize(reader) as Solution;
                result.SolutionPath = Path.GetDirectoryName(fullPath);
                result.FileName = Path.GetFileName(fullPath);
            }

            return result;
        }

        public void Write(string fullPath)
        {
            using StreamWriter wr = new StreamWriter(fullPath);
            XmlSerializer xs = new XmlSerializer(typeof(Solution));
            xs.Serialize(wr, this);
        }

        public void Write()
        {
            using StreamWriter wr = new StreamWriter(Path.Combine(SolutionPath, FileName));
            XmlSerializer xs = new XmlSerializer(typeof(Solution));
            xs.Serialize(wr, this);
        }
    }
}
