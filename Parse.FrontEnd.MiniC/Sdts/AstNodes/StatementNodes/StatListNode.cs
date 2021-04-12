using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.StatementNodes
{
    public class StatListNode : StatementNode
    {
        public StatListNode(AstSymbol node) : base(node)
        {
        }


        // format summary
        // IfSt | IfElseSt | WhileSt | ExpSt | ReturnSt
        public override SdtsNode Build(SdtsParams param)
        {
            foreach (var item in Items)
            {
                var statementNode = item.Build(param) as StatementNode;
                _statementNodes.Add(statementNode);


                if (statementNode is IfStatementNode)
                {

                }
                else if (statementNode is IfElseStatementNode)
                {

                }
                else if (statementNode is WhileStatementNode)
                {

                }
                else if (statementNode is ExprStatementNode)
                {

                }
                else if (statementNode is ReturnStatementNode)
                {

                }
            }

            return this;
        }
    }
}
