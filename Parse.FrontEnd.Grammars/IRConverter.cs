using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using IRData = Parse.FrontEnd.InterLanguages;
using IR = Parse.FrontEnd.InterLanguages.Datas;
using Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.VarDataFormat;
using System.Collections.Generic;
using Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.LiteralDataFormat;

namespace Parse.FrontEnd.Grammars
{
    public class IRConverter
    {
        public static IRData.DataType ToIRDataType(DataType dataType)
        {
            IRData.DataType result = IRData.DataType.i32;

            if (dataType == DataType.Int) result = IRData.DataType.i32;

            return result;
        }

        public static IRData.ReturnType ToIRReturnType(DataType returnType)
        {
            IRData.ReturnType result = IRData.ReturnType.Void;

            if (returnType == DataType.Void) result = IRData.ReturnType.Void;
            else if (returnType == DataType.Int) result = IRData.ReturnType.i32;

            return result;
        }

        public static IR.VarData ToIRData(RealVarData varData) => ToIRData(varData.DclData);

        public static IR.VarData ToIRData(DclData dclData)
        {
            return new IR.VarData(IRConverter.ToIRDataType(dclData.DclSpecData.DataType),
                                                dclData.DclItemData.Name,
                                                dclData.BlockLevel,
                                                dclData.Offset,
                                                dclData.DclItemData.Dimension
                                                );
        }

        public static IR.LiteralData ToIRData(LiteralData literalData)
        {
            if (literalData is CharLiteralData)
                return new IR.LiteralData(IRData.DataType.i8, (literalData as CharLiteralData).Value);
            else if (literalData is ShortLiteralData)
                return new IR.LiteralData(IRData.DataType.i16, (literalData as ShortLiteralData).Value);
            else if (literalData is IntLiteralData)
                return new IR.LiteralData(IRData.DataType.i16, (literalData as IntLiteralData).Value);
            else if (literalData is LongLiteralData)
                return new IR.LiteralData(IRData.DataType.i16, (literalData as LongLiteralData).Value);
            else if (literalData is DoubleLiteralData)
                return new IR.LiteralData(IRData.DataType.i16, (literalData as DoubleLiteralData).Value);

            return null;
        }

        public static IR.FuncData ToIRData(FuncData funcData)
        {
            List<IR.VarData> paramVars = new List<IR.VarData>();

            foreach (var param in funcData.ParamVars)
                paramVars.Add(IRConverter.ToIRData(param));

            return new IR.FuncData(paramVars,
                                                funcData.DclSpecData.Const,
                                                IRConverter.ToIRReturnType(funcData.DclSpecData.DataType),
                                                funcData.Name
                                                );
        }
    }
}
