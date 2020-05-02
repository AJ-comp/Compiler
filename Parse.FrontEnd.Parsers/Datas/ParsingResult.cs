using Parse.Extensions;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Parsers.Properties;
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
                    if(block.ErrorInfos.Count > 0)
                    {
                        result = true;
                        loopOption.Stop();
                    }
                });

                return result;
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
                    foreach(var record in block.Units)
                    {
                        // InputValue value is null or InputValue.Kind is null means that a token is not target to parsing. (ex : " ", "\r", "\n", etc)
                        if (record.InputValue == null || record.InputValue.Kind == null) continue;

                        var param1 = Convert.ToString(record.BeforeStack.Reverse(), " ");
                        var param2 = record.InputValue.ToString();
                        var param3 = record.Action.ToString() + " ";
                        var param4 = record.RecoveryMessage;
                        var param5 = Convert.ToString(record.AfterStack.Reverse(), " ");

                        if (record.IsError) param3 += record.ErrorMessage;
//                        else if (record.Action.Direction != ActionDir.accept)
//                            param3 += (record.Action.Dest is NonTerminalSingle) ? (record.Action.Dest as NonTerminalSingle).ToGrammarString() : string.Empty;

                        this.AddRow(result, param1, param2, param3, param4, param5);
                    }
                }

                return result;
            }
        }

        public TreeSymbol ToParseTree
        {
            get
            {
                TreeSymbol result = null;
                if (this.Success == false) return result;

                return this.Last().Units.Last().AfterStack.SecondItemPeek() as TreeSymbol;
            }
        }

        public TreeSymbol ToAST
        {
            get
            {
                var rootTree = this.ToParseTree;
                if (rootTree == null) return rootTree;

                return this.CreateAst(null, rootTree);
            }
        }

        private TreeNonTerminal CreateAst(TreeSymbol newParentTree, TreeSymbol curTree)
        {
            TreeNonTerminal result = null;

            if (curTree is TreeTerminal)
            {
                var convertedParentTree = curTree as TreeTerminal;
                if (convertedParentTree.Token.Kind.Meaning)
                    (newParentTree as TreeNonTerminal).Add(convertedParentTree);
            }
            else
            {
                var convertedParentTree = curTree as TreeNonTerminal;
                if (convertedParentTree._signPost.MeaningUnit != null)
                {
                    result = new TreeNonTerminal(convertedParentTree._signPost);

                    if (newParentTree == null) newParentTree = result;
                    else if(newParentTree != result)
                    {
                        (newParentTree as TreeNonTerminal).Add(result);
                        newParentTree = result;
                    }
                }

                // it can't use Parallel because order.
                foreach (var node in convertedParentTree) this.CreateAst(newParentTree, node);
            }

            return result;
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
        /// This function returns it after adding a new ParsingUnit on current ParsingBlock.
        /// </summary>
        /// <param name="blockIndex"></param>
        /// <returns></returns>
        internal ParsingUnit AddUnitOnCurBlock(int blockIndex)
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

        /// <summary>
        /// This function returns ParsingBlock that can be parsing and exist anterior from the block of the current index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ParsingBlock GetFrontBlockCanParse(int index)
        {
            ParsingBlock result = null;
            if (index <= 0) return result;

            for (int i = index - 1; i >= 0; i--)
            {
                if (this[i].Token.Kind == null) continue;

                result = this[i];
                break;
            }

            return result;
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
