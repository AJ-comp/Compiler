using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes
{
    public class ProgramNode : MiniCNode
    {
        public ProgramNode(AstSymbol node) : base(node)
        {
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
                    minicNode.Build(param);
                }
                // Global function
                else if (minicNode is FuncDefNode)
                {
                    param.Offset = 0;

                    var node = minicNode.Build(param) as FuncDefNode;
                    SymbolTable.FuncDataList.Add(node.FuncData);
                }
            }

            return this;
        }
    }
}
