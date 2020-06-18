using Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.LiteralDataFormat;
using Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.VarDataFormat;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat
{
    public enum DataType { Unknown, Void, Int }
    public enum EtcInfo { Normal, Extern, Param }

    public class VarDataList : List<VarData>
    {
        public VarDataList Parent { get; }
        public VarDataList Child { get; }

        //public VarDataList ThisList
        //{
        //    get
        //    {
        //        var result = new VarDataList();

        //        var data = from t in this
        //                   where t.This == true
        //                   select t;

        //        result.AddRange(data);
        //        return result;
        //    }
        //}

        public VarData GetVarByName(string name)
        {
            VarData result = null;

            foreach(var item in this)
            {
                if(item.IsMatchWithVarName(name))
                {
                    result = item;
                    break;
                }
            }

            return result;
        }

        //public VarDataList GetVarListByBlockLevel(int blockLevel)
        //{
        //    var result = new VarDataList();

        //    var data = from t in this
        //                    where t.DclData.BlockLevel == blockLevel
        //                    select t;

        //    result.AddRange(data);
        //    return result;
        //}
    }

    public class DclItemData : IStorableToHashTable
    {
        public string Name => NameToken?.Input;
        public int Level => System.Convert.ToInt32(LevelToken?.Input);
        public int Dimension => System.Convert.ToInt32(DimensionToken?.Input);

        public TokenData NameToken { get; internal set; }
        public TokenData LevelToken { get; internal set; }
        public TokenData DimensionToken { get; internal set; }
        public LiteralData Value { get; internal set; } = new UnknownLiteralData(UnknownState.NotInitialized, null);

        public string KeyString => string.Format("{0}", Name);

        public override string ToString() => string.Format("name : {0}, level : {1}, dimension : {2}", Name, Level.ToString(), Dimension.ToString());
    }

    public class DclSpecData : IStorableToHashTable
    {
        public bool Const => (ConstToken == null) ? false : true;
        public DataType DataType { get; internal set; }

        public TokenData ConstToken { get; internal set; } = null;
        public TokenData DataTypeToken { get; internal set; }

        public string KeyString => string.Empty;

        public override string ToString() => string.Format("const : {0}, DataType : {1}", Const.ToString(), DataType.ToString());
    }


    public class DclData : IStorableToHashTable
    {
        public DclSpecData DclSpecData { get; internal set; }
        public DclItemData DclItemData { get; internal set; }
        public int BlockLevel { get; internal set; }
        public int Offset { get; internal set; }
        public EtcInfo Etc { get; internal set; }

        public string KeyString => string.Format("{0} {1}", DclItemData.KeyString, BlockLevel.ToString());

        public override string ToString()
            => string.Format("{0}, {1}, block level : {2}, offset : {3}, Etc : {4}", 
                                        DclSpecData.ToString(), DclItemData.ToString(), BlockLevel, Offset, Etc.ToString());
    }
}
