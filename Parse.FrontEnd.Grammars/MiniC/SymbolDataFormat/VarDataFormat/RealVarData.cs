using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.MiddleEnd.IR.Datas.Types;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.VarDataFormat
{
    public class RealVarData : VarData
    {
        public DclData DclData { get; }
        public bool This { get; internal set; } = false;

        public override DType TypeName => TypeConverter.ToIRDataType(DclData);
        public override string Name => DclData.DclItemData.Name;
        public override int Block => DclData.BlockLevel;
        public override int Offset => DclData.Offset;
        public override int Length => DclData.DclItemData.Dimension;
        public override ValueData Value
        {
            get => DclData.Value;
            set => DclData.Value = value;
        }

        public RealVarData(DclData dclData)
        {
            DclData = dclData;
        }

        public RealVarData(DclData dclData, LiteralData value) : this(dclData)
        {
            dclData.Value = value;
        }

        public override string ToString() => string.Format("{0}", DclData.ToString());
    }
}
