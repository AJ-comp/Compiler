using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Parse
{
    public enum IRBinaryOperation
    {
        [Description("=")] Assign,

        [Description("==")] EQ,
        [Description("!=")] NE,
        [Description(">")] GT,
        [Description(">=")] GE,
        [Description("<")] LT,
        [Description("<=")] LE,

        [Description("+")] Add,
        [Description("-")] Sub,
        [Description("*")] Mul,
        [Description("/")] Div,
        [Description("%")] Mod,

        [Description("+=")] AddAssign,
        [Description("-=")] SubAssign,
        [Description("*=")] MulAssign,
        [Description("/=")] DivAssign,
        [Description("%=")] ModAssign,

        [Description("&&")] And,
        [Description("||")] Or,

        [Description("&")] BitAnd,
        [Description("|")] BitOr,
        [Description(">>")] RightShift,
        [Description("<<")] LeftShift,

        [Description("&=")] BitAndAssign,
        [Description("|=")] BitOrAssign,
        [Description(">>=")] RightShiftAssign,
        [Description("<<=")] LeftShiftAssign,
    }

    public enum IRSingleOperation
    {
        [Description("*")] DeRef,
        [Description("!")] Not,
        [Description("~")] BitNot,
        PreInc,
        PreDec,
        PostInc,
        PostDec,
    }

    public enum IRCompareOperation
    {
        [Description("==")] EQ,
        [Description("!=")] NE,
        [Description(">")] GT,
        [Description(">=")] GE,
        [Description("<")] LT,
        [Description("<=")] LE,
    };

    public enum IRArithmeticOperation
    {
        [Description("+")] Add,
        [Description("-")] Sub,
        [Description("*")] Mul,
        [Description("/")] Div,
        [Description("%")] Mod,
    };

    public enum IRLogicalOperation
    {
        [Description("&&")] And,
        [Description("||")] Or,
        [Description("!")] Not,
    }

    public enum IRBitwiseOperation
    {
        [Description("&")] BitAnd,
        [Description("|")] BitOr,
        [Description(">>")] RightShift,
        [Description("<<")] LeftShift,
        [Description("~")] BitNot,
        [Description("^")] BitXor,
    }


    public enum IRBitAssignOperation
    {
        [Description("&=")] BitAndAssign,
        [Description("|=")] BitOrAssign,
        [Description(">>=")] RightShiftAssign,
        [Description("<<=")] LeftShiftAssign,
    }

    public enum Info { PreInc, PreDec, PostInc, PostDec };
}
