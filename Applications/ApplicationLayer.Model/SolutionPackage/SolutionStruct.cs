using Parse.BackEnd.Target;
using Parse.FrontEnd.Grammars;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    [XmlInclude(typeof(ProjectStruct))]
    public class SolutionStruct : HirStruct
    {
        [XmlIgnore]
        public static string Extension => "ajn";
        public double Version { get; set; }

        [XmlIgnore]
        public List<PathInfo> CurrentProjectPath { get; } = new List<PathInfo>();
        public List<PathInfo> SyncWithXMLProjectPaths { get; } = new List<PathInfo>();

        [XmlIgnore]
        public ObservableCollection<ProjectStruct> Projects { get; } = new ObservableCollection<ProjectStruct>();

        [XmlIgnore]
        public bool IsChanged => (this.CurrentProjectPath.Equals(this.SyncWithXMLProjectPaths)) ? false : true;

        public SolutionStruct()
        {
            this.Projects.CollectionChanged += Projects_CollectionChanged;
        }

        private void Projects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for(int i=0; i<e.NewItems?.Count; i++)
            {
                ProjectStruct child = e.NewItems[i] as ProjectStruct;
                child.Parent = this;

                if (File.Exists(child.FullPath)) continue;

                Directory.CreateDirectory(child.BasePath);
                this.CurrentProjectPath.Add(new PathInfo(child.RelativePath, child.IsAbsolutePath));
            }
        }

        public static SolutionStruct Create(string solutionPath, string solutionName, Grammar grammar, Target target)
        {
            SolutionStruct result = new SolutionStruct();
            string solutionNameWithExtension = Path.GetFileNameWithoutExtension(solutionName);

            result.OPath = solutionPath;
            result.FullName = solutionName;
            result.Version = 1.0;

            ProjectGenerator projectGenerator = ProjectGenerator.CreateProjectGenerator(grammar);
            if (projectGenerator == null) return result;

            result.Projects.Add(projectGenerator.CreateDefaultProject(solutionNameWithExtension, false, solutionNameWithExtension, target, result));
            result.Projects.Add(projectGenerator.CreateDefaultProject(solutionNameWithExtension + "abc", false, solutionNameWithExtension + "abc", target, result));

            return result;
        }
    }


    public class PathInfo
    {
        public string Path { get; set; }
        public bool IsAbsolute { get; set; }

        public PathInfo() { }

        public PathInfo(string path, bool isAbsolute)
        {
            Path = path;
            IsAbsolute = isAbsolute;
        }
    }
}
