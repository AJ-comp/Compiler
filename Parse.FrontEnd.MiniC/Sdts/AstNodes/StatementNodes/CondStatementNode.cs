using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.LogicalExprNodes;
using Parse.FrontEnd.MiniC.Sdts.Datas;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.StatementNodes
{
    public abstract class CondStatementNode : StatementNode, IConditionStatement
    {
        public ICompareExpression ConditionExpression { get; protected set; }
        public StatementNode TrueStatement { get; protected set; }
        public StatementNode FalseStatement { get; protected set; }

        protected CondStatementNode(AstSymbol node) : base(node)
        {
        }


        // [0] : TerminalNode [if]
        // [1] : ExprNode
        // [2] : StatementNode [statement]
        public override SdtsNode Build(SdtsParams param)
        {
            if (Items[1] is CompareExprNode)
            {
                ConditionExpression = Items[1].Build(param) as CompareExprNode;
            }
            else if(Items[1] is NotExprNode)
            {
                ConditionExpression = Items[1].Build(param) as NotExprNode;
            }
            else
            {
                // add an error
            }

            TrueStatement = Items[2].Build(param) as StatementNode;

            return this;
        }
    }
}
