using ParsingLibrary.Datas;
using ParsingLibrary.Datas.RegularGrammar;
using ParsingLibrary.Parsers.Datas;
using ParsingLibrary.Parsers.EventArgs;
using ParsingLibrary.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using static ParsingLibrary.Parsers.Datas.LRParsingData;

namespace ParsingLibrary.Parsers.Collections
{
    public class ParsingRule : List<LRParsingData>
    {
        private string introduce = "Vi / Vt";
        private Stack<object> stack = new Stack<object>();

        /// <summary>
        /// The Error Handler that if the action failed.
        /// TokenData : input data
        /// TerminalSet : correct input set
        /// </summary>
        public Action<TokenData, TerminalSet> ActionFailed { get; set; } = null;
        /// <summary>
        /// The Error Handler that if the goto failed.
        /// TokenData : input data
        /// NonTerminalSet : possible nonterminal set
        /// </summary>
        public Action<TokenData, HashSet<NonTerminal>> GotoFailed { get; set; } = null;
        /// <summary>
        /// The Error Handler that if the action completed.
        /// </summary>
        public Action<LRParsingEventArgs> ActionCompleted { get; set; } = null;

        private TerminalSet RefTerminalSet
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

        private HashSet<NonTerminal> RefNonTerminalSet
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

        private void CreateColumns(DataTable dataTable)
        {
            // action table
            foreach (var item in this.RefTerminalSet)
            {
                DataColumn column = new DataColumn();

                column.DataType = typeof(string);
                column.ColumnName = item.ToString();
                column.Caption = item.ToString();
                column.ReadOnly = true;
                column.DefaultValue = "";
                dataTable.Columns.Add(column);
            }

            // goto table
            foreach (var item in this.RefNonTerminalSet)
            {
                DataColumn column = new DataColumn();

                column.DataType = typeof(string);
                column.ColumnName = item.ToString();
                column.Caption = item.ToString();
                column.ReadOnly = true;
                column.DefaultValue = "";
                dataTable.Columns.Add(column);
            }
        }

        private void CreateRows(DataTable dataTable)
        {
            int index = 0;
            foreach (var item in this)
            {
                DataRow row = dataTable.NewRow();

                row[this.introduce] = "I" + index++.ToString();
                foreach (var matchedItem in item.MatchedValueSet)
                {
                    string moveInfo = matchedItem.Value.Item1.ToString();
                    var destInfo = (matchedItem.Value.Item2 is NonTerminalSingle) ? (matchedItem.Value.Item2 as NonTerminalSingle).ToGrammarString() : (matchedItem.Value.Item2 as int?).ToString();

                    row[matchedItem.Key.ToString()] = moveInfo + " [" + destInfo + "]";
                }

                dataTable.Rows.Add(row);
            }
        }

        private ActionInfo ShiftAndReduce(TokenData inputValue, Stack<object> prevStack)
        {
            LRParsingData.ActionInfo result = LRParsingData.ActionInfo.failed;
            var topData = this.stack.Peek();

            if (topData is NonTerminalSingle) return result;

            var IxMetrix = this[(int)topData];

            // invalid input symbol
            if (!IxMetrix.MatchedValueSet.ContainsKey(inputValue.Kind))
            {
                this.ActionFailed?.Invoke(inputValue, IxMetrix.PossibleTerminalSet);
                return result;
            }

            var matchedValue = IxMetrix.MatchedValueSet[inputValue.Kind];
            if (matchedValue.Item1 == LRParsingData.ActionInfo.shift)
            {
                this.stack.Push(inputValue);
                this.stack.Push(matchedValue.Item2);

                result = LRParsingData.ActionInfo.shift;
            }
            else if (matchedValue.Item1 == LRParsingData.ActionInfo.reduce)
            {
                var reduceDest = matchedValue.Item2 as NonTerminalSingle;

                for(int i=0; i<reduceDest.Count*2; i++) this.stack.Pop();
                this.stack.Push(reduceDest);

                result = LRParsingData.ActionInfo.reduce;
            }
            else if (matchedValue.Item1 == LRParsingData.ActionInfo.accept) result = LRParsingData.ActionInfo.accept;

            this.ActionCompleted?.Invoke(new LRParsingEventArgs(prevStack, this.stack, inputValue, matchedValue.Item1, matchedValue.Item2));

            return result;
        }

        private bool GoTo(TokenData inputValue, Stack<object> prevStack)
        {
            bool result = true;
            var topData = this.stack.Peek();

            if (!(topData is NonTerminalSingle)) return false;

            var seeSingleNT = topData as NonTerminalSingle;
            var secondData = this.stack.SecondItemPeek();
            var IxMetrix = this[(int)secondData];

            // can't goto action
            if (!IxMetrix.MatchedValueSet.ContainsKey(seeSingleNT.ToNonTerminal()))
            {
                result = false;
                this.GotoFailed?.Invoke(inputValue, IxMetrix.PossibleNonTerminalSet);
            }
            else
            {
                var matchedValue = IxMetrix.MatchedValueSet[seeSingleNT.ToNonTerminal()];

                this.stack.Push((int)matchedValue.Item2);
                this.ActionCompleted?.Invoke(new LRParsingEventArgs(prevStack, this.stack, inputValue, matchedValue.Item1, matchedValue.Item2));
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parsingHistory"></param>
        /// <param name="relationData"></param>
        public void Calculate(CanonicalHistory parsingHistory, RelationData relationData)
        {
            for(int i=0; i<=parsingHistory.MaxIxIndex; i++)
            {
                var curStatus = parsingHistory.GetCurStatusFromIxIndex(i);

                var matchValueSet = parsingHistory.GetCanSeeMatchValue(i, relationData);
                this.Add(new LRParsingData(curStatus, matchValueSet));
            }
        }

        public void ParsingInit()
        {
            this.stack.Clear();
            this.stack.Push(0);
        }

        /// <summary>
        /// Calculate the next stack status using the current stack and input terminal.
        /// </summary>
        /// <param name="stack">current stack</param>
        /// <param name="inputValue">input terminal</param>
        /// <returns></returns>
        public ActionInfo Parsing(TokenData inputValue)
        {
            ActionInfo result = ActionInfo.failed;
            Stack<object> prevStack = this.stack.Clone();

            if (this.GoTo(inputValue, prevStack))
            {
                result = ActionInfo.moveto;
            }
            else
            {
                result = this.ShiftAndReduce(inputValue, prevStack);
            }

            return result;
        }

        /*
        public bool ContainsKey(Canonical item1, Symbol item2)
        {
            return this.ContainsKey(new Tuple<Canonical, Symbol>(item1, item2));
        }
        */


        /*
        public TerminalSet PossibleTerminalSet(Canonical curStatus)
        {
            TerminalSet result = new TerminalSet();

            foreach (var item in this)
            {
                if (item.Key.Item2 != curStatus) continue;

                result.Add(item.Key.Item1);
            }

            return result;
        }
        */


        public DataTable ToDataTable()
        {
            DataTable result = new DataTable();
            result.Columns.Add(this.introduce, typeof(string));

            this.CreateColumns(result);
            this.CreateRows(result);

            return result;
        }
    }
}
