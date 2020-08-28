using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.LiteralNodes;
using System;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes
{
    public class ModExprNode : ArithmeticExprNode
    {
        public ModExprNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            base.Build(param);

            try
            {
                if (!(Left is LiteralNode) || !(Right is LiteralNode)) return this;

                if (Left is IntLiteralNode)
                    (Left as IntLiteralNode).LiteralData.Mod(Right.Result);
            }
            catch (Exception ex)
            {

            }

            return this;
        }
    }
}
