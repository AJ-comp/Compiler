using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.Datas
{
    public interface ISymbolData : IHasName
    {
        Access AccessType { get; }
        List<SdtsNode> ReferenceTable { get; }

        int Block { get; }
        int Offset { get; }
    }

    // Here, Symbol means the referenceable symbol from external.
    public interface IHasSymbol
    {
        IEnumerable<ISymbolData> SymbolList { get; }
    }
}
