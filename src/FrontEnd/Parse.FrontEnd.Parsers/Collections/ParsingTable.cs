using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;
using System.Data;

namespace Parse.FrontEnd.Parsers.Collections
{
    public interface IParsingTable
    {
        string Introduce { get; set; }

        DataTable ToTableFormat { get; }

        IEnumerable<Symbol> AllSymbols { get; }

        void CreateParsingTable(params object[] datas);
    }
}
