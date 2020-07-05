using Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.LiteralDataFormat;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.VarDataFormat
{
    public class VirtualVarData : VarData
    {
        private int _blockLevel;
        private int _offset;
        private int _dimension;
        private LiteralData _literalData = new UnknownLiteralData(UnknownState.NotInitialized, null);

        public TokenData VarToken { get; }

        public override string Name => VarToken.Input;
        public override int Block => _blockLevel;
        public override int Offset => _offset;
        public override int Length => _dimension;
        public override LiteralData Value
        {
            get => _literalData;
            set => _literalData = value;
        }

        public VirtualVarData(TokenData varToken, int blockLevel, int offset, int dimension)
        {
            VarToken = varToken;
            _blockLevel = blockLevel;
            _offset = offset;
            _dimension = dimension;
        }
    }
}
