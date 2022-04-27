using ApplicationLayer.Common;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;
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


        public ClassTreeNodeModel(ClassDefNode classData)
        {
            _classData = classData;
        }


        private ClassDefNode _classData;
    }
}
