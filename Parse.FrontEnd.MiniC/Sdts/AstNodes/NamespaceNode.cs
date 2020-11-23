using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes
{
    public class NamespaceNode : MiniCNode
    {
        public MiniCNamespaceData NamespaceData { get; }
        public IEnumerable<FuncDefNode> FuncDefNodes => _funcDefNodes;

        public NamespaceNode(AstSymbol node) : base(node)
        {
        }

        // [0] : Ident (TerminalNode)
        // [1:n] : Dcl? (AstNonTerminal)
        // [n+1:y] : FuncDef? (AstNonTerminal)
        public override SdtsNode Build(SdtsParams param)
        {
            _varNodes.Clear();
            _funcDefNodes.Clear();

            Items[0].Build(param);

            // it needs to clone an param
            var newParam = CreateParamForNewBlock(param);
            SymbolTable = newParam.SymbolTable;

            var items = Items.Skip(1);
            foreach (var item in items)
            {
                var minicNode = item as MiniCNode;

                // Global variable
                if (minicNode is VariableDclsNode)
                {
                    // children node is parsing only variable elements so it doesn't need to clone an param
                    var varDclsNode = minicNode.Build(newParam) as VariableDclsNode;
                }
                // Global function
                else if (minicNode is FuncDefNode)
                {
                    newParam.Offset = 0;

                    var node = minicNode.Build(newParam) as FuncDefNode;
                    SymbolTable.FuncTable.CreateNewBlock(node.FuncData, this);
                    _funcDefNodes.Add(node);
                }
            }

            return this;
        }


        private List<VariableDclsNode> _varNodes = new List<VariableDclsNode>();
        private List<FuncDefNode> _funcDefNodes = new List<FuncDefNode>();
    }
}
