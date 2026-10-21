using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Janglim.FrontEnd;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

[Trait("Category", "ContextualLexing")]
public class ParserDrivenRegressionTests
{
    private const string Arithmetic = """
        S : Expr ;
        Expr : Expr '+' Term | Expr '-' Term | Term ;
        Term : Term '*' Atom | Atom ;
        Atom : num | '(' Expr ')' ;
        num := NumberLiteral "[0-9]+" ;
        comment := Comment "/\*[\s\S]*?\*/" ;
        """;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Source_and_list_inputs_preserve_tree_shape_precedence_and_errors_with_or_without_logging(bool logging)
    {
        var grammar = Read(Arithmetic);
        var parser = new LALRParser(grammar, logging);
        var lexer = LexerFor(grammar);
        var policy = new LexicalPolicy(new LexicalGoal(Scannable(grammar)));
        var cases = new (string Text, int Value)[]
        {
            ("1", 1),
            ("1+2*3", 7),
            ("(1+2)*3", 9),
            ("8-3-1", 4),
            ("9-(2+3)*1", 4),
            (" ((12)) ", 12),
            ("1 /* operator gap */ + 2 * 3 /* end */", 7),
            ("\r\n(8-3)\r*2\n", 10)
        };

        foreach (var item in cases)
        {
            var list = parser.Parsing(lexer.Lexing(item.Text).TokensForParsing);
            Assert.True(list.Success, item.Text + ": " + parser.FiredException);
            string originalTree = Shape(list.ToParseTree);
            var source = parser.Parsing(lexer.CreateTokenSource(item.Text, policy));

            Assert.True(source.Success, item.Text + ": " + parser.FiredException);
            Assert.Null(parser.FiredException);
            Assert.Empty(source.AllErrors);
            Assert.Equal(originalTree, Shape(source.ToParseTree));
            Assert.Equal(item.Value, Evaluate(source.ToParseTree));
            Assert.Equal(list.ToParseTree.AllTokens.Select(t => (t.Input, t.StartIndex, t.EndIndex)),
                source.ToParseTree.AllTokens.Select(t => (t.Input, t.StartIndex, t.EndIndex)));
            Assert.NotSame(list.ToParseTree, source.ToParseTree);
            if (logging)
            {
                // Legacy list input uses a textual EOF marker; a token source uses zero-width EOF.
                Assert.Equal(list.Trace.Steps.Select(step => (step.Action.GetType(), TraceToken(step))),
                    source.Trace.Steps.Select(step => (step.Action.GetType(), TraceToken(step))));
            }
            else
            {
                Assert.Null(source.Logger);
                Assert.Empty(list.Trace.Steps);
                Assert.Empty(source.Trace.Steps);
            }
        }

        foreach (string text in new[] { "", "1+", "(1+2", "1**2", "1@2" })
        {
            var list = parser.Parsing(lexer.Lexing(text).TokensForParsing);
            var source = parser.Parsing(lexer.CreateTokenSource(text, policy));
            Assert.False(list.Success);
            Assert.False(source.Success);
            Assert.True(source.HasError);
            Assert.Null(source.ToParseTree);
            Assert.NotNull(parser.FiredException);
            if (!logging)
            {
                Assert.Empty(list.Trace.Steps);
                Assert.Empty(source.Trace.Steps);
            }
            Assert.Equal(list.AllErrors.SelectMany(e => e.ErrTokens)
                    .Select(t => t.Kind is EndMarker ? text.Length : t.StartIndex).OrderBy(i => i),
                source.AllErrors.SelectMany(e => e.ErrTokens)
                    .Select(t => t.Kind is EndMarker ? text.Length : t.StartIndex).OrderBy(i => i));
        }
    }

