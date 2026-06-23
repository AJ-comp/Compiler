using Parse.Extensions;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Properties;
using Parse.FrontEnd.ParseTree;
using Parse.FrontEnd.Tokenize;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Parsers.Datas
{
    public enum Direction { Backward, Forward }


    /// <summary>
    /// 
    /// </summary>
    /// <see cref="https://www.lucidchart.com/documents/edit/c96f0bde-4111-4957-bf65-75b56d8074dc/0_0?beaconFlowId=687BBA49A656D177"/>
    public class ParsingResult : List<ParsingBlock>, ICloneable
    {
        private Stack<ConflictItem> _conflictStateStack = new Stack<ConflictItem>();
        private HashSet<ConflictItem> _searchedConflictItems = new HashSet<ConflictItem>();

        public LexingData LexingData { get; set; }
        public ParsingLogger Logger { get; }


        public ConflictAction BackTracking()
        {
            if (_conflictStateStack.Count() == 0) return null;
            var result = _conflictStateStack.Peek();

            if (result.Actions.Count() == 1)
            {
                _conflictStateStack.Pop();

                // remove conflict state that bigger than conflict state that poped.
                // 
                _searchedConflictItems.RemoveWhere(x => x.AmbiguousBlockIndex > result.AmbiguousBlockIndex);
            }
            else result.Actions.RemoveAt(0);

            // clear units from block + 1 that conflict is fired to end.
            for (int i = result.AmbiguousBlockIndex + 1; i < Count; i++) this[i].Clear();

            // make up block that conflict is fired.
            var parsingBlock = this[result.AmbiguousBlockIndex];
            //            parsingBlock.RemoveRange(result.UnitIndexInBlock + 1, parsingBlock.Count() - result.UnitIndexInBlock);
            parsingBlock.RemoveRange(result.UnitIndexInBlock + 1, parsingBlock.Count() - result.UnitIndexInBlock - 1);

            return new ConflictAction(result.State, result.AmbiguousBlockIndex, result.Actions.First());
        }


        public bool Success
        {
            get; internal set;
            /*
        {
            if (this.Count == 0) return false;

            var lastUnit = this.Last().Units.Last();
            if (lastUnit == null) return false;

            return (lastUnit.Action.Direction == ActionDir.accept) ? true : false;
        }
        */
        }

        public bool HasError
        {
            get
            {
                bool result = false;

                Parallel.ForEach(this, (block, loopOption) =>
                {
                    //                    if (block.ErrorUnits.Count > 0)
                    if (block.ErrorInfos.Count > 0)
                    {
                        result = true;
                        loopOption.Stop();
                    }
                });

                return result;
            }
        }


        public IEnumerable<ParsingErrorInfo> AllErrors
        {
            get
            {
                ConcurrentBag<ParsingErrorInfo> result = new ConcurrentBag<ParsingErrorInfo>();

                Parallel.ForEach(this, (block, loopOption) =>
                {
                    foreach (var errorInfo in block.ErrorInfos)
                    {
                        result.Add(errorInfo);
                    }
                });

                return result;
            }
        }

        public AstSymbol AstRoot
        {
            get
            {
                if (!Success) return null;

                return this.Last().Units.Last().AfterStack.AstListStack.Peek() as AstSymbol;
            }
        }

        public DataTable ToParsingHistory
        {
            get
            {
                DataTable result = new DataTable();

                this.AddColumn(result, Resource.StackBeforeParsing);
                this.AddColumn(result, Resource.InputSymbol);
                this.AddColumn(result, Resource.ActionInfo);
                this.AddColumn(result, Resource.RecoveryInfo);
                this.AddColumn(result, Resource.StackAfterParsing);
                this.AddColumn(result, Resource.StackForAst);

                foreach (var record in Logger)
                {
                    var param1 = Convert.ToString(record.Unit.BeforeStack.Stack.Reverse(), " ");
                    var param2 = record.Unit.InputValue?.ToString();
                    var param3 = record.Unit.Action.ToString() + " ";
                    var param4 = record.RecoveryMessage;
                    var param5 = Convert.ToString(record.Unit.AfterStack.Stack.Reverse(), " ");
                    var param6 = AstListViewer.ToString(record.Unit.AfterStack.AstListStack.Reverse());

                    if (record.Unit.IsError) param3 += record.Unit.ErrorMessage;
                    //                        else if (record.Action.Direction != ActionDir.accept)
                    //                            param3 += (record.Action.Dest is NonTerminalSingle) ? (record.Action.Dest as NonTerminalSingle).ToGrammarString() : string.Empty;

                    this.AddRow(result, param1, param2, param3, param4, param5, param6);
                }

                return result;
            }
        }

        public ParseTreeSymbol ToParseTree
        {
            get
            {
                ParseTreeSymbol result = null;
                if (this.Success == false) return result;
                if (this.Count == 0) return result;

                return this.Last().Units.Last().AfterStack.Stack.SecondItemPeek() as ParseTreeSymbol;
            }
        }

        private void AddColumn(DataTable table, string columnName)
        {
            table.Columns.Add(columnName);
            table.Columns[columnName].DefaultValue = string.Empty;
        }

        private void AddColumn(DataTable table, string columnName, Type type)
        {
            table.Columns.Add(columnName);
            table.Columns[columnName].DataType = type;
        }

        private void AddRow(DataTable table, params object[] datas)
        {
            DataRow row = table.NewRow();
            for (int i = 0; i < datas.Length; i++) row[i] = datas[i];
            table.Rows.Add(row);
        }

        public ParsingResult(bool bLogging)
        {
            if (bLogging) Logger = new ParsingLogger();
        }

        public ParsingResult(IEnumerable<ParsingBlock> parsingBlocks, bool bLogging) : this(bLogging)
        {
            AddRange(parsingBlocks);
        }


        public void AddToConfilctStateStack(ConflictItem item)
        {
            if (_searchedConflictItems.Contains(item)) return;

            _conflictStateStack.Push(item);
            _searchedConflictItems.Add(item);
        }


        /*******************************************************/
        /// <summary>
        /// This function checks whether block and block is connected right.
        /// </summary>
        /// <param name="blockIndexToCheck"></param>
        /// <returns></returns>
        /*******************************************************/
        public bool IsBlockConnected(int blockIndexToCheck)
        {
            // case last unit
            if (blockIndexToCheck >= this.Count - 1) return true;

            var prevBlock = this[blockIndexToCheck];
            var nextBlock = this[blockIndexToCheck + 1];

            if (prevBlock.Units.Count == 0 || nextBlock.Units.Count == 0) return false;

            // this means  (check prevBlock[Last] afterStack == nextBlock[First] beforeStack)
            return prevBlock.Units.Last().AfterStack.ApproxEqual(nextBlock.Units.First().BeforeStack);
        }


        public bool JoinBlock(int stdBlockIndex)
        {
            if (!IsBlockConnected(stdBlockIndex)) return false;

            var prevBlock = this[stdBlockIndex];
            var nextBlock = this[stdBlockIndex + 1];

            if (!prevBlock.Last().AfterStack.SyncParent(nextBlock.First().BeforeStack)) return false;
            nextBlock.First().BeforeStack = prevBlock.Last().AfterStack;

            return true;
        }


        /***************************************************************/
        /// <summary>
        /// This function returns it after adding a new ParsingUnit on current ParsingBlock.
        /// </summary>
        /// <param name="blockIndex"></param>
        /// <returns></returns>
        /***************************************************************/
        public ParsingUnit AddUnitOnCurBlock(int blockIndex)
        {
            var prevBlock = this.GetFrontBlock(blockIndex);
            var curBlock = this[blockIndex];

            ParsingUnit result = (curBlock.Units.Count > 0) ? new ParsingUnit(curBlock.Units.Last().AfterStack) :
                                            (prevBlock == null) ? ParsingUnit.FirstParsingUnit : new ParsingUnit(prevBlock.Units.Last().AfterStack);
            result.InputValue = this[blockIndex].Token;

            return result;
        }


        /*******************************************************************/
        /// <summary>
        /// This function returns ParsingBlock that exist anterior from the block of the current index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /*******************************************************************/
        public ParsingBlock GetFrontBlock(int index) => (index <= 0) ? null : this[index - 1];


        public ParsingBlock GetBlockSameStack(ParsingStackUnit target)
        {
            ParsingBlock result = null;

            foreach (var block in this)
            {
                foreach (var unit in block)
                {
                    if (unit.BeforeStack.ApproxEqual(target))
                    {
                        result = block;
                        break;
                    }
                }

                if (result != null) break;
            }

            return result;
        }


        public TokenData GetNextTokenData(int stdBlockIndex, int indexToGet)
        {
            try
            {
                TokenData result = null;

                // It has to start from current block index because may there is multiple TokenData in the current block.
                var curBlock = GetNotIgnoredBlockOnFlow(stdBlockIndex++, Direction.Forward);
                if (curBlock == null) return result;

                while (true)
                {
                    var tokenList = curBlock.UnitTokenList;

                    if (tokenList.Count() <= indexToGet)
                    {
                        indexToGet -= tokenList.Count();
                        curBlock = GetNotIgnoredBlockOnFlow(stdBlockIndex++, Direction.Forward);
                    }
                    else
                    {
                        result = tokenList.ElementAt(indexToGet);
                        break;
                    }
                }

                return result;
            }
            catch
            {
                return null;
            }
        }

        public TokenData GetPrevTokenData(int stdBlockIndex, int indexToGet)
        {
            try
            {
                TokenData result = null;

                // It has to start from current block index because may there is multiple TokenData in the current block.
                var curBlock = GetNotIgnoredBlockOnFlow(stdBlockIndex--, Direction.Backward);
                if (curBlock == null) return result;

                while (true)
                {
                    var tokenList = curBlock.UnitTokenList;

                    if (tokenList.Count() <= indexToGet)
                    {
                        indexToGet -= tokenList.Count();
                        curBlock = GetNotIgnoredBlockOnFlow(stdBlockIndex--, Direction.Backward);
                    }
                    else
                    {
                        result = tokenList.Reverse().ElementAt(indexToGet);
                        break;
                    }
                }

                return result;
            }
            catch
            {
                return null;
            }
        }


        /***************************************************************/
        /// <summary>
        /// This function returns block that is not ignored and on the basis of blockIndex.
        /// ignored property may be set up if error fired.
        /// </summary>
        /// <param name="blockIndex"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        /***************************************************************/
        public ParsingBlock GetNotIgnoredBlockOnFlow(int blockIndex, Direction direction)
        {
            try
            {
                ParsingBlock curBlock = null;
                do
                {
                    curBlock = this[blockIndex];

                    if (direction == Direction.Backward) blockIndex--;
                    else blockIndex++;
                } while (curBlock.IsIgnore);

                return curBlock;
            }
            catch
            {
                return null;
            }
        }

        public void AddAt(int index, TokenData tokenData) => Insert(index, new ParsingBlock(tokenData, Logger));


        /***************************************************************/
        /// <summary>
        /// Change the block of the index to the new ParsingBlock created with parameter.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="tokenData"></param>
        /***************************************************************/
        public void ChangeBlock(int index, TokenData tokenData) => this[index] = new ParsingBlock(tokenData, Logger);


        /***************************************************************/
        /// <summary>
        /// Change the block of the index to the new ParsingBlock created with parameter.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="parsingUnit"></param>
        /// <param name="token"></param>
        /***************************************************************/
        public void ChangeBlock(int index, ParsingUnit parsingUnit, TokenData token) => this[index] = new ParsingBlock(parsingUnit, token, Logger);
        public void AddNewBlock(TokenData tokenData) => Add(new ParsingBlock(tokenData, Logger));

        public object Clone() => new ParsingResult(this, Logger != null) { Success = this.Success };


        /*  LLParsing Tree
        public override string ToParsingTreeString()
        {
            string result = string.Empty;

            ushort depth = 1;
            foreach (DataRow item in this.ParsingHistory.Rows)
            {
                if (item[2].ToString() != "expand") continue;

                result += (item[3] as NonTerminalSingle).ToTreeString(depth++);
            }

            return result;
        }
        */
    }
}
