using Parse.FrontEnd.Ast;
using Parse.Types.ConstantTypes;
using System;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes
{
    public class AddExprNode : ArithmeticExprNode
    {
        public AddExprNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            base.Build(param);

            try
            {
                if (Left.Result is IntConstant)
                    Result = (Left.Result as IntConstant).Add(Right.Result);
            }
            catch(Exception ex)
            {

            }

            return this;
        }
    }
}
