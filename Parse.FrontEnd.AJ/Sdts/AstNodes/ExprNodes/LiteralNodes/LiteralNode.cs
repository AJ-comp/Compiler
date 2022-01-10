using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using Parse.Types;
using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes
{
    public abstract class LiteralNode : ExprNode
    {
        public TokenData Token { get; protected set; }
        public ConstantAJ Constant { get; }

        protected LiteralNode(AstSymbol node) : base(node)
        {
        }

        public static LiteralNode CreateLiteralNode(ConstantAJ value)
        {
            if (value.Type.DataType == AJDataType.Int) return new IntegerLiteralNode((int)value.Value);
            if (value.Type.DataType == AJDataType.Short) return new IntegerLiteralNode((int)value.Value);

            return null;
        }
    }
}
