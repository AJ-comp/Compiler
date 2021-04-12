﻿using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.MiniC.Sdts.Datas;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.StatementNodes
{
    public class ExprStatementNode : StatementNode, IExprTypeStatement
    {
        public ExprNode Expr { get; private set; }

        public ExprStatementNode(AstSymbol node) : base(node)
        {
        }


        // format summary
        // (AddAssign | SubAssign | MulAssign | DivAssign | ...).Optional ;
        public override SdtsNode Build(SdtsParams param)
        {
            if (Items.Count > 0)
            {
                Expr = Items[0].Build(param) as ExprNode;
            }

            return this;
        }
    }
}
