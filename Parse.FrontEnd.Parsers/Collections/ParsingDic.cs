using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Data;

namespace Parse.FrontEnd.Parsers.Collections
{
    public class ParsingDic : Dictionary<Tuple<Terminal, NonTerminal>, NonTerminalSingle>
    {
        private string introduce = "Vn/Vt";
        private List<Tuple<Terminal, NonTerminal, NonTerminalSingle>> duplicateList = new List<Tuple<Terminal, NonTerminal, NonTerminalSingle>>();

        public NonTerminalSingle this[Terminal item1, NonTerminal item2]
        {
            get => this[new Tuple<Terminal, NonTerminal>(item1, item2)];
            set => this[new Tuple<Terminal, NonTerminal>(item1, item2)] = value;
        }

        public bool ContainsKey(Terminal item1, NonTerminal item2)
        {
            return this.ContainsKey(new Tuple<Terminal, NonTerminal>(item1, item2));
        }

        public void Replace(Tuple<Terminal, NonTerminal> key, NonTerminalSingle replaceVaue)
        {
            if (this.ContainsKey(key))
            {
                this.duplicateList.Add(new Tuple<Terminal, NonTerminal, NonTerminalSingle>(key.Item1, key.Item2, this[key]));
                this[key] = replaceVaue;
            }
        }

        public TerminalSet PossibleTerminalSet(NonTerminal curStatus)
        {
            TerminalSet result = new TerminalSet();

            foreach(var item in this)
            {
                if (item.Key.Item2 != curStatus) continue;

                result.Add(item.Key.Item1);
            }

            return result;
        }

        public DataTable ToDataTable(HashSet<NonTerminal> nonTerminals)
        {
            DataTable result = new DataTable();
            result.Columns.Add(this.introduce, typeof(string)); // dt.Columns.Add("Id", typeof(int));

            foreach (var item in this)
            {
                if (result.Columns.Contains(item.Key.Item1.ToString())) continue;

                DataColumn column = new DataColumn();

                column.DataType = typeof(string);
                column.ColumnName = item.Key.Item1.ToString();
                column.Caption = item.Key.Item1.ToString();
                column.ReadOnly = true;
                column.DefaultValue = string.Empty;
                result.Columns.Add(column);
            }

            foreach (var nonterminal in nonTerminals)
            {
                DataRow row = result.NewRow();

                row[this.introduce] = nonterminal.ToString();

                foreach (var item in this)
                {
                    Terminal terminal = item.Key.Item1;
                    Tuple<Terminal, NonTerminal> key = new Tuple<Terminal, NonTerminal>(terminal, nonterminal);
                    if (!this.ContainsKey(key)) continue;

                    var value = this[new Tuple<Terminal, NonTerminal>(terminal, nonterminal)];
                    row[terminal.ToString()] = value.ToGrammarString();
                }

                result.Rows.Add(row);
            }

            return result;
        }
    }
}
