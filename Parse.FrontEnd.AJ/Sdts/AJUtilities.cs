using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.AstNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.AJ.Sdts.Datas.Variables;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types;
using Parse.Types.ConstantTypes;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts
{
    public class AJUtilities
    {
        public static bool AddErrorUseDefinedIdent(AJNode node, TokenData tokenData = null)
        {
            if (tokenData == null)
            {
                node.ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(node.AllTokens, nameof(AlarmCodes.MCL0017), AlarmCodes.MCL0017)
                );
            }
            else
            {
                node.ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(tokenData, nameof(AlarmCodes.MCL0017), AlarmCodes.MCL0017)
                );
            }

            return true;
        }


        public static bool AddErrorDefineCantOwn(AJNode node, TokenData tokenData)
        {
            node.ConnectedErrInfoList.Add
            (
                new MeaningErrInfo(tokenData, nameof(AlarmCodes.MCL0018), AlarmCodes.MCL0018)
            );

            return true;
        }


        /// <summary>
        /// This function returns all SymbolTable list of from fromNode to root node.
        /// first = leaf, last = root
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<AJSymbolTable> GetReferableSymbolTablelList(AJNode fromNode)
        {
            List<AJSymbolTable> result = new List<AJSymbolTable>();
            AJNode trasverNode = fromNode;

            while (trasverNode != null)
            {
                if (trasverNode.SymbolTable != null)
                    result.Add(trasverNode.SymbolTable);

                trasverNode = trasverNode.Parent as AJNode;
            }

            return result;
        }

        public static AJSymbolTable GetSymbolTableOfAnyNode(AJNode fromNode, Type anyNodeType)
        {
            AJSymbolTable result = null;
            AJNode trasverNode = fromNode;

            while (trasverNode != null)
            {
                if (trasverNode.SymbolTable != null && trasverNode.GetType() == anyNodeType)
                {
                    result = trasverNode.SymbolTable;
                    break;
                }

                trasverNode = trasverNode.Parent as AJNode;
            }

            return result;
        }


        /// <summary>
        /// This function returns VarData that matched with 'varTokenToFind' from SymbolTable list referenceable.
        /// </summary>
        /// <param name="fromNode"></param>
        /// <returns></returns>
        public static AJReferenceRecord<VariableMiniC> GetVarRecordFromReferableST(AJNode fromNode, TokenData varTokenToFind)
        {
            if (varTokenToFind == null) return null;

            var tableList = GetReferableSymbolTablelList(fromNode);

            foreach (var table in tableList)
            {
                foreach (var var in table.VarTable)
                {
                    if (var.DefineField.Name != varTokenToFind.Input) continue;

                    return var;
                }
            }

            return null;
        }


        public static IEnumerable<VariableMiniC> GetReferableVarDatas(AJNode fromNode)
        {
            List<VariableMiniC> result = new List<VariableMiniC>();
            var symbolDatas = fromNode.GetReferableSymbols();

            foreach (var symbolData in symbolDatas)
            {
                if (!(symbolData is VariableMiniC)) continue;

                result.Add(symbolData as VariableMiniC);
            }

            return result;
        }

        public static IEnumerable<FuncDefData> GetFuncDataList(AJNode fromNode, TokenData funcTokenToFind)
        {
            if (funcTokenToFind == null) return null;

            AJNode curNode = fromNode;
            List<FuncDefData> result = new List<FuncDefData>();

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

        public static bool IsContainUnknownValue(IConstant left, IConstant right)
            => (left.ValueState != State.Fixed || right.ValueState != State.Fixed);




        public static string ToSymbolString(IRCompareSymbol irSymbol)
        {
            if (irSymbol == IRCompareSymbol.EQ) return AJGrammar.Equal.Value;
            if (irSymbol == IRCompareSymbol.GE) return AJGrammar.GreaterEqual.Value;
            if (irSymbol == IRCompareSymbol.GT) return AJGrammar.GreaterThan.Value;
            if (irSymbol == IRCompareSymbol.LE) return AJGrammar.LessEqual.Value;
            if (irSymbol == IRCompareSymbol.LT) return AJGrammar.LessThan.Value;

            return AJGrammar.NotEqual.Value;
        }

        public static bool IsArithmeticType(StdType type)
        {
            if (type == StdType.Byte) return true;
            if (type == StdType.Short) return true;
            if (type == StdType.Int) return true;
            if (type == StdType.Double) return true;

            return false;
        }
    }
}
