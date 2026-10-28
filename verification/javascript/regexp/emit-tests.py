import json
from collections import defaultdict
from pathlib import Path

corpus = json.loads(Path('artifacts/regexp/corpus.json').read_text(encoding='utf-8'))
groups = defaultdict(list)
for item in corpus:
    groups[(item['category'], item['expected'])].append(item['literal'])

def literal(text): return json.dumps(text, ensure_ascii=False)

text = '''using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Janglim.FrontEnd.Grammars.JavaScript;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

// These are RegExpPattern syntax checks, not matching tests. In particular,
// nested quantifiers and lookarounds must never execute a user pattern here.
[Collection(JavaScriptExpressionCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptRegExp")]
public sealed class JavaScriptRegExpTests
{
    private readonly JavaScriptExpressionFixture _fixture;
    public JavaScriptRegExpTests(JavaScriptExpressionFixture fixture) => _fixture = fixture;

'''
for (name, expected), literals in groups.items():
    text += '    [Theory]\n'
    for input in literals:
        text += f'    [InlineData({literal(input)})]\n'
    text += f'    public void {name}_are_{"accepted" if expected else "rejected"}(string input)\n'
    text += f'        => {"Accept" if expected else "Reject"}(input);\n\n'

text += r'''    // These script names and aliases were added in Unicode 17.0.0.
    // The pinned ECMAScript 2026 data, rather than the host runtime's Unicode
    // version, determines which property names the language accepts.
    [Theory]
    [InlineData("/\\p{Script=Beria_Erfe}/u")]
    [InlineData("/\\p{sc=Berf}/v")]
    [InlineData("/\\p{Script=Sidetic}/u")]
    [InlineData("/\\p{scx=Sidt}/v")]
    [InlineData("/\\p{Script=Tolong_Siki}/u")]
    [InlineData("/\\p{sc=Tols}/v")]
    [InlineData("/\\p{Script=Tai_Yo}/u")]
    [InlineData("/\\p{Script_Extensions=Tayo}/v")]
    public void Unicode_17_script_property_names_follow_the_pinned_ECMAScript_2026_version(string input)
        => Accept(input);

    [Theory]
    [InlineData("/(a+)+$/")]
    [InlineData("/(a|aa)+$/u")]
    [InlineData("/(?<=a*)b/v")]
    [InlineData("/(a)\\1/")]
    [InlineData("/(?<item>a)|(?<item>b)/v")]
    public void Syntax_validation_accepts_runtime_sensitive_patterns_without_matching_them(string input)
        => Accept(input);

    [Theory]
    [InlineData("/(?<name>a)\\k<name>/dgimsuy")]
    [InlineData("/(?<name>a)\\k<name>/dgimsvy")]
    [InlineData("/[a-z]/dgimsuy")]
    [InlineData("/[[a-z]--[aeiou]]/dgimsvy")]
    public void Global_matching_flags_do_not_change_the_body_syntax_goal(string input)
        => Accept(input);

    [Theory]
    [InlineData("/* header */ /(/", "/(/")]
    [InlineData("/* 😀 */ /[z-a]/u", "/[z-a]/u")]
    [InlineData("\uFEFF\u00A0 /(?<same>a)(?<same>b)/u", "/(?<same>a)(?<same>b)/u")]
    [InlineData("\n /* gap */ /\\q/u", "/\\q/u")]
    [InlineData("\n\t /a{3,2}/v", "/a{3,2}/v")]
    public void Body_diagnostics_use_original_UTF16_source_offsets_including_leading_trivia(
        string input, string literal)
    {
        var parsed = _fixture.Parse(input);
        Assert.True(parsed.Success, input + Environment.NewLine + _fixture.Parser.FiredException);
        Assert.Empty(parsed.AllErrors);
        var token = Assert.Single(parsed.ToParseTree.AllTokens);
        Assert.Equal(literal, token.Input);
        Assert.Equal(input.IndexOf(literal, StringComparison.Ordinal), token.StartIndex);
        var diagnostics = _fixture.Language.Validate(parsed.ToParseTree);
        Assert.NotEmpty(diagnostics);
        Assert.All(diagnostics, diagnostic => AssertBodyDiagnostic(diagnostic, token.StartIndex, literal));
    }

    [Fact]
    public void Body_validation_preserves_the_concrete_tree_and_returns_the_same_public_diagnostics()
    {
        const string input = "[/(?<name>a)(?<name>b)/u, /[z-a]/v, /(?i:a)/]";
        var parsed = _fixture.Parse(input);
        Assert.True(parsed.Success, _fixture.Parser.FiredException?.ToString());
        var tree = parsed.ToParseTree;
        var tokens = tree.AllTokens.ToArray();
        var originalSpellings = tokens.Select(token => token.Input).ToArray();
        var originalOffsets = tokens.Select(token => (token.StartIndex, token.EndIndex)).ToArray();
        var sentinel = new object();
        tokens[0].TokenCell.RefOptionalData = sentinel;

        var factory = _fixture.Language.Validate(tree);
        var direct = JavaScriptExpressionValidation.Validate(tree);
        var repeated = _fixture.Language.Validate(tree);
        Assert.Equal(2, factory.Count);
        Assert.Equal(factory.Select(Snapshot), direct.Select(Snapshot));
        Assert.Equal(factory.Select(Snapshot), repeated.Select(Snapshot));
        Assert.Equal(originalSpellings, tree.AllTokens.Select(token => token.Input));
        Assert.Equal(originalOffsets, tree.AllTokens.Select(token => (token.StartIndex, token.EndIndex)));
        Assert.Same(sentinel, tokens[0].TokenCell.RefOptionalData);
        Assert.All(tokens, token => Assert.Contains(tree.AllTokens, item => ReferenceEquals(item, token)));
    }

    [Fact]
    public void Each_invalid_literal_has_its_own_diagnostic_and_valid_literals_are_unchanged()
    {
        const string input = "[/(/, /a{4,2}/u, /[a-z]/v, /\\q/u]";
        var parsed = _fixture.Parse(input);
        Assert.True(parsed.Success, _fixture.Parser.FiredException?.ToString());
        var tokens = parsed.ToParseTree.AllTokens.Where(
            token => token.Kind.Caption == "RegularExpressionLiteral").ToArray();
        Assert.Equal(4, tokens.Length);
        var diagnostics = _fixture.Language.Validate(parsed.ToParseTree);
        Assert.Equal(3, diagnostics.Count);
        foreach (int index in new[] { 0, 1, 3 })
        {
            var token = tokens[index];
            AssertBodyDiagnostic(Assert.Single(diagnostics, diagnostic =>
                diagnostic.SourceOffset >= token.StartIndex && diagnostic.SourceOffset <= token.EndIndex),
                token.StartIndex, token.Input);
        }
        Assert.DoesNotContain(diagnostics, diagnostic =>
            diagnostic.SourceOffset >= tokens[2].StartIndex && diagnostic.SourceOffset <= tokens[2].EndIndex);
    }

    [Fact]
    public void Modes_capture_names_and_capture_counts_do_not_leak_between_literals_or_validations()
    {
        var inputs = new[]
        {
            ("/\\q/", true), ("/\\q/u", false), ("/\\q/", true),
            ("/(?<name>a)\\k<name>/u", true), ("/\\k<name>/u", false),
            ("/(a)\\1/u", true), ("/\\1/u", false), ("/\\1/", true),
            ("/[[a-z]--[aeiou]]/v", true), ("/[[a-z]--[aeiou]]/u", false),
            ("/(?<name>a)|(?<name>b)/u", true), ("/(?<name>a)(?<name>b)/u", false)
        };
        for (int round = 0; round < 3; round++)
        foreach (var (input, valid) in inputs)
        {
            if (valid) Accept(input);
            else Reject(input);
        }
    }

    [Fact]
    public void Independent_immutable_trees_can_be_validated_concurrently_without_sharing_pattern_state()
    {
        var examples = new[]
        {
            ("/(?<a>x)\\k<a>/u", true),
            ("/(?<a>x)(?<a>y)/u", false),
            ("/[\\q{ab}&&\\q{ab|cd}]/v", true),
            ("/[^\\q{ab}]/v", false),
            ("/\\8/", true),
            ("/\\8/u", false)
        };
        var trees = examples.Select(example =>
        {
            var parsed = _fixture.Parse(example.Item1);
            Assert.True(parsed.Success, example.Item1 + Environment.NewLine + _fixture.Parser.FiredException);
            return parsed.ToParseTree;
        }).ToArray();

        Parallel.For(0, 48, index =>
        {
            int selected = index % examples.Length;
            var diagnostics = _fixture.Language.Validate(trees[selected]);
            Assert.Equal(examples[selected].Item2, diagnostics.Count == 0);
            if (!examples[selected].Item2)
                Assert.All(diagnostics, diagnostic => Assert.Equal("regexp_syntax", diagnostic.Code));
        });
    }

    [Fact]
    public void Long_decimal_quantifiers_and_backreferences_are_compared_without_integer_overflow()
    {
        string smaller = "1" + new string('0', 2048);
        string larger = "2" + new string('0', 2048);
        Accept("/a{" + smaller + "," + larger + "}/u");
        Accept("/a{" + smaller + "," + smaller + "}/v");
        Reject("/a{" + larger + "," + smaller + "}/u");
        Reject("/(a)\\" + new string('9', 2048) + "/u");
    }

    [Fact]
    public void Deep_groups_long_alternatives_and_forward_references_do_not_require_JavaScript_execution()
    {
        const int depth = 1024;
        string groups = string.Concat(Enumerable.Repeat("(?:", depth)) + "a" + new string(')', depth);
        Accept("/" + groups + "/u");
        Reject("/" + groups.Substring(0, groups.Length - 1) + "/v");
        Accept("/" + string.Join("|", Enumerable.Repeat("a", 2048)) + "/v");
        Accept("/\\128" + string.Concat(Enumerable.Repeat("(a)", 128)) + "/u");
        Reject("/\\129" + string.Concat(Enumerable.Repeat("(a)", 128)) + "/u");
    }

    [Fact]
    public void Deep_Unicode_sets_use_explicit_frames_and_preserve_nested_string_restrictions()
    {
        const int depth = 1024;
        string open = new string('[', depth);
        string close = new string(']', depth);
        Accept("/" + open + "a-z" + close + "/v");
        Reject("/" + open + "a-z" + close.Substring(1) + "/v");
        Accept("/[^" + open + "\\q{a|b}" + close + "]/v");
        Reject("/[^" + open + "\\q{ab}" + close + "]/v");
    }

    [Fact]
    public void An_invalid_speculative_RegExp_body_does_not_reject_the_selected_division_expression()
    {
        const string input = "value/+other/ divisor";
        var source = _fixture.Language.CreateTokenSource(input);
        var regex = source.Policy.Goals.Single(goal =>
            goal.Terminals.Any(terminal => terminal.Caption == "RegularExpressionLiteral") &&
            !goal.Terminals.Any(terminal => terminal.Caption == "TemplateTail"));
        var division = source.Policy.Goals.Single(goal =>
            !goal.Terminals.Any(terminal => terminal.Caption == "RegularExpressionLiteral") &&
            !goal.Terminals.Any(terminal => terminal.Caption == "TemplateTail"));
        var value = source.Peek(source.Start, division);
        Assert.Equal("value", value.Token.Input);
        var slashPosition = value.Next;

        // A prediction may read the same source position with a different goal.
        // This token has valid literal boundaries and flags but an invalid body.
        // Body validation belongs to the final CST, not this speculative Peek.
        var speculative = source.Peek(slashPosition, regex);
        Assert.Equal("/+other/", speculative.Token.Input);
        Assert.Equal("RegularExpressionLiteral", speculative.Token.Kind.Caption);
        Assert.Equal("/", source.Peek(slashPosition, division).Token.Input);
        Assert.Equal("/+other/", source.Peek(slashPosition, regex).Token.Input);
        Assert.Equal(5, slashPosition.SourceOffset);
        Assert.Equal(0, source.Start.SourceOffset);

        var parsed = _fixture.Parser.Parsing(source);
        Assert.True(parsed.Success, input + Environment.NewLine + _fixture.Parser.FiredException);
        Assert.Empty(parsed.AllErrors);
        Assert.Empty(_fixture.Language.Validate(parsed.ToParseTree));
        Assert.Equal(new[] { "value", "/", "+", "other", "/", "divisor" },
            parsed.ToParseTree.AllTokens.Select(token => token.Input));
        Assert.DoesNotContain(parsed.ToParseTree.AllTokens,
            token => token.Kind.Caption == "RegularExpressionLiteral");

        // Removing the separating space changes a hypothetical RegExp flag span,
        // but the actual division parse is still exactly the same expression.
        var compact = _fixture.Success("value/+other/divisor");
        Assert.Equal(parsed.ToParseTree.AllTokens.Select(token => token.Input),
            compact.AllTokens.Select(token => token.Input));
        Reject("/+other/");
    }

    private void Accept(string input)
    {
        var parsed = _fixture.Parse(input);
        Assert.True(parsed.Success, input + Environment.NewLine + _fixture.Parser.FiredException);
        Assert.Empty(parsed.AllErrors);
        var diagnostics = _fixture.Language.Validate(parsed.ToParseTree);
        Assert.True(diagnostics.Count == 0, input + Environment.NewLine +
            string.Join(Environment.NewLine, diagnostics.Select(diagnostic =>
                diagnostic.Code + ": " + diagnostic.Message)));
        Assert.Equal(input, Assert.Single(parsed.ToParseTree.AllTokens).Input);
    }

    private void Reject(string input)
    {
        var parsed = _fixture.Parse(input);
        Assert.True(parsed.Success, "RegExpPattern errors are checked after lexical boundary parsing: " +
            input + Environment.NewLine + _fixture.Parser.FiredException);
        Assert.Empty(parsed.AllErrors);
        var diagnostics = _fixture.Language.Validate(parsed.ToParseTree);
        Assert.NotEmpty(diagnostics);
        Assert.All(diagnostics, diagnostic => AssertBodyDiagnostic(diagnostic, 0, input));
    }

    private static void AssertBodyDiagnostic(JavaScriptDiagnostic diagnostic, int tokenStart, string literal)
    {
        Assert.Equal("regexp_syntax", diagnostic.Code);
        Assert.False(string.IsNullOrWhiteSpace(diagnostic.Message));
        int closingSlash = tokenStart + literal.LastIndexOf('/');
        Assert.InRange(diagnostic.SourceOffset, tokenStart + 1, closingSlash);
        Assert.InRange(diagnostic.Length, 0, closingSlash - diagnostic.SourceOffset);
    }

    private static (string Code, string Message, int Offset, int Length) Snapshot(JavaScriptDiagnostic diagnostic)
        => (diagnostic.Code, diagnostic.Message, diagnostic.SourceOffset, diagnostic.Length);
}
'''

