using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes
{
    public class NamespaceNode : MiniCNode
    {
        public IEnumerable<ClassData> ClassDatas => NamespaceData?.ClassDatas;
        public NamespaceData NamespaceData { get; private set; }

        public NamespaceNode(AstSymbol node) : base(node)
        {
        }

        // [0] : Ident (TerminalNode)
        // [1:n] : (ClassDefNode | StructDefNode | EnumNode)*  (NonTerminal)
        public override SdtsNode Build(SdtsParams param)
        {
            HashSet<ClassData> _classDatas = new HashSet<ClassData>();

            var nameNode = Items[0].Build(param) as TerminalNode;

            // it needs to clone an param
            var newParam = CreateParamForNewBlock(param);

            var items = Items.Skip(1);
            foreach (var item in items)
            {
                var minicNode = item as MiniCNode;

                if (minicNode is ClassDefNode)
                {
                    newParam.Offset = 0;

                    var node = minicNode.Build(newParam) as ClassDefNode;
                    _classDatas.Add(node.ClassData);
                }
                else if(minicNode is StructDefNode)
                {
                    newParam.Offset = 0;

                    var node = minicNode.Build(newParam) as StructDefNode;
                    _structDefNodes.Add(node);
                }
            }

            List<TokenData> identTokens = new List<TokenData>();
            identTokens.Add(nameNode.Token);
            NamespaceData = new NamespaceData(identTokens, _classDatas);

            return this;
        }

        private HashSet<StructDefNode> _structDefNodes = new HashSet<StructDefNode>();
    }
}
