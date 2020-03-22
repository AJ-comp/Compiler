using Parse.Extensions;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static Parse.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Parse.FrontEnd.Parsers.Datas
{
    /// <summary>
    /// 
    /// </summary>
    /// <see cref="https://www.lucidchart.com/documents/edit/c96f0bde-4111-4957-bf65-75b56d8074dc/0_0?beaconFlowId=687BBA49A656D177"/>
    public class ParsingResult : List<ParsingBlock>
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

                this.AddColumn(result, "prev stack");
                this.AddColumn(result, "input symbol");
                this.AddColumn(result, "action information");
                this.AddColumn(result, "recovery message");
                this.AddColumn(result, "current stack");

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
                        else if (record.Action.Direction != ActionDir.accept)
                            param3 += (record.Action.Dest is NonTerminalSingle) ? (record.Action.Dest as NonTerminalSingle).ToGrammarString() : string.Empty;

                        this.AddRow(result, param1, param2, param3, param4, param5);
                    }
                }

                return result;
            }
        }

        public IReadOnlyList<NonTerminalSingle> ToParseTree
        {
            get
            {
                List<NonTerminalSingle> result = new List<NonTerminalSingle>();

                foreach (var block in this)
                {
                    foreach (var record in block.Units)
                    {
                        if (record.Action.Direction == ActionDir.reduce)
                            result.Add(record.Action.Dest as NonTerminalSingle);
                    }
                }

                return result;
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
