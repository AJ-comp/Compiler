using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.Grammars.TypeScript;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.ParseTree;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.TypeScript;

[CollectionDefinition(Name)]
public sealed class TypeScriptTypeCollection
{
    public const string Name = "TypeScript type syntax";
}

internal sealed class TypeScriptTypeFixture
{
    private static readonly Lazy<TypeScriptTypeFixture> Shared = new(() => new());
    internal static TypeScriptTypeFixture Instance => Shared.Value;
    internal TypeScriptGrammar Language { get; } = new(inputKind: TypeScriptInputKind.Type);
    internal LALRParser Parser { get; }

    private TypeScriptTypeFixture() => Parser = new LALRParser(Language.Grammar, false);

    internal ParseTreeNonTerminal Accept(string input)
    {
        var parsed = Parser.Parsing(Language.CreateTokenSource(input));
        Assert.True(parsed.Success, input + Environment.NewLine + Parser.FiredException + Environment.NewLine +
            string.Join(Environment.NewLine, parsed.AllErrors.Select(e => e.Message)));
        Assert.Empty(parsed.AllErrors);
        var diagnostics = Language.Validate(parsed.ToParseTree);
        Assert.True(diagnostics.Count == 0, input + Environment.NewLine +
            string.Join(Environment.NewLine, diagnostics.Select(d => d.Code + ": " + d.Message)));
        return Assert.IsType<ParseTreeNonTerminal>(parsed.ToParseTree);
    }

    internal void Reject(string input)
    {
        var parsed = Parser.Parsing(Language.CreateTokenSource(input));
        if (!parsed.Success)
        {
            Assert.NotEmpty(parsed.AllErrors);
            return;
        }
        Assert.NotEmpty(Language.Validate(parsed.ToParseTree));
    }
}

[Collection(TypeScriptTypeCollection.Name)]
[Trait("Category", "TypeScript")]
[Trait("Feature", "TypeScriptTypes")]
public sealed class TypeScriptTypeTests
{
    private readonly TypeScriptTypeFixture fixture = TypeScriptTypeFixture.Instance;

    [Theory]
    [InlineData("any")]
    [InlineData("unknown")]
    [InlineData("string")]
    [InlineData("number")]
    [InlineData("bigint")]
    [InlineData("symbol")]
    [InlineData("boolean")]
    [InlineData("undefined")]
    [InlineData("never")]
    [InlineData("object")]
    [InlineData("void")]
    [InlineData("null")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("'literal'")]
    [InlineData("42")]
    [InlineData("-42")]
    [InlineData("123n")]
    [InlineData("-123n")]
    [InlineData("`literal`")]
    [InlineData("this")]
    [InlineData("User")]
    [InlineData("Domain.User")]
    [InlineData("Array<string>")]
    [InlineData("Map<string, Array<User>>")]
    [InlineData("Outer<Middle<Inner<User>>>")]
    [InlineData("Tuple<string, number,>")]
    [InlineData("any.Qualified")]
    public void Primitive_literal_and_reference_types(string input) => fixture.Accept(input);

    [Theory]
    [InlineData("A | B & C")]
    [InlineData("| A | B")]
    [InlineData("& A & B")]
    [InlineData("(A | B)[]")]
    [InlineData("A[][]")]
    [InlineData("T[K]")]
    [InlineData("T[keyof T][number][]")]
    [InlineData("keyof T")]
    [InlineData("unique symbol")]
    [InlineData("readonly string[]")]
    [InlineData("T extends U ? X : Y")]
    [InlineData("T extends U ? X extends Y ? A : B : C")]
    [InlineData("T extends infer U ? U : never")]
    [InlineData("T extends infer U extends string ? U : never")]
    [InlineData("typeof service.current")]
    [InlineData("typeof factory<string>")]
    [InlineData("typeof this")]
    public void Operators_precedence_conditional_and_queries(string input) => fixture.Accept(input);

