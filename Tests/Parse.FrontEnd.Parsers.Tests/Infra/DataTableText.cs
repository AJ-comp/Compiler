using System.Data;
using System.Linq;
using System.Text;

namespace Parse.FrontEnd.Parsers.Tests.Infra;

/// <summary>
/// Serializes a <see cref="DataTable"/> (the format the engine already uses for parsing
/// tables, FIRST/FOLLOW, canonical sets and ambiguity reports) into a stable, diff-able
/// text block for snapshotting. Self-contained so it does not depend on the production
/// CSV writer.
/// </summary>
internal static class DataTableText
{
    public static string ToText(DataTable table)
    {
        var sb = new StringBuilder();

        sb.Append(string.Join(" | ", table.Columns.Cast<DataColumn>().Select(c => c.ColumnName)));
        sb.Append('\n');

        foreach (DataRow row in table.Rows)
        {
            var cells = row.ItemArray.Select(v => (v == null ? string.Empty : v.ToString())
                                                      .Replace("\r", " ")
                                                      .Replace("\n", " "));
            sb.Append(string.Join(" | ", cells));
            sb.Append('\n');
        }

        return sb.ToString();
    }
}
