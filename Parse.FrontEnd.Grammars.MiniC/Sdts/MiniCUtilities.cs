using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables;
using Parse.FrontEnd.Grammars.Properties;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
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
            if(varTokenToCheck == null)
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
                if(trasverNode.SymbolTable != null)
                    result.Add(trasverNode.SymbolTable);

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

        public static MiniCFuncData GetFuncDataFromReferableST(MiniCNode fromNode, TokenData varTokenToFind)
        {
            if (varTokenToFind == null) return null;

            MiniCFuncData result = null;
            var tableList = GetReferableSymbolTablelList(fromNode);

            foreach (var table in tableList)
            {
                var funcData = table.GetFuncByName(varTokenToFind.Input);
                if (funcData != null)
                {
                    result = funcData;
                    break;
                }
            }

            return result;
        }
    }
}
