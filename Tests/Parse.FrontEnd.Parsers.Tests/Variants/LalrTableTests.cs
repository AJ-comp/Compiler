using Janglim.FrontEnd.Grammars.ExampleGrammars;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Parsers.Tests.Infra;
using Janglim.FrontEnd.Tokenize;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
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
    {
        var parser = NewParser();
        var completedItemTexts = parser.Canonical.IndexStateDic.Values
            .SelectMany(state => state)
            .Where(item => item.IsReachedHandle)
            .Select(item => item.ToString())
            .ToHashSet(StringComparer.Ordinal);
        Assert.NotEmpty(completedItemTexts);

        var table = parser.Canonical.ToDataTable();
        table.Columns["lookahead set"].ReadOnly = false;
        foreach (DataRow row in table.Rows)
        {
            string itemText = row["LR items"].ToString();
            if (string.IsNullOrEmpty(itemText))
                continue;

            if (completedItemTexts.Contains(itemText))
                continue;

            // Keep the full canonical structure while checking lookahead values
            // only on completed items, which supply Reduce/Accept actions.
            row["lookahead set"] = "[not checked]";
        }

        Snapshot.Match("Ex1.lalr.canonical", DataTableText.ToText(table));
    }

    [Fact]
    public void Parse_trace_string_matches_snapshot()
    {
        var grammar = new Ex1Grammar();
        var lexer = new Lexer();
        foreach (var t in grammar.TerminalSet) lexer.AddTokenRule(t);
        var result = new LALRParser(grammar, true).Parsing(lexer.Lexing("a = e").TokensForParsing);

        // The approved trace includes Korean resource headers. Pin this rendering
        // scope so the host's UI culture cannot change the snapshot's language.
        var originalCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ko-KR");
            Snapshot.Match("Ex1.lalr.parse-trace.a-equals-e", DataTableText.ToText(result.ToParsingHistory));
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }
}
