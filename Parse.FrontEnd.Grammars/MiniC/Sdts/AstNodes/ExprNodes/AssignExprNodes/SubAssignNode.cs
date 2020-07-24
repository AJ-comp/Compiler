using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.AssignExprNodes
{
    public class SubAssignNode : ExprNode
    {
        public SubAssignNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            throw new NotImplementedException();
        }
    }
}
