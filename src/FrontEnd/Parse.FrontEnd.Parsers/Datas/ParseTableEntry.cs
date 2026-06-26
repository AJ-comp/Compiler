using Janglim.FrontEnd.RegularGrammar;

namespace Janglim.FrontEnd.Parsers.Datas
{
    /// <summary>
    /// One cell of the parse table, flattened: at parser state <see cref="State"/>, on grammar symbol
    /// <see cref="Symbol"/>, the table's action is <see cref="Action"/>. A single (state, symbol) cell
    /// can produce more than one entry when the grammar is ambiguous (a conflict carries several
    /// actions for the same lookahead).
    /// </summary>
    public sealed class ParseTableEntry
    {
        /// <summary>The state index, matching the <c>I0</c>, <c>I1</c>, … numbering of the table.</summary>
        public int State { get; }

        /// <summary>The column symbol (a terminal for an ACTION cell, a non-terminal for a GOTO cell).</summary>
        public Symbol Symbol { get; }

        /// <summary>The strongly-typed action at this cell (never null in an entry).</summary>
        public ParseAction Action { get; }

        public ParseTableEntry(int state, Symbol symbol, ParseAction action)
        {
            State = state;
            Symbol = symbol;
            Action = action;
        }
    }
}
