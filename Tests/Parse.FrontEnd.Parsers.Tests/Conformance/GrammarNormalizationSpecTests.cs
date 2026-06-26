using System.Linq;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// TARGET SPEC for the GrammarNormalization pass (optional-absorb). Once GN expands a
/// directly-written optional (`const?`) into explicit alternatives during normalization,
/// the NATURAL grammar must parse under PLAIN LALR (no backtracking) with zero conflicts —
/// exactly like the hand-expanded <see cref="ConstExpandedDeclExprGrammar"/> already does.
///
/// These are RED today: <see cref="PaperConflictDeclExprGrammar"/> uses `.Optional()` and
/// still carries the epsilon, so plain LALR conflicts and the const-less declaration fails.
/// Skipped until GN is implemented; unskip when it lands.
/// </summary>
[Trait("Category", "GrammarNormalization")]
public class GrammarNormalizationSpecTests
{
    private static int ConflictCount()
    {
        var g = new PaperConflictDeclExprGrammar();
        g.NormalizeOptionals();
        return new LALRParser(g, false).CheckAmbiguity()
                   .Count(i => i.IsShiftReduceConflict || i.IsReduceReduceConflict);
    }

    private static ParsingResult ParsePlain(string input)
    {
        var g = new PaperConflictDeclExprGrammar();
        g.NormalizeOptionals();
        var lexer = new Lexer();
        foreach (var t in g.TerminalSet) lexer.AddTokenRule(t);
        return new LALRParser(g, false).Parsing(lexer.Lexing(input).TokensForParsing);
    }

    [Fact]
    public void Normalized_optional_grammar_has_no_conflict_under_plain_LALR()
        => Assert.Equal(0, ConflictCount());

    [Theory]
    [InlineData("a b ;")]            // const-less declaration (the one that fails before GN)
    [InlineData("const a b ;")]      // const declaration
    [InlineData("a = b ;")]          // expression
    public void Normalized_optional_grammar_parses_under_plain_LALR(string input)
        => Assert.True(ParsePlain(input).Success, $"plain LALR should ACCEPT after GN: \"{input}\"");
}
