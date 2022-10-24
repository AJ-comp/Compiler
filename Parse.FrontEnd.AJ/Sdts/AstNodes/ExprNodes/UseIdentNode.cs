using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes
{
    public class UseIdentNode : ExprNode
    {
        public TokenData IdentToken;
        public ISymbolData UsedSymbolData => GetSymbol(IdentToken);


        public UseIdentNode(AstSymbol node) : base(node)
        {
        }


        /// <summary>
        /// format summary  <br/>
        /// [0] : Ident     <br/>
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);

            var tNode = Items[0].Compile(param) as TerminalNode;
            IdentToken = tNode.Token;

            var symbol = GetSymbol(IdentToken);
            if (symbol is null)
            {
                Alarms.Add(AJAlarmFactory.CreateMCL0001(IdentToken));
                return this;
            }

            Type = symbol.Type;
            if (Type is AJUnknownType) AddAlarmUnknownType(IdentToken);

            return this;
        }


        public override IRExpression To()
        {
            // only variable
            var ajVar = UsedSymbolData as VariableAJ;

            IRUseIdentExpr result = new IRUseIdentExpr(Type.ToIR(), ajVar.ToIR());

            return result;
        }

        public override IRExpression To(IRExpression from)
        {
            throw new System.NotImplementedException();
        }
    }
}
