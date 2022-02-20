using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.RegularGrammar;
using Parse.Types;
using System;

namespace Parse.FrontEnd.AJ.Sdts
{
    public static class AJUtilities
    {
        public static bool IsEqual(this TokenData obj, Terminal target) => obj.Input == target.Caption;

        public static string ToSymbolString(IRCompareOperation irSymbol)
        {
            if (irSymbol == IRCompareOperation.EQ) return AJGrammar.Equal.Value;
            if (irSymbol == IRCompareOperation.GE) return AJGrammar.GreaterEqual.Value;
            if (irSymbol == IRCompareOperation.GT) return AJGrammar.GreaterThan.Value;
            if (irSymbol == IRCompareOperation.LE) return AJGrammar.LessEqual.Value;
            if (irSymbol == IRCompareOperation.LT) return AJGrammar.LessThan.Value;

            return AJGrammar.NotEqual.Value;
        }

        public static bool IsArithmeticType(StdType type)
        {
            if (type == StdType.Char) return true;
            if (type == StdType.Short) return true;
            if (type == StdType.Int) return true;
            if (type == StdType.Double) return true;

            return false;
        }

        public static uint SizeOf(DataTypeNode type)
        {
            if (type.Type == AJDataType.Bool) return 1;
            if (type.Type == AJDataType.Byte) return 1;
            if (type.Type == AJDataType.Short) return 2;
            if (type.Type == AJDataType.Int) return 4;
            if (type.Type == AJDataType.Double) return 8;
            if (type.Type == AJDataType.Enum) return 4;

            if (type is ClassDefNode)
            {

            }

            throw new Exception();
        }
    }
}
