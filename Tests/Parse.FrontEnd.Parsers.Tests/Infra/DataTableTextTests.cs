using System.Data;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.Infra;

public class DataTableTextTests
{
    [Theory]
    [InlineData("\r\n")]
    [InlineData("\n")]
    [InlineData("\r")]
    public void Cell_newlines_have_the_same_snapshot_representation(string newline)
    {
        var table = new DataTable();
        table.Columns.Add("Payload");
        table.Rows.Add("before" + newline + "after");

        Assert.Equal("Payload\nbefore  after\n", DataTableText.ToText(table));
    }
}
