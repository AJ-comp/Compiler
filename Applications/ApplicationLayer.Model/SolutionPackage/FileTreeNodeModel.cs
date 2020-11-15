using ApplicationLayer.Common;
using ApplicationLayer.Common.Interfaces;
using System;
using System.IO;

namespace ApplicationLayer.Models.SolutionPackage
{
    public abstract class FileTreeNodeModel : PathTreeNodeModel, IManagedable
    {
        /********************************************************************************************
         * property section
         ********************************************************************************************/
        public string Data
        {
            get => (this.IsExistFile) ? File.ReadAllText(this.FullPath) : string.Empty;
            set
            {
                if(this.IsExistFile == false)  return;

                File.WriteAllText(this.FullPath, value);
            }
        }

        public string CurrentData
        {
            get => _currentData;
            set
            {
                _currentData = value;

                Changed?.Invoke(this, null);
            }
        }
        public string FileType => System.IO.Path.GetExtension(this.FileName).Replace(".", "");
        public bool IsLoaded { get; set; } = false;



        /********************************************************************************************
         * override property section
         ********************************************************************************************/
        public override string DisplayName
        {
            get => FileName;
            set
            {
                var originalFullPath = System.IO.Path.Combine(this.FullOnlyPath, this.FileName);
                FileName = value;
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

        public override string FullOnlyPath => (Parent == null) ? string.Empty : Parent.FullOnlyPath;
        public override bool IsExistFile => File.Exists(FullPath);
        public override string FullPath => System.IO.Path.Combine(this.FullOnlyPath, this.FileName);



        /********************************************************************************************
         * interface property section
         ********************************************************************************************/
        public IManagableElements ManagerTree => ProjectTree;

        public ProjectTreeNodeModel ProjectTree
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
         * event handler section
         ********************************************************************************************/
        public override event EventHandler<FileChangedEventArgs> Changed;



        /********************************************************************************************
         * constructor section
         ********************************************************************************************/
        public FileTreeNodeModel(string path, string filename) : base(path, filename)
        {
            this.IsEditable = true;
        }



        /********************************************************************************************
         * override method section
         ********************************************************************************************/
        public override void RemoveChild(TreeNodeModel nodeToRemove) { }




        public static FileTreeNodeModel CreateFileTreeNodeModel(string path, string fileName)
        {
            var extension = System.IO.Path.GetExtension(fileName);

            if (extension == ".mc" || extension == ".mh") return new SourceFileTreeNodeModel(path, fileName);

            return new UndefinedTreeNodeModel(path, fileName);
        }



        private string _currentData;
    }
}
