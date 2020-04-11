using ApplicationLayer.Common;
using ApplicationLayer.Common.Interfaces;
using Parse.BackEnd.Target;
using Parse.FrontEnd.Grammars;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    [XmlRoot ("Solution")]
    public class SolutionTreeNodeModel : PathTreeNodeModel, IManagableElements
    {
        /********************************************************************************************
         * private field section
         ********************************************************************************************/
        private double versionRecentSaved;
        private ObservableCollection<ProjectTreeNodeModel> projects = new ObservableCollection<ProjectTreeNodeModel>();



        /********************************************************************************************
         * property section
         ********************************************************************************************/
        public double Version { get; set; }

        public bool IsChanged
        {
            get
            {
                if (versionRecentSaved != Version) return true;
                if (ProjectPaths.Count != Projects.Count) return true;

                foreach (var project in Projects)
                {
                    var path = new PathInfo(project.Path, project.FileName);
                    if (ProjectPaths.Contains(path) == false) return true;
                }

                return false;
            }
        }

        [XmlIgnore] public string SolutionName
        {
            get => System.IO.Path.GetFileNameWithoutExtension(FileName);
            set
            {
                var ext = System.IO.Path.GetExtension(FileName);
                FileName = string.Format("{0}{1}", value, ext);
            }
        }

        [XmlIgnore] public int LoadedProjectCount => Projects.Count;

        [XmlIgnore] public ObservableCollection<ProjectTreeNodeModel> Projects => projects;

        [XmlElement("Project")] public Collection<PathInfo> ProjectPaths { get; private set; } = new Collection<PathInfo>();



        /********************************************************************************************
         * override property section
         ********************************************************************************************/
        public override string DisplayName
        {
            get => SolutionName;
            set
            {
                var extension = System.IO.Path.GetExtension(this.FileName);

                var originalFullPath = System.IO.Path.Combine(this.FullOnlyPath, this.FileName);
                this.FileName = string.Format("{0}{1}", value, extension);
                var toChangeFullPath = System.IO.Path.Combine(this.FullOnlyPath, this.FileName);

                if (File.Exists(originalFullPath) == false)
                {
                    // It has to change a current status to error status.
                    return;
                }
                File.Move(originalFullPath, toChangeFullPath);
            }
        }

        public override string FullOnlyPath => Path;



        /********************************************************************************************
         * public event handler section
         ********************************************************************************************/
        public event EventHandler<FileChangedEventArgs> Changed;
        public event EventHandler<FileChangedEventArgs> ChildrenChanged;



        /********************************************************************************************
         * constructor section
         ********************************************************************************************/
        public SolutionTreeNodeModel() : base(string.Empty, string.Empty)
        {
            this.IsEditable = true;
            this.Projects.CollectionChanged += Projects_CollectionChanged;
        }

        public SolutionTreeNodeModel(string solutionPath, string solutionName) : base(solutionPath, solutionName + ".ajn")
        {
            this.Path = solutionPath;
            this.SolutionName = solutionName;
            this.IsEditable = true;
            this.SyncWithCurrentValue();
        }



        /********************************************************************************************
         * private method section
         ********************************************************************************************/
        private void Projects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null) return;

            foreach(var item in e.NewItems)
            {
                ProjectTreeNodeModel project = item as ProjectTreeNodeModel;
                project.Changed += Project_Changed;
            }
        }

        private void Project_Changed(object sender, FileChangedEventArgs e)
        {
            this.ChildrenChanged?.Invoke(sender, e);
        }



        /********************************************************************************************
         * public method section
         ********************************************************************************************/
        public void AddProject(ProjectTreeNodeModel newProject)
        {
            newProject.Parent = this;
            this.projects.Add(newProject);
        }

        /// <summary>
        /// This function loads projects in the ProjectPaths.
        /// </summary>
        /// <returns>Returns true if all project loaded successful else returns false.</returns>
        public bool LoadProject()
        {
            bool result = true;

            foreach(var projectPath in ProjectPaths)
            {
                var projectTreeNode = ProjectTreeNodeModel.CreateProjectTreeNodeModel(this.Path, projectPath);
                if (projectTreeNode == null)
                {
                    result = false;
                    projectTreeNode = new ErrorProjectTreeNodeModel(projectPath.Path, projectPath.FileName);
                }

                this.AddProject(projectTreeNode);
            }

            return result;
        }



        /********************************************************************************************
         * override method section
         ********************************************************************************************/
        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
            if(nodeToRemove is ProjectTreeNodeModel)
                this.projects.Remove(nodeToRemove as ProjectTreeNodeModel);
        }



        /********************************************************************************************
         * public static method section
         ********************************************************************************************/
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



        /********************************************************************************************
         * interface method section
         ********************************************************************************************/
        public void SyncWithLoadValue() => Version = versionRecentSaved;

        public void SyncWithCurrentValue()
        {
            versionRecentSaved = Version;

            ProjectPaths.Clear();
            foreach (var project in Projects)
                ProjectPaths.Add(new PathInfo(project.Path, project.FileName));
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
