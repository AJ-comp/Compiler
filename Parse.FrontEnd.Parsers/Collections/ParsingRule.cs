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
        /// TokenData : status when error generated
        /// TerminalSet : correct input set
        /// </summary>
        public Action<LRParsingEventArgs, TerminalSet> ActionFailed { get; set; } = null;
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

        private ActionInfo Process(Tuple<ActionInfo,object>matchedValue, TokenData inputValue, Stack<object> prevStack)
        {
            ActionInfo result = ActionInfo.failed;

            if (matchedValue.Item1 == ActionInfo.shift)
            {
                this.stack.Push(inputValue);
                this.stack.Push(matchedValue.Item2);

                result = ActionInfo.shift;
            }
            else if (matchedValue.Item1 == ActionInfo.reduce)
            {
                var reduceDest = matchedValue.Item2 as NonTerminalSingle;
                //                if(reduceDest.IsEpsilon

                for (int i = 0; i < reduceDest.Count * 2; i++) this.stack.Pop();
                this.stack.Push(reduceDest);

                result = ActionInfo.reduce;
            }
            else if (matchedValue.Item1 == ActionInfo.epsilon_reduce)
            {
                this.stack.Push(matchedValue.Item2 as NonTerminalSingle);


                result = ActionInfo.reduce;
            }
            else if (matchedValue.Item1 == ActionInfo.accept) result = ActionInfo.accept;

            return result;
        }

        /// <summary>
        /// This function processes shift and reduce process.
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="prevStack"></param>
        /// <returns></returns>
        private ActionInfo ShiftAndReduce(TokenData inputValue, Stack<object> prevStack)
        {
            var topData = this.stack.Peek();

            if (topData is NonTerminalSingle) return ActionInfo.failed;

            var IxMetrix = this[(int)topData];

            // invalid input symbol, can't shift
            if (!IxMetrix.MatchedValueSet.ContainsKey(inputValue.Kind))
            {
                var data = new LRParsingEventArgs(prevStack, this.stack, inputValue, ActionInfo.failed, null);
                this.ActionFailed?.Invoke(new LRParsingEventArgs(prevStack, this.stack, inputValue, ActionInfo.failed, null), IxMetrix.PossibleTerminalSet);
                return ActionInfo.failed;
            }

            var matchedValue = IxMetrix.MatchedValueSet[inputValue.Kind];
            var result = this.Process(matchedValue, inputValue, prevStack);

            this.ActionCompleted?.Invoke(new LRParsingEventArgs(prevStack, this.stack, inputValue, matchedValue.Item1, matchedValue.Item2));

            return result;
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
                this.ActionCompleted?.Invoke(new LRParsingEventArgs(prevStack, this.stack, inputValue, matchedValue.Item1, matchedValue.Item2));
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
