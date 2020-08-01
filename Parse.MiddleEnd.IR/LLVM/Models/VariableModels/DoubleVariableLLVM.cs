using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using Parse.Types.ConstantTypes;
using Parse.Types.Operations;
using System;

namespace Parse.MiddleEnd.IR.LLVM.Models.VariableModels
{
    public class DoubleVariableLLVM : VariableLLVM, IRDoubleVar
    {
        public int Size => 64;
        public bool Nan { get; }

        public override DType TypeName => DType.Double;

        public DoubleVariableLLVM(int offset, DoubleConstant value) : base(offset, value)
        {
            Nan = value.Nan;
        }

        public DoubleVariableLLVM(string varName, DoubleConstant value) : base(varName, value)
        {
        }

        public DoubleVariableLLVM(IRDoubleVar var) : base(var)
        {
            Nan = var.Nan;
        }

        public IConstant Equal(IValue operand) => Operation.ArithmeticEqual(this, operand);
        public IConstant NotEqual(IValue operand) => Operation.ArithmeticNotEqual(this, operand);
        public IConstant Add(IValue operand) => Operation.ArithmeticAdd(this, operand);
        public IConstant Sub(IValue operand) => Operation.ArithmeticSub(this, operand);
        public IConstant Mul(IValue operand) => Operation.ArithmeticMul(this, operand);
        public IConstant Div(IValue operand) => Operation.ArithmeticDiv(this, operand);
        public IConstant Mod(IValue operand) => Operation.ArithmeticMod(this, operand);

        public override IConstant Assign(IValue operand)
        {
            throw new NotImplementedException();
        }
    }
}
