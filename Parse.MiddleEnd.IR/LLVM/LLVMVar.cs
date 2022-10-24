using AJ.Common.Helpers;
using Parse.Extensions;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using Parse.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace Parse.MiddleEnd.IR.LLVM
{
    public enum LLVMVarType
    {
        [Description("%")] NormalVar,
        [Description("%")] NamedVar,
        [Description("%inc")] IncVar,
        [Description("%dec")] DecVar,
        [Description("%cmp")] CmpVar,
        [Description("%if.then")] IfVar,
        [Description("%if.else")] IfElseVar,
        [Description("%if.end")] IfEndVar,

        [Description("%add")] AddVar,
        [Description("%sub")] SubVar,
        [Description("%mul")] MulVar,
        [Description("%div")] DivVar,

        LiteralVar,
    }

    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class LLVMVar
    {
        public LLVMVarType VarType { get; }
        public IRExpr Expr { get; }

        public string OriginalName => $"{Expr}";

        /// <summary>
        /// This value set only in LLVMFunction
        /// </summary>
        public string NameInLLVMFunction { get; internal set; } = string.Empty;
        public string TypeName
        {
            get
            {
                string result = string.Empty;

                if (_type.Type == StdType.Void) result = "void";
                else if (_type.Type == StdType.Bit) result = "i1";
                else if (_type.Type == StdType.Char) result = "i8";
                else if (_type.Type == StdType.UChar) result = "i8";
                else if (_type.Type == StdType.Short) result = "i16";
                else if (_type.Type == StdType.UShort) result = "i16";
                else if (_type.Type == StdType.Int) result = "i32";
                else if (_type.Type == StdType.UInt) result = "i32";
                else if (_type.Type == StdType.Double) result = "double";
                else if (_type.Type == StdType.Struct) result = $"%struct.{OriginalName.Replace(".", "_")}{_type.PointerLevel.ToAnyStrings("*")}";

                return result;
            }
        }
        public uint Size => _type.Size;


        public LLVMVar(LLVMVarType varType, IRExpr expr)
        {
            VarType = varType;
            Expr = expr;
            _type = Expr.Type;
        }

        public LLVMVar(LLVMVarType varType, IRType type)
        {
            VarType = varType;
            _type = type;
        }


        public static LLVMVar CreateCmpVar() => new LLVMVar(LLVMVarType.CmpVar, new IRType(StdType.Bit, 0));
        public static LLVMVar CreateLabelVar(LLVMVarType type) => new LLVMVar(type, new IRType(StdType.Bit, 0));


        private IRType _type;

        private string GetDebuggerDisplay() => NameInLLVMFunction;
    }


    public class LLVMNamedVar : LLVMVar
    {
        public IRVariable Variable { get; }

        public LLVMNamedVar(IRVariable variable) : base(LLVMVarType.NamedVar, variable.Type)
        {
            Variable = variable;
        }
    }


    public class LLVMLiteralVar : LLVMVar
    {
        public object Value { get; }

        public LLVMLiteralVar(IRLiteralExpr expr) : base(LLVMVarType.LiteralVar, expr.Type)
        {
            Value = expr.Value;
        }
    }
}
