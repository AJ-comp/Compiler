using Parse.FrontEnd.Ast;
using System.Diagnostics;

namespace ApplicationLayer.Models.GraphModels
{
    [DebuggerDisplay("{ID}-{IsAst}")]
    public class TreeSymbolVertex : PocVertex
    {
        public override string ID => TreeSymbol.ToString();
        public bool IsAst
        {
            get
            {
                bool result = false;

                if (TreeSymbol is TreeTerminal)
                    result = (TreeSymbol as TreeTerminal).Token.Kind.Meaning;
                else if(TreeSymbol is TreeNonTerminal)
                {
                    var treeNonTerminal = (TreeSymbol as TreeNonTerminal);
                    result = (treeNonTerminal._signPost.MeaningUnit == null) ? false : true;
                }

                return result;
            }
        }
        public bool IsVirtual => TreeSymbol.IsVirtual;
        public bool HasVirtualChild => TreeSymbol.HasVirtualChild;

        public TreeSymbol TreeSymbol { get; private set; }

        public TreeSymbolVertex(TreeSymbol treeSymbol)
        {
            TreeSymbol = treeSymbol;
        }

        public override string ToString() => string.Format("{0}-{1}", ID, IsAst);
    }
}
