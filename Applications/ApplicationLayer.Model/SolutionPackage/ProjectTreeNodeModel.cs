using ApplicationLayer.Common;
using ApplicationLayer.Common.Interfaces;
using System;
using System.IO;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    public abstract class ProjectTreeNodeModel : PathTreeNodeModel, IManagableElements, IManagedable
    {
        /********************************************************************************************
         * override property section
         ********************************************************************************************/
        public override string FullOnlyPath => (Parent == null) ? Path : System.IO.Path.Combine(Parent?.FullOnlyPath, Path);
        public override string FullPath => System.IO.Path.Combine(this.FullOnlyPath, this.FileName);
        public override bool IsExistFile => File.Exists(FullPath);

        public override string DisplayName
        {
            get => System.IO.Path.GetFileNameWithoutExtension(this.FileName);
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

                this.Changed?.Invoke(this, new FileChangedEventArgs(FileChangedKind.Rename, originalFullPath, toChangeFullPath));
            }
        }



        /********************************************************************************************
         * interface property section
         ********************************************************************************************/
        public IManagableElements ManagerTree
        {
            get
            {
                TreeNodeModel current = this;

                while (true)
                {
                    if (current == null) return null;
                    else if (current is ProjectTreeNodeModel) return current as ProjectTreeNodeModel;

                    current = current.Parent;
                }
            }
        }



        /********************************************************************************************
         * abstract property section
         ********************************************************************************************/
        public abstract bool IsChanged { get; }
        public abstract string ProjectType { get; }



        /********************************************************************************************
         * event section
         ********************************************************************************************/
        public override event EventHandler<FileChangedEventArgs> Changed;
        public event EventHandler<FileChangedEventArgs> ChildrenChanged;



        /********************************************************************************************
         * constructor section
         ********************************************************************************************/
        public ProjectTreeNodeModel() : base(string.Empty, string.Empty)
        {
            this.IsEditable = true;
            this.children.CollectionChanged += Children_CollectionChanged;
        }

        public ProjectTreeNodeModel(string path, string projName) : base(path, projName)
        {
            this.IsEditable = true;
            this.children.CollectionChanged += Children_CollectionChanged;
        }



        /********************************************************************************************
         * static method section
         ********************************************************************************************/
        public static ProjectTreeNodeModel CreateProjectTreeNodeModel(string solutionPath, PathInfo projectPath)
        {
            Type type = TreeNodeCreator.GetType(projectPath.FileName);
            string fullPath = (projectPath.IsAbsolute) ? projectPath.FullPath : System.IO.Path.Combine(solutionPath, projectPath.FullPath);

            try
            {
                ProjectTreeNodeModel project;
                using (StreamReader sr = new StreamReader(fullPath))
                {
                    XmlSerializer xs = new XmlSerializer(type);
                    project = xs.Deserialize(sr) as ProjectTreeNodeModel;
                    project.Path = projectPath.Path;
                    project.FileName = projectPath.FileName;
                }
                project.LoadElement();
                project.SyncWithCurrentValue();

                return project;
            }
            catch
            {
                return null;
            }
        }

        

        /********************************************************************************************
         * abstract function section
         ********************************************************************************************/
        public abstract void LoadElement();
        public abstract void Save();
        public abstract void SyncWithLoadValue();
        public abstract void SyncWithCurrentValue();



        /********************************************************************************************
         * event handler section
         ********************************************************************************************/
        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null) return;

            foreach (var item in e.NewItems)
            {
                var node = item as TreeNodeModel;
                node.Changed += Node_Changed; ;
            }
        }

        private void Node_Changed(object sender, FileChangedEventArgs e)
        {
            ChildrenChanged?.Invoke(this, e);
        }
    }
}
