using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using Parse.Types.ConstantTypes;
using System.Collections.Generic;
using System.ComponentModel;

namespace Parse.MiddleEnd.IR.Interfaces
{
    public interface IRItem
    {
    }

    public interface IRExpr : IRItem
    {
        IConstant Result { get; }
    }


    public interface IRBinOpExpr : IRExpr
    {
        IRExpr Left { get; }
        IRExpr Right { get; }
    }

    public interface IRSingleExpr : IRExpr
    {
        IRExpr Expr { get; }
    }


    public enum IRCompareSymbol
    {
        [Description("==")] EQ,
        [Description("!=")] NE,
        [Description(">")] GT,
        [Description(">=")] GE,
        [Description("<")] LT,
        [Description("<=")] LE
    };

    public enum IROperation
    {
        [Description("+")] Add,
        [Description("-")] Sub,
        [Description("*")] Mul,
        [Description("/")] Div,
        [Description("%")] Mod
    };

    public interface IRLogicalOpExpr : IRExpr
    {
        IRCompareSymbol CompareSymbol { get; }
    }


    public interface IRCompareOpExpr : IRBinOpExpr, IRLogicalOpExpr
    {
    }

    public interface IRArithmeticOpExpr : IRBinOpExpr
    {
        IROperation Operation { get; }
    }

    public interface IRAssignOpExpr : IRExpr
    {
        public IRDeclareVar Var { get; }
        public IRExpr Right { get; }
    }

    public enum Info { PreInc, PreDec, PostInc, PostDec };
    public interface IRIncDecExpr : IRExpr
    {
        Info ProcessInfo { get; }
        IRDeclareVar Var { get; }
    }

    public interface IRArithmeticAssignOpExpr : IRAssignOpExpr
    {
        IROperation Operation { get; }
    }










    public interface IRLiteralExpr : IRExpr
    {
    }

    public interface IRCharLiteralExpr : IRLiteralExpr
    {
        char Value { get; }
    }

    public interface IRBitLiteralExpr : IRLiteralExpr
    {
        bool Value { get; }
    }

    public interface IRInt8LiteralExpr : IRLiteralExpr
    {
        bool Signed { get; }
        byte Value { get; }
    }

    public interface IRInt16LiteralExpr : IRLiteralExpr
    {
        bool Signed { get; }
        short Value { get; }
    }

    public interface IRInt32LiteralExpr : IRLiteralExpr
    {
        bool Signed { get; }
        int Value { get; }
    }

    public interface IRFloatLiteralExpr : IRLiteralExpr
    {
        float Value { get; }
    }

    public interface IRDoubleLiteralExpr : IRLiteralExpr
    {
        double Value { get; }
    }

    public interface IRStringLiteralExpr : IRLiteralExpr
    {
        string Value { get; }
    }










    public interface IRUseVarExpr : IRExpr
    {
        IRDeclareVar DeclareInfo { get; }
    }

    public interface IRUseDeRefExpr : IRExpr
    {
        IRDeclareVar Var { get; }
    }

    public interface IRUseMemberVarExpr : IRExpr
    {
        IRDeclareStructTypeVar StructVar { get; }
        int Offset { get; }
    }

    public interface IRUseFunctionExpr : IRExpr
    {
        IRFuncDefInfo Func { get; }
    }


    public interface IRCallExpr : IRExpr
    {
        IRFuncDefInfo FuncDefInfo { get; }
        IEnumerable<IRExpr> Params { get; }
    }
}
