using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat
{
    public class VarSymbolTable : List<VarSymbolRecord>
    {
        public VarSymbolTable ChildBlock { get; }
    }

    public class VarSymbolRecord
    {
        public DataType DataType { get; }
        public string Name { get; }
        public int Offset { get; }
        public int Level { get; }
        public int Etc { get; }
    }
}
