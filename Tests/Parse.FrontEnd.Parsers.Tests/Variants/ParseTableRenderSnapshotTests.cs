using System.Text.RegularExpressions;
using Parse.FrontEnd.Grammars.ExampleGrammars;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Parsers.Tests.Infra;
using Xunit;

namespace Parse.FrontEnd.Parsers.Tests;

/// <summary>
/// Characterization snapshots of ParsingTable.ToTableFormat across several grammars, used as the
/// equivalence baseline for refactoring the renderer onto the shared ActionData.Action projection.
/// Ex8_10 exercises shift/reduce/goto/accept over an arithmetic grammar; PaperConflict (which keeps
/// its const? optional, AbsorbOptionals=false) exercises epsilon-reduce and shift/reduce conflict
/// cells that hold more than one action. If the refactor preserves behavior, these stay byte-identical.
/// </summary>
[Trait("Category", "Variant")]
public class ParseTableRenderSnapshotTests
{
    // Auto-generated nonterminals are named G<n> from a PROCESS-GLOBAL counter, so the exact number
    // shifts with test order/count across a run. Normalize it (G<n> -> G#) so the snapshot pins the
    // table SHAPE, not the non-deterministic counter. (The counter's non-determinism itself is a
    // separate static-state item, tracked apart from this rendering test.)
    private static string Normalize(string tableText)
        => Regex.Replace(tableText, @"\bG\d+\b", "G#");

    [Fact]
    public void Ex8_10_parsing_table_matches_snapshot()
        => Snapshot.Match("Ex8_10.lalr.parsing-table",
            Normalize(DataTableText.ToText(new LALRParser(new Ex8_10Grammar(), false).ParsingTable.ToTableFormat)));

    [Fact]
    public void PaperConflict_parsing_table_matches_snapshot()
        => Snapshot.Match("PaperConflict.lalr.parsing-table",
            Normalize(DataTableText.ToText(new LALRParser(new PaperConflictDeclExprGrammar(), false).ParsingTable.ToTableFormat)));
}
