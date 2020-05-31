using System;

namespace Parse.FrontEnd.Ast
{
    public abstract class AstBuildParams : ICloneable
    {
        public SymbolTable SymbolTable { get; set; }

        protected AstBuildParams(SymbolTable SymbolTable)
        {
            this.SymbolTable = SymbolTable;
        }

        public abstract object Clone();
    }
}
