using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.Datas
{
    public class MiniCSymbolTable : SymbolTable
    {
        public MiniCSymbolTable Base { get; }
        public IEnumerable<MiniCFuncData> FuncDataList => _funcList;
        public IEnumerable<MiniCVarRecord> VarList => _varList;

        public IEnumerable<MiniCVarRecord> AllVarList
        {
            get
            {
                List<MiniCVarRecord> result = new List<MiniCVarRecord>();

                MiniCSymbolTable currentTable = this;
                while(currentTable != null)
                {
                    result.AddRange(currentTable.VarList);
                    currentTable = currentTable.Base;
                }

                return result;
            }
        }

        public void AddVarData(VariableMiniC dataToAdd, ReferenceInfo referenceInfo)
        {
            if (dataToAdd.NameToken.IsVirtual) return;

            _varList.Add(new MiniCVarRecord(dataToAdd, referenceInfo));
        }

        public void AddFuncData(MiniCFuncData dataToAdd)
        {
            if (dataToAdd.NameToken.IsVirtual) return;

            _funcList.Add(dataToAdd);
        }

        public VariableMiniC GetVarByName(string name)
        {
            VariableMiniC result = null;

            foreach (var varRecord in AllVarList)
            {
                if (varRecord.VarField.IsMatchWithVarName(name))
                {
                    result = varRecord.VarField;
                    break;
                }
            }

            return result;
        }

        public MiniCFuncData GetFuncByName(string name)
        {
            MiniCFuncData result = null;

            foreach (var funcData in FuncDataList)
            {
                if (funcData.Name == name)
                {
                    result = funcData;
                    break;
                }
            }

            return result;
        }

        public MiniCSymbolTable(MiniCSymbolTable @base = null)
        {
            Base = @base;
        }


        private List<MiniCVarRecord> _varList = new List<MiniCVarRecord>();
        private List<MiniCFuncData> _funcList = new List<MiniCFuncData>();
    }
}
