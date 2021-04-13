using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LogicalExprNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public class IfStatementNode : StatementNode, IConditionStatement
    {
        // for interface ********************************************/
        public ICompareExpression ConditionExpression { get; private set; }
        public StatementNode TrueStatement { get; private set; }
        public virtual StatementNode FalseStatement => throw new System.NotImplementedException();
        /*******************************************************/


        public IfStatementNode(AstSymbol node) : base(node)
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
            else if (Items[1] is NotExprNode)
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
