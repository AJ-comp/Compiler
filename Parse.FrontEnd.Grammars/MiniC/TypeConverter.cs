using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.MiddleEnd.IR.Datas.Types;
using Local = Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;

namespace Parse.FrontEnd.Grammars.MiniC
{
    internal class TypeConverter
    {
        public static DType ToIRDataType(DclData dclData)
        {
            Local.DataType dataType = dclData.DclSpecData.DataType;

            DType result = DType.Unknown;

            if (dataType == Local.DataType.Int)
                result = DType.Int;

            return result;
        }
    }
}