# The generated segment uses JSON-compatible C# escaped strings. The hand-written
# segment is raw Python containing the C# escapes exactly as written.

script_valid = [
 r'const pattern = /(?<name>a)\k<name>/u;',
 r'let value = /[\p{ASCII}&&\p{Letter}]/v;',
 r'/a|b/; /[\q{ab|cd}]/v;',
 r'if (value) /(?i:a)/.test(value);',
 r'if (value) {} /(?<x>a)|(?<x>b)/u.test(value);',
 r'function f() { return /(?<=prefix)value/u; }',
 r'const f = () => /(?:a{2})/;',
 r'const f = () => { return /\p{RGI_Emoji}/v; };',
 r'const f = () => {}; /(?i:a)/;',
 r'const f = ({value = /[a-z]/u} = {}) => value;',
 r'const [first = /a+/u] = values;',
 r'let {value: local = /[a--b]/v} = object;',
 r'class A { field = /[a-z]/u; static value = /(?-i:a)/i; }',
 r'class A { #value = /(?<name>a)/u; method() { return this.#value; } }',
 r'class A { static { /[\q{ab}]/v; } }',
 r'class A extends Base { method() { return /a+/; } }',
 r'const value = `${/(?<name>a)/u}`;',
 r'const value = tag`outer ${/[[a-z]--[aeiou]]/v} tail`;',
 r'import(path, {with: {pattern: /[a-z]/u}});',
 r'for (const value of /a/.source) {}',
 r'for (let value = /(?:a)/; value; value = /b/) {}',
 r'switch (value) { case /[a-z]/u: break; default: /a/; }',
 r'try {} catch (error) { /a{1,2}/u; } finally { /b/; }',
 r'({[/a/.source]: /b/});',
 r'value / /(?<name>a)\k<name>/u.source;',
 r'value /= /a/.source;',
 r'value\n/other/divisor;'.replace(r'\n', '\n'),
 r'function f() { return\n/[a-z]/u; }'.replace(r'\n', '\n'),
]
script_invalid = [
 r'const pattern = /(/;',
 r'let value = /\q/u;',
 r'/a/; /[z-a]/u;',
 r'if (value) /a{3,2}/.test(value);',
 r'if (value) {} /(?<x>a)(?<x>b)/u.test(value);',
 r'function f() { return /(?g:a)/; }',
 r'const f = () => /(?=a)*/u;',
 r'const f = () => { return /\P{RGI_Emoji}/v; };',
 r'const f = () => {}; /\k<missing>/u;',
 r'const f = ({value = /[!!]/v} = {}) => value;',
 r'const [first = /\p{greek}/u] = values;',
 r'let {value: local = /[a&&]/v} = object;',
 r'class A { field = /\8/u; }',
 r'class A { #value = /(?<>a)/u; }',
 r'class A { static { /[^\q{ab}]/v; } }',
 r'class A extends Base { method() { return /a++/; } }',
 r'const value = `${/a{2,1}/u}`;',
 r'const value = tag`outer ${/[\d-a]/u} tail`;',
 r'import(path, {with: {pattern: /\p{Invalid}/u}});',
 r'for (const value of /\9/u.source) {}',
 r'for (let value = /a}/u; value; value = /b/) {}',
 r'switch (value) { case /[a-\d]/u: break; }',
 r'try {} catch (error) { /(?-:a)/; }',
 r'({[/a/.source]: /(?<a>x)(?<a>y)/});',
]
module_valid = [
 r'export const pattern = /(?<name>a)\k<name>/u;',
 r'export default /[\q{ab|cd}]/v;',
 r'export default class { field = /[a-z]/u; }',
 r'export default function() { return /(?i:a)/; }',
 r'export function f(value = /(?<=a)b/u) { return value; }',
 r'const pattern = /\p{RGI_Emoji}/v; export {pattern};',
 r'import value from "module"; /(?<name>a)/u.test(value);',
 r'import "module"; /[a--b]/v;',
 r'await /(?<name>a)\k<name>/u;',
 r'for await (const value of source) { /[a-z]/u; }',
 r'const value = import.meta.url + /a/.source;',
 r'import(/a/.source);',
 r'export class A { static { /(?s:.)/; } }',
 r'export const f = () => /(?<name>a)|(?<name>b)/v;',
 r'const value = `${/[\p{Letter}&&\p{ASCII}]/v}`; export {value};',
]
module_invalid = [
 r'export const pattern = /(/;',
 r'export default /[z-a]/u;',
 r'export default class { field = /\p{Unknown}/u; }',
 r'export default function() { return /\q/u; }',
 r'export function f(value = /(?<=a)*/) { return value; }',
 r'const pattern = /[^\q{ab}]/v; export {pattern};',
 r'import value from "module"; /(?<name>a)(?<name>b)/u.test(value);',
 r'import "module"; /[a&&b--c]/v;',
 r'await /(?<name>a)\k<missing>/u;',
 r'for await (const value of source) { /[\d-a]/u; }',
 r'const value = import.meta.url + /a{3,2}/.source;',
 r'import(/\p{RGI_Emoji}/u.source);',
 r'export class A { static { /(?ii:.)/; } }',
 r'export const f = () => /(?<name>a)(?<name>b)/v;',
 r'const value = `${/\8/u}`; export {value};',
]
for name,fixture,valid,invalid in [('Script','JavaScriptScriptFixture',script_valid,script_invalid),
                                   ('Module','JavaScriptModuleFixture',module_valid,module_invalid)]:
    text += f'''\n[Collection(JavaScript{name}Collection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptRegExp")]
public sealed class JavaScriptRegExp{name}IntegrationTests
{{
    private readonly {fixture} _fixture;
    public JavaScriptRegExp{name}IntegrationTests({fixture} fixture) => _fixture = fixture;

'''
    for accepted,inputs in [(True,valid),(False,invalid)]:
        text += '    [Theory]\n'
        for input in inputs:
            text += f'    [InlineData({literal(input)})]\n'
        text += f'    public void {name}_{"accepts" if accepted else "rejects"}_RegExp_bodies_in_nested_JavaScript_contexts(string input)\n'
        if accepted:
            text += '        => _fixture.Success(input);\n\n'
        else:
            text += '''    {
        var parsed = _fixture.Parse(input);
        Assert.True(parsed.Success, input + Environment.NewLine + _fixture.Parser.FiredException);
        Assert.Empty(parsed.AllErrors);
        var diagnostics = _fixture.Language.Validate(parsed.ToParseTree);
        Assert.NotEmpty(diagnostics);
        Assert.All(diagnostics, diagnostic =>
        {
            Assert.Equal("regexp_syntax", diagnostic.Code);
            Assert.InRange(diagnostic.SourceOffset, 0, input.Length);
            Assert.InRange(diagnostic.Length, 0, input.Length - diagnostic.SourceOffset);
        });
    }

'''
    text += '}\n'
Path('artifacts/regexp/integration.json').write_text(json.dumps([
    dict(goal=goal, expected=expected, input=input)
    for goal,valid,invalid in [('Script',script_valid,script_invalid),('Module',module_valid,module_invalid)]
    for expected,inputs in [(True,valid),(False,invalid)] for input in inputs
], ensure_ascii=False, indent=2),encoding='utf-8')
Path('Tests/Parse.FrontEnd.Parsers.Tests/JavaScript/JavaScriptRegExpTests.cs').write_text(text,encoding='utf-8')
print('Wrote corpus cases:', len(corpus))
