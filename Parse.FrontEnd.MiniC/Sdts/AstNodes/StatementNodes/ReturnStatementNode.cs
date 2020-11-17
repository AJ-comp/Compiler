using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.StatementNodes
{
    public class ReturnStatementNode : StatementNode
    {
        public ExprNode ReturnValue { get; private set; }

        public ReturnStatementNode(AstSymbol node) : base(node)
        {
        }


        // [0] : return (AstTerminal)
        // [1] : ExpSt (AstNonTerminal)
        public override SdtsNode Build(SdtsParams param)
        {
            ReturnValue = Items[1].Build(param) as ExprNode;

            return this;
        }
    }
}
