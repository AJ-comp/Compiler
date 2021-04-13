using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.Types;
using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes
{
    public abstract class ExprNode : AJNode, IExprExpression
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
