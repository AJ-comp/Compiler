using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes
{
    public class UseIdentNode : ExprNode
    {
        public TokenData IdentToken { get; private set; }
        public VariableAJ Var { get; private set; }
        public FuncDefNode Func { get; private set; }
        public ISymbolData UsedSymbolData => GetSymbol(IdentToken);


        public UseIdentNode(AstSymbol node) : base(node)
        {
        }

        /// <summary>
        /// [0] : ident <br/>
        /// This function checks condition as the below <br/>
        /// 1. is it defined?
        /// 2. if ident is variable is it initialized before use?
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override SdtsNode Compile(CompileParameter param)
        {
            ConnectedErrInfoList.Clear();

            var node = Items[0].Compile(param) as TerminalNode;
            IdentToken = node.Token;

            var symbolData = UsedSymbolData;
            if (symbolData == null) Alarms.Add(AJAlarmFactory.CreateMCL0001(IdentToken));
            else if(symbolData.Token.IsVirtual) Alarms.Add(AJAlarmFactory.CreateMCL0001(IdentToken));
            else if (symbolData is VariableAJ)
            {
                Var = symbolData as VariableAJ;
                if (!Var.IsInitialized) Alarms.Add(AJAlarmFactory.CretaeMCL0005(Var.Token));

                Result = (symbolData as VariableAJ).ToConstantAJ();
            }
            else if (symbolData is FuncDefNode)
            {
                Func = symbolData as FuncDefNode;
                Result = (symbolData as FuncDefNode).ReturnValue;
            }

            DBContext.Instance.Insert(this);

            return this;
        }

        public override IRExpression To()
        {
            throw new System.NotImplementedException();
        }

        public override IRExpression To(IRExpression from)
        {
            throw new System.NotImplementedException();
        }
    }
}
