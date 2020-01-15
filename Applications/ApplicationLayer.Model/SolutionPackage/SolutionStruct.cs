using Parse.BackEnd.Target;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
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
        public string RelativePath => Path.Combine(this.OPath, this.FullName);

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
    public class SolutionStruct : HirStruct
    {
        [XmlIgnore]
        public static string Extension => "ajn";
        public double Version { get; set; }

        public StringCollection ProjectPaths { get; } = new StringCollection();

        [XmlIgnore]
        public ObservableCollection<ProjectStruct> Projects { get; } = new ObservableCollection<ProjectStruct>();

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
                this.ProjectPaths.Add(child.RelativePath);
            }
        }

        public void Load(string fullPath)
        {
            this.OPath = Path.GetDirectoryName(fullPath);
        }

        public static SolutionStruct Create(string solutionPath, string solutionName, Grammar grammar, Target target)
        {
            SolutionStruct result = new SolutionStruct();
            string solutionNameWithExtension = Path.GetFileNameWithoutExtension(solutionName);

            result.OPath = solutionPath;
            result.FullName = solutionName;
            result.Version = 1.0;

            ProjectGenerator projectGenerator = null;
            if (grammar is MiniCGrammar)
                projectGenerator = new MiniCGenerator();

            if (projectGenerator == null) return result;

            result.Projects.Add(projectGenerator.CreateDefaultProject(solutionNameWithExtension, solutionNameWithExtension, target, result));
            result.Projects.Add(projectGenerator.CreateDefaultProject(solutionNameWithExtension, solutionNameWithExtension + "abc", target, result));

            return result;
        }
    }
}
