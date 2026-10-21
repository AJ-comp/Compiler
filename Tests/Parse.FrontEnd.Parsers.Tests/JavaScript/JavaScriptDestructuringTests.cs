using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

// ECMAScript 2026, 13.15.1 and 13.15.5: assignment patterns cover array/object
// literals only at the left of plain '='. These tests compile syntax; they never
// execute JavaScript, resolve names, or require the RHS to be iterable at runtime.
[Collection(JavaScriptExpressionCollection.Name)]
[Trait("Category", "JavaScript")]
public sealed class JavaScriptDestructuringTests
{
    private readonly JavaScriptExpressionFixture _fixture;
    public JavaScriptDestructuringTests(JavaScriptExpressionFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("[] = values")]
    [InlineData("[,] = values")]
    [InlineData("[,,] = values")]
    [InlineData("[a] = values")]
    [InlineData("[a,] = values")]
    [InlineData("[,a] = values")]
    [InlineData("[a,,b,] = values")]
    [InlineData("[a,,,] = values")]
    [InlineData("[a, a] = values")]
    [InlineData("[a, b] = [b, a]")]
    [InlineData("[[a], [b, c]] = values")]
    [InlineData("[{name}, [first,,third]] = values")]
    [InlineData("[a = 1, b = a + 1] = values")]
    [InlineData("[[a] = fallback, {name} = user] = values")]
    [InlineData("[a = b = c] = values")]
    [InlineData("[a = (b, c)] = values")]
    [InlineData("[a = condition ? b : c] = values")]
    [InlineData("[a = b in object] = values")]
    [InlineData("[a = ({x: 1}), b = [1, 2]] = values")]
    [InlineData("[object.value, object[index], f().member] = values")]
    [InlineData("[a /* before comma */, /* before b */ b] = values")]
    [InlineData("[\na,\r\nb\u2028] = values")]
    public void Array_assignment_patterns_accept_elisions_defaults_members_and_nesting(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("({} = source)")]
    [InlineData("({a} = source)")]
    [InlineData("({a,} = source)")]
    [InlineData("({a, b} = source)")]
    [InlineData("({a, a} = source)")]
    [InlineData("({name: localName} = source)")]
    [InlineData("({a: same, b: same} = source)")]
    [InlineData("({name = fallback} = source)")]
    [InlineData("({name: localName = fallback} = source)")]
    [InlineData("({a = b = c} = source)")]
    [InlineData("({a = (b, c)} = source)")]
    [InlineData("({a: {b = fallback}} = source)")]
    [InlineData("({a: [first,,third] = fallback} = source)")]
    [InlineData("({a: object.value, b: f().member} = source)")]
    [InlineData("({[key]: target} = source)")]
    [InlineData("({[(first, second)]: target} = source)")]
    [InlineData("({[key = nextKey]: target} = source)")]
    [InlineData("({[condition ? left : right]: target = fallback} = source)")]
    [InlineData("({'name': target, 1: number, 1n: bigint} = source)")]
    [InlineData("({if: target, default: other, true: third, null: fourth} = source)")]
    [InlineData("({a: await, await = fallback} = source)")]
    [InlineData("({ /* before key */ name /* before equals */ = fallback } = source)")]
    public void Object_assignment_patterns_accept_shorthand_renaming_computed_keys_and_defaults(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("[...rest] = values")]
    [InlineData("[, ...rest] = values")]
    [InlineData("[a,, ...rest] = values")]
    [InlineData("[...object.value] = values")]
    [InlineData("[...object[index]] = values")]
    [InlineData("[...f().member] = values")]
    [InlineData("[...[a, b]] = values")]
    [InlineData("[...[a, ...rest]] = values")]
    [InlineData("[...[a = fallback]] = values")]
    [InlineData("[...{length}] = values")]
    [InlineData("[...{length: count, ...other}] = values")]
    [InlineData("[...[...{length}]] = values")]
    [InlineData("[a, ...[]] = values")]
    [InlineData("[...{}] = values")]
    [InlineData("({...rest} = source)")]
    [InlineData("({a, ...rest} = source)")]
    [InlineData("({a = fallback, ...object.rest} = source)")]
    [InlineData("({...object[index]} = source)")]
    [InlineData("({...f().member} = source)")]
    [InlineData("({a: {...inner}, ...outer} = source)")]
    public void Array_rest_can_contain_patterns_while_object_rest_accepts_simple_targets(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("[...rest,] = values")]
    [InlineData("[a, ...rest,] = values")]
    [InlineData("[...rest,,] = values")]
    [InlineData("[...rest, a] = values")]
    [InlineData("[...first, ...second] = values")]
    [InlineData("[...rest = fallback] = values")]
    [InlineData("[...rest += fallback] = values")]
    [InlineData("[...[a], b] = values")]
    [InlineData("[...[a, ...rest,]] = values")]
    [InlineData("[...[a] = fallback] = values")]
    [InlineData("[...{a} = fallback] = values")]
    [InlineData("({...rest,} = source)")]
    [InlineData("({a, ...rest,} = source)")]
    [InlineData("({...rest, a} = source)")]
    [InlineData("({...first, ...second} = source)")]
    [InlineData("({...rest = fallback} = source)")]
    [InlineData("({...{a}} = source)")]
    [InlineData("({...{}} = source)")]
    [InlineData("({...[a]} = source)")]
    [InlineData("({...[]} = source)")]
    [InlineData("({a: {...rest,}} = source)")]
    public void Rest_must_be_last_without_a_trailing_comma_or_initializer(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("[(a)] = values")]
    [InlineData("[((a))] = values")]
    [InlineData("[(object.value)] = values")]
    [InlineData("[(object?.value).next] = values")]
    [InlineData("[...(rest)] = values")]
    [InlineData("[...(object.value)] = values")]
    [InlineData("({name: (target)} = source)")]
    [InlineData("({name: ((target)) = fallback} = source)")]
    [InlineData("({...(rest)} = source)")]
    [InlineData("({...(object.value)} = source)")]
    [InlineData("([a] = values)")]
    [InlineData("(({a} = source))")]
    [InlineData("[([a] = values).value] = others")]
    [InlineData("({a: ({b} = source).value} = other)")]
    public void Parenthesized_simple_targets_are_distinct_from_parenthesized_patterns(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("([a]) = values")]
    [InlineData("(([])) = values")]
    [InlineData("({a}) = source")]
    [InlineData("(({})) = source")]
    [InlineData("[([a])] = values")]
    [InlineData("[({a})] = values")]
    [InlineData("[...([a])] = values")]
    [InlineData("[...({a})] = values")]
    [InlineData("[(a = fallback)] = values")]
    [InlineData("[(a, b)] = values")]
    [InlineData("({a: ([b])} = source)")]
    [InlineData("({a: ({b})} = source)")]
    [InlineData("({a: (b = fallback)} = source)")]
    [InlineData("({...({a})} = source)")]
    public void Parentheses_cannot_turn_an_array_object_or_assignment_expression_into_a_simple_target(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("({__proto__: a, __proto__: b} = source)")]
    [InlineData("({'__proto__': a, '__proto__': b} = source)")]
    [InlineData("({__proto__: a, '\\u005f_proto__': b} = source)")]
    [InlineData("({__proto__: a, \\u005f_proto__: b} = source)")]
    [InlineData("({a: {__proto__: b, __proto__: c}} = source)")]
    [InlineData("[{__proto__: a, __proto__: b}] = values")]
    [InlineData("[...{__proto__: a, __proto__: b}] = values")]
    [InlineData("({a = ({__proto__: b, __proto__: c} = source)} = other)")]
    [InlineData("({[({__proto__: a, __proto__: b} = source)]: target} = other)")]
    public void Duplicate_proto_property_names_are_allowed_in_assignment_patterns(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("({__proto__: a, __proto__: b})")]
    [InlineData("({a = {__proto__: b, __proto__: c}} = source)")]
    [InlineData("[a = {__proto__: b, __proto__: c}] = values")]
    [InlineData("({[{__proto__: b, __proto__: c}]: a} = source)")]
    [InlineData("({a: object[{__proto__: b, __proto__: c}]} = source)")]
    [InlineData("({a: {b = {__proto__: c, __proto__: d}}} = source)")]
    [InlineData("({a} = {__proto__: b, __proto__: c})")]
    public void Ordinary_object_literals_inside_defaults_keys_targets_or_rhs_keep_proto_early_errors(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("({a = 1})")]
    [InlineData("{a = 1}")]
    [InlineData("[{a = 1}]")]
    [InlineData("f({a = 1})")]
    [InlineData("({a = 1}).value")]
    [InlineData("({a = 1}).value = source")]
    [InlineData("[a = ({b = 1})] = values")]
    [InlineData("({a = {b = 1}} = source)")]
    [InlineData("({[{b = 1}]: a} = source)")]
    [InlineData("({a: object[{b = 1}]} = source)")]
    [InlineData("({a} = {b = 1})")]
    [InlineData("({a: target = {b = 1}} = source)")]
    [InlineData("({a: ({b = 1})} = source)")]
    [InlineData("[...({a = 1})] = values")]
    [InlineData("tag`${{a = 1}}`")]
    public void Cover_initialized_names_are_rejected_everywhere_an_ordinary_object_literal_is_required(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("({a = ({b = 1} = nested)} = source)")]
    [InlineData("[a = ({b = 1} = nested)] = values")]
    [InlineData("({[({key = 1} = nested)]: target} = source)")]
    [InlineData("({a: object[({key = 1} = nested)]} = source)")]
    [InlineData("({a = [{b = 1}] = nested} = source)")]
    [InlineData("({a} = ({b = 1} = nested))")]
    [InlineData("tag`${({a = 1} = source)}`")]
    [InlineData("condition ? ({a = 1} = source) : ([b] = values)")]
    [InlineData("f(({a = 1} = source), [b] = values)")]
    [InlineData("({a} = source, [b] = values)")]
    public void Nested_assignment_contexts_allow_cover_names_without_leaking_into_neighboring_literals(string input)
        => _fixture.Success(input);

