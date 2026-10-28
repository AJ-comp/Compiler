using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.ParseTree;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

[CollectionDefinition(Name)]
public sealed class JavaScriptGrammarCollection : ICollectionFixture<JavaScriptGrammarFixture>
{
    public const string Name = "Unified JavaScript grammar API";
}

public sealed class JavaScriptGrammarFixture
{
    private readonly Dictionary<(JavaScriptInputKind, JavaScriptMode, bool),
        (JavaScriptGrammar Language, LALRParser Parser)> profiles = new();

    public (JavaScriptGrammar Language, LALRParser Parser) Profile(JavaScriptInputKind kind,
        JavaScriptMode mode = JavaScriptMode.Strict, bool web = false)
    {
        var key = (kind, mode, web);
        if (!profiles.TryGetValue(key, out var profile))
        {
            // Exercise the actual parameterless entry point for the ordinary default.
            var language = key == (JavaScriptInputKind.Script, JavaScriptMode.Strict, false)
                ? new JavaScriptGrammar()
                : new JavaScriptGrammar(mode: mode, webCompatibility: web, inputKind: kind);
            profile = (language, new LALRParser(language.Grammar, false));
            profiles.Add(key, profile);
        }
        return profile;
    }

    public ParseTreeNonTerminal Accept(JavaScriptInputKind kind, string input,
        JavaScriptMode mode = JavaScriptMode.Strict, bool web = false)
    {
        var profile = Profile(kind, mode, web);
        var parsed = profile.Parser.Parsing(profile.Language.CreateTokenSource(input));
        Assert.True(parsed.Success, input + Environment.NewLine + profile.Parser.FiredException);
        Assert.Empty(parsed.AllErrors);
        var tree = Assert.IsType<ParseTreeNonTerminal>(parsed.ToParseTree);
        var diagnostics = profile.Language.Validate(tree);
        Assert.True(diagnostics.Count == 0, input + Environment.NewLine +
            string.Join(Environment.NewLine, diagnostics.Select(d => d.Code + ": " + d.Message)));
        return tree;
    }

    public void Reject(JavaScriptInputKind kind, string input,
        JavaScriptMode mode = JavaScriptMode.Strict, bool web = false)
    {
        var profile = Profile(kind, mode, web);
        var parsed = profile.Parser.Parsing(profile.Language.CreateTokenSource(input));
        if (!parsed.Success)
        {
            Assert.NotEmpty(parsed.AllErrors);
            return;
        }
        Assert.NotEmpty(profile.Language.Validate(parsed.ToParseTree));
    }
}

