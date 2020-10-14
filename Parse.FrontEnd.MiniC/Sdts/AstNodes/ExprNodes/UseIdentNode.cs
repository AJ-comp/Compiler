using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables;
using Parse.FrontEnd.MiniC.Properties;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes
{
    public class UseIdentNode : ExprNode
    {
        public bool IsReverseRef { get; private set; } = false;
        public TokenData IdentToken { get; private set; }

        public VariableMiniC VarData => MiniCUtilities.GetVarRecordFromReferableST(this, IdentToken)?.DefineField;


        public UseIdentNode(AstSymbol node) : base(node)
        {
        }

        // [0] : * (option)
        // [1] : ident
        public override SdtsNode Build(SdtsParams param)
        {
            if (Items.Count == 2)
            {
                IsReverseRef = true;
                var ident = Items[1].Build(param) as TerminalNode;
                IdentToken = ident.Token;
            }
            else
            {
                var node = Items[0].Build(param) as TerminalNode;
                IdentToken = node.Token;
            }

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
            if (varData != null)
            {
                CheckReverseRef(varData.DefineField);
                return false;
            }

            var funcData = MiniCUtilities.GetFuncDataFromReferableST(this, IdentToken);
            if (funcData != null) return false;

            // Add semantic error information if varData is exist in the SymbolTable.
            if (IsReverseRef)
                ConnectedErrInfoList.Add
                    (
                        new MeaningErrInfo(IdentToken,
                                                        nameof(AlarmCodes.MCL0019), AlarmCodes.MCL0019)
                    );

            ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(IdentToken,
                                                    nameof(AlarmCodes.MCL0001),
                                                    string.Format(AlarmCodes.MCL0001, IdentToken.Input))
                );

            return true;
        }


        private void CheckReverseRef(VariableMiniC var)
        {
            // Add semantic error information if varData is exist in the SymbolTable.
            if (IsReverseRef && var.DataType != VariableMiniC.MiniCDataType.Address)
                ConnectedErrInfoList.Add
                    (
                        new MeaningErrInfo(IdentToken,
                                                        nameof(AlarmCodes.MCL0019), AlarmCodes.MCL0019)
                    );
        }
    }
}
