using Parse.FrontEnd;
using Parse.FrontEnd.Ast;

namespace ApplicationLayer.Models.GraphModels
{
    public class AstSymbolVertex : PocVertex
    {
        public SdtsNode TreeSymbol { get; private set; }

        public AstSymbolVertex(SdtsNode treeSymbol)
        {
            TreeSymbol = treeSymbol;
        }

        public override string ID => TreeSymbol.GetType().Name;
    }
}
