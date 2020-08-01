using Parse.FrontEnd.Ast;
using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes
{
    public abstract class ExprNode : MiniCNode
    {
        public IConstant Result { get; protected set; }

        protected ExprNode(AstSymbol node) : base(node)
        {
        }
    }
}
