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

        public static IR.IRData ToIRData(RealVarData varData) => ToIRData(varData.DclData);

        public static IR.IRData ToIRData(DclData dclData)
        {
            return new IR.IRVarData(IRConverter.ToIRDataType(dclData.DclSpecData.DataType),
                                                dclData.DclItemData.Name,
                                                dclData.BlockLevel,
                                                dclData.Offset,
                                                dclData.DclItemData.Dimension
                                                );
        }

        public static IR.IRData ToIRData(LiteralData literalData)
        {
            if (literalData is CharLiteralData)
                return new IR.IRIntegerLiteralData(IRData.DataType.i8, (literalData as CharLiteralData).Value);
            else if (literalData is ShortLiteralData)
                return new IR.IRIntegerLiteralData(IRData.DataType.i16, (literalData as ShortLiteralData).Value);
            else if (literalData is IntLiteralData)
                return new IR.IRIntegerLiteralData(IRData.DataType.i16, (literalData as IntLiteralData).Value);
            else if (literalData is LongLiteralData)
                return new IR.IRIntegerLiteralData(IRData.DataType.i16, (int)(literalData as LongLiteralData).Value);
            else if (literalData is DoubleLiteralData)
                return new IR.IRDoubleLiteralData((literalData as DoubleLiteralData).Value);

            return null;
        }

        public static IR.IRFuncData ToIRData(FuncData funcData)
        {
            List<IR.IRVarData> paramVars = new List<IR.IRVarData>();

            foreach (var param in funcData.ParamVars)
            {
                var irData = IRConverter.ToIRData(param) as IR.IRVarData;
                
                paramVars.Add(irData);
            }

            return new IR.IRFuncData(paramVars,
                                                funcData.DclSpecData.Const,
                                                IRConverter.ToIRReturnType(funcData.DclSpecData.DataType),
                                                funcData.Name
                                                );
        }
    }
}
