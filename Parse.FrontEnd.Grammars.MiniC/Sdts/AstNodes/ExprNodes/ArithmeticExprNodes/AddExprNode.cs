using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
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
                if (Left.Result is IntLiteralData)
                    Result = (Left.Result as IntLiteralData).Add(Right.Result);
            }
            catch(Exception ex)
            {

            }

            return this;
        }
    }
}
