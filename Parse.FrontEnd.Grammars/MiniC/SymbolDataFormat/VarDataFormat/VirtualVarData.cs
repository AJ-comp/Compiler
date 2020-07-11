using Parse.MiddleEnd.IR.Datas.Types;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.VarDataFormat
{
    public class VirtualVarData : VarData
    {
        private int _blockLevel;
        private int _offset;
        private int _dimension;
        private ValueData _valueData;

        public TokenData VarToken { get; }

        public override string Name => VarToken.Input;
        public override int Block => _blockLevel;
        public override int Offset => _offset;
        public override int Length => _dimension;
        public override ValueData Value
        {
            get => _valueData;
            set => _valueData = value;
        }

        public override DType TypeName => DType.Unknown;

        public VirtualVarData(TokenData varToken, int blockLevel, int offset, int dimension)
        {
            VarToken = varToken;
            _blockLevel = blockLevel;
            _offset = offset;
            _dimension = dimension;
        }
    }
}
