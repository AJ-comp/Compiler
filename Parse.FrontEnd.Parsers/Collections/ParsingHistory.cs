using Parse.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Data;

namespace Parse.FrontEnd.Parsers.Collections
{
    public class ParsingHistory : DataTable
    {
        public List<NonTerminalSingle> TreeInfo { get; } = new List<NonTerminalSingle>();

        public void AddColumn(string columnName)
        {
            this.Columns.Add(columnName);
            this.Columns[columnName].DefaultValue = string.Empty;
        }

        public void AddColumn(string columnName, Type type)
        {
            this.Columns.Add(columnName);
            this.Columns[columnName].DataType = type;
        }

        public void AddRow(params object[] datas)
        {
            DataRow row = this.NewRow();
            for (int i = 0; i < datas.Length; i++) row[i] = datas[i];
            this.Rows.Add(row);
        }

        public void AddTreeInfo(NonTerminalSingle singleNT)
        {
            this.TreeInfo.Add(singleNT);
        }
    }
}
