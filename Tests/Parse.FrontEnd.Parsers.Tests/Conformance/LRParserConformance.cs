using System.Linq;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.ExampleGrammars;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Tokenize;
using Xunit;
using static Parse.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Parse.FrontEnd.Parsers.Tests;

/// <summary>
/// CONFORMANCE: behavior that EVERY LR parser variant must satisfy identically — parse the
/// same language, detect the same conflicts, walk the input the same way. Written once here;
/// each variant gets a concrete subclass supplying <see cref="CreateParser"/>. Adding a
/// subclass for SLR/CLR/LL (once correct) automatically runs this whole suite against it,
/// so every parser is verified the same way without duplicating tests.
/// </summary>
public abstract class LRParserConformance
{
    protected abstract LRParser CreateParser(Grammar grammar, bool logging);

    private ParsingResult Parse(Grammar grammar, string input, bool logging = false)
    {
        var lexer = new Lexer();
        foreach (var t in grammar.TerminalSet) lexer.AddTokenRule(t);
        return CreateParser(grammar, logging).Parsing(lexer.Lexing(input).TokensForParsing);
    }

    private int ConflictCount(Grammar grammar)
        => CreateParser(grammar, false).CheckAmbiguity()
               .Count(i => i.IsShiftReduceConflict || i.IsReduceReduceConflict);

    // ---- parsing: accept / reject ----

    [Theory]
    [InlineData("a = e")]
    [InlineData("a . a = e")]
    [InlineData("a [ e ] = e")]
    [InlineData("a [ e e ] = e")]
    [InlineData("a . a . a = e")]
    public void Accepts_valid_Ex1_sentences(string input)
        => Assert.True(Parse(new Ex1Grammar(), input).Success, $"expected ACCEPT: \"{input}\"");

    [Theory]
    [InlineData("a =")]
    [InlineData("a e")]
    [InlineData("= e")]
    [InlineData("a [ e = e")]
    public void Rejects_invalid_Ex1_sentences(string input)
        => Assert.False(Parse(new Ex1Grammar(), input).Success, $"expected REJECT: \"{input}\"");

    [Theory]
    [InlineData("a")]
    [InlineData("a + a")]
    [InlineData("a + a + a")]
    public void Parses_left_recursive_grammar(string input)
        => Assert.True(Parse(new SelfRefExprGrammar(), input).Success, $"expected ACCEPT: \"{input}\"");

    // ---- conflict detection ----

    [Fact]
    public void Reports_conflict_for_ambiguous_grammar()
        => Assert.True(ConflictCount(new SelfRefExprGrammar()) > 0,
            "an ambiguous grammar (S -> S + S | a) must report a conflict");

    [Fact]
    public void Reports_no_conflict_for_unambiguous_grammar()
        => Assert.Equal(0, ConflictCount(new Ex1Grammar()));

    // ---- table is built ----

    [Fact]
    public void Builds_a_nonempty_parsing_table()
    {
        var parser = CreateParser(new Ex1Grammar(), false);
        Assert.True(parser.ParsingTable.AllSymbols.Any(), "parsing table has no symbols");
        Assert.True(parser.Canonical.ToCanonicalLineList().Any(), "canonical collection is empty");
    }

    // ---- parsing process (logging) ----

    [Fact]
    public void Parsing_with_logging_records_a_trace_ending_in_accept()
    {
        var result = Parse(new Ex1Grammar(), "a = e", logging: true);
        Assert.True(result.Success);

        var history = result.ToParsingHistory;
        Assert.True(history.Rows.Count > 0, "logging should record parse steps");
        var lastRow = history.Rows[history.Rows.Count - 1].ItemArray;
        Assert.Contains("accept", string.Join(" ", lastRow).ToLowerInvariant());
    }

    /// <summary>
    /// Robust process check: the SEQUENCE OF ACTION TYPES (ActionDir enum) must be the same
    /// for any correct LR parser. Immune to state renumbering / formatting / localization.
    /// </summary>
    [Fact]
    public void Action_sequence_for_a_equals_e_is_stable()
    {
        var result = Parse(new Ex1Grammar(), "a = e", logging: true);
        Assert.True(result.Success);

        var actions = result.Logger.Select(h => h.Unit.Action.Direction).ToArray();
        Assert.Equal(new[]
        {
            ActionDir.Shift,          // a
            ActionDir.EpsilonReduce,  // B -> epsilon
            ActionDir.Goto,           // B
            ActionDir.Shift,          // =
            ActionDir.Shift,          // e
            ActionDir.Reduce,         // A -> a B = e
            ActionDir.Goto,           // A
            ActionDir.Accept,
        }, actions);
    }
}
