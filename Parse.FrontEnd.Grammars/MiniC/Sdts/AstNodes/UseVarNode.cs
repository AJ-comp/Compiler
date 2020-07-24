using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes
{
    public class UseVarNode : MiniCNode
    {
        public TokenData VarToken { get; private set; }

        public UseVarNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            var node = Items[0].Build(param) as TerminalNode;
            VarToken = node.Token;

            // If varToken is not declared, add as virtual token to the SymbolTable.
            if (!MiniCChecker.IsNotDeclared(this, VarToken))
            {
                // create MiniCVarData with virtual property.
                var varData = new MiniCVarData(VarToken, param.BlockLevel, param.Offset);

                SymbolTable.VarList.Add(varData);
            }

            return this;
        }
    }
}
