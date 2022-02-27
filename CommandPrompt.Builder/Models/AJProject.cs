using Compile.AJ;
using AJ.Common.Helpers;
using Parse.FrontEnd;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace CommandPrompt.Builder.Models
{
    public class AJProject
    {
        public bool IsStarting { get; set; } = false;
        public string Target { get; set; }

        [XmlElement("RemoveFiles")] public Collection<PathModel> RemoveFiles { get; } = new Collection<PathModel>();
        [XmlElement("References")] public Collection<PathModel> References { get; } = new Collection<PathModel>();


        [XmlIgnore] public static string Extension => ".ajproj";
        [XmlIgnore] public string ProjectPath { get; private set; }
        [XmlIgnore] public string FileName { get; private set; }
        [XmlIgnore] public string FullPath => Path.Combine(ProjectPath, FileName);


        public ProjectBuildResult Build(AJCompiler compiler, bool printParsingHistory = false)
        {
            var result = new ProjectBuildResult(ProjectPath);
            var sources = Directory.GetFiles(ProjectPath, "*.aj");

            foreach (var source in sources)
            {
                var compileParameter = new CompileParameter();
                compileParameter.FileFullPath = Path.Combine(ProjectPath, source);

                var compileResult = compiler.Compile(compileParameter);
                compileParameter.ReferenceFiles.Add(compileParameter.FileFullPath, compileResult.RootNode);

                if(printParsingHistory)
                {
                    var file = $"{Path.GetFileNameWithoutExtension(source)}.csv";
                    var dir = $"{ProjectPath}/Debug";

                    Directory.CreateDirectory(dir);
                    compileResult.ParsingResult.ToParsingHistory.ToCSV(Path.Combine(dir, file));
                }

                result.Add(compileParameter.FileFullPath, compileResult);
            }

            return result;
        }


        public static AJProject Read(string fullPath)
        {
            AJProject result;
            using (Stream reader = new FileStream(fullPath, FileMode.Open))
            {
                XmlSerializer xs = new XmlSerializer(typeof(AJProject));
                result = xs.Deserialize(reader) as AJProject;
                result.ProjectPath = Path.GetDirectoryName(fullPath);
                result.FileName = Path.GetFileName(fullPath);
            }

            return result;
        }


        public void Write(string fullPath)
        {
            using StreamWriter wr = new StreamWriter(fullPath);
            XmlSerializer xs = new XmlSerializer(typeof(AJProject));
            xs.Serialize(wr, this);
        }
    }
}
