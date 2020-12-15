using Parse.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.MiniC.Sdts.Datas
{
    public class NamespaceData : ISymbolData
    {
        public List<SdtsNode> ReferenceTable { get; } = new List<SdtsNode>();
        public string Name => NameTokens.ItemsString(PrintType.String, string.Empty, ".");
        public IEnumerable<ClassData> ClassDatas => _classDatas;

        public IEnumerable<TokenData> NameTokens => _nameTokens;

        public NamespaceData(IEnumerable<TokenData> nameTokens, IEnumerable<ClassData> classDatas)
        {
            _nameTokens.AddRange(nameTokens);
            _classDatas.AddRange(classDatas);
        }


        internal List<TokenData> _nameTokens = new List<TokenData>();
        internal List<ClassData> _classDatas = new List<ClassData>();
    }
}
