using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.AssignExprNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types;
using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes
{
    public abstract class BinaryExprNode : ExprNode
    {
        public ExprNode LeftNode => Items[0] as ExprNode;
        public ExprNode RightNode => Items[1] as ExprNode;


        public bool IsCalculateable { get; protected set; }
        public bool IsBothLiteral => (LeftNode is LiteralNode && RightNode is LiteralNode);


        protected BinaryExprNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            LeftNode.ConnectedErrInfoList.Clear();
            RightNode.ConnectedErrInfoList.Clear();

            // ExprNode or TerminalNode
            Items[0].Build(param);
            Items[1].Build(param);

            if (!(this is AssignNode))
            {
                if (LeftNode is UseIdentNode) IsNotInit(LeftNode as UseIdentNode);
            }

            if (RightNode is UseIdentNode) IsNotInit(RightNode as UseIdentNode);

            return this;
        }

        protected IConstant ArithimeticOperation(IROperation operation)
        {
            // 확정된 타입이지만 연산을 할 수 없다면 unknown type을 유도하는 첫번째 로직이기에 오류를 표시합니다.
            return AJOperator.ArithimeticOperation(LeftNode.Result, RightNode.Result, operation, AddMCL0011Exception);
        }

        protected void AddMCL0011Exception()
        {
            // Add semantic error information if varData is exist in the SymbolTable.
            ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(MeaningTokens,
                                                    nameof(AlarmCodes.MCL0011),
                                                    string.Format(AlarmCodes.MCL0011, 
                                                                         LeftNode.Result.ToString(), 
                                                                         RightNode.Result.ToString()))
                );
        }

        private void IsNotInit(UseIdentNode varNode)
        {
            //            varNode.SymbolTable.AllVarTable
            var varRecord = AJUtilities.GetVarRecordFromReferableST(this, varNode.IdentToken);
            if (varRecord == null) return;
            if (varRecord.DefineField.IsVirtual) return;
            //            if (varRecord.DefineField.VariableProperty == VarProperty.Param) return;
            if (varRecord.InitValue != null) return;

            // Add semantic error information if varData is exist in the SymbolTable.
            ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(varRecord.DefineField.NameToken,
                                                    nameof(AlarmCodes.MCL0005),
                                                    string.Format(AlarmCodes.MCL0005, varRecord.DefineField.Name))
                );
        }


        private bool IsContainFloatingType(StdType type1, StdType type2) => (type1 == StdType.Double || type2 == StdType.Double);
        private bool IsContainStringType(StdType type1, StdType type2) => (type1 == StdType.String || type2 == StdType.String);
    }
}
