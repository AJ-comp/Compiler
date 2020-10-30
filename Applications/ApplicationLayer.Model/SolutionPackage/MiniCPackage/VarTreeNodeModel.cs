using ApplicationLayer.Common;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables;
using System;

namespace ApplicationLayer.Models.SolutionPackage.MiniCPackage
{
    public class VarTreeNodeModel : TreeNodeModel
    {
        public bool IsConst => (_varData != null) && _varData.Const;
        public string DataType => (_varData != null) ? _varData.DataType.ToString() : string.Empty;
        public string Name => (_varData != null) ? _varData.Name : string.Empty;
        public string Dimension => (_varData != null) ? _varData.Dimension.ToString() : string.Empty;
        public string BlockIndex => (_varData != null) ? _varData.Block.ToString() : string.Empty;
        public string Offset => (_varData != null) ? _varData.Offset.ToString() : string.Empty;
        public bool IsGlobal => (_varData?.Block== 0);
        public bool IsParam => (_varData?.VariableProperty == VarProperty.Param);

        public override string DisplayName
        {
            get => string.Format("{0} : {1}", Name, DataType);
            set => throw new Exception();
        }

        public override string FullOnlyPath => string.Empty;

        public override event EventHandler<FileChangedEventArgs> Changed;

        public VarTreeNodeModel(VariableMiniC varData)
        {
            _varData = varData;
        }

        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
        }


        private VariableMiniC _varData;
    }
}
