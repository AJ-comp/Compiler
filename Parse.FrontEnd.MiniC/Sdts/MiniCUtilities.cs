using Parse.FrontEnd.MiniC.Properties;
using Parse.FrontEnd.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types;
using Parse.Types.ConstantTypes;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC.Sdts
{
    public class MiniCUtilities
    {
        public static bool AddErrorUseDefinedIdent(MiniCNode node, TokenData tokenData = null)
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


        public static bool AddErrorDefineCantOwn(MiniCNode node, TokenData tokenData)
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
        public static IEnumerable<MiniCSymbolTable> GetReferableSymbolTablelList(MiniCNode fromNode)
        {
            List<MiniCSymbolTable> result = new List<MiniCSymbolTable>();
            MiniCNode trasverNode = fromNode;

            while (trasverNode != null)
            {
                if (trasverNode.SymbolTable != null)
                    result.Add(trasverNode.SymbolTable);

                trasverNode = trasverNode.Parent as MiniCNode;
            }

            return result;
        }

        public static MiniCSymbolTable GetSymbolTableOfAnyNode(MiniCNode fromNode, Type anyNodeType)
        {
            MiniCSymbolTable result = null;
            MiniCNode trasverNode = fromNode;

            while (trasverNode != null)
            {
                if (trasverNode.SymbolTable != null && trasverNode.GetType() == anyNodeType)
                {
                    result = trasverNode.SymbolTable;
                    break;
                }

                trasverNode = trasverNode.Parent as MiniCNode;
            }

            return result;
        }


        /// <summary>
        /// This function returns VarData that matched with 'varTokenToFind' from SymbolTable list referenceable.
        /// </summary>
        /// <param name="fromNode"></param>
        /// <returns></returns>
        public static MiniCReferenceRecord<VariableMiniC> GetVarRecordFromReferableST(MiniCNode fromNode, TokenData varTokenToFind)
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


        public static IEnumerable<VariableMiniC> GetReferableVarDatas(MiniCNode fromNode)
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

        public static IEnumerable<FuncDefData> GetFuncDataList(MiniCNode fromNode, TokenData funcTokenToFind)
        {
            if (funcTokenToFind == null) return null;

            MiniCNode curNode = fromNode;
            List<FuncDefData> result = new List<FuncDefData>();

            while (true)
            {
                if (curNode == null) break;
                if (!(curNode is IHasFuncInfos)) curNode = curNode.Parent as MiniCNode;

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
            if (irSymbol == IRCompareSymbol.EQ) return MiniCGrammar.Equal.Value;
            if (irSymbol == IRCompareSymbol.GE) return MiniCGrammar.GreaterEqual.Value;
            if (irSymbol == IRCompareSymbol.GT) return MiniCGrammar.GreaterThan.Value;
            if (irSymbol == IRCompareSymbol.LE) return MiniCGrammar.LessEqual.Value;
            if (irSymbol == IRCompareSymbol.LT) return MiniCGrammar.LessThan.Value;

            return MiniCGrammar.NotEqual.Value;
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
