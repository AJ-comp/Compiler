using Parse.FrontEnd.Ast;

namespace ApplicationLayer.Models.GraphModels
{
    public class AstSymbolVertex : PocVertex
    {
        public AstSymbol TreeSymbol { get; private set; }

        public AstSymbolVertex(AstSymbol treeSymbol)
        {
            TreeSymbol = treeSymbol;
        }

        public override string ID => TreeSymbol.ToString();
    }
}
