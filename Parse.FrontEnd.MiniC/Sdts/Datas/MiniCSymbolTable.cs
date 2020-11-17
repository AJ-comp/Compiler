using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Parse.FrontEnd.MiniC.Sdts.Datas
{
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class MiniCSymbolTable : SymbolTable
    {
        public MiniCSymbolTable Base { get; }
        public FuncTable FuncTable => _funcTable;
        public VarTable VarTable => _varTable;

        public IEnumerable<VarTable> AllVarTable
        {
            get
            {
                List<VarTable> result = new List<VarTable>();

                MiniCSymbolTable currentTable = this;
                while(currentTable != null)
                {
                    result.Add(currentTable.VarTable);
                    currentTable = currentTable.Base;
                }

                return result;
            }
        }

        public VariableMiniC GetVarByName(string name)
        {
            VariableMiniC result = null;

            foreach (var varTable in AllVarTable)
            {
                result = varTable.GetMatchedItemWithName(name);

                if (result != null) break;
            }

            return result;
        }

        public MiniCFuncData GetFuncByName(string name)
        {
            MiniCFuncData result = null;

            foreach (var funcData in FuncTable)
            {
                if (funcData.DefineField.Name == name)
                {
                    result = funcData.DefineField;
                    break;
                }
            }

            return result;
        }

        public MiniCSymbolTable(MiniCSymbolTable @base = null)
        {
            Base = @base;
        }

        private VarTable _varTable = new VarTable();
        private FuncTable _funcTable = new FuncTable();

        private string DebuggerDisplay
            => string.Format("FuncTable items : {0}, VarTable items : {1}", _funcTable.Count(), _varTable.Count());
    }
}
