using Parse.Extensions;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Data;
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
            get
            {
                bool result = true;

                Parallel.ForEach(this, (block, loopOption) =>
                {
                    if (block.ErrorUnits.Count > 0)
                    {
                        result = false;
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
                this.AddColumn(result, "current stack");

                foreach (var block in this)
                {
                    foreach(var record in block.Units)
                    {
                        // InputValue value is null means that a token is not target to parsing. (ex : " ", "\r", "\n", etc)
                        if (record.InputValue == null) continue;

                        var param1 = Convert.ToString(record.BeforeStack.Reverse(), " ");
                        var param2 = record.InputValue.ToString();
                        var param3 = record.Action.Direction.ToString() + " ";
                        var param4 = Convert.ToString(record.AfterStack.Reverse(), " ");

                        if (record.Action.Direction == ActionDir.failed)
                        {
                            param3 += record.ErrorMessage;
                            param4 += string.Empty;
                        }
                        else if (record.Action.Direction != ActionDir.accept)
                            param3 += (record.Action.Dest is NonTerminalSingle) ? (record.Action.Dest as NonTerminalSingle).ToGrammarString() : record.Action.Dest.ToString();

                        this.AddRow(result, param1, param2, param3, param4);
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
