using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParsingLibrary.Datas.RegularGrammar
{
    public enum BridgeType { Concatenation, Alternation };

    public interface IBridgeable
    {
        BridgeType BridgeType { get; }
    }
}
