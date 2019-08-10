using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.Collections
{
    public class SymbolTableStack : Stack<SymbolTable>
    {
        public bool Contains(TokenData[] tokenList)
        {
            return false;
        }
    }
}
