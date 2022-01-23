using AJ.Common.Helpers;
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

                result.CreateColumns(_prevStatus, _markSymbol, _currentStatus, _markListColumn, _follow, _lookAhead);
                this.CreateRows(result);

                return result;
            }
        }


        private void CreateRows(DataTable dataTable)
        {
            foreach (var item in this)
            {
                DataRow row = dataTable.NewRow();

                row[_prevStatus] = $"I{item.CanonicalLine.PrevStatusIndex}";
                row[_markSymbol] = item.CanonicalLine.SeeingMarkSymbol;
                row[_currentStatus] = $"I{item.CanonicalLine.CurrentStatusIndex}";

                foreach (var canonicalItem in item.CanonicalLine.CurrentCanonical)
                {
                    row[_markListColumn] = canonicalItem.ToString();
                    row[_follow] = canonicalItem.Follow;
                    row[_lookAhead] = canonicalItem.LookAhead;

                    dataTable.Rows.Add(row);
                    row = dataTable.NewRow();
                }

                row[_prevStatus] = "result";
                row[_markListColumn] = (item.Result) ? "no conflict" : item.AmbiguityContent;

                dataTable.Rows.Add(row);

                row = dataTable.NewRow();
                dataTable.Rows.Add(row);
            }
        }


        private string _prevStatus = "prev status";
        private string _markSymbol = "seeing mark symbol";
        private string _currentStatus = "current status";
        private string _markListColumn = "mark list & analysis result";
        private string _follow = "follow";
        private string _lookAhead = "lookahead";
        //        private string _analysisResultColumn = "analysis result";
    }



    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class AmbiguityCheckItem
    {
        public CanonicalLine CanonicalLine { get; }
        public bool Result => !IsReduceReduceConflict && !IsShiftReduceConflict;
        public string AmbiguityContent
        {
            get
            {
                if (Result) return "no conflict";

                string result = string.Empty;

                if (IsReduceReduceConflict) result = "reduce-reduce conflict";
                if (IsShiftReduceConflict) result += "shift-reduce conflict";

                return result;
            }
        }

        public bool IsReduceReduceConflict { get; }
        public bool IsShiftReduceConflict { get; }


        public AmbiguityCheckItem(CanonicalLine line, ReduceParameter reduceParameter)
        {
            CanonicalLine = line;
            _reduceParameter = reduceParameter;

            var state = CanonicalLine.CurrentCanonical;
            IsReduceReduceConflict = state.IsReduceReduceConflict(_reduceParameter);
            IsShiftReduceConflict = state.IsShiftReduceConflict(_reduceParameter);
        }

        public string GetDebuggerDisplay() => $"{AmbiguityContent}:{CanonicalLine.GetDebuggerDisplay()}";


        private ReduceParameter _reduceParameter;
    }
}
