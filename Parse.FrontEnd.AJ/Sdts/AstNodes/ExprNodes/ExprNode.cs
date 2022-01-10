using AJ.Common;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes
{
    public abstract class ExprNode : AJNode, IHasType, IExportable<IRExpression>
    {
        public object ResultValue => Result.Value;
        public AJTypeInfo Type => Result.Type;
        public ConstantAJ Result { get; set; }

        public bool AlwaysTrue { get; } = false;

        protected ExprNode(AstSymbol node) : base(node)
        {
        }

        public abstract IRExpression To();
        public abstract IRExpression To(IRExpression from);
    }
}
