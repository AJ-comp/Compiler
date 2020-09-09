using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using Parse.MiddleEnd.IR;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes
{
    public class ProgramNode : MiniCNode
    {
        public IReadOnlyList<VariableDclsNode> VarNodes => _varNodes;
        public IReadOnlyList<FuncDefNode> FuncDefNodes => _funcDefNodes;

        public Func<SdtsNode, IRExpression> ConvertFunc { get; set; }

        public ProgramNode(AstSymbol node) : base(node)
        {
            IsNeedWhileIRGeneration = true;
        }

        // [0:n] : Dcl? (AstNonTerminal)
        // [n+1:y] : FuncDef? (AstNonTerminal)
        public override SdtsNode Build(SdtsParams param)
        {
            SymbolTable = (param as MiniCSdtsParams).SymbolTable;

            foreach (var item in Items)
            {
                var minicNode = item as MiniCNode;

                // Global variable
                if (minicNode is VariableDclsNode)
                {
                    // children node is parsing only variable elements so it doesn't need to clone an param
                    _varNodes.Add(minicNode.Build(param) as VariableDclsNode);
                }
                // Global function
                else if (minicNode is FuncDefNode)
                {
                    param.Offset = 0;

                    var node = minicNode.Build(param) as FuncDefNode;
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