[Collection(JavaScriptGrammarCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptUnifiedApi")]
public sealed class JavaScriptGrammarTests
{
    private readonly JavaScriptGrammarFixture fixture;
    public JavaScriptGrammarTests(JavaScriptGrammarFixture fixture) => this.fixture = fixture;

    [Fact]
    public void Default_entry_point_reads_a_complete_strict_Script()
    {
        var profile = fixture.Profile(JavaScriptInputKind.Script);
        Assert.Equal(JavaScriptInputKind.Script, profile.Language.InputKind);
        Assert.Equal(JavaScriptMode.Strict, profile.Language.Mode);
        Assert.False(profile.Language.WebCompatibility);
        Assert.Equal(2026, JavaScriptGrammar.Edition);
        var tree = fixture.Accept(JavaScriptInputKind.Script,
            "const price = 100; const count = 3; const total = price * count; console.log(total);");
        Assert.Equal("Script", tree.SignPost.Name);
        fixture.Reject(JavaScriptInputKind.Script, "with (settings) { value; }");
    }

    [Theory]
    [InlineData(JavaScriptInputKind.Script, "")]
    [InlineData(JavaScriptInputKind.Script, "value;")]
    [InlineData(JavaScriptInputKind.Module, "")]
    [InlineData(JavaScriptInputKind.Module, "value;")]
    [InlineData(JavaScriptInputKind.Expression, "value")]
    public void Input_kind_preserves_the_tree_root_used_by_context_validation(JavaScriptInputKind kind, string input)
    {
        Assert.Equal(kind, fixture.Profile(kind).Language.InputKind);
        Assert.Equal(kind.ToString(), fixture.Accept(kind, input).SignPost.Name);
    }

    [Theory]
    [InlineData(JavaScriptInputKind.Script, "const value = 1; next(value);", true)]
    [InlineData(JavaScriptInputKind.Module, "const value = 1; next(value);", true)]
    [InlineData(JavaScriptInputKind.Expression, "const value = 1; next(value);", false)]
    [InlineData(JavaScriptInputKind.Script, "import value from 'dependency'; export {value};", false)]
    [InlineData(JavaScriptInputKind.Module, "import value from 'dependency'; export {value};", true)]
    [InlineData(JavaScriptInputKind.Expression, "import value from 'dependency'; export {value};", false)]
    [InlineData(JavaScriptInputKind.Script, "import.meta", false)]
    [InlineData(JavaScriptInputKind.Module, "import.meta", true)]
    [InlineData(JavaScriptInputKind.Expression, "import.meta", false)]
    [InlineData(JavaScriptInputKind.Script, "await work();", false)]
    [InlineData(JavaScriptInputKind.Module, "await work();", true)]
    [InlineData(JavaScriptInputKind.Expression, "await work()", false)]
    [InlineData(JavaScriptInputKind.Script, "import('dependency')", true)]
    [InlineData(JavaScriptInputKind.Module, "import('dependency')", true)]
    [InlineData(JavaScriptInputKind.Expression, "import('dependency')", true)]
    [InlineData(JavaScriptInputKind.Expression, "", false)]
    [InlineData(JavaScriptInputKind.Expression, "value;", false)]
    [InlineData(JavaScriptInputKind.Expression, "value; next;", false)]
    public void One_public_type_keeps_input_specific_acceptance_rules(JavaScriptInputKind kind, string input, bool valid)
    {
        if (valid) fixture.Accept(kind, input);
        else fixture.Reject(kind, input);
    }

    [Theory]
    [InlineData(JavaScriptInputKind.Script, "with (settings) { value; }")]
    [InlineData(JavaScriptInputKind.Script, "var total = 077;")]
    [InlineData(JavaScriptInputKind.Expression, "077 + value")]
    [InlineData(JavaScriptInputKind.Expression, "eval = 1")]
    public void Non_strict_mode_remains_available_for_Script_and_expression_input(JavaScriptInputKind kind, string input)
    {
        Assert.Equal(JavaScriptMode.NonStrict, fixture.Profile(kind, JavaScriptMode.NonStrict).Language.Mode);
        fixture.Accept(kind, input, JavaScriptMode.NonStrict);
        fixture.Reject(kind, input);
    }

    [Theory]
    [InlineData(false, "with (settings) { value; }")]
    [InlineData(true, "with (settings) { value; }")]
    [InlineData(false, "var total = 077;")]
    [InlineData(true, "var total = 077;")]
    [InlineData(false, "var eval = 1;")]
    [InlineData(true, "var eval = 1;")]
    public void Module_input_always_applies_strict_checks_even_when_NonStrict_is_requested(bool web, string input)
    {
        var profile = fixture.Profile(JavaScriptInputKind.Module, JavaScriptMode.NonStrict, web);
        Assert.Equal(JavaScriptInputKind.Module, profile.Language.InputKind);
        Assert.Equal(JavaScriptMode.Strict, profile.Language.Mode);
        Assert.Equal(web, profile.Language.WebCompatibility);
        fixture.Reject(JavaScriptInputKind.Module, input, JavaScriptMode.NonStrict, web);
    }

    [Fact]
    public void Combining_statement_and_module_rules_does_not_allow_with_and_export_in_one_input()
    {
        const string input = "with (settings) { value; } export const result = 1;";
        fixture.Reject(JavaScriptInputKind.Script, input, JavaScriptMode.NonStrict, web: true);
        fixture.Reject(JavaScriptInputKind.Module, input, JavaScriptMode.NonStrict, web: true);
    }

    [Fact]
    public void Module_web_compatibility_keeps_its_own_comment_and_catch_rules()
    {
        const string legacyCatch = "try { read(); } catch (error) { var error; }";
        fixture.Accept(JavaScriptInputKind.Module, legacyCatch, JavaScriptMode.NonStrict, web: true);
        fixture.Reject(JavaScriptInputKind.Module, legacyCatch, JavaScriptMode.NonStrict);
        const string htmlComment = "<!-- comment\nvalue;";
        fixture.Accept(JavaScriptInputKind.Script, htmlComment, JavaScriptMode.NonStrict, web: true);
        fixture.Reject(JavaScriptInputKind.Module, htmlComment, JavaScriptMode.NonStrict, web: true);
        fixture.Reject(JavaScriptInputKind.Module, htmlComment, JavaScriptMode.NonStrict);
    }

    [Fact]
    public void Input_kinds_and_modes_keep_separate_grammars_policies_and_sources()
    {
        var script = fixture.Profile(JavaScriptInputKind.Script).Language;
        var module = fixture.Profile(JavaScriptInputKind.Module).Language;
        var expression = fixture.Profile(JavaScriptInputKind.Expression).Language;
        Assert.NotSame(script.Grammar, module.Grammar);
        Assert.NotSame(module.Grammar, expression.Grammar);
        Assert.NotSame(script.Policy, module.Policy);
        const string input = "const regex = /ab/g;";
        var scriptSource = script.CreateTokenSource(input);
        var moduleSource = module.CreateTokenSource(input);
        Assert.Same(script.Policy, scriptSource.Policy);
        Assert.Same(module.Policy, moduleSource.Policy);
        Assert.NotSame(scriptSource.Start.SourceIdentity, moduleSource.Start.SourceIdentity);
        var first = fixture.Accept(JavaScriptInputKind.Script, input);
        fixture.Accept(JavaScriptInputKind.Module, input);
        fixture.Accept(JavaScriptInputKind.Expression, "value / 2");
        Assert.Empty(script.Validate(first));
        Assert.Equal("Script", fixture.Accept(JavaScriptInputKind.Script, input).SignPost.Name);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    [InlineData(int.MaxValue)]
    public void Invalid_input_kinds_are_rejected_before_preparing_grammar(int value)
        => Assert.Equal("inputKind", Assert.Throws<ArgumentOutOfRangeException>(() =>
            new JavaScriptGrammar(inputKind: (JavaScriptInputKind)value)).ParamName);

    [Theory]
    [InlineData(JavaScriptInputKind.Script)]
    [InlineData(JavaScriptInputKind.Module)]
    [InlineData(JavaScriptInputKind.Expression)]
    public void Invalid_modes_are_rejected_for_every_input_kind(JavaScriptInputKind kind)
        => Assert.Equal("mode", Assert.Throws<ArgumentOutOfRangeException>(() =>
            new JavaScriptGrammar((JavaScriptMode)17, inputKind: kind)).ParamName);
}
