using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.Ast
{
    public abstract class AstSymbol : IShowable
    {
        public AstSymbol Parent { get; internal set; } = null;

        public SymbolTable ConnectedSymbolTable { get; set; }
        public MeaningErrInfoList ConnectedErrInfoList { get; } = new MeaningErrInfoList();
        public object ConnectedIrUnit { get; set; }

        public bool IsDummy => (ConnectedErrInfoList.Count == 0 && ConnectedIrUnit == null) ? true : false;

        /// <summary>
        /// Remove all connected information on this tree.
        /// </summary>
        public void ClearConnectedInfo()
        {
            ConnectedSymbolTable = null;
            ConnectedErrInfoList.Clear();
        }

        public abstract string ToGrammarString();
        public abstract string ToTreeString(ushort depth = 1);
    }
}
