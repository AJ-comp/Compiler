using Janglim.FrontEnd;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.Tests.JavaScript;
using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.TypeScript;

[Trait("Category", "TypeScript")]
[Trait("Feature", "TypeScriptProjection")]
[Collection(JavaScriptGrammarCollection.Name)]
public sealed class TypeScriptProjectionTests
{
    private readonly JavaScriptGrammarFixture fixture;
    public TypeScriptProjectionTests(JavaScriptGrammarFixture fixture) => this.fixture = fixture;

    [Fact]
    public void Annotation_is_removed_only_from_the_validation_view()
    {
        var identifier = Node("BindingIdentifier", Token("value", 6));
        var annotation = Node("TSAnnotation", Token(":", 11), Node("TSType", Token("number", 13)));
        var original = Node("LexicalBinding", identifier, annotation);
        var before = Descendants(original).ToArray();
        var parents = before.Select(node => node.Parent).ToArray();
        var tokens = original.AllTokens.ToArray();

        var projected = Assert.IsType<ParseTreeNonTerminal>(Project(original));

        Assert.Equal("LexicalBinding", projected.SignPost.Name);
        Assert.Single(projected.Items);
        Assert.Equal("value", Assert.Single(projected.AllTokens).Input);
        Assert.Equal(new[] { "value", ":", "number" }, original.AllTokens.Select(token => token.Input));
        for (int i = 0; i < before.Length; i++) Assert.Same(parents[i], before[i].Parent);
        Assert.Equal(tokens, original.AllTokens);
        Assert.NotSame(original, projected);
        Assert.NotSame(identifier, projected[0]);
        Assert.Same(identifier[0].AllTokens[0], projected[0].AllTokens[0]);
        Assert.Same(projected, projected[0].Parent);
        Assert.Null(projected.Parent);
    }

    [Theory]
    [InlineData("TSAnnotation")]
    [InlineData("TSReturnType")]
    [InlineData("TSTypeParameters")]
    [InlineData("TSTypeArguments")]
    [InlineData("TSOptional")]
    [InlineData("TSDefinite")]
    [InlineData("TSModifiers")]
    [InlineData("TSThisParameter")]
    public void Type_syntax_wrappers_are_not_mistaken_for_runtime_children(string role)
    {
        var original = Node("FormalParameter", Node("BindingIdentifier", Token("value", 10)),
            Node(role, Token("typeOnly", 30)));
        var projected = Assert.IsType<ParseTreeNonTerminal>(Project(original));
        Assert.Single(projected.Items);
        Assert.Equal("BindingIdentifier", Assert.IsType<ParseTreeNonTerminal>(projected[0]).SignPost.Name);
        Assert.Equal("value", Assert.Single(projected.AllTokens).Input);
    }

    [Theory]
    [InlineData("TSAsExpression")]
    [InlineData("TSSatisfiesExpression")]
    [InlineData("TSNonNullExpression")]
    [InlineData("TSTypeAssertion")]
    [InlineData("TSAsExpressionNoIn")]
    [InlineData("TSTypeAssertionNoIn__Yield__Await")]
    public void Runtime_assertions_recover_the_operand_without_reparsing_or_relocating_it(string role)
    {
        var operand = Node("IdentifierReference", Token("value", 21));
        var type = Node("TSType", Token("Value", 11));
        var original = role.StartsWith("TSTypeAssertion", StringComparison.Ordinal)
            ? Node(role, Token("<", 10), type, Token(">", 16), operand)
            : Node(role, operand, Token(role == "TSNonNullExpression" ? "!" : "as", 26), type);

        var projected = Assert.IsType<ParseTreeNonTerminal>(Project(original));

        Assert.Equal("IdentifierReference", projected.SignPost.Name);
        Assert.Same(operand.AllTokens[0], Assert.Single(projected.AllTokens));
        Assert.Equal(21, projected.AllTokens[0].StartIndex);
        Assert.Same(original, operand.Parent);
        Assert.Null(projected.Parent);
    }

