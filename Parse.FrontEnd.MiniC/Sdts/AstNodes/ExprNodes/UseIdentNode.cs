using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;
using Parse.FrontEnd.MiniC.Properties;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes
{
    public class UseIdentNode : ExprNode
    {
        public TokenData IdentToken { get; private set; }

        public VariableMiniC VarData => MiniCUtilities.GetVarRecordFromReferableST(this, IdentToken)?.DefineField;


        public UseIdentNode(AstSymbol node) : base(node)
        {
        }

        // [0] : ident
        public override SdtsNode Build(SdtsParams param)
        {
            ConnectedErrInfoList.Clear();

            var node = Items[0].Build(param) as TerminalNode;
            IdentToken = node.Token;

            Result = VarData?.ValueConstant;

            // If varToken is not declared, add as virtual token to the SymbolTable.
            if (IsNotDeclared())
            {
                // create MiniCVarData with unknown property.
                //                var varData = new UnknownVarData(IdentToken, param.BlockLevel, param.Offset);

                //                (param as MiniCSdtsParams).SymbolTable.AddVarData(varData, new ReferenceInfo(this, ));
            }

            return this;
        }


        /// <summary>
        /// This function checks whether "varTokenToCheck" is Declarated.
        /// </summary>
        /// <returns></returns>
        private bool IsNotDeclared()
        {
            var varData = MiniCUtilities.GetVarRecordFromReferableST(this, IdentToken);
            if (varData != null) return false;

            var funcData = MiniCUtilities.GetFuncDataFromReferableST(this, IdentToken);
            if (funcData != null) return false;

            ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(IdentToken,
                                                    nameof(AlarmCodes.MCL0001),
                                                    string.Format(AlarmCodes.MCL0001, IdentToken.Input))
                );

            return true;
        }
    }
}
