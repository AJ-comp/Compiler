using ConsoleTables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Parse.FrontEnd.Tokenize
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class TokenPos
    {
        public int Line { get; set; } = -1;
        public int CharColumn { get; set; } = -1;
        public int EndLine { get; set; } = -1;
        public int EndColumn { get; set; } = -1;


        /// <summary>
        /// The column index of the token unit.
        /// </summary>
        public int TokenColumn { get; set; } = -1;


        public string ToTableFormat()
        {
            Dictionary<string, string> datas = new Dictionary<string, string>
            {
                { "Line", $"{Line}" },
                { "Column", $"{CharColumn}" },
            };

            var table = new ConsoleTable(datas.Keys.ToArray());
            table.AddRow(datas.Values.ToArray());

            return table.ToStringAlternative();
        }

        private string GetDebuggerDisplay() => $"Line: {Line} Column: {CharColumn} EndLine: {EndLine} EndColumn: {EndColumn}";
    }
}
