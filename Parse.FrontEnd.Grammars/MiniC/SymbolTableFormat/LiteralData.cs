namespace Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat
{
    public enum UnknownState { NotInitialized=1, DynamicAllocation=2 }

    public abstract class LiteralData
    {
        public string LiteralName => ValueToken?.Input;

        public TokenData ValueToken { get; }

        public bool IsVirtual { get; protected set; } = false;

        protected LiteralData(TokenData valueToken)
        {
            ValueToken = valueToken;
        }

        public abstract LiteralData Add(LiteralData right);
        public abstract LiteralData Sub(LiteralData right);
        public abstract LiteralData Mul(LiteralData right);
        public abstract LiteralData Div(LiteralData right);
        public abstract LiteralData Mod(LiteralData right);
    }

    public abstract class DecidedLiteralData : LiteralData
    {
        protected enum OpKind { Add, Sub, Mul, Div, Mod };

        protected DecidedLiteralData(TokenData valueToken) : base(valueToken)
        {
        }
    }

    public class CharLiteralData : DecidedLiteralData
    {
        public CharLiteralData(object value, TokenData valueToken) : base(valueToken)
        {
        }

        public override LiteralData Add(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Div(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Mod(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Mul(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Sub(LiteralData right)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ShortLiteralData : DecidedLiteralData
    {
        public ShortLiteralData(object value, TokenData valueToken) : base(valueToken)
        {
        }

        public override LiteralData Add(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Div(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Mod(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Mul(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Sub(LiteralData right)
        {
            throw new System.NotImplementedException();
        }
    }

    public class IntLiteralData : DecidedLiteralData
    {
        private int _value = 0;

        public int Value => (IsVirtual) ? _value : System.Convert.ToInt32(ValueToken?.Input);

        public IntLiteralData(TokenData token) : base(token)
        {
        }

        public IntLiteralData(int value) : base(null)
        {
            _value = value;
            IsVirtual = true;
        }

        private static LiteralData CoreLogic(IntLiteralData left, LiteralData right, OpKind opKind)
        {
            if (right is IntLiteralData)
            {
                var cRight = right as IntLiteralData;

                int value = (opKind == OpKind.Add) ? (int)left.Value + (int)cRight.Value :
                                (opKind == OpKind.Sub) ? (int)left.Value - (int)cRight.Value :
                                (opKind == OpKind.Mul) ? (int)left.Value * (int)cRight.Value :
                                (opKind == OpKind.Div) ? (int)left.Value / (int)cRight.Value :
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

        public override LiteralData Add(LiteralData right) => CoreLogic(this, right, OpKind.Add);
        public override LiteralData Sub(LiteralData right) => CoreLogic(this, right, OpKind.Sub);
        public override LiteralData Mul(LiteralData right) => CoreLogic(this, right, OpKind.Mul);
        public override LiteralData Div(LiteralData right) => CoreLogic(this, right, OpKind.Div);
        public override LiteralData Mod(LiteralData right) => CoreLogic(this, right, OpKind.Mod);
    }

    public class LongLiteralData : DecidedLiteralData
    {
        public LongLiteralData(object value, TokenData valueToken) : base(valueToken)
        {
        }

        public override LiteralData Add(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Div(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Mod(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Mul(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Sub(LiteralData right)
        {
            throw new System.NotImplementedException();
        }
    }

    public class FloatLiteralData : DecidedLiteralData
    {
        public FloatLiteralData(object value, TokenData valueToken) : base(valueToken)
        {
        }

        public override LiteralData Add(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Div(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Mod(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Mul(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Sub(LiteralData right)
        {
            throw new System.NotImplementedException();
        }
    }

    public class DoubleLiteralData : DecidedLiteralData
    {
        public DoubleLiteralData(object value, TokenData valueToken) : base(valueToken)
        {
        }

        public override LiteralData Add(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Div(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Mod(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Mul(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Sub(LiteralData right)
        {
            throw new System.NotImplementedException();
        }
    }

    public class StringLiteralData : DecidedLiteralData
    {
        public StringLiteralData(object value, TokenData valueToken) : base(valueToken)
        {
        }

        public override LiteralData Add(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Div(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Mod(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Mul(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Sub(LiteralData right)
        {
            throw new System.NotImplementedException();
        }
    }

    public class StructLiteralData : DecidedLiteralData
    {
        public StructLiteralData(object value, TokenData valueToken) : base(valueToken)
        {
        }

        public override LiteralData Add(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Div(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Mod(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Mul(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Sub(LiteralData right)
        {
            throw new System.NotImplementedException();
        }
    }

    public class UnknownLiteralData : LiteralData
    {
        public UnknownState State { get; }
        public bool IsOnlyNotInit => (State == UnknownState.NotInitialized) ? true : false;
        public bool IsNotInitAndDynamicAlloc
        {
            get
            {
                if ((State & UnknownState.NotInitialized) != UnknownState.NotInitialized) return false;
                if ((State & UnknownState.DynamicAllocation) != UnknownState.DynamicAllocation) return false;

                return true;
            }
        }

        public UnknownLiteralData(UnknownState state, TokenData valueToken) : base(valueToken)
        {
            State = state;
        }

        public override LiteralData Add(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Sub(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Mul(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Div(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Mod(LiteralData right)
        {
            throw new System.NotImplementedException();
        }
    }
}
