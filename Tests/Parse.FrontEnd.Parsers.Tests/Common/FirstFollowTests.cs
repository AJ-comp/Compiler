using Parse.FrontEnd.Grammars.ExampleGrammars;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Parsers.Tests.Infra;
using Xunit;

namespace Parse.FrontEnd.Parsers.Tests;

/// <summary>
/// COMMON: FIRST/FOLLOW is a property of the grammar — the same for every LR variant.
/// It is computed here via LALR (the working parser) and pinned as a snapshot.
/// </summary>
[Trait("Category", "Common")]
public class FirstFollowTests
{
    [Fact]
    public void Ex1_first_follow_matches_snapshot()
    {
        var parser = new LALRParser(new Ex1Grammar(), false);
        Snapshot.Match("Ex1.first-follow", DataTableText.ToText(parser.GetFirstAndFollow().ToTableFormat));
    }
}
