using System.Collections.Generic;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Tokenize;
using Xunit;
using static Janglim.FrontEnd.Grammars.Ebnf.EbnfSym;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// Validates that a grammar built DYNAMICALLY (from an EbnfModel, not from C# fields) actually drives
/// the engine — registers its symbols, wires its productions, and parses real input. This de-risks the
/// dynamic-construction path (working around the base Grammar's field-reflection) before the EBNF text
/// reader is built on top of it.
/// </summary>
[Trait("Category", "Ebnf")]
public class EbnfGrammarTests
{
    // E : E '+' T | T ;   T : T '*' F | F ;   F : '(' E ')' | id ;   id := "[a-zA-Z]+" ;
    private static EbnfGrammar Arithmetic()
    {
        var model = new EbnfModel();
        model.Tokens.Add(new EbnfTokenDef { Name = "id", Pattern = "[a-zA-Z]+" });

        var e = new EbnfRuleDef { Name = "E" };
        e.Alternatives.Add(new List<EbnfSym> { Ref("E"), Literal("+"), Ref("T") });
        e.Alternatives.Add(new List<EbnfSym> { Ref("T") });

        var t = new EbnfRuleDef { Name = "T" };
        t.Alternatives.Add(new List<EbnfSym> { Ref("T"), Literal("*"), Ref("F") });
        t.Alternatives.Add(new List<EbnfSym> { Ref("F") });

        var f = new EbnfRuleDef { Name = "F" };
        f.Alternatives.Add(new List<EbnfSym> { Literal("("), Ref("E"), Literal(")") });
        f.Alternatives.Add(new List<EbnfSym> { Ref("id") });

        model.Rules.Add(e);
        model.Rules.Add(t);
        model.Rules.Add(f);

        return new EbnfGrammar(model);
    }

    [Theory]
    [InlineData("a + a * a")]
    [InlineData("( a + a ) * a")]
    [InlineData("a")]
    public void Dynamic_grammar_parses_valid_input(string input)
    {
        var g = Arithmetic();
        var lexer = new Lexer();
        foreach (var term in g.TerminalSet) lexer.AddTokenRule(term);

        var result = new LALRParser(g, false).Parsing(lexer.Lexing(input).TokensForParsing);

        Assert.True(result.Success, $"expected to parse: \"{input}\"");
    }

    [Fact]
    public void Dynamic_grammar_rejects_invalid_input()
    {
        var g = Arithmetic();
        var lexer = new Lexer();
        foreach (var term in g.TerminalSet) lexer.AddTokenRule(term);

        var result = new LALRParser(g, false).Parsing(lexer.Lexing("a +").TokensForParsing);

        Assert.False(result.Success);
    }
}
