using Parse.FrontEnd.ParseTree;
using System.Diagnostics;

namespace ApplicationLayer.Models.GraphModels
{
    [DebuggerDisplay("{ID}-{IsAst}")]
    public class ParseTreeSymbolVertex : PocVertex
    {
        public ParseTreeSymbol TreeSymbol { get; private set; }
        public bool IsAst
        {
            get
            {
                bool result = false;

                if (TreeSymbol is ParseTreeTerminal)
                    result = (TreeSymbol as ParseTreeTerminal).Token.Kind.Meaning;
                else if(TreeSymbol is ParseTreeNonTerminal)
                {
                    var treeNonTerminal = (TreeSymbol as ParseTreeNonTerminal);
                    result = (treeNonTerminal.SignPost.MeaningUnit == null) ? false : true;
                }

                return result;
            }
        }
        public bool IsVirtual => TreeSymbol.IsVirtual;
        public bool HasVirtualChild => TreeSymbol.HasVirtualChild;

        public override string ID => TreeSymbol.ToString();

        public ParseTreeSymbolVertex(ParseTreeSymbol treeSymbol)
        {
            TreeSymbol = treeSymbol;
        }

        public override string ToString() => string.Format("{0}-{1}", ID, IsAst);
    }
}
