using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public class ReturnStatementNode : StatementNode
    {
        /// <summary>
        /// return expression
        /// </summary>
        public ExprNode Expr { get; private set; }

        public ReturnStatementNode(AstSymbol node) : base(node)
        {
        }



        /// <summary>
        /// format summary  <br/>
        /// [0] : return (AstTerminal)  <br/>
        /// [1] : ExpSt? (AstNonTerminal)   <br/>
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);

            var classDefNode = GetParent(typeof(ClassDefNode)) as ClassDefNode;
            var funcNode = GetParent(typeof(FuncDefNode)) as FuncDefNode;

            // return;
            if (Items.Count == 1)
            {
                var returnNode = Items[0].Compile(param) as TerminalNode;

                if (funcNode.ReturnDataType != AJDataType.Void)
                    Alarms.Add(new MeaningErrInfo(returnNode.Token, nameof(AlarmCodes.AJ0029), AlarmCodes.AJ0029));
            }
            // return value;
            else
            {
                Expr = Items[1].Compile(param) as ExprNode;

                if (funcNode.ReturnDataType == AJDataType.Void)
                    Alarms.Add(new MeaningErrInfo(Expr.AllTokens, nameof(AlarmCodes.AJ0028), AlarmCodes.AJ0028));
                else if (!funcNode.ReturnType.IsIncludeType(Expr.Type))
                    Alarms.Add(AJAlarmFactory.CreateAJ0030(Expr, funcNode.ReturnType));
            }

            return this;
        }

        public override IRExpression To() => new IRReturnExpr(Expr.To() as IRExpr);

        public override IRExpression To(IRExpression from)
        {
            throw new System.NotImplementedException();
        }
    }
}
