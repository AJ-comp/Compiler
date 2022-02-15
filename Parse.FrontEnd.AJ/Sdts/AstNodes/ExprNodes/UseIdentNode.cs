using Parse.FrontEnd.AJ.Data;
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
        /// format summary  <br/>
        /// [0] : Ident     <br/>
        /// [1:n] : Ident   (option)    <br/>
        /// This function checks condition as the below <br/>
        /// 1. is it defined?   <br/>
        /// 2. if ident is the reference type variable is it initialized before use?   <br/>
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

            var node = Items[0].Compile(param) as TerminalNode;
            IdentToken = node.Token;

            if (!CheckIsDefinedSymbol(IdentToken)) return this;

            var symbolData = UsedSymbolData;
            if (symbolData is VariableAJ)
            {
                Var = symbolData as VariableAJ;
                if(Var.VariableType == VarType.ReferenceType && !Var.IsInitialized)
                    Alarms.Add(AJAlarmFactory.CretaeMCL0005(IdentToken));

                Result = (symbolData as VariableAJ).ToConstantAJ();
            }
            else if (symbolData is FuncDefNode)
            {
                Func = symbolData as FuncDefNode;
                Result = (symbolData as FuncDefNode).ReturnValue;
            }

            //            DBContext.Instance.Insert(this);

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
