using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.Ast;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class ProgramNode : AJNode
    {
        public List<UsingStNode> UsingNamespaces { get; } = new List<UsingStNode>();
        public List<NamespaceNode> Namespaces { get; } = new List<NamespaceNode>();


        public HashSet<AJTypeInfo> NoLinkedTypeSet { get; } = new HashSet<AJTypeInfo>();
        public HashSet<IHasVarList> ShortCutDeclareVarSet { get; } = new HashSet<IHasVarList>();
        public HashSet<TypeDefNode> ShortCutTypeDefSet { get; } = new HashSet<TypeDefNode>();
        public HashSet<AJNode> UnLinkedSymbol { get; } = new HashSet<AJNode>();


        public ProgramNode(AstSymbol node) : base(node)
        {
            IsNeedWhileIRGeneration = true;
        }

        // [0:n] : Using? (AstNonTerminal)
        // [n+1:y] : Namespace? (AstNonTerminal)
        public override SdtsNode Compile(CompileParameter param)
        {
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
            foreach (var unlinkNode in UnLinkedSymbol) unlinkNode.Compile(null);


            return this;
        }
    }
}
