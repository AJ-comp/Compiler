using Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.LiteralDataFormat;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.VarDataFormat
{
    public class RealVarData : VarData
    {
        public DclData DclData { get; }
        public bool This { get; internal set; } = false;
        public bool IsCalculatable => !(Value is UnknownLiteralData);

        public override string VarName => DclData.DclItemData.Name;
        public override int BlockLevel => DclData.BlockLevel;
        public override int Offset => DclData.Offset;
        public override int Dimension => DclData.DclItemData.Dimension;
        public override LiteralData Value
        {
            get => DclData.DclItemData.Value;
            set => DclData.DclItemData.Value = value;
        }

        public RealVarData(DclData dclData)
        {
            DclData = dclData;
        }

        public RealVarData(DclData dclData, LiteralData value) : this(dclData)
        {
            dclData.DclItemData.Value = value;
        }

        public override string ToString() => string.Format("{0}", DclData.ToString());
    }
}