    [Fact]
    public void Nested_assertions_preserve_the_runtime_tree_and_original_parent_links()
    {
        var operand = Node("IdentifierReference", Token("value", 5));
        var inner = Node("TSNonNullExpression", operand, Token("!", 10));
        var outer = Node("TSSatisfiesExpression", inner, Token("satisfies", 12), Node("TSType", Token("T", 22)));
        var original = Node("Expression", outer);
        var projected = Assert.IsType<ParseTreeNonTerminal>(Project(original));
        Assert.Equal("IdentifierReference", Assert.IsType<ParseTreeNonTerminal>(Assert.Single(projected.Items)).SignPost.Name);
        Assert.Same(inner, operand.Parent);
        Assert.Same(outer, inner.Parent);
        Assert.Same(original, outer.Parent);
    }

    [Fact]
    public void Empty_predicate_nodes_are_not_mistaken_for_a_second_assertion_operand()
    {
        var operand = Node("IdentifierReference", Token("value", 2));
        var original = Node("TSNonNullExpression", operand, Node("NoLineTerminator"), Token("!", 7));
        var projected = Assert.IsType<ParseTreeNonTerminal>(Project(original));
        Assert.Equal("IdentifierReference", projected.SignPost.Name);
        Assert.Same(operand.AllTokens[0], Assert.Single(projected.AllTokens));
    }

    [Theory]
    [InlineData("TSTypeAliasDeclaration")]
    [InlineData("TSInterfaceDeclaration")]
    [InlineData("TSAmbientDeclaration")]
    [InlineData("TSFunctionOverload")]
    [InlineData("TSMethodOverload")]
    [InlineData("TSTypeOnlyImport")]
    [InlineData("TSTypeOnlyExport")]
    public void Type_only_declarations_are_excluded_from_JavaScript_binding_analysis(string role)
    {
        var original = Node("Script", Node(role, Node("BindingIdentifier", Token("typeName", 1))),
            Node("ExpressionStatement", Node("IdentifierReference", Token("runtimeName", 25)), Token(";", 36)));
        var projected = Assert.IsType<ParseTreeNonTerminal>(Project(original));
        Assert.Equal(new[] { "runtimeName", ";" }, projected.AllTokens.Select(token => token.Input));
        Assert.Equal(2, original.Count);
        Assert.Single(projected.Items);
    }

    [Fact]
    public void Await_and_yield_rule_suffixes_do_not_hide_type_only_roles()
    {
        var original = Node("FormalParameter__Yield__Await", Node("BindingIdentifier__Yield__Await", Token("value", 0)),
            Node("TSAnnotation__Yield__Await", Token(":", 5), Node("TSType__Yield__Await", Token("T", 6))));
        var projected = Assert.IsType<ParseTreeNonTerminal>(Project(original));
        Assert.Equal("FormalParameter__Yield__Await", projected.SignPost.Name);
        Assert.Single(projected.Items);
        Assert.Single(projected.AllTokens);
    }

    [Fact]
    public void Structurally_equal_nodes_at_different_offsets_remain_distinct()
    {
        var first = Node("IdentifierReference", Token("same", 3));
        var second = Node("IdentifierReference", Token("same", 40));
        var projected = Assert.IsType<ParseTreeNonTerminal>(Project(Node("Expression", first, Token(",", 7), second)));
        Assert.NotSame(projected[0], projected[2]);
        Assert.Equal(new[] { 3, 7, 40 }, projected.AllTokens.Select(token => token.StartIndex));
        Assert.Same(first.AllTokens[0], projected[0].AllTokens[0]);
        Assert.Same(second.AllTokens[0], projected[2].AllTokens[0]);
    }

    [Fact]
    public void Projection_uses_an_iterative_walk_for_deep_concrete_trees()
    {
        ParseTreeSymbol original = Token("value", 12);
        const int depth = 20000;
        for (int i = 0; i < depth; i++) original = Node("Expression", original);
        var projected = Project(original);
        int count = 0;
        while (projected is ParseTreeNonTerminal node)
        {
            count++;
            projected = Assert.Single(node.Items);
        }
        Assert.Equal(depth, count);
        Assert.Equal(12, Assert.IsType<ParseTreeTerminal>(projected).Token.StartIndex);
    }

