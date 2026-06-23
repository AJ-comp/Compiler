using ApplicationLayer.Common;
using ApplicationLayer.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class FolderTreeNodeModel : PathTreeNodeModel
    {
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

                if (Directory.Exists(originalFullPath) == false)
                {
                    // It has to change a current status to error status.
                    return;
                }
                Directory.Move(originalFullPath, toChangeFullPath);
            }
        }

        public override string FullOnlyPath => (Parent == null) ? string.Empty : System.IO.Path.Combine(Parent.FullOnlyPath, Path);
        public override string FullPath => System.IO.Path.Combine(this.FullOnlyPath, this.FileName);
        public override bool IsExistFile => File.Exists(FullPath);




        /********************************************************************************************
         * event handler section
         ********************************************************************************************/
        public override event EventHandler<FileChangedEventArgs> Changed;



        /********************************************************************************************
         * constructor section
         ********************************************************************************************/
        public FolderTreeNodeModel(string path, string folderName) : base(path, folderName)
        {
        }



        /********************************************************************************************
         * override method section
         ********************************************************************************************/
        public override void RemoveChild(TreeNodeModel nodeToRemove) { }
    }
}
