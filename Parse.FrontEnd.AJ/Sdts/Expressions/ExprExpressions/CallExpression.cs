using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types.ConstantTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.Expressions.ExprExpressions
{
    public class CallExpression : ExprExpression, IRCallExpr
    {
        public IRFuncDefInfo FuncDefInfo { get; }
        public IEnumerable<IRExpr> Params { get; }

        public CallExpression(ICallExpression expr)
        {
            FuncDefInfo = new FunctionExpression(expr.CallFuncDef).ToIRData();
        }
    }
}
