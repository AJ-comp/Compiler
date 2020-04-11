using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using System;

namespace ApplicationLayer.Models.SolutionPackage.MiniCPackage
{
    public class VarTreeNodeModel : TreeNodeModel
    {
        public DataType DataType { get; } = DataType.Unknown;

        public string VarName { get; } = string.Empty;

        public override string DisplayName
        {
            get => string.Format("{0} : {1}", VarName, DataType);
            set => throw new Exception();
        }

        public override string FullOnlyPath => string.Empty;

        public VarTreeNodeModel(DataType dataType, string varName)
        {
            DataType = dataType;
            VarName = varName;
        }

        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
        }
    }
}
