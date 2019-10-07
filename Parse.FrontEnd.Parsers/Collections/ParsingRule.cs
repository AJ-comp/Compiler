using Parse.Extensions;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.EventArgs;
using Parse.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Data;
using static Parse.FrontEnd.Parsers.Datas.LRParsingData;

namespace Parse.FrontEnd.Parsers.Collections
{
    public class ParsingRule : List<LRParsingData>
    {
        private string introduce = "Vi / Vt";
        private Stack<object> stack = new Stack<object>();

        /// <summary>
        /// The Error Handler that if the action failed.
        /// LRParsingEventArgs : The state information when error generated
        /// TerminalSet : The possible input terminal set
        /// </summary>
        public event EventHandler<ParsingFailedEventArgs> ActionFailed;
        /// <summary>
        /// The Error Handler that if the goto failed.
        /// TokenData : input data
        /// NonTerminalSet : possible nonterminal set
        /// </summary>
        public Action<TokenData, HashSet<NonTerminal>> GotoFailed { get; set; } = null;
        /// <summary>
        /// The Error Handler that if the action completed.
        /// </summary>
        public event EventHandler<LRParsingEventArgs> ActionCompleted;

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

        private ActionData Process(Tuple<ActionDir,object>matchedValue, TokenData inputValue, Stack<object> prevStack)
        {
            ActionData result = new ActionData();
            result.ActionDest = matchedValue.Item2;

            if (matchedValue.Item1 == ActionDir.shift)
            {
                this.stack.Push(inputValue);
                this.stack.Push(matchedValue.Item2);

                result.ActionDirection = ActionDir.shift;
            }
            else if (matchedValue.Item1 == ActionDir.reduce)
            {
                var reduceDest = matchedValue.Item2 as NonTerminalSingle;

                for (int i = 0; i < reduceDest.Count * 2; i++) this.stack.Pop();
                this.stack.Push(reduceDest);
                result.ActionDirection = ActionDir.reduce;
            }
            else if (matchedValue.Item1 == ActionDir.epsilon_reduce)
            {
                this.stack.Push(matchedValue.Item2 as NonTerminalSingle);

                result.ActionDirection = ActionDir.epsilon_reduce;
            }
            else if (matchedValue.Item1 == ActionDir.accept) result.ActionDirection = ActionDir.accept;

            return result;
        }

        /// <summary>
        /// This function processes shift and reduce process.
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="prevStack"></param>
        /// <returns></returns>
        private ActionDir ShiftAndReduce(TokenData inputValue, Stack<object> prevStack)
        {
            var topData = this.stack.Peek();

            if (topData is NonTerminalSingle) return ActionDir.failed;

            var IxMetrix = this[(int)topData];

            // invalid input symbol, can't shift
            if (!IxMetrix.MatchedValueSet.ContainsKey(inputValue.Kind))
            {
                var data = new LRParsingEventArgs(prevStack, this.stack, inputValue, new ActionData(ActionDir.failed, null));
                this.ActionFailed?.Invoke(this, new ParsingFailedEventArgs(prevStack, this.stack, inputValue, new ActionData(ActionDir.failed, null), IxMetrix.PossibleTerminalSet));
                return ActionDir.failed;
            }

            var matchedValue = IxMetrix.MatchedValueSet[inputValue.Kind];
            var result = this.Process(matchedValue, inputValue, prevStack);

            this.ActionCompleted?.Invoke(this, new LRParsingEventArgs(prevStack, this.stack, inputValue, result));

            return result.ActionDirection;
        }

        /// <summary>
        /// This function processes goto process.
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="prevStack"></param>
        /// <returns></returns>
        private bool GoTo(TokenData inputValue, Stack<object> prevStack)
        {
            bool result = true;
            var topData = this.stack.Peek();

            if (!(topData is NonTerminalSingle)) return false;

            var seenSingleNT = topData as NonTerminalSingle;
            var secondData = this.stack.SecondItemPeek();
            var IxMetrix = this[(int)secondData];

            // can't goto action
            if (!IxMetrix.MatchedValueSet.ContainsKey(seenSingleNT.ToNonTerminal()))
            {
                result = false;
                this.GotoFailed?.Invoke(inputValue, IxMetrix.PossibleNonTerminalSet);
            }
            else
            {
                var matchedValue = IxMetrix.MatchedValueSet[seenSingleNT.ToNonTerminal()];

                this.stack.Push((int)matchedValue.Item2);
                var actionData = new ActionData(matchedValue.Item1, matchedValue.Item2);
                this.ActionCompleted?.Invoke(this, new LRParsingEventArgs(prevStack, this.stack, inputValue, actionData));
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parsingHistory"></param>
        /// <param name="relationData"></param>
        public void Calculate(CanonicalTable parsingHistory, RelationData relationData)
        {
            for(int i=0; i<=parsingHistory.MaxIxIndex; i++)
            {
                var curStatus = parsingHistory.GetStatusFromIxIndex(i);

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
        public ActionDir Parsing(TokenData inputValue)
        {
            ActionDir result = ActionDir.failed;
            Stack<object> prevStack = this.stack.Clone();

            if (this.GoTo(inputValue, prevStack))
            {
                result = ActionDir.moveto;
            }
            else
            {
                result = this.ShiftAndReduce(inputValue, prevStack);
            }

            return result;
        }


        public DataTable ToDataTable()
        {
            DataTable result = new DataTable();
            result.Columns.Add(this.introduce, typeof(string));

            /*
            DataColumn column = new DataColumn();

            column.DataType = typeof(string);
            column.ColumnName = "(";
            column.Caption = "(";
            column.ReadOnly = true;
            column.DefaultValue = "";
            result.Columns.Add(column);
            */

            this.CreateColumns(result);
            this.CreateRows(result);

            return result;
        }
    }
}
