using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.Ast;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class ProgramNode : AJNode
    {
        public List<UsingStNode> UsingNamespaces { get; } = new List<UsingStNode>();
        public List<NamespaceNode> Namespaces { get; } = new List<NamespaceNode>();


        public HashSet<IHasVarList> ShortCutDeclareVarSet { get; } = new HashSet<IHasVarList>();
        public HashSet<TypeDefNode> ShortCutTypeDefSet { get; } = new HashSet<TypeDefNode>();
        public HashSet<AJNode> UnLinkedSymbols { get; } = new HashSet<AJNode>();
        public HashSet<AJNode> LinkedSymbols { get; } = new HashSet<AJNode>();
        public HashSet<AJNode> AmbiguityLinkedSymbols { get; } = new HashSet<AJNode>();
        public HashSet<AJNode> CompletedSymbols { get; } = new HashSet<AJNode>();


        public bool IsBuild { get; private set; }
        public string FullPath { get; set; }


        public IEnumerable<NamespaceNode> AccessableNamespaces
        {
            get
            {
                HashSet<NamespaceNode> result = new HashSet<NamespaceNode>();

                foreach (var usingNamespace in UsingNamespaces)
                {
                    if (NamespaceDictionary.Instance.ContainsKey(usingNamespace.FullName))
                        result.UnionWith(NamespaceDictionary.Instance[usingNamespace.FullName]);
                }

                foreach (var ns in Namespaces) result.UnionWith(NamespaceDictionary.Instance[ns.FullName]);

                return result;
            }
        }


        public ProgramNode(AstSymbol node) : base(node)
        {
            IsNeedWhileIRGeneration = true;
        }

        // [0:n] : Using? (AstNonTerminal)
        // [n+1:y] : Namespace? (AstNonTerminal)
        public override SdtsNode Compile(CompileParameter param)
        {
            param.RootNode = this;

            base.Compile(param);
            IsBuild = param.Build;
            FullPath = param.FileFullPath;

            Namespaces.Clear();
            param.RootNode = this;

            foreach (var item in Items)
            {
                var ajNode = item as AJNode;

                // using statement list
                if (ajNode is UsingStNode)
                {
                    UsingNamespaces.Add(ajNode.Compile(param) as UsingStNode);
                }
                // namespace statement list
                else if (ajNode is NamespaceNode)
                {
                    Namespaces.Add(ajNode.Compile(param) as NamespaceNode);
                }
            }

            // retry compile for undefined symbol
            var copiedUnLinkedSymbols = new HashSet<SdtsNode>(AllAlarmNodes);
            foreach (var unlinkNode in copiedUnLinkedSymbols)
            {
                if (!(unlinkNode is ExprNode)) continue;

                var exprNode = unlinkNode as ExprNode;
                if (!exprNode.IsRoot) continue;

                exprNode.Compile(null);
            }

            return this;
        }
    }
}
