using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.LR;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// The parser surfaces grammar conflicts (shift/reduce, reduce/reduce) instead of only resolving them
/// silently: HasConflicts / Conflicts / ConflictReport, plus the opt-in ThrowIfConflicts() hard-stop.
/// </summary>
[Trait("Category", "Conflicts")]
public class ConflictReportingTests
{
    // classic ambiguous expression grammar (no precedence) -> shift/reduce conflicts
    private const string Ambiguous = @"
        E  : E '+' E | E '*' E | id ;
        id := ""[a-z]+"" ;
    ";

    // the same language, unambiguous (layered precedence) -> conflict-free
    private const string Clean = @"
        Expr   : Expr '+' Term | Term ;
        Term   : Term '*' Factor | Factor ;
        Factor : '(' Expr ')' | id ;
        id     := ""[a-zA-Z]+"" ;
    ";

    private static LALRParser ParserFor(string grammar)
    {
        var read = EbnfGrammarReader.Read(grammar);
        Assert.True(read.Success, string.Join("; ", read.Errors));
        return new LALRParser(read.Grammar, false);
    }

    [Fact]
    public void Ambiguous_grammar_reports_conflicts()
    {
        var parser = ParserFor(Ambiguous);

        Assert.True(parser.HasConflicts);
        Assert.NotEmpty(parser.Conflicts);
        Assert.Contains("conflict", parser.ConflictReport);
    }

    [Fact]
    public void Unambiguous_grammar_reports_none()
    {
        var parser = ParserFor(Clean);

        Assert.False(parser.HasConflicts);
        Assert.Empty(parser.Conflicts);
        Assert.Equal("no grammar conflicts", parser.ConflictReport);
    }

    [Fact]
    public void ThrowIfConflicts_throws_only_on_conflict()
    {
        Assert.Throws<GrammarConflictException>(() => ParserFor(Ambiguous).ThrowIfConflicts());

        var clean = ParserFor(Clean);
        Assert.Same(clean, clean.ThrowIfConflicts());   // no throw; returns self for chaining
    }
}
