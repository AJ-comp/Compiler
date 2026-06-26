using System.Collections.Generic;

namespace Janglim.FrontEnd.Parsers.Collections
{
    public class SymbolTableStack : Stack<SymbolTable>
    {
        public bool Contains(TokenData[] tokenList)
        {
            return false;
        }
    }
}
