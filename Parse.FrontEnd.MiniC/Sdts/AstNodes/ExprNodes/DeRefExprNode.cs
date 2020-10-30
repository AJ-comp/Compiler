using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables;
using Parse.FrontEnd.MiniC.Properties;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes
{
    public class DeRefExprNode : ExprNode
    {
        public UseIdentNode IdentNode { get; private set; }

        public DeRefExprNode(AstSymbol node) : base(node)
        {
        }


        // * Expr
        // [1] : ExprNode
        public override SdtsNode Build(SdtsParams param)
        {
            var node = Items[0].Build(param) as ExprNode;
            SymbolTable = (param as MiniCSdtsParams).SymbolTable;

            if (node is UseIdentNode)
            {
                IdentNode = node as UseIdentNode;
                var varData = MiniCUtilities.GetVarRecordFromReferableST(this, IdentNode.IdentToken);
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
