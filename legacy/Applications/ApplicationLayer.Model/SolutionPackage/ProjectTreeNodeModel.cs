using ApplicationLayer.Common;
using ApplicationLayer.Common.Interfaces;
using ApplicationLayer.Common.Utilities;
using Compile.Models;
using Parse.BackEnd.Target;
using Parse.MiddleEnd.IR.LLVM;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    public abstract class ProjectTreeNodeModel : PathTreeNodeModel, IManagableElements, IManagedable
    {
        // this property has to bring a information via current mode. (ex : debug or release)
        [XmlIgnore] public ProjectProperty ProjectData => GetProjectProperty(ProjectProperty.Configure.Debug);
        [XmlIgnore] public string MCPUType => LLVMConverter.GetMCPUOption(ProjectData.Target);
        [XmlIgnore] public Target MCUType => AssemblyManager.CreateInstanceFromClassName(ProjectData.Target) as Target;

        [XmlElement("IsStartingProject")]  public bool StartingProject { get; set; }
        [XmlElement("ProjectType")] public ProjectKinds ProjectType { get; set; }



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
        public abstract string LanguageType { get; }
        /// <summary>
        /// This property returns the reference file list of the files in project.
        /// </summary>
        public abstract IEnumerable<FileReferenceInfo> FileReferenceInfos { get; }



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
            this._children.CollectionChanged += Children_CollectionChanged;
        }

        public ProjectTreeNodeModel(ProjectData projectData)
            : base(projectData.ProjectPath, projectData.ProjectNameWithExtension)
        {
            IsEditable = true;
            ProjectType = projectData.ProjectType.ProjectKind;
            _children.CollectionChanged += Children_CollectionChanged;
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
        public abstract ProjectProperty GetProjectProperty(ProjectProperty.Configure configure);



        /********************************************************************************************
         * event handler section
         ********************************************************************************************/
        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null) return;

            foreach (var item in e.NewItems)
            {
                var node = item as TreeNodeModel;
                node.Changed += Node_Changed;
            }
        }

        private void Node_Changed(object sender, FileChangedEventArgs e)
        {
            ChildrenChanged?.Invoke(this, e);
        }
    }
}
