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
        public IEnumerable<NamespaceData> NamespaceDatas => _namespaceDatas;

        public Func<SdtsNode, IRExpression> ConvertFunc { get; set; }

        public ProgramNode(AstSymbol node) : base(node)
        {
            IsNeedWhileIRGeneration = true;
        }

        // [0:n] : Using? (AstNonTerminal)
        // [n+1:y] : Namespace? (AstNonTerminal)
        public override SdtsNode Build(SdtsParams param)
        {
            _namespaceDatas.Clear();

            var rootParam = param as MiniCSdtsParams;
            rootParam.RootData.ProgramNodes.Remove(this);
            rootParam.RootData.ProgramNodes.Add(this);

            foreach (var item in Items)
            {
                var minicNode = item as MiniCNode;

                // using statement list
                if (minicNode is UsingStNode)
                {
//                    _varNodes.Add(minicNode.Build(param) as UsingStNode);
                }
                // namespace statement list
                else if (minicNode is NamespaceNode)
                {
                    var node = minicNode.Build(param) as NamespaceNode;
                    _namespaceDatas.Add(node.NamespaceData);
                }
            }

            return this;
        }

        private List<NamespaceData> _namespaceDatas = new List<NamespaceData>();
    }
}
