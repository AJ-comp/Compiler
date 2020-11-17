using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using Parse.Types.ConstantTypes;
using Parse.Types.Operations;
using Parse.Types.VarTypes;
using System;

namespace Parse.FrontEnd.MiniC.Sdts.Datas.Variables
{
    public class IntVariableMiniC : VariableMiniC, IInt, IRSignableVar
    {
        public bool Signed { get; }
        public int Size => 32;
        public override DType TypeName => DType.Int;

        public IntVariableMiniC(MiniCTypeInfo typeDatas, 
                                            TokenData nameToken,
                                            TokenData levelToken, 
                                            TokenData dimensionToken,
                                            int blockLevel, 
                                            int offset, 
                                            VarProperty varProperty, 
                                            ExprNode value)
                                        : base(typeDatas, nameToken, levelToken, dimensionToken,
                                                    blockLevel, offset, varProperty, VariableMiniC.Convert(varProperty, value))
        {

        }

        public override bool CanAssign(IValue operand)
        {
            if (!Operation.CanOperation(this, operand)) return false;
            if (!(operand is IIntegerKind)) return false;

            return true;
        }

        public override IConstant Assign(IValue operand)
        {
            if(!CanAssign(operand)) throw new NotSupportedException();

            if (operand is IVariable)
            {
                var valueConstant = (operand as IVariable).ValueConstant;

                // operand may be not int type so it has to make int type explicity.
                ValueConstant = new IntConstant((int)valueConstant.Value,
                                                                    valueConstant.ValueState);
            }
            else
            {
                var valueConstant = (operand as IConstant);

                // operand may be not int type so it has to make int type explicity.
                ValueConstant = new IntConstant((int)valueConstant.Value,
                                                                    valueConstant.ValueState);
            }

            return ValueConstant;
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
    }
}
