using Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.VarDataFormat;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat
{
    public class FuncDataList : List<FuncData>
    {
        public FuncData ThisFuncData
        {
            get
            {
                FuncData result = null;

                foreach(var item in this)
                {
                    if(item.This)
                    {
                        result = item;
                        break;
                    }
                }

                return result;
            }
        }
    }

    public class FuncData : IStorableToHashTable
    {
        public VarDataList Parent { get; } = new VarDataList();
        public VarDataList Child { get; } = new VarDataList();

        public DclSpecData DclSpecData { get; internal set; }
        public string Name => NameToken?.Input;
        public TokenData NameToken { get; internal set; }
        public List<RealVarData> ParamVars { get; internal set; } = new List<RealVarData>();
        public bool This { get; internal set; } = false;
        public int Offset { get; internal set; }

        public string KeyString => string.Format("func {0}", Name);

        public override string ToString() => string.Format("{0}, func name : {1}, local var count : {2}", DclSpecData.ToString(), Name, ParamVars.Count);
    }
}
