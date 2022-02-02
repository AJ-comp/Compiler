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
        public int Column { get; set; } = -1;


        public string ToTableFormat()
        {
            Dictionary<string, string> datas = new Dictionary<string, string>
            {
                { "Line", $"{Line}" },
                { "Column", $"{Column}" },
            };

            var table = new ConsoleTable(datas.Keys.ToArray());
            table.AddRow(datas.Values.ToArray());

            return table.ToStringAlternative();
        }

        private string GetDebuggerDisplay() => $"Line: {Line} Column: {Column}";
    }
}
