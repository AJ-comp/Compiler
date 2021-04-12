using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.MiddleEnd.IR.Interfaces;
using System;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes
{
    public class ModExprNode : ArithmeticExprNode
    {
        public override IROperation Operation => IROperation.Mod;

        public ModExprNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            return base.Build(param);
        }
    }
}
