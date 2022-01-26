﻿using System.Collections.Generic;

namespace Parse.FrontEnd
{
    public interface ISymbolData : IData
    {
        TokenData NameToken { get; }
        int Block { get; }
        int Offset { get; }
    }

    // Here, Symbol means the referenceable symbol from external.
    public interface ISymbolCenter
    {
        IEnumerable<ISymbolData> SymbolList { get; }
    }
}
