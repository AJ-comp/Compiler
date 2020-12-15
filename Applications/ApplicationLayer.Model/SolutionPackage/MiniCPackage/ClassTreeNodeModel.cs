using ApplicationLayer.Common;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Models.SolutionPackage.MiniCPackage
{
    public class ClassTreeNodeModel : TreeNodeModel
    {
        public override string DisplayName 
        {
            get => _classData.Name;
            set => throw new NotImplementedException();
        }

        public override string FullOnlyPath => string.Empty;

        public override event EventHandler<FileChangedEventArgs> Changed;

        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
            throw new NotImplementedException();
        }


        public ClassTreeNodeModel(ClassData classData)
        {
            _classData = classData;
        }


        private ClassData _classData;
    }
}
