using System.Linq;
using Parse.FrontEnd.Grammars.ExampleGrammars;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Tokenize;
using Xunit;

namespace Parse.FrontEnd.Parsers.Tests;

/// <summary>
/// ParsingResult.Trace is the flat, strongly-typed view of the parse (the data-oriented counterpart of
/// ToParsingHistory). It must mirror result.Logger one-for-one, expose each step's action as a typed
/// ParseAction, and model the LR stack as state-with-symbol-below frames rooted at state 0.
/// </summary>
[Trait("Category", "Introspection")]
public class ParseTraceTests
{
    private static ParsingResult Parse(string input)
    {
        var g = new Ex8_10Grammar();
        var lexer = new Lexer();
        foreach (var t in g.TerminalSet) lexer.AddTokenRule(t);
        return new LALRParser(g, bLogging: true).Parsing(lexer.Lexing(input).TokensForParsing);
    }

    [Fact]
    public void Trace_mirrors_logger_and_exposes_typed_steps()
    {
        var result = Parse("a + a * a");
        Assert.True(result.Success);

        var trace = result.Trace;

        // one step per logger record, in order
        Assert.Equal(result.Logger.Count, trace.Steps.Count);
        for (int i = 0; i < trace.Steps.Count; i++)
        {
            Assert.Equal(i, trace.Steps[i].Index);
            // the typed action matches the kind of the raw ActionData.Action at the same step
            Assert.Equal(result.Logger[i].Unit.Action.Action?.GetType(), trace.Steps[i].Action?.GetType());
        }

        // the real action kinds all show up, and the parse ends by accepting
        Assert.Contains(trace.Steps, s => s.Action is ParseAction.Shift);
        Assert.Contains(trace.Steps, s => s.Action is ParseAction.Reduce);
        Assert.Contains(trace.Steps, s => s.Action is ParseAction.Goto);
        Assert.Contains(trace.Steps, s => s.Action is ParseAction.Accept);
    }

    [Fact]
    public void Stack_frames_are_rooted_at_state_0_with_no_symbol()
    {
        var trace = Parse("a + a * a").Trace;

        Assert.All(trace.Steps, s =>
        {
            Assert.NotEmpty(s.StackBefore);
            Assert.Equal(0, s.StackBefore[0].State);   // base frame is the start state
            Assert.Null(s.StackBefore[0].Symbol);      // ... with no symbol below it
        });
    }
}
