using Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.LiteralDataFormat;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.VarDataFormat
{
    public abstract class VarData
    {
        public abstract string VarName { get; }
        public abstract int BlockLevel { get; }
        public abstract int Offset { get; }
        public abstract int Dimension { get; }
        public abstract LiteralData Value { get; set; }

        public bool IsMatchWithVarName(string name) => (VarName == name) ? true : false;
    }
}
