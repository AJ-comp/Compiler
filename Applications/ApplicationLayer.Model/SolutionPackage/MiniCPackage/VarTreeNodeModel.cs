using ApplicationLayer.Common;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using System;

namespace ApplicationLayer.Models.SolutionPackage.MiniCPackage
{
    public class VarTreeNodeModel : TreeNodeModel
    {
        private DclData _dclData;

        public bool IsConst => (_dclData != null) ? _dclData.DclSpecData.Const : false;
        public string DataType => (_dclData != null) ? _dclData.DclSpecData.DataType.ToString() : string.Empty;
        public string Name => (_dclData != null) ? _dclData.DclItemData.Name : string.Empty;
        public string Dimension => (_dclData != null) ? _dclData.DclItemData.Dimension.ToString() : string.Empty;
        public string BlockIndex => (_dclData != null) ? _dclData.BlockLevel.ToString() : string.Empty;
        public string Offset => (_dclData != null) ? _dclData.Offset.ToString() : string.Empty;
        public bool IsGlobal => (_dclData?.BlockLevel == 0) ? true : false;
        public bool IsParam => (_dclData?.Etc == EtcInfo.Param) ? true : false;

        public override string DisplayName
        {
            get => string.Format("{0} : {1}", Name, DataType);
            set => throw new Exception();
        }

        public override string FullOnlyPath => string.Empty;

        public override event EventHandler<FileChangedEventArgs> Changed;

        public VarTreeNodeModel(DclData dclData)
        {
            _dclData = dclData;
        }

        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
        }
    }
}
