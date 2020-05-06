using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Grammars
{
    public class NodeCheckResult
    {
        public object Data { get; }
        public SymbolTable symbolTable { get; }

        public NodeCheckResult(object data, SymbolTable symbolTable)
        {
            Data = data;
            this.symbolTable = symbolTable;
        }
    }
}
