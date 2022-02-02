using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.Types;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts
{
    public class AJUtilities
    {
        public static IEnumerable<FuncDefNode> GetFuncList(AJNode fromNode, TokenData funcTokenToFind)
        {
            if (funcTokenToFind == null) return null;

            AJNode curNode = fromNode;
            List<FuncDefNode> result = new List<FuncDefNode>();

            while (true)
            {
                if (curNode == null) break;
                if (!(curNode is IHasFuncInfos)) curNode = curNode.Parent as AJNode;

                // This would be class.
                var hasFuncInfo = curNode as IHasFuncInfos;
                foreach (var funcInfos in hasFuncInfo.FuncList)
                {
                    if (funcInfos.Name != funcTokenToFind.Input) continue;

                    result.Add(funcInfos);
                }

                // only class has IHasFuncInfos feature so it doesn't need to see more.
                break;
            }

            return result;
        }


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