    public static IEnumerable<object[]> NonAssignmentOperators()
    {
        foreach (string op in new[] { "+=", "-=", "*=", "/=", "%=", "**=", "<<=", ">>=", ">>>=",
                     "&=", "^=", "|=", "&&=", "||=", "??=" })
        {
            yield return new object[] { "[a] " + op + " values" };
            yield return new object[] { "({a} " + op + " source)" };
        }
        foreach (string pattern in new[] { "[a]", "({a})" })
        foreach (string op in new[] { "++", "--" })
        {
            yield return new object[] { op + pattern };
            yield return new object[] { pattern + op };
        }
    }

    [Theory]
    [MemberData(nameof(NonAssignmentOperators))]
    public void Only_plain_equals_can_reinterpret_a_literal_as_an_assignment_pattern(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("[a += 1] = values")]
    [InlineData("[a ||= fallback] = values")]
    [InlineData("[a + b] = values")]
    [InlineData("[a ? b : c] = values")]
    [InlineData("[...a + b] = values")]
    [InlineData("({a: b += 1} = source)")]
    [InlineData("({a: b ?? fallback} = source)")]
    [InlineData("({a: b ? c : d} = source)")]
    [InlineData("({[first, second]: target} = source)")]
    [InlineData("({a: ...rest} = source)")]
    [InlineData("[... ] = values")]
    [InlineData("({a:} = source)")]
    [InlineData("({,a} = source)")]
    [InlineData("[a = ] = values")]
    [InlineData("({a = } = source)")]
    public void Malformed_pattern_elements_are_not_accepted_as_arbitrary_expressions(string input)
        => _fixture.Invalid(input);

