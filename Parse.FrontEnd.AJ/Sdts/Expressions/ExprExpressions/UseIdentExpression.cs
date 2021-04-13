using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types.ConstantTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.Expressions.ExprExpressions
{
    public class UseIdentExpression : ExprExpression, IRUseVarExpr
    {
        public IRDeclareVar DeclareInfo => throw new NotImplementedException();

        public UseIdentExpression(IUseIdentExpression expr) : base(expr)
        {
        }
    }
}