    [Fact]
    public void Repeated_parses_do_not_share_tree_nodes_or_mutable_token_annotations()
    {
        var grammar = Read(Arithmetic);
        var parser = new LALRParser(grammar);
        var lexer = LexerFor(grammar);
        var policy = new LexicalPolicy(new LexicalGoal(Scannable(grammar)));
        var source = lexer.CreateTokenSource("1+2*3", policy);
        var first = parser.Parsing(source);
        Assert.True(first.Success, parser.FiredException?.ToString());
        var oldNodes = Nodes(first.ToParseTree).ToArray();
        var firstToken = first.ToParseTree.AllTokens[0];
        var annotation = new object();
        firstToken.TokenCell.RefOptionalData = annotation;
        firstToken.IsNotUsed = true;
        string savedTree = Shape(first.ToParseTree);

        var second = parser.Parsing(source);

        Assert.True(second.Success, parser.FiredException?.ToString());
        Assert.Equal(savedTree, Shape(first.ToParseTree));
        Assert.Equal(savedTree, Shape(second.ToParseTree));
        Assert.All(Nodes(second.ToParseTree), node =>
            Assert.DoesNotContain(oldNodes, old => ReferenceEquals(old, node)));
        Assert.Same(annotation, firstToken.TokenCell.RefOptionalData);
        Assert.True(firstToken.IsNotUsed);
        Assert.Null(second.ToParseTree.AllTokens[0].TokenCell.RefOptionalData);
        Assert.False(second.ToParseTree.AllTokens[0].IsNotUsed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_source_cannot_alias_a_grammar_terminal_by_its_numeric_key(bool anotherGrammar)
    {
        var grammar = Read("S : 'x' ;");
        var parser = new LALRParser(grammar, true);
        var original = grammar.TerminalSet.Single(t => t.Value == "x");
        var foreign = anotherGrammar
            ? Read("Other : 'z' ;").TerminalSet.Single(t => t.Value == "z")
            : new InputTerminal(original);
        Assert.Equal(original.UniqueKey, foreign.UniqueKey);
        Assert.NotSame(original, foreign);
        var lexer = new Lexer();
        lexer.AddTokenRule(foreign);
        var policy = new LexicalPolicy(new LexicalGoal(new[] { foreign }));

        var result = parser.Parsing(lexer.CreateTokenSource(foreign.Value, policy));

        Assert.False(result.Success);
        var error = Assert.IsType<ContextualLexingException>(parser.FiredException);
        Assert.Equal(0, error.SourceOffset);
        Assert.Equal(0, error.TokenIndex);
        Assert.DoesNotContain(result.Trace.Steps, step => step.Action is ParseAction.Shift);
        Assert.NotEmpty(result.AllErrors);

        var correctLexer = LexerFor(grammar);
        var correctPolicy = new LexicalPolicy(new LexicalGoal(Scannable(grammar)));
        Assert.True(parser.Parsing(correctLexer.CreateTokenSource("x", correctPolicy)).Success,
            parser.FiredException?.ToString());
    }

    [Fact]
    public void An_aliased_lookahead_is_rejected_before_a_reduce_can_reuse_it()
    {
        var grammar = Read("S : A 'y' ; A : 'x' ;");
        var parser = new LALRParser(grammar, true);
        var x = grammar.TerminalSet.Single(t => t.Value == "x");
        var y = grammar.TerminalSet.Single(t => t.Value == "y");
        var foreignY = new InputTerminal(y);
        var lexer = new Lexer();
        lexer.AddTokenRule(x);
        lexer.AddTokenRule(foreignY);
        lexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Delimiter, " ", false));
        var policy = new LexicalPolicy(new LexicalGoal(new[] { x, foreignY }));

        var result = parser.Parsing(lexer.CreateTokenSource("x y", policy));

        Assert.False(result.Success);
        var error = Assert.IsType<ContextualLexingException>(parser.FiredException);
        Assert.Equal(1, error.SourceOffset);
        Assert.Equal(1, error.TokenIndex);
        Assert.Equal(new[] { "x" }, result.Trace.Steps.Where(step => step.Action is ParseAction.Shift)
            .Select(step => step.Lookahead.Input));
        Assert.DoesNotContain(result.Trace.Steps, step => step.Action is ParseAction.Reduce);
    }

    private static string TraceToken(ParseStep step)
        => step.Lookahead.Kind is EndMarker ? "<EOF>" : step.Lookahead.Input;

    private static EbnfGrammar Read(string text)
    {
        var read = EbnfGrammarReader.Read(text);
        Assert.True(read.Success, string.Join("; ", read.Errors));
        return read.Grammar;
    }

    private static Lexer LexerFor(EbnfGrammar grammar)
    {
        var lexer = new Lexer();
        foreach (var terminal in grammar.TerminalSet) lexer.AddTokenRule(terminal);
        return lexer;
    }

    private static IEnumerable<Terminal> Scannable(EbnfGrammar grammar)
        => grammar.TerminalSet.Where(t => t is not EndMarker && t is not Epsilon);

    private static string Shape(ParseTreeSymbol node)
        => node is ParseTreeTerminal terminal ? "token:" + terminal.Token.Input
            : ((ParseTreeNonTerminal)node).SignPost.Name + "(" +
              string.Join(",", ((ParseTreeNonTerminal)node).Items.Select(Shape)) + ")";

    private static IEnumerable<ParseTreeSymbol> Nodes(ParseTreeSymbol node)
    {
        yield return node;
        if (node is ParseTreeNonTerminal nonTerminal)
            foreach (var child in nonTerminal.Items.SelectMany(Nodes)) yield return child;
    }

    private static int Evaluate(ParseTreeSymbol node)
    {
        if (node is ParseTreeTerminal terminal)
            return int.Parse(terminal.Token.Input, CultureInfo.InvariantCulture);
        var parts = ((ParseTreeNonTerminal)node).Items;
        if (parts.Count == 1) return Evaluate(parts[0]);
        if (parts[0] is ParseTreeTerminal open && open.Token.Input == "(") return Evaluate(parts[1]);
        int left = Evaluate(parts[0]), right = Evaluate(parts[2]);
        return ((ParseTreeTerminal)parts[1]).Token.Input switch
        {
            "+" => left + right,
            "-" => left - right,
            "*" => left * right,
            _ => throw new InvalidOperationException("Unexpected fixture operator.")
        };
    }
}
