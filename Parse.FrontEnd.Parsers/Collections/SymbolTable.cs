using Parse.FrontEnd.Parsers.Datas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
