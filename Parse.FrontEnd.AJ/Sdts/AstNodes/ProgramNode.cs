using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.Ast;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class ProgramNode : AJNode
    {
        public List<UsingStNode> UsingNamespaces { get; } = new List<UsingStNode>();
        public NamespaceNode Namespace { get; private set; }

        public List<TypeDefNode> DefTypes { get; } = new List<TypeDefNode>();
        private HashSet<StructDefNode> _structDefNodes = new HashSet<StructDefNode>();


        public HashSet<IDeclareable> ShortCutDeclareVarSet { get; } = new HashSet<IDeclareable>();
        public HashSet<AJNode> UnLinkedSymbols { get; } = new HashSet<AJNode>();
        public HashSet<AJNode> LinkedSymbols { get; } = new HashSet<AJNode>();
        public HashSet<AJNode> AmbiguityLinkedSymbols { get; } = new HashSet<AJNode>();
        public HashSet<AJNode> CompletedSymbols { get; } = new HashSet<AJNode>();


        public bool IsBuild { get; private set; }
        public string FullPath { get; set; }


        public IEnumerable<ProgramNode> AccessablePrograms
        {
            get
            {
                HashSet<ProgramNode> result = new HashSet<ProgramNode>();

                foreach (var usingNamespace in UsingNamespaces)
                {
                    if (NamespaceDictionary.Instance.ContainsKey(usingNamespace.FullName))
                        result.UnionWith(NamespaceDictionary.Instance[usingNamespace.FullName]);
                }

                result.UnionWith(NamespaceDictionary.Instance[Namespace.FullName]);
                
                return result;
            }
        }


        public ProgramNode(AstSymbol node) : base(node)
        {
            IsNeedWhileIRGeneration = true;
        }

        // [0:n] : Using? (AstNonTerminal)
        // [n+1:1] : Namespace (AstNonTerminal)
        // [n+2:y] : (ClassDefNode | StructDefNode | EnumNode)*  (NonTerminal)   <br/>
        public override SdtsNode Compile(CompileParameter param)
        {
            param.RootNode = this;

            base.Compile(param);
            IsBuild = param.Build;
            FullPath = param.FileFullPath;

            param.RootNode = this;

            int index = 0;
            foreach (var item in Items)
            {
                index++;
                var ajNode = item as AJNode;

                // using statement list
                if (ajNode is UsingStNode)
                {
                    UsingNamespaces.Add(ajNode.Compile(param) as UsingStNode);
                }
                // namespace statement list
                else if (ajNode is NamespaceNode)
                {
                    Namespace = ajNode.Compile(param) as NamespaceNode;
                    break;
                }
            }

            NamespaceDictionary.Instance.Add(this);

            int newBlockOffset = 0;
            foreach (var item in Items.Skip(index))
            {
                if (item is ClassDefNode)
                {
                    var node = item.Compile(param.CloneForNewBlock(newBlockOffset++)) as ClassDefNode;
                    if (node.Alarms.Count == 0) DefTypes.Add(node);
                }
                else if (item is StructDefNode)
                {
                    var node = item.Compile(param.CloneForNewBlock(newBlockOffset++)) as StructDefNode;
                    _structDefNodes.Add(node);
                }
            }

            // retry compile for undefined symbol
            var copiedUnLinkedSymbols = new HashSet<SdtsNode>(AllAlarmNodes);
            foreach (var unlinkNode in copiedUnLinkedSymbols)
            {
                if (unlinkNode is ExprNode)
                {
                    var exprNode = unlinkNode as ExprNode;
                    if (!exprNode.IsRoot) continue;

                    exprNode.Compile(null);
                }
                else if(unlinkNode is DeclareVarNode)
                {
                    unlinkNode.Compile(null);
                }
            }

            return this;
        }
    }
}
