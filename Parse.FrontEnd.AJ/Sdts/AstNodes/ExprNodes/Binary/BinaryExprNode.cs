using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary
{
    public abstract class BinaryExprNode : ExprNode
    {
        public ExprNode LeftNode { get; private set; }
        public ExprNode RightNode { get; private set; }

        public bool IsBothLiteral => LeftNode is LiteralNode && RightNode is LiteralNode;


        protected BinaryExprNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Compile(CompileParameter param)
        {
            Alarms.Clear();

            // ExprNode or TerminalNode
            LeftNode = Items[0].Compile(param) as ExprNode;
            RightNode = Items[1].Compile(param) as ExprNode;

            return this;
        }
    }
}
