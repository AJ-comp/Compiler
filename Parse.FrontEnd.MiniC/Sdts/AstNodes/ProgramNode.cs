using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.MiddleEnd.IR;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes
{
    public class ProgramNode : MiniCNode
    {
        public IEnumerable<NamespaceNode> NamespaceNodes => _namespaceNodes;

        public Func<SdtsNode, IRExpression> ConvertFunc { get; set; }

        public ProgramNode(AstSymbol node) : base(node)
        {
            IsNeedWhileIRGeneration = true;
        }

        // [0:n] : Using? (AstNonTerminal)
        // [n+1:y] : Namespace? (AstNonTerminal)
        public override SdtsNode Build(SdtsParams param)
        {
            SymbolTable = (param as MiniCSdtsParams).SymbolTable;

            foreach (var item in Items)
            {
                var minicNode = item as MiniCNode;

                // Global variable
                if (minicNode is UsingStNode)
                {
//                    _varNodes.Add(minicNode.Build(param) as UsingStNode);
                }
                // Global function
                else if (minicNode is NamespaceNode)
                {
                    var node = minicNode.Build(param) as NamespaceNode;
                    SymbolTable.NamespaceTable.CreateNewBlock(node.NamespaceData, this);
                    _namespaceNodes.Add(node);
                }
            }

            return this;
        }

        private List<NamespaceNode> _namespaceNodes = new List<NamespaceNode>();
    }
}
