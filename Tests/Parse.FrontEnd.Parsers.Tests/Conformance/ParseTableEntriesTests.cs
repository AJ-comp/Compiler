using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.Grammars.ExampleGrammars;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// IParsingTable.Entries is the flat, strongly-typed, cast-free view of the parse table. It must be
/// reachable straight off the interface (no cast), expose only real actions (no null), and faithfully
/// mirror a manual walk of the underlying rows.
/// </summary>
[Trait("Category", "Introspection")]
public class ParseTableEntriesTests
{
    private static List<ParseTableEntry> Entries()
        => new LALRParser(new Ex1Grammar(), false).ParsingTable.Entries.ToList();   // no cast needed

    [Fact]
    public void Entries_have_no_null_actions_and_cover_all_kinds()
    {
        var entries = Entries();

        Assert.NotEmpty(entries);
        Assert.All(entries, e => Assert.NotNull(e.Action));
        Assert.Contains(entries, e => e.Action is ParseAction.Shift);
        Assert.Contains(entries, e => e.Action is ParseAction.Reduce);
        Assert.Contains(entries, e => e.Action is ParseAction.Goto);
        Assert.Contains(entries, e => e.Action is ParseAction.Accept);
    }

    [Fact]
    public void Entries_equal_a_manual_walk_of_the_real_actions()
    {
        var table = (LRParsingTable)new LALRParser(new Ex1Grammar(), false).ParsingTable;

        var manual = table
            .SelectMany(row => row.MatchedValueSet.Values)
            .SelectMany(cell => cell)
            .Count(a => a.Action != null);

        Assert.Equal(manual, Entries().Count);
    }
}
