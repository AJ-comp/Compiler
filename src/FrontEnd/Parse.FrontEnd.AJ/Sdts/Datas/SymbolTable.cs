using Parse.FrontEnd.AJ.Sdts.AstNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Parse.FrontEnd.AJ.Sdts.Datas
{
    public class SymbolTable : Dictionary<string, HashSet<ProgramNode>>
    {
        static readonly SymbolTable _instance = new SymbolTable();

        static SymbolTable() { }

        public static SymbolTable Instance => _instance;

        public void ChangeFileFullPath(string originalFileFulPath, string newFileFullPath)
        {
            foreach (var item in this)
            {
                foreach (var namespaceNode in item.Value)
                {
                    if (namespaceNode.FileFullPath != originalFileFulPath) continue;

                    (namespaceNode.RootNode as ProgramNode).FullPath = newFileFullPath;

                }
            }
        }


        /// <summary>
        /// Get all the TypeDefNode that has a full name that same with full name of typeNode.
        /// </summary>
        /// <param name="typeNode"></param>
        /// <returns></returns>
        public IEnumerable<TypeDefNode> GetAllSameTypeDefNode(TypeDefNode typeNode, bool bRemoveOwn = false)
        {
            List<TypeDefNode> result = new List<TypeDefNode>();

            foreach (var file in this[typeNode.Namespace])
            {
                foreach (var type in file.DefTypes)
                {
                    if (type.FullName == typeNode.FullName) result.Add(type);
                }
            }

            if (bRemoveOwn) result.Remove(typeNode);

            return result;
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Add(ProgramNode node)
        {
            if (!ContainsKey(node.Namespace.FullName))
                Add(node.Namespace.FullName, new HashSet<ProgramNode>());

            this[node.Namespace.FullName].Add(node);
        }
    }
}
