using System;

namespace Parse.FrontEnd
{
    public abstract class SymbolTable
    {
    }


    public abstract class SymbolRecord
    {
        public abstract bool Merge(SymbolRecord src);
    }
}