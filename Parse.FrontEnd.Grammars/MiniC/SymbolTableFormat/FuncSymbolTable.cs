using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat
{
    public class FuncSymbolTable : List<FuncSymbolRecord>
    {
    }

    public class FuncSymbolRecord
    {
        public DataType ReturnType { get; }
        public string Name { get; }
        public int Offset { get; }
        public VarSymbolTable VarSymbolTable { get; }
    }
}
