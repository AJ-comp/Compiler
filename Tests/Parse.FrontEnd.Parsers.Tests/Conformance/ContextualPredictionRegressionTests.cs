using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>Regression cases for conditions reached only after the prediction paths tokenize differently.</summary>
[Trait("Category", "ContextualLexing")]
public class ContextualPredictionRegressionTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void A_false_future_predicate_removes_only_its_own_tokenization(bool allowRegex, bool allowDivision)
    {
        var observations = new List<(string Branch, PredicateContext Context, bool Allowed)>();
        var options = new EbnfReaderOptions().RegisterPredicate("Allow", _ => true)
            .RegisterPredicate("AfterRegex", context =>
            {
                observations.Add(("regex", context, allowRegex));
                return allowRegex;
            })
            .RegisterPredicate("AfterDiv", context =>
            {
                observations.Add(("division", context, allowDivision));
                return allowDivision;
            });
        var read = EbnfGrammarReader.Read("""
            S : 'p' {Allow()}? 'x' rx {AfterRegex()}? ';'
              | 'p' 'x' '/' id '/' id {AfterDiv()}? ';' ;
            id := Identifier "[a-z]+" ;
            rx := StringLiteral "/[a-z]+/[a-z]*" ;
            comment := Comment "/\*[\s\S]*?\*/" ;
            """, options);
        Assert.True(read.Success, string.Join("; ", read.Errors));
        var grammar = read.Grammar;
        var regexTerminal = Assert.Single(grammar.TerminalSet, terminal => terminal.Caption == "rx");
        var terminals = grammar.TerminalSet.Where(terminal => terminal is not EndMarker && terminal is not Epsilon).ToArray();
        var common = terminals.Where(terminal => terminal != regexTerminal && terminal.Value != "/").ToArray();
        var division = new LexicalGoal(common.Concat(terminals.Where(terminal => terminal.Value == "/")));
        var regex = new LexicalGoal(common.Concat(new[] { regexTerminal }));
        var lexer = new Lexer();
        foreach (var terminal in grammar.TerminalSet) lexer.AddTokenRule(terminal);
        const string input = "p x /a/g /* gap\n */ ;";
        var source = lexer.CreateTokenSource(input, new LexicalPolicy(division, regex));
        var parser = new LALRParser(grammar, true);

        var result = parser.Parsing(source);

        Assert.True(result.Success, parser.FiredException?.ToString());
        Assert.Null(parser.FiredException);
        Assert.Empty(result.AllErrors);
        var shifted = result.Trace.Steps.Where(step => step.Action is ParseAction.Shift)
            .Select(step => step.Lookahead).ToArray();
        Assert.Equal(allowRegex ? new[] { "p", "x", "/a/g", ";" } : new[] { "p", "x", "/", "a", "/", "g", ";" },
            shifted.Select(token => token.Input));
        Assert.Single(result.Trace.Steps, step => step.Action is ParseAction.Accept);
        Assert.Contains(observations, item => item.Branch == "regex");
        Assert.Contains(observations, item => item.Branch == "division");
        Assert.Contains(observations, item => !item.Allowed);
        Assert.All(observations, item =>
        {
            bool regexBranch = item.Branch == "regex";
            Assert.Equal(regexBranch ? 3 : 6, item.Context.TokenIndex);
            Assert.Equal(regexBranch ? "/a/g" : "g", item.Context.PreviousToken.Input);
            Assert.Equal(";", item.Context.NextToken.Input);
            Assert.Equal(input.IndexOf(';'), item.Context.NextToken.StartIndex);
            Assert.Equal(" /* gap\n */ ", item.Context.NextToken.LeadingTrivia.Text);
            Assert.True(item.Context.NextToken.HasLineBreakBefore);
        });
    }
}
