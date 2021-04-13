using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.AJ.Sdts.Datas.Variables;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes
{
    /// <summary>
    /// *a
    /// *A()
    /// </summary>
    public class DeRefExprNode : ExprNode, IUseIdentExpression
    {
        public UseIdentNode IdentNode { get; private set; }


        // for interface *******************************************************/
        public IDeclareVarExpression Var => IdentNode?.Var;
        public IFunctionExpression Func => IdentNode?.Func;
        /*****************************************************************/


        public DeRefExprNode(AstSymbol node) : base(node)
        {
        }


        // * Expr
        // [1] : ExprNode
        public override SdtsNode Build(SdtsParams param)
        {
            var node = Items[0].Build(param) as ExprNode;
            SymbolTable = (param as AJSdtsParams).SymbolTable;

            if (node is UseIdentNode)
            {
                IdentNode = node as UseIdentNode;
                var varData = AJUtilities.GetVarRecordFromReferableST(this, IdentNode.IdentToken);
                if (varData != null)
                    CheckDeRefCondition(varData.DefineField);

                // add to symbol table
                SymbolTable.VarTable.AddReferenceRecord(IdentNode.VarData, new ReferenceInfo(this));
            }
            else
            {
                ConnectedErrInfoList.Add
                    (
                        new MeaningErrInfo(IdentNode.AllTokens,
                                                        nameof(AlarmCodes.MCL0019), AlarmCodes.MCL0019)
                    );
            }

            return this;
        }

        private void CheckDeRefCondition(VariableMiniC var)
        {
            // Add semantic error information if varData is exist in the SymbolTable.
            if (var.DataType == MiniCDataType.Address) return;

            ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(IdentNode.IdentToken,
                                                    nameof(AlarmCodes.MCL0019), AlarmCodes.MCL0019)
                );
        }
    }
}
