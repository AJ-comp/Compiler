using Parse.Extensions;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.Datas.LR;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Data;

namespace Parse.FrontEnd.Parsers.Collections
{
    public class LRParsingTable : List<LRParsingRowDataFormat>, IParsingTable
    {
        /// <summary>
        /// The parse table as a flat sequence of strongly-typed entries — one per (state, symbol,
        /// action). Lets a consumer iterate / LINQ the table without casting to this concrete type,
        /// without hand-walking the nested row -> symbol -> action structure, and without the untyped
        /// <c>object Dest</c>. Cells with no real parse action (NotProcessed / Failed) are skipped.
        /// </summary>
        public IEnumerable<ParseTableEntry> Entries
        {
            get
            {
                int state = 0;
                foreach (var row in this)
                {
                    foreach (var cell in row.MatchedValueSet)
                    {
                        foreach (var actionData in cell.Value)
                        {
                            var action = actionData.Action;
                            if (action != null) yield return new ParseTableEntry(state, cell.Key, action);
                        }
                    }

                    state++;
                }
            }
        }

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

        /// <summary>
        /// Renders one action as its table-cell text (e.g. <c>Shift [4]</c>, <c>Reduce [E -&gt; T]</c>,
        /// <c>Accept [Accept -&gt; E]</c>). It reads the action through the shared <see cref="ActionData.Action"/>
        /// projection — the same source of truth as <see cref="Entries"/> — so the rendered table and the
        /// typed API can never drift apart. A direction with no parse action (Failed / NotProcessed) falls
        /// back to its enum name with an empty dest, exactly as the previous inline formula did.
        /// </summary>
        internal static string FormatActionCell(ActionData action)
        {
            string label, dest;
            switch (action.Action)
            {
                case ParseAction.Shift s:  label = "Shift"; dest = s.State.ToString(); break;
                case ParseAction.Goto g:   label = "Goto";  dest = g.State.ToString(); break;
                case ParseAction.Reduce r: label = r.IsEpsilon ? "EpsilonReduce" : "Reduce"; dest = r.Production.ToGrammarString(); break;
                case ParseAction.Accept a: label = "Accept"; dest = a.Production.ToGrammarString(); break;
                default:                   label = action.Direction.ToString(); dest = ""; break; // Failed / NotProcessed
            }

            return label + " [" + dest + "]";
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
                    string key = matchedItem.Key.ToString();
                    List<string> matchedValues = new List<string>();

                    foreach (var mItem in matchedItem.Value)
                        matchedValues.Add(FormatActionCell(mItem));

                    row[key] = matchedValues.ItemsString(PrintType.String, "", Environment.NewLine);
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
