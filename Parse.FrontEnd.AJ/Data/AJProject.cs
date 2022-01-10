using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Parse.FrontEnd.AJ.Data
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class AJProject
    {
        [XmlElement("Debug Property")] public ProjectProperty DebugConfigure { get; set; } = new ProjectProperty();
        [XmlElement("Release Property")] public ProjectProperty ReleaseConfigure { get; set; } = new ProjectProperty();
        public string AssemblyName { get; set; }
        [XmlElement("IsStartingProject")] public string AbsoluteFullPath { get; set; } = string.Empty;
        [XmlIgnore] public IReadOnlyCollection<string> InternalFileRFullPaths => _internalFilePaths;
        public HashSet<string> ExceptFilePaths { get; } = new HashSet<string>();
        public HashSet<AJProject> ReferenceProjects { get; } = new HashSet<AJProject>();

        [XmlIgnore] public string Name => Path.GetFileName(AbsoluteFullPath);

        /************************************************************/
        /// <summary>
        /// <para>Get source file(extension = .aj) full path list in the project.</para>
        /// <para>확장자가 .aj 인 소스 파일 전체 경로 리스트를 가져옵니다. </para>
        /// </summary>
        /************************************************************/
        [XmlIgnore] public IEnumerable<string> SourceFileAFullPaths
        {
            get
            {
                List<string> result = new List<string>();

                foreach (var fileRFullPath in InternalFileRFullPaths)
                {
                    if (Path.GetExtension(fileRFullPath) != ".aj") continue;

                    var projectAPath = Path.GetDirectoryName(AbsoluteFullPath);
                    result.Add(Path.Combine(projectAPath, fileRFullPath));
                }

                return result;
            }
        }


        public AJProject()
        {
        }

        public AJProject(string assemblyName, string absolutePath)
        {
            AssemblyName = assemblyName;
            AbsoluteFullPath = Path.Combine(absolutePath, assemblyName) + ".ajproj";
        }

        public void AddExistingFile(string fileRelativePath)
        {
            var fileFullPath = Path.Combine(AbsoluteFullPath, fileRelativePath);

            if (!File.Exists(fileFullPath)) throw new Exception("can't find file");

            _internalFilePaths.Add(Path.GetFileName(fileFullPath));
        }

        public void AddNewFile(string fileRelativePath)
        {
            var fileFullPath = Path.Combine(AbsoluteFullPath, fileRelativePath);

            _internalFilePaths.Add(fileRelativePath);
            File.Create(fileFullPath);
        }

        public void RemoveFile(string fileRelativePath, bool bDelFile)
        {
            _internalFilePaths.Remove(fileRelativePath);

            var fileFullPath = Path.Combine(AbsoluteFullPath, fileRelativePath);

            if (bDelFile) File.Delete(fileFullPath);
            else ExceptFilePaths.Add(fileRelativePath);
        }

        public void ChangeName(string projectNameToChange)
        {
            var originalFullPath = Path.Combine(AbsoluteFullPath, Name);
            var toChangeFullPath = Path.Combine(AbsoluteFullPath, projectNameToChange);

            File.Move(originalFullPath, toChangeFullPath);
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(AbsoluteFullPath));

            using (StreamWriter wr = new StreamWriter(AbsoluteFullPath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(AJProject));
                xs.Serialize(wr, this);
            }
        }

        public static AJProject Load(string assemblyName, string absolutePath)
        {
            AJProject project = null;
            using (StreamReader sr = new StreamReader(Path.Combine(absolutePath, assemblyName)))
            {
                XmlSerializer xs = new XmlSerializer(typeof(AJProject));
                project = xs.Deserialize(sr) as AJProject;
                project.AssemblyName = assemblyName;
                project.AbsoluteFullPath = Path.Combine(absolutePath, assemblyName) + ".ajproj";
            }

            foreach (var fileFullPath in Directory.GetFiles(project.AbsoluteFullPath, "*.aj", SearchOption.AllDirectories))
            {
                var internalFilePath = fileFullPath.Replace(project.AbsoluteFullPath, "");
                if (internalFilePath[0] == '/') internalFilePath = internalFilePath.Substring(1);
                project._internalFilePaths.Add(internalFilePath);
            }

            foreach (var fileRelativePath in project.ExceptFilePaths)
                project._internalFilePaths.Remove(fileRelativePath);

            return project;
        }

        public override bool Equals(object obj)
        {
            return obj is AJProject info &&
                   AssemblyName == info.AssemblyName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AssemblyName);
        }

        public static bool operator ==(AJProject left, AJProject right)
        {
            return EqualityComparer<AJProject>.Default.Equals(left, right);
        }

        public static bool operator !=(AJProject left, AJProject right)
        {
            return !(left == right);
        }


        private HashSet<string> _internalFilePaths = new HashSet<string>();

        private string DebuggerDisplay
            => $"Assembly: {AssemblyName}, files count: {InternalFileRFullPaths.Count}";
    }
}
