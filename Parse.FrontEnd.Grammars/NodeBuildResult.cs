using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Grammars
{
    public class NodeBuildResult
    {
        public object Data { get; }
        public SymbolTable symbolTable { get; }

        public NodeBuildResult(object data, SymbolTable symbolTable)
        {
            Data = data;
            this.symbolTable = symbolTable;
        }
    }
}
