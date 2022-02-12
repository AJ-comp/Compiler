using Parse.FrontEnd.AJ.Sdts.AstNodes;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Parse.FrontEnd.AJ.Sdts.Datas
{
    public class NamespaceDictionary : Dictionary<string, IEnumerable<NamespaceNode>>
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
        public void Add(NamespaceNode node)
        {
            if (Contains(node)) return;

            if (!ContainsKey(node.FullName))
                Add(node.FullName, new List<NamespaceNode>());

            this[node.FullName].ToList().Add(node);
        }
    }
}
