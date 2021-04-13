using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.AJ.Sdts.Datas.Variables;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes
{
    public class UseIdentNode : ExprNode, IUseIdentExpression
    {
        public TokenData IdentToken { get; private set; }
        public VariableMiniC VarData { get; private set; }
        public StructVariableMiniC StructVarData { get; private set; }
        public FuncDefData FuncData { get; private set; }
        public ISymbolData UsedSymbolData => GetSymbol(IdentToken);


        // for interface **************************************************/
        public IDeclareVarExpression Var => VarData;
        public IFunctionExpression Func => FuncData;
        /*************************************************************/


        public UseIdentNode(AstSymbol node) : base(node)
        {
        }

        // [0] : ident
        public override SdtsNode Build(SdtsParams param)
        {
            ConnectedErrInfoList.Clear();

            var node = Items[0].Build(param) as TerminalNode;
            IdentToken = node.Token;

            var symbolData = UsedSymbolData;
            if (symbolData == null)
            {
                ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(IdentToken,
                                                    nameof(AlarmCodes.MCL0001),
                                                    string.Format(AlarmCodes.MCL0001, IdentToken.Input))
                );
            }
            else if (symbolData is VariableMiniC)
            {
                VarData = symbolData as VariableMiniC;
                Result = (symbolData as VariableMiniC).Result;

                VarData.ReferenceTable.Add(this);
            }
            else if (symbolData is FuncDefData)
            {
                FuncData = symbolData as FuncDefData;
                Result = (symbolData as FuncDefData).ReturnValue;
            }

            return this;
        }
    }
}
