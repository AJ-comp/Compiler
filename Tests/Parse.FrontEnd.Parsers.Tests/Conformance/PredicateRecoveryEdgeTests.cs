using System;
using System.Linq;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;
using static Janglim.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Janglim.FrontEnd.Parsers.Tests;

public class PredicateRecoveryEdgeTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Insertion_at_eof_preserves_end_marker_and_reaches_actual_accept(bool explicitEof)
    {
        PredicateContext observed = null;
        var options = new EbnfReaderOptions().RegisterPredicate("After", context =>
        {
            observed = context;
            return context.PreviousToken.Input == "y" && context.IsEndOfInput;
        });
        var grammar = EbnfGrammarReader.Read("S : 'x' 'y' {After()}? ;", options).Grammar;
        var lexer = Lexer(grammar);
        var replacement = TokenData.CreateFromTokenCell(lexer.Lexing("y").TokensForParsing[0], false);
        var parser = new LALRParser(grammar, true);
        parser.AddErrorHandler(new InsertAtEnd(replacement, explicitEof));

        var result = parser.Parsing(lexer.Lexing("x").TokensForParsing);

        Assert.True(result.Success, parser.FiredException?.Message);
        Assert.Equal(ActionDir.Accept, result.Last().Units.Last().Action.Direction);
        Assert.Same(replacement, observed.PreviousToken);
        Assert.Same(result.Last().Token, observed.NextToken);
        Assert.Equal(2, observed.TokenIndex);
        Assert.Equal(new[] { "x", "y" }, result.ToParseTree.AllTokens.Select(token => token.Input));
    }

    [Fact]
    public void A_valid_input_beyond_ten_thousand_tokens_cannot_be_silently_truncated()
    {
        var options = new EbnfReaderOptions().RegisterPredicate("Allow", _ => true);
        // Keep the LR stack bounded with a left-recursive list. The regression concerns
        // the old top-level 10,000-block cutoff, not storage for a deep right-recursive tree.
        var grammar = EbnfGrammarReader.Read("S : {Allow()}? List 'y' ; List : List 'x' | 'x' ;", options).Grammar;
        var lexer = Lexer(grammar);
        var parser = new LALRParser(grammar);
        var result = parser.Parsing(lexer.Lexing(string.Concat(Enumerable.Repeat("x ", 10005)) + "y").TokensForParsing);

        Assert.True(result.Success, parser.FiredException?.Message);
        Assert.Equal(ActionDir.Accept, result.Last().Units.Last().Action.Direction);
        Assert.Equal(10006, result.SelectMany(block => block.Units).Count(unit => unit.Action.Direction == ActionDir.Shift));
    }

    private static Lexer Lexer(EbnfGrammar grammar)
    {
        var lexer = new Lexer();
        foreach (var token in grammar.TerminalSet) lexer.AddTokenRule(token);
        return lexer;
    }

    private sealed class InsertAtEnd : IErrorHandlable
    {
        private readonly TokenData replacement;
        private readonly bool explicitEof;
        public InsertAtEnd(TokenData replacement, bool explicitEof)
        {
            this.replacement = replacement;
            this.explicitEof = explicitEof;
        }
        public ErrorHandlingResult Call(DataForRecovery data)
        {
            Assert.IsType<EndMarker>(data.CurBlock.Token.Kind);
            var infos = new[] { new ParsingRecoveryData(replacement, "insert missing y") }.AsEnumerable();
            if (explicitEof) infos = infos.Append(new ParsingRecoveryData(data.CurBlock.Token, "finish"));
            var status = ((LRParser)data.Parser).RecoveryBlockParsing(data, infos);
            return data.ToErrorHandlingResult(status == LRParser.SuccessedKind.Completed);
        }
    }
}