    public static IEnumerable<object[]> InvalidTargetPositions()
    {
        foreach (string target in new[] { "1", "true", "null", "this", "a?.member", "a?.[key]", "f()",
                     "new C()", "tag`text`", "`text`", "eval", "arguments", "\\u0065val", "\\u0061rguments" })
        {
            yield return new object[] { "[" + target + "] = values" };
            yield return new object[] { "[..." + target + "] = values" };
            yield return new object[] { "({name: " + target + "} = source)" };
            yield return new object[] { "({..." + target + "} = source)" };
        }
    }

    [Theory]
    [MemberData(nameof(InvalidTargetPositions))]
    public void Every_destructuring_target_obeys_strict_simple_assignment_target_rules(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("[eval.value, arguments[index]] = values")]
    [InlineData("({eval: target, arguments: other} = source)")]
    [InlineData("({eval: eval.value, arguments: arguments[index]} = source)")]
    [InlineData("[a = eval, b = arguments] = values")]
    [InlineData("({[eval]: target, [arguments]: other} = source)")]
    [InlineData("[\\u0061, 한글, $value, await] = values")]
    [InlineData("({\\u0061 = fallback, 한글: \\u0062} = source)")]
    [InlineData("({\\u0069f: target} = source)")]
    [InlineData("[a = this, b = null, c = true] = values")]
    [InlineData("[null.value, true.member, 1..member] = values")]
    [InlineData("[a] = null")]
    [InlineData("({a} = undefined)")]
    [InlineData("[] = 1")]
    public void Strict_target_restrictions_do_not_reject_property_names_references_or_runtime_only_errors(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("({eval} = source)")]
    [InlineData("({arguments = fallback} = source)")]
    [InlineData("({\\u0065val = fallback} = source)")]
    [InlineData("[\\u0069f] = values")]
    [InlineData("({\\u0073tatic} = source)")]
    [InlineData("({yield} = source)")]
    [InlineData("({let = fallback} = source)")]
    [InlineData("[a = eval = value] = values")]
    [InlineData("({[arguments++]: a} = source)")]
    [InlineData("[a = delete value] = values")]
    public void Strict_identifier_and_nested_expression_errors_remain_visible_inside_patterns(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("[a = /a/.test(text), b = value / divisor] = values")]
    [InlineData("({[/a/.source]: a = value / divisor} = source)")]
    [InlineData("({[value / divisor]: a = /a/} = source)")]
    [InlineData("({a = `hello ${name}`, b: target = tag`raw\\xZ`} = source)")]
    [InlineData("({[`key${name}`]: target = `value${other}`} = source)")]
    [InlineData("`${({a = /a/} = source)}`")]
    [InlineData("`${[a = `inner${value}`] = values}`")]
    [InlineData("`${({a: {b = `inner${{c: 1}.c}`}} = source)}`")]
    [InlineData("`outer${({[`${key}`]: a} = source)}tail`")]
    [InlineData("[object[`key${name}`]] = values")]
    [InlineData("[a = tag`raw${({b = 1} = source)}\\uZ`] = values")]
    public void Defaults_and_computed_keys_keep_template_brace_and_division_regex_contexts(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("({a = `\\xZ`} = source)")]
    [InlineData("({[`\\uZ`]: target} = source)")]
    [InlineData("[a = /a/gg] = values")]
    [InlineData("`${({a = {b = 1}} = source)}`")]
    [InlineData("`${({...rest,} = source)}`")]
    public void Pattern_context_does_not_suppress_nested_lexical_or_template_errors(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("[...items, next]")]
    [InlineData("[...items,]")]
    [InlineData("[...items = fallback]")]
    [InlineData("({...first, a, ...second,})")]
    [InlineData("({...{a}})")]
    [InlineData("({...[a]})")]
    [InlineData("f(...items, next)")]
    [InlineData("({a: [1, 2], b: {c: 3}})")]
    public void Ordinary_spread_literals_and_arguments_keep_their_existing_rules(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("  [object?.member] = values", "object?.member")]
    [InlineData("  ({name: arguments} = source)", "arguments")]
    [InlineData("  [a, [eval]] = values", "eval")]
    [InlineData("  [a = arguments = next] = values", "arguments")]
    public void Invalid_leaf_target_diagnostics_identify_the_original_target_range(string input, string target)
    {
        var result = _fixture.Parse(input);
        Assert.True(result.Success, _fixture.Parser.FiredException?.ToString());
        var diagnostic = Assert.Single(_fixture.Language.Validate(result.ToParseTree));
        Assert.Equal("invalid_assignment_target", diagnostic.Code);
        Assert.Equal(input.IndexOf(target, StringComparison.Ordinal), diagnostic.SourceOffset);
        Assert.Equal(target.Length, diagnostic.Length);
        Assert.Equal(target, input.Substring(diagnostic.SourceOffset, diagnostic.Length));
    }