    [Fact]
    public void Static_error_positions_refer_to_the_original_source_after_type_syntax_is_omitted()
    {
        var invalidTarget = Node("NumericLiteral", Token("1", 46, "NumericLiteral"));
        var assignment = Node("AssignmentExpression", invalidTarget,
            Node("AssignmentOperator", Token("=", 48)), Node("NumericLiteral", Token("2", 50, "NumericLiteral")));
        var original = Node("Script", Node("TSInterfaceDeclaration", Token("interface", 0), Token("Shape", 10)),
            Node("StatementList", Node("StatementListItem", Node("ExpressionStatement", assignment, Token(";", 51)))));

        var diagnostics = Validate(original);

        var diagnostic = Assert.Single(diagnostics, item => item.Code == "invalid_assignment_target");
        Assert.Equal(46, diagnostic.SourceOffset);
        Assert.Equal(1, diagnostic.Length);
        Assert.Same(assignment, invalidTarget.Parent);
        Assert.Equal(2, original.Count);
    }

    [Fact]
    public void A_standalone_type_has_no_runtime_validation_tree()
    {
        var original = Node("TSType", Node("TSTypeReference", Token("UnknownType", 7)));
        Assert.Null(Project(original));
        Assert.Empty(Validate(original));
    }

    [Fact]
    public void Required_parameters_after_optional_parameters_are_a_grammar_error()
    {
        var optional = Node("FormalParameter", Node("BindingIdentifier", Token("first", 1)), Node("TSOptional", Token("?", 6)));
        var required = Node("FormalParameter", Node("BindingIdentifier", Token("second", 9)));
        var original = Node("Script", Node("FormalParameters", Node("FormalParameterList", optional, Token(",", 7), required)));
        Assert.Contains(Validate(original), diagnostic => diagnostic.Code == "typescript_required_after_optional");
    }

    [Fact]
    public void An_optional_property_inside_a_parameter_type_does_not_make_the_parameter_optional()
    {
        var first = Node("FormalParameter", Node("BindingIdentifier", Token("first", 1)),
            Node("TSAnnotation", Token(":", 6), Node("TSType", Node("TSTypeProperty", Token("property", 9), Node("TSOptional", Token("?", 17))))));
        var second = Node("FormalParameter", Node("BindingIdentifier", Token("second", 21)));
        var original = Node("Script", Node("FormalParameters", Node("FormalParameterList", first, Token(",", 19), second)));
        Assert.Empty(Validate(original));
    }

    [Fact]
    public void An_optional_parameter_cannot_also_have_an_initializer()
    {
        var parameter = Node("FormalParameter", Node("BindingIdentifier", Token("value", 1)),
            Node("TSOptional", Token("?", 6)), Node("Initializer", Token("=", 8), Node("NumericLiteral", Token("1", 10, "NumericLiteral"))));
        Assert.Contains(Validate(Node("Script", Node("FormalParameters", parameter))),
            diagnostic => diagnostic.Code == "typescript_optional_initializer");
    }

    [Fact]
    public void A_default_initializer_after_an_optional_parameter_remains_permitted()
    {
        var optional = Node("FormalParameter", Node("BindingIdentifier", Token("first", 1)), Node("TSOptional", Token("?", 6)));
        var initialized = Node("FormalParameter", Node("BindingIdentifier", Token("second", 9)),
            Node("Initializer", Token("=", 16), Node("NumericLiteral", Token("1", 18, "NumericLiteral"))));
        Assert.Empty(Validate(Node("Script", Node("FormalParameters", Node("FormalParameterList", optional, Token(",", 7), initialized)))));
    }

    [Theory]
    [InlineData("TSOptional", "typescript_optional_rest")]
    [InlineData("Initializer", "typescript_rest_initializer")]
    public void Rest_parameter_restrictions_are_checked_before_projection(string role, string code)
    {
        var parameter = Node("FunctionRestParameter", Token("...", 1), Node("BindingIdentifier", Token("rest", 4)), Node(role, Token("?", 8)));
        Assert.Contains(Validate(Node("Script", Node("FormalParameters", parameter))), diagnostic => diagnostic.Code == code);
    }

