using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types;
using Parse.Types.ConstantTypes;
using System;

namespace Parse.FrontEnd.AJ.Sdts.Expressions.ExprExpressions
{
    public abstract class LiteralExpression
    {
        public static IRExpr Create(IConstant constant)
        {
            if (constant.TypeKind == StdType.Bit) return new LiteralBoolExpression(false);

            if (constant.TypeKind == StdType.Byte ||
               constant.TypeKind == StdType.Short ||
               constant.TypeKind == StdType.Int) return new LiteralIntegerExpression(0);

            if (constant.TypeKind == StdType.SByte ||
               constant.TypeKind == StdType.UShort ||
               constant.TypeKind == StdType.UInt) return new LiteralIntegerExpression((uint)0);

            if (constant.TypeKind == StdType.Double) return new LiteralDoubleExpression(0);
            if (constant.TypeKind == StdType.String) return new LiteralStringExpression("");

            throw new Exception();
        }
    }
}