    [Fact]
    public void Pattern_validation_is_repeatable_and_does_not_reclassify_a_later_literal()
    {
        const string patternInput = "({__proto__: a, __proto__: b, name = `hello${value}`} = source)";
        var pattern = _fixture.Success(patternInput);
        string before = JavaScriptExpressionFixture.Text(pattern);
        var tokenReferences = pattern.AllTokens.ToArray();
        Assert.Empty(_fixture.Language.Validate(pattern));
        Assert.Empty(_fixture.Language.Validate(pattern));

        // Identical property spellings in a fresh parse must regain literal early errors.
        _fixture.Invalid("({__proto__: a, __proto__: b})");
        _fixture.Invalid("({name = `hello${value}`})");
        _fixture.Invalid("[...rest,] = values");
        var again = _fixture.Success(patternInput);
        Assert.Equal(before, JavaScriptExpressionFixture.Text(pattern));
        Assert.Equal(before, JavaScriptExpressionFixture.Text(again));
        Assert.NotSame(pattern, again);
        Assert.NotSame(tokenReferences[0], again.AllTokens[0]);
        Assert.Empty(_fixture.Language.Validate(pattern));
        _fixture.Success("/a/.test(value) / divisor");
    }

    [Fact]
    public void Pattern_tree_preserves_elisions_defaults_computed_keys_and_source_offsets()
    {
        const string input = "  [ first,, { [key]: target = `hello${name}`, ...rest } ] = values  ";
        var tree = _fixture.Success(input);
        var tokens = tree.AllTokens;
        Assert.Equal(new[] { "[", "first", ",", ",", "{", "[", "key", "]", ":", "target", "=",
            "`hello${", "name", "}`", ",", "...", "rest", "}", "]", "=", "values" },
            tokens.Select(token => token.Input));
        foreach (var token in tokens)
            Assert.Equal(token.Input, input.Substring(token.StartIndex, token.EndIndex - token.StartIndex + 1));
        Assert.Contains(JavaScriptExpressionFixture.Nodes(tree), node => node.SignPost.Name == "ArrayLiteral");
        Assert.Contains(JavaScriptExpressionFixture.Nodes(tree), node => node.SignPost.Name == "ObjectLiteral");
        Assert.Contains(JavaScriptExpressionFixture.Nodes(tree), node => node.SignPost.Name == "TemplateLiteral");
    }