    [Fact]
    public void A_this_pseudo_parameter_must_precede_runtime_parameters()
    {
        var runtime = Node("FormalParameter", Node("BindingIdentifier", Token("value", 1)));
        var pseudo = Node("TSThisParameter", Token("this", 9), Node("TSAnnotation", Token(":", 13), Node("TSType", Token("Context", 15))));
        var original = Node("Script", Node("FormalParameters", Node("FormalParameterList", runtime, Token(",", 7), pseudo)));
        Assert.Contains(Validate(original), diagnostic => diagnostic.Code == "typescript_this_parameter_position");
    }

    [Theory]
    [InlineData("ForInStatement")]
    [InlineData("ForOfStatement")]
    [InlineData("ForAwaitOfStatement")]
    public void For_in_and_for_of_declarations_cannot_use_type_annotations(string role)
    {
        var declaration = Node("ForDeclaration", Token("const", 5), Node("BindingIdentifier", Token("value", 11)),
            Node("TSAnnotation", Token(":", 16), Node("TSType", Token("T", 18))));
        Assert.Contains(Validate(Node("Script", Node(role, declaration))),
            diagnostic => diagnostic.Code == "typescript_for_type_annotation");
    }

    [Fact]
    public void Ambient_function_implementations_are_rejected_without_treating_the_body_as_runtime_code()
    {
        var original = Node("Script", Node("TSAmbientDeclaration", Token("declare", 0),
            Node("FunctionDeclaration", Token("function", 8), Node("FunctionBody", Token("{", 20), Token("}", 21)))));
        var diagnostics = Validate(original);
        Assert.Single(diagnostics);
        Assert.Equal("typescript_ambient_implementation", diagnostics[0].Code);
        Assert.Equal(0, diagnostics[0].SourceOffset);
        Assert.Equal(22, diagnostics[0].Length);
    }

    [Theory]
    [InlineData("TSAsExpression", "as")]
    [InlineData("TSSatisfiesExpression", "satisfies")]
    [InlineData("TSNonNullExpression", "!")]
    public void Erasing_an_assertion_cannot_manufacture_a_use_strict_directive(string role, string punctuation)
    {
        var assertion = Node(role, Node("StringLiteral", Token("'use strict'", 0, "StringLiteral")),
            Token(punctuation, 13), Node("TSType", Token("string", 23)));
        var first = Node("StatementListItem", Node("Statement", Node("ExpressionStatement", Node("Expression", assertion), Token(";", 29))));
        var withStatement = Node("StatementListItem", Node("Statement", Node("WithStatement", Token("with", 31))));
        var original = Node("Script", Node("StatementList", first, withStatement));

        Assert.Empty(Validate(original, JavaScriptMode.NonStrict));
        Assert.Contains(Validate(original), diagnostic => diagnostic.Code == "strict_with");
        Assert.Same(assertion, assertion[0].Parent);
    }

    [Fact]
    public void An_asserted_string_also_ends_the_prologue_before_a_later_use_strict_statement()
    {
        var assertion = Node("TSAsExpression", Node("StringLiteral", Token("'ordinary'", 0, "StringLiteral")),
            Token("as", 11), Node("TSType", Token("string", 14)));
        var first = Node("StatementListItem", Node("ExpressionStatement", Node("Expression", assertion), Token(";", 20)));
        var directive = Node("StatementListItem", Node("ExpressionStatement", Node("StringLiteral", Token("'use strict'", 22, "StringLiteral")), Token(";", 34)));
        var withStatement = Node("StatementListItem", Node("WithStatement", Token("with", 36)));
        Assert.Empty(Validate(Node("Script", Node("StatementList", first, directive, withStatement)), JavaScriptMode.NonStrict));
    }

