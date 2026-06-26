using System.Linq;
using Janglim.FrontEnd.Grammars.ExampleGrammars;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Xunit;
using static Janglim.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// Exhaustive equivalence proof for the renderer refactor: LRParsingTable.FormatActionCell (which now
/// renders each cell through the shared ActionData.Action projection) must produce the EXACT string the
/// old inline formula produced, for EVERY ActionDir — including Failed / NotProcessed, which no grammar
/// emits so the table snapshots can never reach them. Combined with the byte-identical multi-grammar
/// table snapshots, this pins the renderer as behavior-identical.
/// </summary>
[Trait("Category", "Introspection")]
public class ParseActionCellFormatTests
{
    // The exact pre-refactor formula, preserved here verbatim as the reference oracle.
    private static string OldFormula(ActionData a)
    {
        var moveInfo = a.Direction.ToString();
        var destInfo = (a.Dest is NonTerminalSingle) ? ((NonTerminalSingle)a.Dest).ToGrammarString()
                                                      : (a.Dest as int?).ToString();
        return moveInfo + $" [{destInfo}]";
    }

    // A real production object (NonTerminalSingle) lifted from a built table, for the reduce/accept cases.
    private static NonTerminalSingle SomeProduction()
        => (NonTerminalSingle)((LRParsingTable)new LALRParser(new Ex8_10Grammar(), false).ParsingTable)
            .SelectMany(r => r.MatchedValueSet.Values)
            .SelectMany(c => c)
            .First(a => a.Dest is NonTerminalSingle).Dest;

    [Fact]
    public void FormatActionCell_equals_the_old_formula_for_every_direction()
    {
        var prod = SomeProduction();

        var cases = new[]
        {
            new ActionData(ActionDir.Shift, 4),
            new ActionData(ActionDir.Goto, 9),
            new ActionData(ActionDir.Reduce, prod),
            new ActionData(ActionDir.EpsilonReduce, prod),
            new ActionData(ActionDir.Accept, prod),
            new ActionData(ActionDir.Failed, new object()),   // Failed carries an error handler, not a state/production
            new ActionData(ActionDir.NotProcessed, null),
        };

        foreach (var a in cases)
            Assert.Equal(OldFormula(a), LRParsingTable.FormatActionCell(a));
    }
}
