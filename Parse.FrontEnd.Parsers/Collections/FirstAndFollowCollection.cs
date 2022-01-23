using AJ.Common.Helpers;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace Parse.FrontEnd.Parsers.Collections
{
    public class FirstAndFollowCollection : List<FirstAndFollowItem>
    {

        public DataTable ToTableFormat
        {
            get
            {
                DataTable result = new DataTable();

                result.CreateColumns(_symbolColumn, _firstColumn, _followColumn);
                this.CreateRows(result);

                return result;
            }
        }


        private void CreateRows(DataTable dataTable)
        {
            foreach (var item in this)
            {
                DataRow row = dataTable.NewRow();

                row[_symbolColumn] = item.Symbol.ToString();
                row[_firstColumn] = item.First;
                row[_followColumn] = item.Follow;

                dataTable.Rows.Add(row);
            }
        }


        private string _symbolColumn = "symbol";
        private string _firstColumn = "first";
        private string _followColumn = "follow";
    }


    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class FirstAndFollowItem
    {
        public Symbol Symbol { get; }
        public TerminalSet First { get; }
        public TerminalSet Follow { get; }

        public FirstAndFollowItem(Symbol symbol, TerminalSet first, TerminalSet follow)
        {
            Symbol = symbol;
            First = first;
            Follow = follow;
        }

        public string GetDebuggerDisplay()
        {
            string result = string.Empty;

            if (Symbol is Terminal)
            {
                var terminal = Symbol as Terminal;

                result = $"{terminal.Value}[{First}:{Follow}]";
            }
            else
            {
                var nonTerminal = Symbol as NonTerminal;

                result = $"{nonTerminal.Name}[{First}:{Follow}]";
            }

            return result;
        }
    }
}
