using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public class MiniCUtilities
    {
        /// <summary>
        /// This function returns all SymbolTable list of from fromNode to root node.
        /// first = leaf, last = root
        /// </summary>
        /// <returns></returns>
        public static IReadOnlyList<MiniCSymbolTable> GetReferableSymbolTablelList(MiniCNode fromNode)
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
        public static VariableMiniC GetVarDataFromReferableST(MiniCNode fromNode, TokenData varTokenToFind)
        {
            if (varTokenToFind == null) return null;

            VariableMiniC result = null;
            var tableList = GetReferableSymbolTablelList(fromNode);

            foreach (var table in tableList)
            {
                var varData = table.GetVarByName(varTokenToFind.Input);
                if (varData != null)
                {
                    result = varData;
                    break;
                }
            }

            return result;
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
