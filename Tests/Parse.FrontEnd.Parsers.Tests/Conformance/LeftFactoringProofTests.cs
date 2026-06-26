using System.Linq;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// EMPIRICAL PROOF that the LGLR paper's Declare-vs-Expression conflict is removable by
/// left-factoring (see <see cref="LeftFactoredDeclExprGrammar"/>): a PLAIN LALR parser
/// (no <c>UseBackTrackingOnConflict</c>, no GLR) builds a CONFLICT-FREE table and parses
/// both declarations (incl. const, incl. dotted name) and assignments (dotted both sides).
/// If LALR truly "could not tell the two apart", this table would report a conflict and
/// these inputs would not parse deterministically. It does, and they do.
/// </summary>
[Trait("Category", "LeftFactoring")]
public class LeftFactoringProofTests
{
    private static ParsingResult Parse(string input)
    {
        var grammar = new LeftFactoredDeclExprGrammar();
        var lexer = new Lexer();
        foreach (var t in grammar.TerminalSet) lexer.AddTokenRule(t);
        // PLAIN LALR — no backtracking, no GLR.
        return new LALRParser(grammar, false).Parsing(lexer.Lexing(input).TokensForParsing);
    }

    private static int ConflictCount()
    {
        var grammar = new LeftFactoredDeclExprGrammar();
        return new LALRParser(grammar, false).CheckAmbiguity()
                   .Count(i => i.IsShiftReduceConflict || i.IsReduceReduceConflict);
    }

    [Fact]
    public void Plain_LALR_table_has_no_conflicts()
        => Assert.Equal(0, ConflictCount());

    [Theory]
    [InlineData("a = c ;")]               // expression, simple
    [InlineData("a.b = c.d ;")]           // expression, dotted on BOTH sides  (철수.영희 = 철수.영희 ;)
    [InlineData("a.b.c = d.e.f ;")]       // expression, long chains both sides
    public void Accepts_expression(string input)
        => Assert.True(Parse(input).Success, $"expected ACCEPT (expr): \"{input}\"");

    [Theory]
    [InlineData("a b ;")]                 // declaration: type name
    [InlineData("a.b c.d ;")]             // declaration: dotted type AND dotted name
    [InlineData("const a.b c.d ;")]       // const declaration, both dotted (const? 철수.영희 민수.민수 ;)
    public void Accepts_declaration(string input)
        => Assert.True(Parse(input).Success, $"expected ACCEPT (decl): \"{input}\"");

    [Theory]
    [InlineData("a.b = ;")]               // expression missing rhs
    [InlineData("= c.d ;")]               // expression missing lhs
    [InlineData("a.b c.d e.f ;")]         // declaration with too many chains
    public void Rejects_malformed(string input)
        => Assert.False(Parse(input).Success, $"expected REJECT: \"{input}\"");
}
