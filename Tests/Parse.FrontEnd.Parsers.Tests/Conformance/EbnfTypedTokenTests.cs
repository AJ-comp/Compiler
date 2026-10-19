using System;
using System.Linq;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

[Trait("Category", "Ebnf")]
public class EbnfTypedTokenTests
{
    // The grammar's double quotes are doubled; regex backslashes remain untouched.
    private const string Assignments = """"
        Program : Statement+ ;
        Statement : 'const' id '=' Value ';' ;
        Value : text | ch ;
        id := Identifier "[a-zA-Z_][a-zA-Z0-9_]*" ;
        text := StringLiteral """(?:[^""\\\r\n]|\\[^\r\n])*""" ;
        ch := CharLiteral "'(?:[^'\\\r\n]|\\[^\r\n])'" ;
        """";

    [Theory]
    [InlineData("\"hello\"")]
    [InlineData("\"\"")]
    [InlineData("\"hello world\"")]
    [InlineData("\"it's a string\"")]
    [InlineData("\"한글 日本語 😀\"")]
    [InlineData("\"say \\\"hello\\\"\"")]
    [InlineData("\"path\\\\file\\n\"")]
    public void String_literals_parse_as_one_typed_token_without_decoding_input(string value)
        => AssertAssignment(value, "text", TokenType.Literal.StringLiteral);

    [Theory]
    [InlineData("'a'")]
    [InlineData("'한'")]
    [InlineData("' '")]
    [InlineData("'\\n'")]
    [InlineData("'\\''")]
    [InlineData("'\\\\'")]
    [InlineData("'\"'")]
    public void Char_literals_parse_as_one_typed_token_without_decoding_input(string value)
        => AssertAssignment(value, "ch", TokenType.Literal.CharLiteral);

    [Theory]
    [InlineData("const x = \"unterminated;")]
    [InlineData("const x = 'a;")]
    [InlineData("const x = '';")]
    [InlineData("const x = 'ab';")]
    [InlineData("const x = \"line\nbreak\";")]
    [InlineData("const x = 'a\n';")]
    [InlineData("const x = \"bad\\\nbreak\";")]
    public void Malformed_literals_are_not_silently_accepted(string input)
    {
        var grammar = Read(Assignments);
        var lexed = CreateLexer(grammar).Lexing(input);
        Assert.Contains(lexed.TokensForParsing, token => token.PatternInfo.Terminal is NotDefined);
        Assert.False(new LALRParser(grammar, false).Parsing(lexed.TokensForParsing).Success);
    }

