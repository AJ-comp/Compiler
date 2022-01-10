using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Parse.MiddleEnd.IR
{
    public class IRCode
    {
    }


    public enum OperatorCode
    {
        [Description("equal")] Equal,
        [Description("add")] Add,
        [Description("sub")] Sub,
        [Description("mul")] Mul,
        [Description("div")] Div,
        [Description("mod")] Mod,
        [Description("add_assign")] AddAssign,
        [Description("sub_assign")] SubAssign,
        [Description("mul_assign")] MulAssign,
        [Description("div_assign")] DivAssign,
        [Description("remain_assign")] RemainAssign,

        [Description("call")] Call,
        [Description("pre_inc")] PreInc,
        [Description("post_inc")] PostInc,
        [Description("pre_dec")] PreDec,
        [Description("post_dec")] PostDec,

        [Description("and")] And,
        [Description("or")] Or,
        [Description("not")] Not,
        [Description("xor")] Xor,
        [Description("leftshift")] LeftShift,
        [Description("rightshift")] RightShift,

        [Description("and_assign")] AndAssign,
        [Description("or_assign")] OrAssign,
        [Description("not_assign")] NotAssign,
        [Description("xor_assign")] XorAssign,
        [Description("leftshift_assign")] LeftShiftAssign,
        [Description("rightshift_assign")] RightShiftAssign,
    }


    public enum OperatorKindCode
    {
        [Description("assign")] Assign,
        [Description("arithmetic")] Arithmetic,
        [Description("bitwise")] BitWise,

        [Description("call")] Call,
        [Description("inc_dec")] IncDec,
    }
}
