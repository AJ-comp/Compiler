using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.Types;
using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes
{
    public abstract class ExprNode : MiniCNode, IExprExpression
    {
        public StdType DataKind => Result.TypeKind;
        public object ResultValue => Result.Value;
        public IConstant Result { get; protected set; }

        public bool AlwaysTrue { get; } = false;

        protected ExprNode(AstSymbol node) : base(node)
        {
        }
    }
}
