using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes
{
    public class AddExprNode : ArithmeticExprNode
    {
        public override IROperation Operation => IROperation.Add;

        public AddExprNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            return base.Build(param);
        }
    }
}
