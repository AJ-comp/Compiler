using Parse.FrontEnd.Ast;
using Parse.Types;
using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes
{
    public abstract class LiteralNode : ExprNode
    {
        public TokenData Token { get; protected set; }

        protected LiteralNode(AstSymbol node) : base(node)
        {
        }

        public static LiteralNode CreateLiteralNode(IConstant value)
        {
            if (value is IntConstant) return new IntLiteralNode(value as IntConstant);

            return null;
        }
    }
}
