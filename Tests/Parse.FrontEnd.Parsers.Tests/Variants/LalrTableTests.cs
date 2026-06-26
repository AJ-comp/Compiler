using Janglim.FrontEnd.Grammars.ExampleGrammars;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Parsers.Tests.Infra;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// VARIANT-specific (LALR): the canonical item sets, ACTION/GOTO table and parse-trace
/// rendering are particular to the LALR (LALRC1) algorithm — these differ from SLR by
/// construction. Characterization snapshots that pin LALR's current output.
/// </summary>
[Trait("Category", "Variant")]
public class LalrTableTests
{
    private static LALRParser NewParser() => new LALRParser(new Ex1Grammar(), false);

    [Fact]
    public void Parsing_table_matches_snapshot()
        => Snapshot.Match("Ex1.lalr.parsing-table", DataTableText.ToText(NewParser().ParsingTable.ToTableFormat));

    [Fact]
    public void Canonical_matches_snapshot()
        => Snapshot.Match("Ex1.lalr.canonical", DataTableText.ToText(NewParser().Canonical.ToDataTable()));

    [Fact]
    public void Parse_trace_string_matches_snapshot()
    {
        var grammar = new Ex1Grammar();
        var lexer = new Lexer();
        foreach (var t in grammar.TerminalSet) lexer.AddTokenRule(t);
        var result = new LALRParser(grammar, true).Parsing(lexer.Lexing("a = e").TokensForParsing);

        Snapshot.Match("Ex1.lalr.parse-trace.a-equals-e", DataTableText.ToText(result.ToParsingHistory));
    }
}
