using Parse.Extensions;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Parsers.Properties;
using Parse.FrontEnd.ParseTree;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Parsers.Datas
{
    /// <summary>
    /// 
    /// </summary>
    /// <see cref="https://www.lucidchart.com/documents/edit/c96f0bde-4111-4957-bf65-75b56d8074dc/0_0?beaconFlowId=687BBA49A656D177"/>
    public class ParsingResult : List<ParsingBlock>, ICloneable
    {
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

                foreach (var block in this)
                {
                    foreach (var record in block.History)
                    {
                        var param1 = Convert.ToString(record.Unit.BeforeStack.Stack.Reverse(), " ");
                        var param2 = record.Unit.InputValue?.ToString();
                        var param3 = record.Unit.Action.ToString() + " ";
                        var param4 = record.RecoveryMessage;
                        var param5 = Convert.ToString(record.Unit.AfterStack.Stack.Reverse(), " ");

                        if (record.Unit.IsError) param3 += record.Unit.ErrorMessage;
                        //                        else if (record.Action.Direction != ActionDir.accept)
                        //                            param3 += (record.Action.Dest is NonTerminalSingle) ? (record.Action.Dest as NonTerminalSingle).ToGrammarString() : string.Empty;

                        this.AddRow(result, param1, param2, param3, param4, param5);
                    }
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

        public ParsingResult() { }
        public ParsingResult(IEnumerable<ParsingBlock> parsingBlocks) => this.AddRange(parsingBlocks);


        /// <summary>
        /// This function checks whether block and block is connected right.
        /// </summary>
        /// <param name="blockIndexToCheck"></param>
        /// <returns></returns>
        public bool IsRightBlockConnected(int blockIndexToCheck)
        {
            if (blockIndexToCheck >= this.Count - 1) return true;

            var prevBlock = this[blockIndexToCheck];
            var nextBlock = this[blockIndexToCheck + 1];

            // this means  (check prevBlock[Last] afterStack == nextBlock[First] beforeStack)
            return prevBlock.Units.Last().AfterStack.Stack.SequenceEqual(nextBlock.Units.First().BeforeStack.Stack);
        }

        /// <summary>
        /// This function returns it after adding a new ParsingUnit on current ParsingBlock.
        /// </summary>
        /// <param name="blockIndex"></param>
        /// <returns></returns>
        public ParsingUnit AddUnitOnCurBlock(int blockIndex)
        {
            var prevBlock = this.GetFrontBlock(blockIndex);
            var curBlock = this[blockIndex];

            ParsingUnit result = (curBlock.Units.Count > 0) ? new ParsingUnit(curBlock.Units.Last().AfterStack) :
                                            (prevBlock == null) ? ParsingUnit.FirstParsingUnit : new ParsingUnit(prevBlock.Units.Last().AfterStack);
            result.InputValue = this[blockIndex].Token;

            return result;
        }

        /// <summary>
        /// This function returns ParsingBlock that exist anterior from the block of the current index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ParsingBlock GetFrontBlock(int index) => (index <= 0) ? null : this[index - 1];

        public TokenData GetBeforeTokenData(int stdBlockIndex, int indexToGet)
        {
            try
            {
                TokenData result = null;
                var curBlock = GetRealBlock(stdBlockIndex--);

                while(true)
                {
                    var tokenList = curBlock.UnitTokenList.Reverse();

                    if (tokenList.Count() <= indexToGet)
                    {
                        indexToGet -= tokenList.Count();
                        curBlock = GetRealBlock(stdBlockIndex--);
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

        public ParsingBlock GetRealBlock(int blockIndex)
        {
            try
            {
                ParsingBlock curBlock = null;
                do
                {
                    curBlock = this[blockIndex--];
                } while (curBlock.IsIgnore);

                return curBlock;
            }
            catch
            {
                return null;
            }
        }

        public object Clone() => new ParsingResult(this) { Success = this.Success };


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
