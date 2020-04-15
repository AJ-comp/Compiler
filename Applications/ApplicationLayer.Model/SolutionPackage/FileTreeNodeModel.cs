using ApplicationLayer.Common;
using ApplicationLayer.Common.Interfaces;
using ApplicationLayer.Models.SolutionPackage.MiniCPackage;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using System;
using System.Collections.Generic;
using System.IO;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class FileTreeNodeModel : PathTreeNodeModel, IManagedable
    {
        /********************************************************************************************
         * property section
         ********************************************************************************************/
        public string Data { get; set; }
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
        public override event EventHandler<FileChangedEventArgs> Changed;



        /********************************************************************************************
         * constructor section
         ********************************************************************************************/
        public FileTreeNodeModel(string path, string filename) : base(path, filename)
        {
            this.IsEditable = true;

            AddChildren(new VarTreeNodeModel(DataType.Int, "test"));

            List<VarTreeNodeModel> paramList = new List<VarTreeNodeModel>();
            paramList.Add(new VarTreeNodeModel(DataType.Int, "param1"));
            paramList.Add(new VarTreeNodeModel(DataType.Int, "param2"));
            AddChildren(new FuncTreeNodeModel() { Params = paramList, ReturnType = DataType.Int, FuncName = "func" });
        }



        /********************************************************************************************
         * override method section
         ********************************************************************************************/
        public override void RemoveChild(TreeNodeModel nodeToRemove) { }
    }
}
