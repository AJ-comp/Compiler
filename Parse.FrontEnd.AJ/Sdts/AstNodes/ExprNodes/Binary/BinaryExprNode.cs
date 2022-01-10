using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary
{
    public abstract class BinaryExprNode : ExprNode
    {
        public ExprNode LeftNode => Items[0] as ExprNode;
        public ExprNode RightNode => Items[1] as ExprNode;

        public bool IsBothLiteral => LeftNode is LiteralNode && RightNode is LiteralNode;


        protected BinaryExprNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Compile(CompileParameter param)
        {
            Alarms.Clear();

            // ExprNode or TerminalNode
            Items[0].Compile(param);
            Items[1].Compile(param);

            return this;
        }
    }
}
