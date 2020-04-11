using ApplicationLayer.Common;
using ApplicationLayer.Common.Interfaces;
using System;
using System.IO;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class FileTreeNodeModel : PathTreeNodeModel, IManagedable
    {
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
            }
        }

        public override string FullOnlyPath => Parent.FullOnlyPath;



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
         * event handler section
         ********************************************************************************************/
        public event EventHandler<FileChangedEventArgs> Changed;



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
    }
}
