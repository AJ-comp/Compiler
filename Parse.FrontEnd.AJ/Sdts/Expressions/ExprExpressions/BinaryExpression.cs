using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.Expressions.ExprExpressions
{
    public abstract class BinaryExpression : ExprExpression
    {
        protected BinaryExpression()
        {
        }

        protected BinaryExpression(ExprNode node) : base(node)
        {
        }
    }
}
