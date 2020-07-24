using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.LogicalExprNodes
{
    public class NotEqualExprNode : LogicalExprNode
    {
        public NotEqualExprNode(AstSymbol node) : base(node)
        {
        }
    }
}
