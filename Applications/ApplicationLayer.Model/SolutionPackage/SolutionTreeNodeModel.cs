using ApplicationLayer.Common.Interfaces;
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
    public class SolutionTreeNodeModel : PathTreeNodeModel, IManagableElements
    {
        private double versionRecentSaved;
        private ObservableCollection<ProjectTreeNodeModel> projects = new ObservableCollection<ProjectTreeNodeModel>();

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
        public ObservableCollection<ProjectTreeNodeModel> Projects => projects;

        [XmlElement("Project")]
        public Collection<PathInfo> ProjectPaths { get; private set; } = new Collection<PathInfo>();

        public bool IsChanged
        {
            get
            {
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
        }

        public SolutionTreeNodeModel(string solutionPath, string solutionName) : base(solutionPath, solutionName + ".ajn", true)
        {
            this.Path = solutionPath;
            this.SolutionName = solutionName;
            this.SyncWithCurrentValue();
        }

        /// <summary>
        /// If the project name was changed then it has to synchronize because of the project file name was already changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProjectNode_Renamed(object sender, Tuple<PathInfo, PathInfo> e)
        {
            var foundIndex = this.ProjectPaths.IndexOf(e.Item1);
            if (foundIndex < 0) return;

            this.ProjectPaths[foundIndex] = e.Item2;
        }

        /// <summary>
        /// If the project was deleted then it has to synchronize because of the project file was already deleted.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProjectNode_Deleted(object sender, PathInfo e)
        {
            var foundIndex = this.ProjectPaths.IndexOf(e);
            if (foundIndex < 0) return;

            this.ProjectPaths.RemoveAt(foundIndex);
        }

        /// <summary>
        /// Even though the project was unloaded synchronize process is not performed because of the project file exists.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProjectNode_Unloaded(object sender, PathInfo e)
        {
        }

        public void AddProject(ProjectTreeNodeModel newProject)
        {
            newProject.Parent = this;
            this.projects.Add(newProject);
        }

        public static SolutionTreeNodeModel Create(string solutionPath, string solutionName, Grammar grammar, Target target)
        {
            SolutionTreeNodeModel result = new SolutionTreeNodeModel(solutionPath, solutionName);
            result.Version = 1.0;

            ProjectGenerator projectGenerator = ProjectGenerator.CreateProjectGenerator(grammar);
            if (projectGenerator == null) return result;

            var newProject = projectGenerator.CreateDefaultProject(solutionName, false, solutionName, target);
            result.AddProject(newProject);
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

                this.AddProject(result);
            }
        }

        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
            if(nodeToRemove is ProjectTreeNodeModel)
                this.projects.Remove(nodeToRemove as ProjectTreeNodeModel);
        }

        public void SyncWithLoadValue() => Version = versionRecentSaved;

        public void SyncWithCurrentValue()
        {
            versionRecentSaved = Version;

            ProjectPaths.Clear();
            foreach (var project in Projects)
                ProjectPaths.Add(new PathInfo(project.Path, project.FileName, project.ProjectType, project.IsAbsolute));
        }

        public void Save()
        {
            Directory.CreateDirectory(this.Path);
            using (StreamWriter wr = new StreamWriter(this.PathWithFileName))
            {
                XmlSerializer xs = new XmlSerializer(typeof(SolutionTreeNodeModel));
                xs.Serialize(wr, this);
            }
        }
    }
}
