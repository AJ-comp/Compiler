using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using Parse.Types;
using Parse.Types.ConstantTypes;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes
{
    public abstract class LiteralNode : ExprNode
    {
        public TokenData Token { get; protected set; }

        protected LiteralNode(AstSymbol node) : base(node)
        {
        }

        public static LiteralNode CreateLiteralNode(ExprNode exprNode)
        {
            if (exprNode.Type.DataType == AJDataType.Int) return new IntegerLiteralNode((int)exprNode.Value);
            if (exprNode.Type.DataType == AJDataType.Short) return new IntegerLiteralNode((int)exprNode.Value);

            return null;
        }
    }
}
