using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Ast
{
    public abstract class AstSymbol : IShowable
    {
        public AstSymbol Parent { get; internal set; } = null;

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
