using Parse.FrontEnd.Parsers.Datas;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.Collections
{
    public class SymbolTable : HashSet<SymbolRowData>
    {
        public bool Contains(TokenData token)
        {
            bool result = false;

            foreach(var item in this)
            {
            }

            return result;
        }
    }
}
