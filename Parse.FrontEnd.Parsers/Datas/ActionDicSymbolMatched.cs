using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Text;
using static Parse.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Parse.FrontEnd.Parsers.Datas
{
    /// <summary>
    /// The information for actions when seen symbol.
    /// </summary>
    public class ActionDicSymbolMatched : Dictionary<Symbol, Tuple<ActionDir, object>>
    {
    }
}
