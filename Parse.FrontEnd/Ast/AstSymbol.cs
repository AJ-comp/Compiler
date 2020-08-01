using Parse.FrontEnd.ParseTree;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.Ast
{
    public abstract class AstSymbol : IShowable
    {
        public AstSymbol Parent { get; internal set; } = null;
        public ParseTreeSymbol ConnectedParseTree { get; internal set; }

        public abstract IReadOnlyList<TokenData> AllTokens { get; }

        /// <summary>
        /// Remove all connected information on this tree.
        /// </summary>
        public void ClearConnectedInfo()
        {
        }

        public abstract string ToGrammarString();
        public abstract string ToTreeString(ushort depth = 1);
    }
}
