using Parse.FrontEnd.AJ.Sdts.AstNodes;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Parse.FrontEnd.AJ.Sdts.Datas
{
    public class NamespaceDictionary : Dictionary<string, List<ProgramNode>>
    {
        static NamespaceDictionary _instance;

        static NamespaceDictionary() { }

        public static NamespaceDictionary Instance
        {
            get
            {
                if (_instance == null) _instance = new NamespaceDictionary();

                return _instance;
            }
        }

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


        public bool Contains(NamespaceNode node)
        {
            bool result = false;
            if (!ContainsKey(node.FullName)) return result;

            var namespaceInfos = this[node.FullName];
            foreach (var namespaceNode in namespaceInfos)
            {
                if (namespaceNode != node) continue;

                result = true;
                break;
            }

            return result;
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Add(ProgramNode node)
        {
            if (Contains(node.Namespace)) return;

            if (!ContainsKey(node.Namespace.FullName))
                Add(node.Namespace.FullName, new List<ProgramNode>());

            this[node.Namespace.FullName].Add(node);
        }
    }
}