    [Theory]
    [InlineData("[]")]
    [InlineData("[string, number]")]
    [InlineData("[string, number?, ...boolean[]]")]
    [InlineData("[name: string, count?: number, ...rest: boolean[]]")]
    [InlineData("[...Head, Last]")]
    [InlineData("readonly [name: string, count?: number]")]
    [InlineData("[first: A, B, ...C[]]")]
    [InlineData("[A,]")]
    [InlineData("{}")]
    [InlineData("{name: string; count?: number}")]
    [InlineData("{readonly name: string, count: number,}")]
    [InlineData("{name: string\ncount: number}")]
    [InlineData("{[key: string]: unknown}")]
    [InlineData("{readonly [key: string]: number}")]
    [InlineData("{[Symbol.iterator](): Iterator<User>}")]
    [InlineData("{readonly: boolean; get: string; set: number; new: number}")]
    [InlineData("{get value(): string; set value(input: string);}")]
    public void Tuple_and_object_members(string input) => fixture.Accept(input);

    [Theory]
    [InlineData("() => void")]
    [InlineData("(input: string) => number")]
    [InlineData("(this: Receiver, input?: string) => void")]
    [InlineData("(...args: string[]) => void")]
    [InlineData("({name}: User, [value]: [number]) => string")]
    [InlineData("<T extends Entity = User>(value: T) => T")]
    [InlineData("new (input: string) => Result")]
    [InlineData("abstract new <T>(value: T) => Container<T>")]
    [InlineData("((input: string) => number)[]")]
    [InlineData("string | (() => number)")]
    [InlineData("{(input: string): number; <T>(value: T): T; new (): Result}")]
    [InlineData("{transform<T>(input: T): T; optional?(input: string): void}")]
    [InlineData("(value: unknown) => value is string")]
    [InlineData("(value: unknown) => asserts value")]
    [InlineData("(value: unknown) => asserts value is string")]
    [InlineData("{check(): this is Derived}")]
    public void Function_construct_call_and_predicate_types(string input) => fixture.Accept(input);

    [Theory]
    [InlineData("{[K in keyof T]: T[K]}")]
    [InlineData("{readonly [K in keyof T]?: T[K]}")]
    [InlineData("{-readonly [K in keyof T]-?: T[K]}")]
    [InlineData("{+readonly [K in keyof T]+?: T[K];}")]
    [InlineData("{[K in keyof T as `get${K}`]: T[K]}")]
    [InlineData("`prefix${T}suffix`")]
    [InlineData("`${A}-${B}-${C}`")]
    [InlineData("import('library').User")]
    [InlineData("import('library').Factory<User>")]
    [InlineData("typeof import('library')")]
    [InlineData("import('library', {with: {'resolution-mode': 'import'}}).User")]
    public void Mapped_template_and_import_types(string input) => fixture.Accept(input);

    [Theory]
    [InlineData("")]
    [InlineData("Array<>")]
    [InlineData("Array<string")]
    [InlineData("Array<string>>")]
    [InlineData("A |")]
    [InlineData("A &")]
    [InlineData("T extends U ? X")]
    [InlineData("T extends ? X : Y")]
    [InlineData("(value: string)")]
    [InlineData("(value: string) =>")]
    [InlineData("new ()")]
    [InlineData("abstract () => T")]
    [InlineData("[name:]")]
    [InlineData("{name:}")]
    [InlineData("{[K in]: T[K]}")]
    [InlineData("keyof")]
    [InlineData("typeof")]
    [InlineData("infer")]
    [InlineData("import()")]
    [InlineData("<T>()")]
    public void Malformed_type_syntax_is_rejected(string input) => fixture.Reject(input);

    [Fact]
    public void Concrete_tree_keeps_type_roles_and_original_source_positions()
    {
        const string input = "Outer<Middle<Inner<User>>>";
        var tree = fixture.Accept(input);
        var nodes = Descendants(tree).ToArray();
        Assert.Contains(nodes, n => n.SignPost.Name.StartsWith("TSTypeReference", StringComparison.Ordinal));
        Assert.Contains(nodes, n => n.SignPost.Name.StartsWith("TSTypeArguments", StringComparison.Ordinal));
        var tokens = tree.AllTokens.ToArray();
        Assert.Equal(input, string.Concat(tokens.Select(t => t.Input)));
        var close = tokens.Where(t => t.Input == ">").ToArray();
        Assert.Equal(3, close.Length);
        Assert.All(close, t => Assert.Equal("TSTypeClose", t.Kind.Caption));
        Assert.Equal(new[] { input.Length - 3, input.Length - 2, input.Length - 1 }, close.Select(t => t.StartIndex));
        foreach (var token in tokens)
            Assert.Equal(token.Input, input.Substring(token.StartIndex, token.EndIndex - token.StartIndex + 1));
    }

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
