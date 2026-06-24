using Parse.FrontEnd.Grammars.ExampleGrammars;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Parsers.Tests.Infra;
using Xunit;

namespace Parse.FrontEnd.Parsers.Tests;

/// <summary>
/// VARIANT-specific (SLR): SLR's own canonical sets, table and ambiguity report.
/// NOTE: SLR (C0) is currently BROKEN — it builds reduce-less tables (see
/// <see cref="SlrKnownIssueTests"/>). These snapshots therefore pin the current (buggy)
/// output; they exist to detect change and will flip when SLR is fixed.
/// </summary>
[Trait("Category", "Variant")]
public class SlrTableTests
{
    private static SLRParser NewParser() => new SLRParser(new Ex1Grammar(), false);

    [Fact]
    public void Parsing_table_matches_snapshot()
        => Snapshot.Match("Ex1.slr.parsing-table", DataTableText.ToText(NewParser().ParsingTable.ToTableFormat));

    [Fact]
    public void Canonical_matches_snapshot()
        => Snapshot.Match("Ex1.slr.canonical", DataTableText.ToText(NewParser().Canonical.ToDataTable()));

    [Fact]
    public void Ambiguity_matches_snapshot()
        => Snapshot.Match("Ex1.slr.ambiguity", DataTableText.ToText(NewParser().CheckAmbiguity().ToTableFormat));
}
