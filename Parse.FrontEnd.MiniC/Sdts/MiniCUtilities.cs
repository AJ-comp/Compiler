using Parse.FrontEnd.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;
using Parse.FrontEnd.MiniC.Properties;
using System.Collections.Generic;
using System;

namespace Parse.FrontEnd.MiniC.Sdts
{
    public class MiniCUtilities
    {

        /// <summary>
        /// This function adds duplicated error to the node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="varTokenToCheck"></param>
        /// <returns></returns>
        public static bool AddDuplicatedError(MiniCNode node, TokenData varTokenToCheck = null)
        {
            if (varTokenToCheck == null)
            {
                node.ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(node.AllTokens,
                                                    nameof(AlarmCodes.MCL0009),
                                                    string.Format(AlarmCodes.MCL0009, node.AllTokens[0].Input))
                );
            }
            else
            {
                node.ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(varTokenToCheck,
                                                    nameof(AlarmCodes.MCL0009),
                                                    string.Format(AlarmCodes.MCL0009, varTokenToCheck.Input))
                );
            }

            return true;
        }

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

        public static IEnumerable<IHasVarInfos> GetReferableHasVarNodes(MiniCNode fromNode)
        {
            var transNode = fromNode;
            List<IHasVarInfos> result = new List<IHasVarInfos>();

            while (transNode != null)
            {
                if (transNode is IHasVarInfos)
                    result.Add(transNode as IHasVarInfos);

                transNode = transNode.Parent as MiniCNode;
            }

            return result;
        }


        public static IEnumerable<VariableMiniC> GetReferableVarDatas(MiniCNode fromNode)
        {
            List<VariableMiniC> result = new List<VariableMiniC>();
            var hasVarList = GetReferableHasVarNodes(fromNode);

            foreach(var hasVar in hasVarList)
            {
                if (hasVar.VarList == null) continue;

                result.AddRange(hasVar.VarList);
            }

            return result;
        }


        public static VariableMiniC GetVarDCLSymbolData(MiniCNode fromNode, TokenData varTokenToFind)
        {
            if (varTokenToFind == null) return null;

            VariableMiniC result = null;
            MiniCNode curNode = fromNode;

            while (true)
            {
                if (curNode == null) break;
                if (!(curNode is IHasVarInfos)) curNode = curNode.Parent as MiniCNode;

                var hasVarInfo = curNode as IHasVarInfos;
                foreach (var varInfo in hasVarInfo.VarList)
                {
                    if (varInfo.NameToken.Input != varTokenToFind.Input) continue;

                    result = varInfo;
                    break;
                }

                if (result != null) break;
            }

            return result;
        }

        public static IEnumerable<FuncData> GetFuncDataList(MiniCNode fromNode, TokenData funcTokenToFind)
        {
            if (funcTokenToFind == null) return null;

            MiniCNode curNode = fromNode;
            List<FuncData> result = new List<FuncData>();

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
    }
}
