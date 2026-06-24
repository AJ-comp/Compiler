using System.Linq;
using Parse.FrontEnd.Grammars.ExampleGrammars;
using Parse.FrontEnd.Parsers.Tests.Infra;
using Xunit;

namespace Parse.FrontEnd.Parsers.Tests;

/// <summary>
/// COMMON (parser-independent): the grammar itself — what the C# EBNF DSL builds.
/// No parser is involved.
/// </summary>
[Trait("Category", "Common")]
public class GrammarTests
{
    [Fact]
    public void Ex1_builds_and_exposes_root()
    {
        var grammar = new Ex1Grammar();
        Assert.NotNull(grammar.EbnfRoot);
    }

    [Fact]
    public void Ex1_exposes_its_terminals()
    {
        var terminals = new Ex1Grammar().TerminalSet.Select(t => t.Value).ToHashSet();
        foreach (var v in new[] { "a", "e", ".", "=", "[", "]" })
            Assert.Contains(v, terminals);
    }

    [Fact]
    public void Ex1_ebnf_matches_snapshot()
    {
        var grammar = new Ex1Grammar();
        Snapshot.Match("Ex1.ebnf", string.Join("\n", grammar.ToEbnfString()));
    }
}
