using System;

namespace Parse.FrontEnd
{
    public abstract class SymbolTable : ICloneable
    {
        public abstract object Clone();
    }


    public abstract class SymbolRecord
    {
        public abstract bool Merge(SymbolRecord src);
    }
}