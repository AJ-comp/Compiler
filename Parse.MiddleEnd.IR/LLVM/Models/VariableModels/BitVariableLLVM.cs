using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using Parse.Types.ConstantTypes;
using Parse.Types.Operations;
using System;

namespace Parse.MiddleEnd.IR.LLVM.Models.VariableModels
{
    public class BitVariableLLVM : VariableLLVM, IBit
    {
        public override DType TypeName => DType.Bit;

        public BitVariableLLVM(int offset, BitConstant value) : base(offset, value)
        {
        }

        public BitVariableLLVM(int offset, bool value) : this(offset, new BitConstant(value))
        {
        }

        public BitVariableLLVM(int offset, uint pointerLevel) : base(offset, new BitConstant(false, State.NotInit, pointerLevel))
        {
        }

        public BitVariableLLVM(string varName, BitConstant value) : base(varName, value)
        {
        }

        public BitVariableLLVM(IRVar var) : base(var)
        {
        }

        public IConstant And(IValue operand) => Operation.BitTypeAnd(this, operand);
        public IConstant BitAnd(IValue operand) => Operation.BitTypeBitAnd(this, operand);
        public IConstant BitNot() => Operation.BitTypeBitNot(this);
        public IConstant BitOr(IValue operand) => Operation.BitTypeBitOr(this, operand);
        public IConstant BitXor(IValue operand) => Operation.BitTypeBitXor(this, operand);
        public IConstant Equal(IValue operand) => Operation.BitTypeEqual(this, operand);
        public IConstant LeftShift(int count) => Operation.BitTypeLeftShift(this, count);
        public IConstant Not() => Operation.BitTypeNot(this);
        public IConstant NotEqual(IValue operand) => Operation.BitTypeNotEqual(this, operand);
        public IConstant Or(IValue operand) => Operation.BitTypeOr(this, operand);
        public IConstant RightShift(int count) => Operation.BitTypeRightShift(this, count);

        public override IConstant Assign(IValue operand)
        {
            throw new NotImplementedException();
        }
    }
}
