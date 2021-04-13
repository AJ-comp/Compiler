using Parse.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.Datas
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class NamespaceData : ISymbolData
    {
        public List<SdtsNode> ReferenceTable { get; } = new List<SdtsNode>();
        public string Name => NameTokens.ItemsString(PrintType.String, string.Empty, ".");

        public Access AccessType => Access.Public;
        public IEnumerable<ClassDefData> ClassDatas => _classDatas;
        public IEnumerable<TokenData> NameTokens => _nameTokens;

        public IEnumerable<ISymbolData> AllSymbols
        {
            get
            {
                List<ISymbolData> result = new List<ISymbolData>();

                foreach (var item in ClassDatas) result.Add(item);

                return result;
            }
        }

        public int Block { get; private set; }
        public int Offset { get; internal set; }

        public NamespaceData(int blockLevel, List<TokenData> nameTokens, HashSet<ClassDefData> classDatas)
        {
            Block = blockLevel;

            _nameTokens = nameTokens;
            _classDatas = classDatas;
        }


        internal List<TokenData> _nameTokens = new List<TokenData>();
        internal HashSet<ClassDefData> _classDatas = new HashSet<ClassDefData>();

        private string GetDebuggerDisplay()
        {
            return $"namespace {Name} (Class count: {ClassDatas.Count()})";
        }
    }
}
