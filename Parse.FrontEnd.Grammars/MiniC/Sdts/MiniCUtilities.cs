using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
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
        public static MiniCVarData GetVarDataFromReferableST(MiniCNode fromNode, TokenData varTokenToFind)
        {
            if (varTokenToFind == null) return null;
            if (varTokenToFind.IsVirtual) return null;

            MiniCVarData result = null;
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
    }
}
