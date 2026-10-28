using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.ParseTree;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.TypeScript;

[Collection(TypeScriptRuntimeCollection.Name)]
[Trait("Category", "TypeScript")]
[Trait("Feature", "TypeScriptAsSatisfies")]
public sealed class TypeScriptAsSatisfiesTests
{
    private readonly TypeScriptRuntimeFixture fixture = TypeScriptRuntimeFixture.Shared.Value;

    [Theory]
    [InlineData("let result = value as User;")]
    [InlineData("let result = value satisfies User;")]
    [InlineData("let result = value as unknown as User;")]
    [InlineData("let result = value as User satisfies Named;")]
    [InlineData("let result = value satisfies User as Named;")]
    [InlineData("let result = value satisfies User satisfies Named;")]
    [InlineData("let result = [1, 2] as const;")]
    [InlineData("let result = ({name: 'test'} as const);")]
    [InlineData("let result = value satisfies const;")]
    [InlineData("let result = value as readonly string[];")]
    [InlineData("let result = value satisfies readonly string[];")]
    [InlineData("let result = value as User | undefined;")]
    [InlineData("let result = value satisfies User & Named;")]
    [InlineData("let result = value as T extends U ? X : Y;")]
    [InlineData("let result = value as (() => number);")]
    [InlineData("let result = value as {name: string};")]
    [InlineData("let result = value satisfies {name: string};")]
    [InlineData("let result = value as Map<string, Array<User>>;")]
    [InlineData("let result = value as Box<User> > other;")]
    [InlineData("let result = value as Box<User> >> shift;")]
    [InlineData("let result = value satisfies Box<User> >>> shift;")]
    [InlineData("let result = value as number + other;")]
    [InlineData("let result = left + right as number;")]
    [InlineData("let result = flag ? value as User : fallback;")]
    [InlineData("let result = value as number == other;")]
    [InlineData("let result = left == value as number;")]
    [InlineData("let result = flag && value as User;")]
    [InlineData("let result = value as boolean || fallback;")]
    [InlineData("let result = value satisfies User ? yes : no;")]
    [InlineData("let result = typeof value as string;")]
    [InlineData("let result = (value as User).name;")]
    [InlineData("let result = (value as Handler)(argument);")]
    [InlineData("let result = (value satisfies User).name;")]
    [InlineData("let result = [input as User, other satisfies Named];")]
    [InlineData("let result = {value: input as User};")]
    [InlineData("function f(value: unknown) { return value as User; }")]
    [InlineData("let value = input\nas(User);")]
    [InlineData("let value = input\nsatisfies(User);")]
    [InlineData("let value = input as\nUser;")]
    [InlineData("let value = input satisfies\nUser;")]
    [InlineData("let value = input /*\n*/ as(User);")]
    [InlineData("let value = input as /*\n*/ User;")]
    [InlineData("let as = 1; let satisfies = 2; as + satisfies;")]
    [InlineData("let result = object.as + object.satisfies;")]
    [InlineData("for (let item = value as Item; check; step) {}")]
    [InlineData("for (let item = value satisfies Item; check; step) {}")]
    [InlineData("let result = (value as number) / divisor;")]
    [InlineData("let result = value as `prefix${Name}`;")]
    [InlineData("let result = object[key as string];")]
    [InlineData("let result = (value => value) satisfies Handler;")]
    public void Accepts_runtime_assertions_without_performing_type_checking(string source)
        => fixture.Accept(source);

    [Theory]
    [InlineData("let result = value as;")]
    [InlineData("let result = value satisfies;")]
    [InlineData("let result = as User;")]
    [InlineData("let result = satisfies User;")]
    [InlineData("let result = value as <User>;")]
    [InlineData("let result = value satisfies <User>;")]
    [InlineData("let result = value as Array<>;")]
    [InlineData("let result = value satisfies Array<>;")]
    [InlineData("let result = value as Box<User;")]
    [InlineData("let result = value as T |;")]
    [InlineData("let result = value satisfies T &;")]
    [InlineData("let result = value as const Type;")]
    [InlineData("let result = left + right as T * third;")]
    [InlineData("let result = left < right as T + third;")]
    [InlineData("let result = left + right satisfies T * third;")]
    [InlineData("let result = left < right satisfies T + third;")]
    public void Rejects_malformed_types_and_operators_that_change_erased_precedence(string source)
        => fixture.Reject(source);

