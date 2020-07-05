using Parse.MiddleEnd.IR;
using Parse.MiddleEnd.IR.Datas.Types;
using Parse.MiddleEnd.IR.Datas.ValueDatas;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.LiteralDataFormat
{
    public class IntLiteralData : LiteralData, IRValue<Int>
    {
        private int _value = 0;

        public int RealValue => (IsVirtual) ? _value : System.Convert.ToInt32(ValueToken?.Input);

        public override bool IsZero => throw new System.NotImplementedException();
        public override DType TypeName => DType.Int;
        public override object Value => _value;
        public override bool Signed => throw new System.NotImplementedException();
        public override bool IsNan => throw new System.NotImplementedException();

        public IntLiteralData(TokenData token) : base(token)
        {
        }

        public IntLiteralData(int value) : base(null)
        {
            _value = value;
            IsVirtual = true;
        }

        private static LiteralData CoreLogic(IntLiteralData left, IRValue right, IROperation opKind)
        {
            if (right is IntLiteralData)
            {
                var cRight = right as IntLiteralData;

                int value = (opKind == IROperation.Add) ? (int)left.Value + (int)cRight.Value :
                                (opKind == IROperation.Sub) ? (int)left.Value - (int)cRight.Value :
                                (opKind == IROperation.Mul) ? (int)left.Value * (int)cRight.Value :
                                (opKind == IROperation.Div) ? (int)left.Value / (int)cRight.Value :
                                (int)left.Value % (int)cRight.Value;

                return new IntLiteralData(value);
            }
            else if (right is UnknownLiteralData)
            {
                var cRight = right as UnknownLiteralData;
                return new UnknownLiteralData(cRight.State, null);
            }

            return null;
        }

        public override IRValue Add(IRValue right) => CoreLogic(this, right, IROperation.Add);
        public override IRValue Sub(IRValue right) => CoreLogic(this, right, IROperation.Sub);
        public override IRValue Mul(IRValue right) => CoreLogic(this, right, IROperation.Mul);
        public override IRValue Div(IRValue right) => CoreLogic(this, right, IROperation.Div);
        public override IRValue Mod(IRValue right) => CoreLogic(this, right, IROperation.Mod);

        public override string ToString() => Value.ToString();

        public override object Clone() => (IsVirtual) ? new IntLiteralData((int)Value) : new IntLiteralData(ValueToken);

        public override IRValue<Bit> LogicalOp(IRValue t, IRCondition cond)
        {
            throw new System.NotImplementedException();
        }

        public override bool? IsEqual(IRValue t)
        {
            throw new System.NotImplementedException();
        }

        public override bool? IsNotEqual(IRValue t)
        {
            throw new System.NotImplementedException();
        }

        public override bool? IsGreaterThan(IRValue t)
        {
            throw new System.NotImplementedException();
        }

        public override bool? IsLessThan(IRValue t)
        {
            throw new System.NotImplementedException();
        }

        public override IRValue BinOp(IRValue t, IROperation operation)
        {
            throw new System.NotImplementedException();
        }
    }
}
