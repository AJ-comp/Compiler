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
        public bool Result { get; internal set; }

        public NodeBuildResult(object data, SymbolTable symbolTable, bool result = false)
        {
            Data = data;
            this.symbolTable = symbolTable;
            Result = result;
        }
    }
}