    [Fact]
    public void Nested_and_wide_patterns_finish_without_losing_their_validation_context()
    {
        string nested = "value";
        for (int depth = 0; depth < 20; depth++) nested = depth % 2 == 0 ? "[" + nested + "]" : "{a: " + nested + "}";
        _fixture.Success("(" + nested + " = source)");
        string wide = "[" + string.Join(",", Enumerable.Range(0, 64).Select(index => "a" + index + " = " + index)) + "] = values";
        _fixture.Success(wide);
        _fixture.Invalid(wide.Replace("a63 = 63", "a63 += 63"));
    }

    [Fact]
    public void Deep_rest_initializer_diagnostic_covers_the_whole_assignment_without_surrounding_trivia()
    {
        string initializer = "rest = " + new string('(', 64) + "fallback" + new string(')', 64);
        string input = "  [ ... " + initializer + "  ] = values  ";
        AssertDiagnosticRange(input, "invalid_rest_element", initializer);
    }

    [Fact]
    public void Forbidden_deep_object_rest_pattern_diagnostic_covers_the_whole_nested_pattern()
    {
        string target = "value";
        for (int depth = 0; depth < 64; depth++) target = "{name: " + target + "}";
        string input = "  ({ ... " + target + "  } = source)  ";
        AssertDiagnosticRange(input, "invalid_assignment_target", target);
    }

    [Fact]
    public void Nonfinal_wide_array_rest_diagnostic_includes_every_nested_element_and_the_spread_prefix()
    {
        string rest = "...[" + string.Join(", ", Enumerable.Range(0, 128).Select(index => "target" + index)) + "]";
        string input = "  [ " + rest + ", tail ] = values  ";
        AssertDiagnosticRange(input, "invalid_rest_element", rest);
    }

    private void AssertDiagnosticRange(string input, string code, string sourceRange)
    {
        var result = _fixture.Parse(input);
        Assert.True(result.Success, _fixture.Parser.FiredException?.ToString());
        var diagnostic = Assert.Single(_fixture.Language.Validate(result.ToParseTree));
        Assert.Equal(code, diagnostic.Code);
        Assert.Equal(input.IndexOf(sourceRange, StringComparison.Ordinal), diagnostic.SourceOffset);
        Assert.Equal(sourceRange.Length, diagnostic.Length);
        Assert.Equal(sourceRange, input.Substring(diagnostic.SourceOffset, diagnostic.Length));
    }
}
