using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat
{
    public class FuncData : IStorableToHashTable
    {
        public DclSpecData DclSpecData { get; internal set; }
        public string Name => NameToken?.Input;
        public TokenData NameToken { get; internal set; }
        public List<VarData> LocalVars { get; internal set; } = new List<VarData>();

        public string KeyString => string.Format("func {0}", Name);

        public override string ToString() => string.Format("{0}, func name : {1}, local var count : {2}", DclSpecData.ToString(), Name, LocalVars.Count);
    }
}
