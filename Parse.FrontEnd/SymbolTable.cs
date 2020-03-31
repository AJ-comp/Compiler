using System;
using System.Collections;

namespace Parse.FrontEnd
{
    public class SymbolTable : Hashtable
    {
    }


    public abstract class SymbolRecord
    {
        public abstract bool Merge(SymbolRecord src);
    }
}