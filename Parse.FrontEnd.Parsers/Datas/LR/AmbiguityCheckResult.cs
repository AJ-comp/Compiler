using Parse.FrontEnd.Parsers.Collections;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace Parse.FrontEnd.Parsers.Datas.LR
{
    public class AmbiguityCheckResult : List<AmbiguityCheckItem>
    {

        public DataTable ToTableFormat
        {
            get
            {
                DataTable result = new DataTable();

                this.CreateColumns(result);
                this.CreateRows(result);

                return result;
            }
        }


        private void CreateColumns(DataTable dataTable)
        {
            DataColumn column1 = new DataColumn
            {
                DataType = typeof(string),
                ColumnName = _prevStatusColumn,
                Caption = _prevStatusColumn,
                ReadOnly = true,
                DefaultValue = ""
            };
            dataTable.Columns.Add(column1);

            DataColumn column2 = new DataColumn
            {
                DataType = typeof(string),
                ColumnName = _markSymbolColumn,
                Caption = _markSymbolColumn,
                ReadOnly = true,
                DefaultValue = ""
            };
            dataTable.Columns.Add(column2);

            DataColumn column3 = new DataColumn
            {
                DataType = typeof(string),
                ColumnName = _currentStatusColumn,
                Caption = _currentStatusColumn,
                ReadOnly = true,
                DefaultValue = ""
            };
            dataTable.Columns.Add(column3);

            DataColumn column4 = new DataColumn
            {
                DataType = typeof(string),
                ColumnName = _markListColumn,
                Caption = _markListColumn,
                ReadOnly = true,
                DefaultValue = ""
            };
            dataTable.Columns.Add(column4);
        }

        private void CreateRows(DataTable dataTable)
        {
            foreach (var item in this)
            {
                DataRow row = dataTable.NewRow();

                row[_prevStatusColumn] = $"I{item.CanonicalLine.PrevStatusIndex}";
                row[_markSymbolColumn] = item.CanonicalLine.SeeingMarkSymbol;
                row[_currentStatusColumn] = $"I{item.CanonicalLine.CurrentStatusIndex}";

                foreach (var canonicalItem in item.CanonicalLine.CurrentCanonical)
                {
                    row[_markListColumn] = canonicalItem.ToString();

                    dataTable.Rows.Add(row);
                    row = dataTable.NewRow();
                }

                row[_prevStatusColumn] = "result";
                row[_markListColumn] = (item.Result) ? "no conflict" : item.AmbiguityContent;

                dataTable.Rows.Add(row);

                row = dataTable.NewRow();
                dataTable.Rows.Add(row);
            }
        }


        private string _prevStatusColumn = "prev status";
        private string _markSymbolColumn = "seeing mark symbol";
        private string _currentStatusColumn = "current status";
        private string _markListColumn = "mark list & analysis result";
//        private string _analysisResultColumn = "analysis result";
    }


    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class AmbiguityCheckItem
    {
        public CanonicalLine CanonicalLine { get; internal set; }
        public bool Result => string.IsNullOrEmpty(AmbiguityContent);
        public string AmbiguityContent { get; internal set; }

        public string GetDebuggerDisplay() => $"{AmbiguityContent}:{CanonicalLine.GetDebuggerDisplay()}";
    }
}
