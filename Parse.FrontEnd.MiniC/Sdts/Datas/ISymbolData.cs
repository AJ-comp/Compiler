using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.MiniC.Sdts.Datas
{
    public interface ISymbolData : IHasName
    {
        List<SdtsNode> ReferenceTable { get; }
    }
}
