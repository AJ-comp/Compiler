using System.Linq;
using Parse.FrontEnd.Grammars.ExampleGrammars;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.Datas.LR;
using Parse.FrontEnd.Parsers.LR;
using Xunit;

namespace Parse.FrontEnd.Parsers.Tests;

/// <summary>
/// The additive typed projection <see cref="ActionData.Action"/> must faithfully mirror the legacy
/// <c>Direction</c> + untyped <c>Dest</c> for EVERY cell of the parse table, and the table must exercise
/// the real action kinds. This is the read-side typed API; it changes no behavior (the snapshot tests in
/// LalrTableTests prove the rendering is untouched).
/// </summary>
[Trait("Category", "Introspection")]
public class ParseActionTypedTests
{
    private static LRParsingTable Table()
        => (LRParsingTable)new LALRParser(new Ex1Grammar(), false).ParsingTable;

    [Fact]
    public void Typed_action_mirrors_direction_and_dest_for_every_cell()
    {
        foreach (var row in Table())
            foreach (var cell in row.MatchedValueSet)
                foreach (var a in cell.Value)
                {
                    switch (a.Direction)
                    {
                        case LRParsingRowDataFormat.ActionDir.Shift:
                            Assert.Equal((int)a.Dest, Assert.IsType<ParseAction.Shift>(a.Action).State);
                            break;
                        case LRParsingRowDataFormat.ActionDir.Goto:
                            Assert.Equal((int)a.Dest, Assert.IsType<ParseAction.Goto>(a.Action).State);
                            break;
                        case LRParsingRowDataFormat.ActionDir.Reduce:
                            Assert.Same(a.Dest, Assert.IsType<ParseAction.Reduce>(a.Action).Production);
                            Assert.False(((ParseAction.Reduce)a.Action).IsEpsilon);
                            break;
                        case LRParsingRowDataFormat.ActionDir.EpsilonReduce:
                            Assert.Same(a.Dest, Assert.IsType<ParseAction.Reduce>(a.Action).Production);
                            Assert.True(((ParseAction.Reduce)a.Action).IsEpsilon);
                            break;
                        case LRParsingRowDataFormat.ActionDir.Accept:
                            Assert.Same(a.Dest, Assert.IsType<ParseAction.Accept>(a.Action).Production);
                            break;
                        default: // NotProcessed / Failed — not a parse action
                            Assert.Null(a.Action);
                            break;
                    }
                }
    }

    [Fact]
    public void Table_exercises_shift_reduce_goto_and_accept()
    {
        var actions = Table()
            .SelectMany(row => row.MatchedValueSet.Values)
            .SelectMany(list => list)
            .Select(a => a.Action)
            .ToList();

        Assert.Contains(actions, x => x is ParseAction.Shift);
        Assert.Contains(actions, x => x is ParseAction.Reduce);
        Assert.Contains(actions, x => x is ParseAction.Goto);
        Assert.Contains(actions, x => x is ParseAction.Accept);
    }
}
