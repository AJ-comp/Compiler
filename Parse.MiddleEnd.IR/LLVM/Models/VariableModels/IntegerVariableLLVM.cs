﻿using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using Parse.Types.ConstantTypes;
using Parse.Types.Operations;
using System;

namespace Parse.MiddleEnd.IR.LLVM.Models.VariableModels
{
    public abstract class IntegerVariableLLVM : VariableLLVM, IRIntegerVar
    {
        public bool Signed { get; }
        public abstract int Size { get; }

        protected IntegerVariableLLVM(int offset, IntegerConstant value) : base(offset, value)
        {
            Signed = value.Signed;
        }

        protected IntegerVariableLLVM(string varName, Constant value) : base(varName, value)
        {
        }

        protected IntegerVariableLLVM(IRIntegerVar var) : base(var)
        {
            Signed = var.Signed;
        }

        public IConstant Equal(IValue operand) => Operation.ArithmeticEqual(this, operand);
        public IConstant NotEqual(IValue operand) => Operation.ArithmeticNotEqual(this, operand);
        public IConstant Add(IValue operand) => Operation.ArithmeticAdd(this, operand);
        public IConstant Sub(IValue operand) => Operation.ArithmeticSub(this, operand);
        public IConstant Mul(IValue operand) => Operation.ArithmeticMul(this, operand);
        public IConstant Div(IValue operand) => Operation.ArithmeticDiv(this, operand);
        public IConstant Mod(IValue operand) => Operation.ArithmeticMod(this, operand);
        public IConstant BitAnd(IValue operand) => Operation.IntegerKindBitAnd(this, operand);
        public IConstant BitOr(IValue operand) => Operation.IntegerKindBitOr(this, operand);
        public IConstant BitNot() => Operation.IntegerKindBitNot(this);
        public IConstant BitXor(IValue operand) => Operation.IntegerKindBitXor(this, operand);
        public IConstant LeftShift(int count) => Operation.IntegerKindLeftShift(this, count);
        public IConstant RightShift(int count) => Operation.IntegerKindRightShift(this, count);


        public override IConstant Assign(IValue operand)
        {
            throw new NotImplementedException();
        }
    }
}