    [Theory]
    [InlineData("left + right as T;", "TSAsExpression", "left+right")]
    [InlineData("left as T + right;", "TSAsExpression", "left")]
    [InlineData("left == right as T;", "TSAsExpression", "right")]
    [InlineData("left < right as T;", "TSAsExpression", "left<right")]
    [InlineData("left as T > right;", "TSAsExpression", "left")]
    [InlineData("left + right satisfies T;", "TSSatisfiesExpression", "left+right")]
    [InlineData("left satisfies T && right;", "TSSatisfiesExpression", "left")]
    [InlineData("value as A as B;", "TSAsExpression", "valueasA")]
    [InlineData("value as A satisfies B;", "TSSatisfiesExpression", "valueasA")]
    [InlineData("value satisfies A as B;", "TSAsExpression", "valuesatisfiesA")]
    public void Precedence_and_left_association_keep_the_correct_runtime_operand(
        string source, string assertionRole, string expectedOperand)
    {
        var tree = Parse(source);
        var assertion = Descendants(tree).Where(node => Role(node) == assertionRole)
            .OrderByDescending(node => node.AllTokens.Count).First();
        var operand = assertion.Items.OfType<ParseTreeNonTerminal>()
            .First(node => node.AllTokens.Count != 0);

        Assert.Equal(expectedOperand, Compact(operand));
    }

    [Fact]
    public void Parsed_assertions_keep_the_original_tokens_positions_and_parent_links_during_validation()
    {
        const string source = "const result = (value as Box<User>) satisfies Named;";
        var tree = Parse(source);
        var nodes = Descendants(tree).ToArray();
        var parents = nodes.Select(node => node.Parent).ToArray();
        var tokens = tree.AllTokens.ToArray();

        Assert.Contains(nodes, node => Role(node) == "TSAsExpression");
        Assert.Contains(nodes, node => Role(node) == "TSSatisfiesExpression");
        Assert.Empty(fixture.Language.Validate(tree));

        Assert.Equal(tokens, tree.AllTokens);
        for (int i = 0; i < nodes.Length; i++) Assert.Same(parents[i], nodes[i].Parent);
        foreach (var token in tokens)
            Assert.Equal(token.Input, source.Substring(token.StartIndex, token.EndIndex - token.StartIndex + 1));
    }

    private ParseTreeNonTerminal Parse(string source)
    {
        var parsed = fixture.Parser.Parsing(fixture.Language.CreateTokenSource(source));
        Assert.True(parsed.Success, source + Environment.NewLine + fixture.Parser.FiredException + Environment.NewLine +
            string.Join(Environment.NewLine, parsed.AllErrors.Select(error => error.Message)));
        Assert.Empty(parsed.AllErrors);
        Assert.Empty(fixture.Language.Validate(parsed.ToParseTree));
        return Assert.IsType<ParseTreeNonTerminal>(parsed.ToParseTree);
    }

    private static string Role(ParseTreeNonTerminal node)
        => node.SignPost.Name.Split(new[] { "__" }, StringSplitOptions.None)[0];

    private static string Compact(ParseTreeSymbol node)
        => string.Concat(node.AllTokens.Select(token => token.Input));

    private static IEnumerable<ParseTreeNonTerminal> Descendants(ParseTreeSymbol root)
    {
        var pending = new Stack<ParseTreeSymbol>();
        pending.Push(root);
        while (pending.Count != 0)
        {
            if (pending.Pop() is not ParseTreeNonTerminal node) continue;
            yield return node;
            foreach (var child in node.Items) pending.Push(child);
        }
    }
}
