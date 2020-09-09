using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.Datas
{
    public class IntLiteralData : IntConstant
    {
        public string LiteralName => ValueToken.Input;
        public TokenData ValueToken { get; }
        public bool IsVirtual { get; protected set; } = false;

        public IntLiteralData(int value, TokenData valueToken) : base(value)
        {
            ValueToken = valueToken;
        }
    }
}
