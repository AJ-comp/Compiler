using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Data
{
    public interface ISymbolData : IData
    {
        TokenData NameToken { get; }
        AJType Type { get; }
        int Block { get; }
        int Offset { get; }
    }

    // Here, Symbol means the referenceable symbol from external.
    public interface ISymbolCenter
    {
        IEnumerable<ISymbolData> SymbolList { get; }
    }
}
