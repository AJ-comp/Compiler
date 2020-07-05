using Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.LiteralDataFormat;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Datas.Types;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.VarDataFormat
{
    public abstract class VarData : IRVar
    {
        public abstract DType TypeName { get; }
        public abstract string Name { get; }
        public abstract int Block { get; }
        public abstract int Offset { get; }
        public abstract int Length { get; }
        public abstract LiteralData Value { get; set; }

        public bool IsMatchWithVarName(string name) => (Name == name) ? true : false;
    }
}
