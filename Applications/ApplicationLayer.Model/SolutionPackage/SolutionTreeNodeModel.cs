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
        private double _versionRecentSaved;



        /********************************************************************************************
         * property section
         ********************************************************************************************/
        public double Version { get; set; }

        public bool IsChanged
        {
            get
            {
                if (_versionRecentSaved != Version) return true;
                if (ProjectPaths.Count != Children.Count) return true;

                foreach (var child in Children)
                {
                    if(child is ProjectTreeNodeModel)
                    {
                        var project = child as ProjectTreeNodeModel;
                        var path = new PathInfo(project.Path, project.FileName);
                        if (ProjectPaths.Contains(path) == false) return true;
                    }
                }

                return false;
            }
        }

        public ProjectTreeNodeModel StartingProject
        {
            get
            {
                ProjectTreeNodeModel result = null;

                foreach (var child in Children)
                {
                    if (child is ProjectTreeNodeModel == false) continue;

                    var project = child as ProjectTreeNodeModel;
                    if (project.StartingProject)
                    {
                        result = project;
                        break;
                    }
                }

                return result;
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

        [XmlIgnore] public string BinFolderPath => System.IO.Path.Combine(Path, "bin");
        [XmlIgnore] public int LoadedProjectCount => Children.Count;
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
        public override string FullPath => System.IO.Path.Combine(this.FullOnlyPath, this.FileName);
        public override bool IsExistFile => File.Exists(FullPath);



        /********************************************************************************************
         * public event handler section
         ********************************************************************************************/
        public override event EventHandler<FileChangedEventArgs> Changed;
        public event EventHandler<FileChangedEventArgs> ChildrenChanged;



        /********************************************************************************************
         * constructor section
         ********************************************************************************************/
        public SolutionTreeNodeModel() : base(string.Empty, string.Empty)
        {
            this.IsEditable = true;
            this._children.CollectionChanged += Projects_CollectionChanged;
        }

        public SolutionTreeNodeModel(string solutionPath, string solutionName) : base(solutionPath, solutionName + ".ajn")
        {
            this.Path = solutionPath;
            this.SolutionName = solutionName;
            this.IsEditable = true;
            this.SyncWithCurrentValue();

            this._children.CollectionChanged += Projects_CollectionChanged;
        }



        /********************************************************************************************
         * private method section
         ********************************************************************************************/
        private void Projects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null) return;

            foreach(var item in e.NewItems)
            {
                if(item is ProjectTreeNodeModel)
                {
                    ProjectTreeNodeModel project = item as ProjectTreeNodeModel;
                    project.Changed += Project_Changed;
                }
            }
        }

        private void Project_Changed(object sender, FileChangedEventArgs e)
        {
            this.ChildrenChanged?.Invoke(this, e);
        }



        /********************************************************************************************
         * public method section
         ********************************************************************************************/
        public void AddProject(ProjectTreeNodeModel newProject)
        {
            newProject.Parent = this;
            this._children.Add(newProject);
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
                this._children.Remove(nodeToRemove as ProjectTreeNodeModel);
        }



        /********************************************************************************************
         * public static method section
         ********************************************************************************************/
        public static SolutionTreeNodeModel Create(string solutionPath, string solutionName, ProjectType projectType, Target target)
        {
            SolutionTreeNodeModel result = new SolutionTreeNodeModel(solutionPath, solutionName);
            result.Version = 1.0;

            ProjectGenerator projectGenerator = ProjectGenerator.CreateProjectGenerator(projectType.Grammar);
            if (projectGenerator == null) return result;

            var projectData = new ProjectData(System.IO.Path.Combine(solutionPath, result.FileNameWithoutExtension),
                                                                result.FileNameWithoutExtension,
                                                                projectType);

            var newProject = projectGenerator.CreateDefaultProject(solutionPath, 
                                                                                              projectData, 
                                                                                              target);
            result.AddProject(newProject);
            result.SyncWithCurrentValue();

            return result;
        }



        /********************************************************************************************
         * interface method section
         ********************************************************************************************/
        public void SyncWithLoadValue() => Version = _versionRecentSaved;

        public void SyncWithCurrentValue()
        {
            _versionRecentSaved = Version;

            ProjectPaths.Clear();
            foreach (var child in Children)
            {
                if(child is ProjectTreeNodeModel)
                {
                    ProjectTreeNodeModel project = child as ProjectTreeNodeModel;
                    ProjectPaths.Add(new PathInfo(project.Path, project.FileName));
                }
            }
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
