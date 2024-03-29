﻿using AJ.Common.Helpers;
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
using System.Threading;

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
        [Description("%while.cond")] WhileCondVar,
        [Description("%while.body")] WhileBodyVar,
        [Description("%while.end")] WhileEndVar,
        [Description("%call")] CallVar,

        [Description("%add")] AddVar,
        [Description("%sub")] SubVar,
        [Description("%mul")] MulVar,
        [Description("%div")] DivVar,
        [Description("%rem")] RemVar,
        [Description("%return")] ReturnVar,

        LiteralVar,
    }

    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class LLVMVar : IRExpr
    {
        public long Index { get; } = Interlocked.Increment(ref _nextId);

        public LLVMVarType VarType { get; }
        public IRExpr Expr { get; }

        public string OriginalName { get; protected set; }

        /// <summary>
        /// This value set only in LLVMFunction
        /// </summary>
        public string NameInFunction { get; internal set; } = string.Empty;
        public string TypeName => LLVMConverter.GetTypeName(Type);
        public uint Size => Type.Size;


        public LLVMVar(LLVMVarType varType, IRExpr expr) : base(expr.Type)
        {
            VarType = varType;
            Expr = expr;
        }

        public LLVMVar(LLVMVarType varType, IRType type) : base(type)
        {
            VarType = varType;
        }


        public static LLVMVar CreateCmpVar() => new LLVMVar(LLVMVarType.CmpVar, new IRType(StdType.Bit, 0));
        public static LLVMVar CreateLabelVar(LLVMVarType type) => new LLVMVar(type, new IRType(StdType.Bit, 0));

        private string GetDebuggerDisplay() => NameInFunction;
        private static long _nextId = long.MinValue;
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


    public class LLVMUniqueVar : LLVMVar
    {
        public LLVMUniqueVar(LLVMVarType varType, IRType type) : base(varType, type)
        {
        }
    }
}
