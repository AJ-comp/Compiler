using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.Datas
{
    public class MiniCSymbolTable : SymbolTable
    {
        public MiniCSymbolTable Base { get; }
        public List<MiniCFuncData> FuncDataList { get; } = new List<MiniCFuncData>();
        public List<MiniCVarData> VarList { get; } = new List<MiniCVarData>();

        public IReadOnlyList<MiniCVarData> AllVarList
        {
            get
            {
                List<MiniCVarData> result = new List<MiniCVarData>();

                MiniCSymbolTable currentTable = this;
                while(currentTable != null)
                {
                    result.AddRange(currentTable.VarList);
                    currentTable = currentTable.Base;
                }

                return result;
            }
        }

        public MiniCVarData GetVarByName(string name)
        {
            MiniCVarData result = null;

            foreach (var varData in AllVarList)
            {
                if (varData.IsMatchWithVarName(name))
                {
                    result = varData;
                    break;
                }
            }

            return result;
        }

        public MiniCSymbolTable(MiniCSymbolTable @base = null)
        {
            Base = @base;
        }
    }
}
