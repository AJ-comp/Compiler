using ParsingLibrary.Datas;
using ParsingLibrary.Datas.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ParsingLibrary.Parsers.Datas.LRParsingData;

namespace ParsingLibrary.Parsers.Collections
{
    public class ParsingHistory : DataTable
    {

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
    }
}