    [Fact]
    public void A_real_string_directive_remains_effective_after_projection()
    {
        var directive = Node("StatementListItem", Node("ExpressionStatement", Node("StringLiteral", Token("'use strict'", 0, "StringLiteral")), Token(";", 12)));
        var withStatement = Node("StatementListItem", Node("WithStatement", Token("with", 14)));
        Assert.Contains(Validate(Node("Script", Node("StatementList", directive, withStatement)), JavaScriptMode.NonStrict),
            diagnostic => diagnostic.Code == "strict_with");
    }

    [Fact]
    public void Namespace_bindings_do_not_leak_into_the_surrounding_JavaScript_scope()
    {
        var inner = LetBinding("value", 20);
        var body = Node("TSNamespaceBody", Token("{", 15), Node("ModuleItemList", Node("ModuleItem", Node("StatementListItem", inner))), Token("}", 32));
        var declaration = Node("TSNamespaceDeclaration", Token("namespace", 0), Node("BindingIdentifier", Token("N", 10)), Node("TSNamespaceTail", body));
        var outside = LetBinding("value", 40);
        var original = Node("Script", Node("StatementList", Node("StatementListItem", declaration), Node("StatementListItem", outside)));

        Assert.Empty(Validate(original));
        var view = Assert.IsType<ParseTreeNonTerminal>(Project(original));
        Assert.Single(Descendants(view).OfType<ParseTreeNonTerminal>(), node => node.SignPost.Name == "FunctionExpression");
        Assert.Same(declaration, declaration[1].Parent);
        Assert.Same(body, body[1].Parent);
    }

    [Fact]
    public void Duplicate_runtime_bindings_inside_a_namespace_remain_visible_to_JavaScript_validation()
    {
        var list = Node("ModuleItemList", Node("ModuleItem", Node("StatementListItem", LetBinding("value", 20))),
            Node("ModuleItem", Node("StatementListItem", LetBinding("value", 40))));
        var original = Node("Script", Node("TSNamespaceDeclaration", Token("namespace", 0), Node("BindingIdentifier", Token("N", 10)),
            Node("TSNamespaceBody", Token("{", 15), list, Token("}", 52))));
        Assert.Contains(Validate(original), diagnostic => diagnostic.Code == "duplicate_binding");
    }

    [Fact]
    public void Namespace_directives_apply_to_namespace_statements()
    {
        var directive = Node("ModuleItem", Node("StatementListItem", Node("ExpressionStatement", Node("StringLiteral", Token("'use strict'", 20, "StringLiteral")), Token(";", 32))));
        var statement = Node("ModuleItem", Node("StatementListItem", Node("WithStatement", Token("with", 40))));
        var original = Node("Script", Node("TSNamespaceDeclaration", Node("BindingIdentifier", Token("N", 10)),
            Node("TSNamespaceBody", Token("{", 15), Node("ModuleItemList", directive, statement), Token("}", 52))));
        Assert.Contains(Validate(original, JavaScriptMode.NonStrict), diagnostic => diagnostic.Code == "strict_with");
    }

    [Theory]
    [InlineData("ReturnStatement", "typescript_namespace_return")]
    [InlineData("NewTarget", "typescript_namespace_new_target")]
    public void A_namespace_scope_does_not_become_a_callable_function_context(string role, string code)
    {
        var statement = role == "NewTarget" ? Node(role, Token("new", 20), Token(".", 23), Token("target", 24)) : Node(role, Token("return", 20));
        var original = Node("Script", Node("TSNamespaceDeclaration", Node("BindingIdentifier", Token("N", 10)),
            Node("TSNamespaceBody", Token("{", 15), Node("ModuleItemList", Node("ModuleItem", statement)), Token("}", 32))));
        Assert.Contains(Validate(original), diagnostic => diagnostic.Code == code);
    }

