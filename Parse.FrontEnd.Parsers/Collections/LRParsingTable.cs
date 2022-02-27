using Parse.FrontEnd.Parsers.Datas.LR;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;
using System.Data;

namespace Parse.FrontEnd.Parsers.Collections
{
    public class LRParsingTable : List<LRParsingRowDataFormat>, IParsingTable
    {
        public TerminalSet RefTerminalSet
        {
            get
            {
                TerminalSet result = new TerminalSet();

                foreach (var item in this)
                {
                    foreach (var matchDataItem in item.MatchedValueSet)
                    {
                        if (matchDataItem.Key is NonTerminal) continue;

                        result.Add(matchDataItem.Key as Terminal);
                    }
                }

                return result;
            }
        }

        public HashSet<NonTerminal> RefNonTerminalSet
        {
            get
            {
                HashSet<NonTerminal> result = new HashSet<NonTerminal>();

                foreach (var item in this)
                {
                    foreach (var matchDataItem in item.MatchedValueSet)
                    {
                        if (matchDataItem.Key is Terminal) continue;

                        result.Add(matchDataItem.Key as NonTerminal);
                    }
                }

                return result;
            }
        }

        public DataTable ToTableFormat
        {
            get
            {
                DataTable result = new DataTable();
                result.Columns.Add(this.Introduce, typeof(string));

                this.CreateColumns(result);
                this.CreateRows(result);

                return result;
            }
        }

        public IEnumerable<Symbol> AllSymbols
        {
            get
            {
                List<Symbol> result = new List<Symbol>();

                result.AddRange(RefNonTerminalSet);
                result.AddRange(RefTerminalSet);

                return result;
            }
        }

        public string Introduce { get; set; } = "Vi / Vt";

        private void CreateColumns(DataTable dataTable)
        {
            // action table
            foreach (var item in this.RefTerminalSet)
            {
                DataColumn column = new DataColumn
                {
                    DataType = typeof(string),
                    ColumnName = item.ToString(),
                    Caption = item.ToString(),
                    ReadOnly = true,
                    DefaultValue = ""
                };
                dataTable.Columns.Add(column);
            }

            // goto table
            foreach (var item in this.RefNonTerminalSet)
            {
                DataColumn column = new DataColumn
                {
                    DataType = typeof(string),
                    ColumnName = item.ToString(),
                    Caption = item.ToString(),
                    ReadOnly = true,
                    DefaultValue = ""
                };
                dataTable.Columns.Add(column);
            }
        }

        private void CreateRows(DataTable dataTable)
        {
            int index = 0;
            foreach (var item in this)
            {
                DataRow row = dataTable.NewRow();

                row[this.Introduce] = $"I{index++}";
                foreach (var matchedItem in item.MatchedValueSet)
                {
                    string moveInfo = matchedItem.Value.Direction.ToString();
                    var destInfo = (matchedItem.Value.Dest is NonTerminalSingle) ? (matchedItem.Value.Dest as NonTerminalSingle).ToGrammarString()
                                                                                                                 : (matchedItem.Value.Dest as int?).ToString();

                    row[matchedItem.Key.ToString()] = moveInfo + $" [{destInfo}]";
                }

                dataTable.Rows.Add(row);
            }
        }

        /// <summary>
        /// This function calculates the core datas for parsing table.
        /// </summary>
        /// <param name="datas">First param needs CanonicalRelation type and Second param needs RelationData type</param>
        public void CreateParsingTable(params object[] datas)
        {
            CanonicalRelation canonicalRelation = datas[0] as CanonicalRelation;

            for (int i = 0; i < canonicalRelation.NextIxIndex; i++)
            {
                var curStatus = canonicalRelation.IndexStateDic[i];

                var matchValueSet = canonicalRelation.GetCanSeeMatchValue(i);
                this.Add(new LRParsingRowDataFormat(curStatus, matchValueSet));
            }
        }
    }
}
