using AJ.Common.Helpers;
using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions.ExprExpressions
{
    public class IRBinaryExpr : IRExpr
    {
        public IRBinaryOperation Operation { get; set; }

        public IRExpr Left { get; set; }
        public IRExpr Right { get; set; }

        public bool AlwaysTrue { get; set; }
        public bool AlwaysFalse { get; set; }


        public bool Compareable
        {
            get
            {
                if (Operation == IRBinaryOperation.EQ) return true;
                if (Operation == IRBinaryOperation.NE) return true;
                if (Operation == IRBinaryOperation.GE) return true;
                if (Operation == IRBinaryOperation.GT) return true;
                if (Operation == IRBinaryOperation.LE) return true;
                if (Operation == IRBinaryOperation.LT) return true;

                return false;
            }
        }


        public IRBinaryExpr(IRType type) : base(type)
        {
        }


        public override string ToString()
        {
            return $"{Left} {Operation.ToDescription()} {Right}";
        }
    }
}
