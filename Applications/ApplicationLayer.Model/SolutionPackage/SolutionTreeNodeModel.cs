using ApplicationLayer.Models.SolutionPackage.MiniCPackage;
using Parse.BackEnd.Target;
using Parse.FrontEnd.Grammars;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Serialization;
using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.Models.SolutionPackage
{
    [XmlRoot ("Solution")]
    public class SolutionTreeNodeModel : PathTreeNodeModel
    {
        private double versionRecentSaved;

        public double Version { get; set; }

        [XmlIgnore]
        public string SolutionName
        {
            get => System.IO.Path.GetFileNameWithoutExtension(FileName);
            set
            {
                var ext = System.IO.Path.GetExtension(FileName);
                FileName = string.Format("{0}{1}", value, ext);
            }
        }

        [XmlIgnore]
        public ObservableCollection<ProjectTreeNodeModel> Projects { get; } = new ObservableCollection<ProjectTreeNodeModel>();

        [XmlElement("Project")]
        public Collection<PathInfo> ProjectPaths { get; private set; } = new Collection<PathInfo>();

        public override bool IsChanged
        {
            get
            {
                if (base.IsChanged) return true;

                if (versionRecentSaved != Version) return true;
                if (ProjectPaths.Count != Projects.Count) return true;

                foreach (var project in Projects)
                {
                    var path = new PathInfo(project.Path, project.IsAbsolute);
                    if (ProjectPaths.Contains(path) == false) return true;
                }

                return false;
            }
        }

        public override string DisplayName => 
            string.Format(CommonResource.Solution, SolutionName, "(" + this.Projects.Count + " " + CommonResource.Project + ")");

        public override string FullOnlyPath => Path;



        public SolutionTreeNodeModel() : base(string.Empty, string.Empty)
        {
            this.Projects.CollectionChanged += TreeNodeModel.CollectionChanged;
        }

        public SolutionTreeNodeModel(string solutionPath, string solutionName) : base(solutionPath, solutionName + ".ajn", true)
        {
            this.Path = solutionPath;
            this.SolutionName = solutionName;
            this.SyncWithCurrentValue();

            this.Projects.CollectionChanged += TreeNodeModel.CollectionChanged;
        }

        public static SolutionTreeNodeModel Create(string solutionPath, string solutionName, Grammar grammar, Target target)
        {
            SolutionTreeNodeModel result = new SolutionTreeNodeModel(solutionPath, solutionName);
            result.Version = 1.0;

            ProjectGenerator projectGenerator = ProjectGenerator.CreateProjectGenerator(grammar);
            if (projectGenerator == null) return result;

            result.Projects.Add(projectGenerator.CreateDefaultProject(solutionName, false, solutionName, target));
            result.SyncWithCurrentValue();

            return result;
        }

        public void LoadProject()
        {
            foreach(var projectPath in ProjectPaths)
            {
                Type type = typeof(ProjectTreeNodeModel);
                if (projectPath.Type == LanguageExtensions.MiniC)
                {
//                    result = new MiniCProjectTreeNodeModel();
                    type = typeof(MiniCProjectTreeNodeModel);
                }

                string fullPath = (projectPath.IsAbsolute) ? projectPath.FullPath : System.IO.Path.Combine(this.Path, projectPath.FullPath);

                ProjectTreeNodeModel result;
                using (StreamReader sr = new StreamReader(fullPath))
                {
                    XmlSerializer xs = new XmlSerializer(type);
                    result = xs.Deserialize(sr) as ProjectTreeNodeModel;
                    result.Path = projectPath.Path;
                    result.FileName = projectPath.FileName;
                }
                result.LoadElement();
                result.SyncWithCurrentValue();

                this.Projects.Add(result);
            }
        }

        public override void SyncWithLoadValue() => Version = versionRecentSaved;

        public override void SyncWithCurrentValue()
        {
            versionRecentSaved = Version;

            ProjectPaths.Clear();
            foreach (var project in Projects)
                ProjectPaths.Add(new PathInfo(project.Path, project.FileName, project.ProjectType, project.IsAbsolute));
        }
    }
}