    [Fact]
    public void Function_type_parameter_rules_are_checked_even_when_the_whole_type_is_erased_from_the_JavaScript_view()
    {
        var first = Node("TSParameter", Node("TSParameterBinding", Node("TSBindingIdentifier", Token("first", 1))), Node("TSParameterOptional", Token("?", 6)));
        var second = Node("TSParameter", Node("TSParameterBinding", Node("TSBindingIdentifier", Token("second", 9))));
        var type = Node("TSType", Node("TSFunctionType", Node("TSFunctionParameters", Token("(", 0),
            Node("TSParameterList", first, Token(",", 7), second), Token(")", 15)), Token("=>", 17), Node("TSReturnType", Token("void", 20))));
        Assert.Contains(Validate(type), diagnostic => diagnostic.Code == "typescript_required_after_optional");
    }

    [Fact]
    public void Type_parameter_defaults_and_modifier_placement_are_grammar_checks_without_type_resolution()
    {
        var first = Node("TSTypeParameter", Node("TSBindingIdentifier", Token("T", 10)), Node("TSTypeDefault", Token("=", 12), Node("TSType", Token("Unknown", 14))));
        var second = Node("TSTypeParameter", Node("TSTypeParameterModifiers", Node("TSTypeParameterModifier", Token("const", 23))), Node("TSBindingIdentifier", Token("U", 29)));
        var original = Node("TSTypeAliasDeclaration", Token("type", 0), Token("Alias", 5), Node("TSTypeParameters", Token("<", 9),
            Node("TSTypeParameterList", first, Token(",", 21), second), Token(">", 30)));
        var diagnostics = Validate(original);
        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == "typescript_required_type_parameter");
        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == "typescript_const_type_parameter_context");
    }

    [Fact]
    public void Tuple_order_is_checked_without_requiring_the_element_type_to_resolve()
    {
        var optional = Node("TSTupleElement", Node("TSNamedTupleMember", Token("first", 1), Token("?", 6), Token(":", 7), Node("TSType", Token("Unknown", 9))));
        var required = Node("TSTupleElement", Node("TSNamedTupleMember", Token("second", 18), Token(":", 24), Node("TSType", Token("Missing", 26))));
        var original = Node("TSType", Node("TSTupleType", Token("[", 0), Node("TSTupleElements", optional, Token(",", 16), required), Token("]", 33)));
        Assert.Contains(Validate(original), diagnostic => diagnostic.Code == "typescript_tuple_required_after_optional");
    }

    [Fact]
    public void Type_accessor_signatures_exclude_the_this_pseudo_parameter_from_runtime_arity()
    {
        var pseudo = Node("TSParameter", Node("TSParameterBinding", Token("this", 2)), Node("TSTypeAnnotation", Token(":", 6), Node("TSType", Token("Context", 8))));
        var original = Node("TSType", Node("TSGetterSignature", Token("get", 0), Token("value", 12),
            Node("TSFunctionParameters", Token("(", 1), Node("TSParameterList", pseudo), Token(")", 18))));
        Assert.Empty(Validate(original));
    }

    [Fact]
    public void Index_signatures_require_both_parameter_and_result_annotations()
    {
        var signature = Node("TSType", Node("TSIndexSignature", Token("[", 0),
            Node("TSIndexParameters", Node("TSIndexParameter", Node("TSBindingIdentifier", Token("key", 1)))), Token("]", 4)));
        var diagnostics = Validate(signature);
        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == "typescript_index_parameter_type");
        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == "typescript_index_result_type");
    }

    [Fact]
    public void Projection_rejects_a_null_tree()
        => Assert.Throws<ArgumentNullException>(() => Project(null));

    [Theory]
    [InlineData(JavaScriptInputKind.Script, "const value = 1; const f = (x) => x + value;", true)]
    [InlineData(JavaScriptInputKind.Script, "class C extends B { #x = 1; constructor() { super(); } get x() { return this.#x; } }", true)]
    [InlineData(JavaScriptInputKind.Script, "async function* f() { yield await /a/.exec('a'); }", true)]
    [InlineData(JavaScriptInputKind.Script, "const value = tag`prefix ${total / divisor} suffix`;", true)]
    [InlineData(JavaScriptInputKind.Script, "const {[key]: first = function(first) {}, list: [second,,...rest]} = source;", true)]
    [InlineData(JavaScriptInputKind.Module, "import value, {name as local} from 'source'; export {value, local};", true)]
    [InlineData(JavaScriptInputKind.Module, "await work(); export const value = import.meta;", true)]
    [InlineData(JavaScriptInputKind.Expression, "({[key]: value = 1}) => value", true)]
    [InlineData(JavaScriptInputKind.Script, "1 = 2;", false)]
    [InlineData(JavaScriptInputKind.Script, "let value; const value = 1;", false)]
    [InlineData(JavaScriptInputKind.Script, "function f(a, a) {}", false)]
    [InlineData(JavaScriptInputKind.Script, "for (const value of source) { var value; }", false)]
    [InlineData(JavaScriptInputKind.Module, "export {missing};", false)]
    [InlineData(JavaScriptInputKind.Script, "class C { get value(first) {} }", false)]
    public void JavaScript_semantics_and_diagnostic_spans_are_unchanged_by_the_projection(JavaScriptInputKind kind, string input, bool valid)
    {
        var profile = fixture.Profile(kind);
        var parsed = profile.Parser.Parsing(profile.Language.CreateTokenSource(input));
        Assert.True(parsed.Success, input + Environment.NewLine + profile.Parser.FiredException);
        var original = parsed.ToParseTree;
        var nodes = Descendants(original).ToArray();
        var parents = nodes.Select(node => node.Parent).ToArray();
        var expected = profile.Language.Validate(original);
        var actual = Validate(original);
        Assert.Equal(valid, expected.Count == 0);
        Assert.Equal(expected.Select(item => (item.Code, item.SourceOffset, item.Length)),
            actual.Select(item => (item.Code, item.SourceOffset, item.Length)));
        for (int i = 0; i < nodes.Length; i++) Assert.Same(parents[i], nodes[i].Parent);
    }

    private static ParseTreeSymbol Project(ParseTreeSymbol tree)
        => (ParseTreeSymbol)Invoke("TypeScriptProjection", "Create", new object[] { tree });

    private static IReadOnlyList<JavaScriptDiagnostic> Validate(ParseTreeSymbol tree, JavaScriptMode mode = JavaScriptMode.Strict)
        => (IReadOnlyList<JavaScriptDiagnostic>)Invoke("TypeScriptValidation", "Validate",
            new object[] { tree, mode, false });

    private static object Invoke(string type, string method, object[] arguments)
    {
        var target = typeof(JavaScriptGrammar).Assembly.GetType("Janglim.FrontEnd.Grammars.TypeScript." + type, true);
        try { return target.GetMethod(method, BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, arguments); }
        catch (TargetInvocationException error) when (error.InnerException != null)
        { ExceptionDispatchInfo.Capture(error.InnerException).Throw(); throw; }
    }

    private static ParseTreeNonTerminal Node(string name, params ParseTreeSymbol[] children)
    {
        var rule = new NonTerminal(name);
        rule.Add(new Epsilon());
        var node = new ParseTreeNonTerminal(rule[0]);
        foreach (var child in children) node.Add(child);
        return node;
    }

    private static ParseTreeTerminal Token(string text, int offset, string caption = null)
    {
        var terminal = new Terminal(TokenType.Identifier, text, caption ?? "IdentifierName", true, true);
        return new ParseTreeTerminal(new TokenData(terminal, new TokenCell(offset, 0, offset, text, null)));
    }

    private static ParseTreeNonTerminal LetBinding(string name, int offset)
        => Node("LexicalDeclaration", Token("let", offset - 4), Node("LexicalBindingList",
            Node("UninitializedLexicalBinding", Node("BindingIdentifier", Token(name, offset))), Node("BindingEnd", Token(";", offset + name.Length))));

    private static IEnumerable<ParseTreeSymbol> Descendants(ParseTreeSymbol root)
    {
        var pending = new Stack<ParseTreeSymbol>();
        pending.Push(root);
        while (pending.Count != 0)
        {
            var node = pending.Pop();
            yield return node;
            if (node is ParseTreeNonTerminal parent)
                for (int i = parent.Count - 1; i >= 0; i--) pending.Push(parent[i]);
        }
    }
}
