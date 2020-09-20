using Parse.FrontEnd.ParseTree;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.Ast
{
    public abstract class AstSymbol : IShowable
    {
        public AstSymbol Parent { get; internal set; } = null;
        public ParseTreeSymbol ConnectedParseTree { get; protected set; }
        public SdtsNode Sdts { get; set; }
        public AstSymbol Root
        {
            get
            {
                AstSymbol root = this;

                while (root.Parent != null)
                {
                    root = root.Parent;
                }

                return root;
            }
        }

        public abstract IReadOnlyList<TokenData> AllTokens { get; }

        /// <summary>
        /// Remove all connected information on this tree.
        /// </summary>
        public void ClearConnectedInfo()
        {
        }

        public abstract string ToGrammarString();
        public abstract string ToTreeString(ushort depth = 1);

        public static AstSymbol Create(ParseTreeSymbol parseTreeSymbol)
        {
            AstSymbol result = null;

            if (parseTreeSymbol.IsMeaning == false) return result;

            if (parseTreeSymbol is ParseTreeTerminal)
            {
                var cTree = (parseTreeSymbol as ParseTreeTerminal);
                result = new AstTerminal(cTree.Token);
            }
            else
                result = new AstNonTerminal((parseTreeSymbol as ParseTreeNonTerminal));

            return result;
        }
    }
}
