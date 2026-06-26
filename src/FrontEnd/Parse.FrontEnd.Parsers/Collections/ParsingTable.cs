using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.RegularGrammar;
using System.Collections.Generic;
using System.Data;

namespace Janglim.FrontEnd.Parsers.Collections
{
    public interface IParsingTable
    {
        string Introduce { get; set; }

        DataTable ToTableFormat { get; }

        /// <summary>
        /// The parse table as a flat, strongly-typed sequence — one entry per (state, symbol, action).
        /// The data-oriented counterpart to <see cref="ToTableFormat"/> (which renders for display).
        /// </summary>
        IEnumerable<ParseTableEntry> Entries { get; }

        IEnumerable<Symbol> AllSymbols { get; }

        void CreateParsingTable(params object[] datas);
    }
}