    [Fact]
    public void Both_text_kinds_preserve_raw_regexes_and_identifier_keeps_word_boundaries()
    {
        var grammar = Read(Assignments);
        var text = Assert.Single(grammar.TerminalSet, token => token.Caption == "text");
        var ch = Assert.Single(grammar.TerminalSet, token => token.Caption == "ch");
        var id = Assert.Single(grammar.TerminalSet, token => token.Caption == "id");

        Assert.Equal("\"(?:[^\"\\\\\\r\\n]|\\\\[^\\r\\n])*\"", text.Value);
        Assert.Equal("'(?:[^'\\\\\\r\\n]|\\\\[^\\r\\n])'", ch.Value);
        Assert.Equal(text.Value, text.RegexExpression);
        Assert.Equal(ch.Value, ch.RegexExpression);
        Assert.StartsWith(@"\b", id.RegexExpression);
        Assert.EndsWith(@"\b", id.RegexExpression);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Identifier ")]
    public void Omitted_and_explicit_identifier_preserve_existing_behavior(string kind)
    {
        var grammar = Read("Start : 'if' id ; id := " + kind + "\"[a-z]+\" ;");
        var id = Assert.Single(grammar.TerminalSet, token => token.Caption == "id");
        Assert.Equal(TokenType.Identifier, id.TokenType);
        Assert.StartsWith(@"\b", id.RegexExpression);
        var tokens = CreateLexer(grammar).Lexing("if item").TokensForParsing;
        Assert.Equal(new[] { "if", "item" }, tokens.Select(token => token.Data));
        Assert.NotEqual(id.TokenType, tokens[0].PatternInfo.Terminal.TokenType);
        Assert.True(new LALRParser(grammar, false).Parsing(tokens).Success);
    }

    [Fact]
    public void Token_kind_names_are_not_reserved_in_rule_bodies()
    {
        var grammar = Read("Start : StringLiteral ; StringLiteral := \"[a-z]+\" ;");
        var token = Assert.Single(CreateLexer(grammar).Lexing("hello").TokensForParsing);
        Assert.Equal(TokenType.Identifier, token.PatternInfo.Terminal.TokenType);
    }

    [Fact]
    public void Regex_quote_doubling_does_not_change_adjacent_single_quoted_literals()
    {
        var grammar = Read("Start : 'a''b' ;");
        Assert.Contains(grammar.TerminalSet, token => token.Value == "a");
        Assert.Contains(grammar.TerminalSet, token => token.Value == "b");
        Assert.DoesNotContain(grammar.TerminalSet, token => token.Value == "a'b");
        Assert.True(new LALRParser(grammar, false).Parsing(CreateLexer(grammar).Lexing("a b").TokensForParsing).Success);
    }

    [Fact]
    public void Backslash_before_literal_closing_quote_retains_original_meaning()
    {
        var grammar = Read("Start : '\\' ;");
        var token = Assert.Single(CreateLexer(grammar).Lexing("\\").TokensForParsing);
        Assert.Equal("\\", token.Data);
        Assert.True(new LALRParser(grammar, false).Parsing(new[] { token }).Success);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("\"", "\"\"")]
    [InlineData("\"x\"", "\"\"x\"\"")]
    [InlineData("a\"\"b", "a\"\"\"\"b")]
    [InlineData(@"\d+\s*\b\w+\\", @"\d+\s*\b\w+\\")]
    public void Pattern_reader_decodes_only_doubled_quotes(string expected, string encoded)
    {
        var grammar = Read("Start : token ; token := \"" + encoded + "\" ;");
        Assert.Equal(expected, Assert.Single(grammar.TerminalSet, token => token.Caption == "token").Value);
    }

    [Theory]
    [InlineData("Start : text ;\ntext := string \"x\" ;", "line 2", "unknown token kind")]
    [InlineData("Start : text ;\ntext := Unknown \"x\" ;", "line 2", "Identifier, StringLiteral, CharLiteral, NumberLiteral, Comment, or Reject")]
    [InlineData("Start : text ;\ntext := StringLiteral ;", "line 2", "pattern")]
    [InlineData("Start : text ;\ntext := CharLiteral 'x' ;", "line 2", "pattern")]
    [InlineData("Start : text ;\ntext := CharLiteral \"x\"", "line 2", "';'")]
    [InlineData("Start : text ;\ntext := StringLiteral \"\"\" ;", "line 2", "unterminated pattern")]
    [InlineData("Start : text ;\ntext := Identifier StringLiteral \"x\" ;", "line 2", "pattern")]
    public void Malformed_typed_declarations_report_errors_without_throwing(string source, string line, string fragment)
    {
        var result = EbnfGrammarReader.Read(source);
        Assert.False(result.Success);
        Assert.Null(result.Grammar);
        Assert.Contains(result.Errors, error => error.Contains(line) && error.Contains(fragment));
    }

    [Theory]
    [InlineData(EbnfTokenKind.StringLiteral, "\"[^\"\\r\\n]*\"", "\"hello world\"")]
    [InlineData(EbnfTokenKind.CharLiteral, "'[^'\\r\\n]'", "'a'")]
    public void Hand_built_models_use_the_same_existing_literal_types(EbnfTokenKind kind, string pattern, string input)
    {
        var model = new EbnfModel();
        model.Tokens.Add(new EbnfTokenDef { Name = "value", Pattern = pattern, Kind = kind });
        var rule = new EbnfRuleDef { Name = "Start" };
        rule.Alternatives.Add(new System.Collections.Generic.List<EbnfSym> { EbnfSym.Ref("value") });
        model.Rules.Add(rule);
        var grammar = new EbnfGrammar(model);
        var token = Assert.Single(CreateLexer(grammar).Lexing(input).TokensForParsing);
        Assert.Equal(input, token.Data);
        Assert.True(token.PatternInfo.Terminal.IsTextLiteral);
        Assert.True(new LALRParser(grammar, false).Parsing(new[] { token }).Success);
    }

    [Fact]
    public void Existing_hand_built_token_models_still_default_to_identifier()
        => Assert.Equal(EbnfTokenKind.Identifier, new EbnfTokenDef().Kind);

    [Theory]
    [InlineData(".5")]
    [InlineData("1.")]
    [InlineData("123")]
    public void Number_kind_uses_raw_regex_without_word_boundaries(string input)
    {
        var grammar = Read("Start : number ; number := NumberLiteral \"(?:[0-9]+\\.[0-9]*|\\.[0-9]+|[0-9]+)\" ;");
        var token = Assert.Single(CreateLexer(grammar).Lexing(input).TokensForParsing);
        Assert.Equal(input, token.Data);
        Assert.Equal(TokenType.Literal.Digit10, token.PatternInfo.Terminal.TokenType);
        Assert.Equal(token.PatternInfo.Terminal.Value, token.PatternInfo.Terminal.RegexExpression);
        Assert.True(new LALRParser(grammar, false).Parsing(new[] { token }).Success);
    }

    [Fact]
    public void Lexical_only_comments_are_kept_for_view_but_excluded_from_parsing()
    {
        const string source = """
            Start : 'a' '/' 'b' ;
            comment := Comment "//[^\r\n]*|/\*(?:[^*]|\*(?!/))*\*/" ;
            """;
        var grammar = Read(source);
        var lexed = CreateLexer(grammar).Lexing("// hello\na /* block\ncomment */ / b");
        Assert.Equal(new[] { "a", "/", "b" }, lexed.TokensForParsing.Select(token => token.Data));
        Assert.Equal(2, lexed.TokensForView.Count(token => token.PatternInfo.Terminal.TokenType is Comment));
        Assert.True(new LALRParser(grammar, false).Parsing(lexed.TokensForParsing).Success);
    }

    [Fact]
    public void Comment_token_references_are_reported_as_invalid_grammar()
    {
        var result = EbnfGrammarReader.Read("Start : comment ; comment := Comment \"//[^\\r\\n]*\" ;");
        Assert.False(result.Success);
        Assert.Contains(result.Errors, error => error.Contains("lexical-only"));
    }

    [Fact]
    public void Reject_tokens_mark_invalid_input_without_becoming_grammar_symbols()
    {
        const string source = """
            Start : 'a' '/' '*' 'b' ;
            comment := Comment "/\*[\s\S]*?\*/" ;
            invalid := Reject "/\*(?:(?!\*/)[\s\S])*\z" ;
            """;
        var grammar = Read(source);
        var rejected = Assert.Single(grammar.TerminalSet, token => grammar.IsRejectedToken(token));
        Assert.Equal("invalid", rejected.Caption);
        Assert.Equal(rejected.Value, rejected.RegexExpression);
        Assert.False(rejected.Meaning);

        var lexed = CreateLexer(grammar).Lexing("a /*b");
        var token = Assert.Single(lexed.TokensForView, token => grammar.IsRejectedToken(token.PatternInfo.Terminal));
        Assert.Equal("/*b", token.Data);
        Assert.Equal(2, token.StartIndex);
        Assert.False(new LALRParser(grammar, false).Parsing(lexed.TokensForParsing).Success);
        Assert.DoesNotContain(CreateLexer(grammar).Lexing("a /* valid */ / * b").TokensForView,
            token => grammar.IsRejectedToken(token.PatternInfo.Terminal));
    }

    [Fact]
    public void Reject_token_references_are_reported_as_invalid_grammar()
    {
        var result = EbnfGrammarReader.Read("Start : invalid ; invalid := Reject \"x\" ;");
        Assert.False(result.Success);
        Assert.Contains(result.Errors, error => error.Contains("lexical-only"));
    }

    [Theory]
    [InlineData("number := NumberLiteral \"[0-9]+\" ; invalid := Reject \"[0-9]+\" ;")]
    [InlineData("invalid := Reject \"[0-9]+\" ; number := NumberLiteral \"[0-9]+\" ;")]
    public void Duplicate_numeric_and_reject_patterns_cannot_silently_hide_rejection(string tokens)
    {
        var result = EbnfGrammarReader.Read("Start : number ; " + tokens);
        Assert.False(result.Success);
        Assert.Contains(result.Errors, error => error.Contains("conflicting NumberLiteral or Reject"));
    }

    [Fact]
    public void Unsupported_hand_built_token_kind_is_not_silently_treated_as_identifier()
    {
        var model = new EbnfModel();
        model.Tokens.Add(new EbnfTokenDef { Name = "value", Pattern = "[a-z]+", Kind = (EbnfTokenKind)42 });
        var rule = new EbnfRuleDef { Name = "Start" };
        rule.Alternatives.Add(new System.Collections.Generic.List<EbnfSym> { EbnfSym.Ref("value") });
        model.Rules.Add(rule);
        var error = Assert.Throws<EbnfException>(() => new EbnfGrammar(model));
        Assert.Contains("unsupported token kind", error.Message);
    }

    private static void AssertAssignment(string value, string symbol, TokenType type)
    {
        var grammar = Read(Assignments);
        var input = "const x = " + value + ";";
        var tokens = CreateLexer(grammar).Lexing(input).TokensForParsing;
        Assert.Equal(new[] { "const", "x", "=", value, ";" }, tokens.Select(token => token.Data));
        var token = tokens[3];
        Assert.Equal(symbol, token.PatternInfo.Terminal.Caption);
        Assert.Equal(type, token.PatternInfo.Terminal.TokenType);
        Assert.Equal(10, token.StartIndex);
        Assert.Equal(value, input.Substring(token.StartIndex, token.Data.Length));
        var parser = new LALRParser(grammar, false);
        Assert.Empty(parser.Conflicts);
        Assert.True(parser.Parsing(tokens).Success);
    }

    private static EbnfGrammar Read(string source)
    {
        var result = EbnfGrammarReader.Read(source);
        Assert.True(result.Success, string.Join("; ", result.Errors));
        return result.Grammar;
    }

    private static Lexer CreateLexer(EbnfGrammar grammar)
    {
        var lexer = new Lexer();
        foreach (var terminal in grammar.TerminalSet) lexer.AddTokenRule(terminal);
        return lexer;
    }
}
