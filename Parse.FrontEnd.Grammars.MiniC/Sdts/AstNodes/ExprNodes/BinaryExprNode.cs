using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.Properties;
using Parse.Types;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes
{
    public abstract class BinaryExprNode : ExprNode
    {
        public ExprNode Left => Items[0] as ExprNode;
        public ExprNode Right => Items[1] as ExprNode;

        public bool IsCalculateable => false;


        protected BinaryExprNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            // ExprNode or TerminalNode
            Items[0].Build(param);
            Items[1].Build(param);

            if (Left is UseIdentNode) IsNotInit(Left as UseIdentNode);
            if (Right is UseIdentNode) IsNotInit(Right as UseIdentNode);

            return this;
        }

        protected IArithmetic GetIArithmeticType()
        {
            var leftVarData = (Left as UseIdentNode).VarData;

            return (leftVarData is IArithmetic) ? leftVarData as IArithmetic : null;
        }


        protected void AddMCL0011Exception()
        {
            // Add semantic error information if varData is exist in the SymbolTable.
            ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(MeaningTokens,
                                                    nameof(AlarmCodes.MCL0011),
                                                    string.Format(AlarmCodes.MCL0011, Left.Result.ToString(), Right.Result.ToString()))
                );
        }

        private void IsNotInit(UseIdentNode varNode)
        {
//            varNode.SymbolTable.AllVarTable
            var varRecord = MiniCUtilities.GetVarRecordFromReferableST(this, varNode.IdentToken);
            if (varRecord == null) return;
            if (varRecord.DefineField.IsVirtual) return;
            if (varRecord.InitValue != null) return;

            // Add semantic error information if varData is exist in the SymbolTable.
            ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(varRecord.DefineField.NameToken,
                                                    nameof(AlarmCodes.MCL0005),
                                                    string.Format(AlarmCodes.MCL0005, varRecord.DefineField.Name))
                );
        }
    }
}
