using System;
using System.Linq;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// COMMON (parser-independent): terminals typed StringLiteral/CharLiteral lex with their Value
/// as a raw regex. They must NOT go through the \b...\b word wrapping — a quote is a non-word
/// char, so a wrapped pattern like \b"[^"]*"\b could never match after whitespace and the
/// terminal would be dead (the Qora string-literal report).
/// </summary>
[Trait("Category", "Common")]
public class TextLiteralLexingTests
{
    private static Terminal StringLit()
        => new Terminal(TokenType.Literal.StringLiteral, "\"[^\"\\r\\n]*\"", "string", true, true);

    private static Terminal CharLit()
        => new Terminal(TokenType.Literal.CharLiteral, "'[^'\\r\\n]'", "char", true, true);

    // The terminal set from the Qora report: keyword, identifier, numbers, operators,
    // a line comment and the string/char literals, plus the whitespace delimiters.
    private static Lexer QoraLikeLexer()
    {
        var lexer = new Lexer();
        foreach (var ws in new[] { " ", "\t", "\r", "\n" })
            lexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Delimiter, ws, false));

        lexer.AddTokenRule(new Terminal(TokenType.Keyword, "import", false));
        lexer.AddTokenRule(new Terminal(TokenType.Identifier, "[_a-zA-Z][_a-zA-Z0-9]*", "Ident", true, true));
        lexer.AddTokenRule(new Terminal(TokenType.Literal.Digit10, "-?[0-9]+", "number", true, true));
        lexer.AddTokenRule(new Terminal(TokenType.Literal.Digit10, @"(-?[0-9]+\.[0-9]+)([Ee][+-]?[0-9]+)?", "realnumber", true, true));
        lexer.AddTokenRule(new Terminal(TokenType.Operator.NormalOperator, "=", false));
        lexer.AddTokenRule(new Terminal(TokenType.Operator.NormalOperator, "/", false));
        lexer.AddTokenRule(new Terminal(TokenType.Operator.NormalOperator, ";", false));
        lexer.AddTokenRule(new Terminal(TokenType.Operator, ".", false));
        lexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Comment, "//.*$", false, true));
        lexer.AddTokenRule(StringLit());
        lexer.AddTokenRule(CharLit());

        return lexer;
    }

    [Fact]
    public void String_and_char_literal_regexes_stay_raw()
    {
        var stringLit = StringLit();
        var charLit = CharLit();

        Assert.Equal(stringLit.Value, stringLit.RegexExpression);
        Assert.Equal(charLit.Value, charLit.RegexExpression);
    }

    [Fact]
    public void String_literal_lexes_as_one_token()
    {
        var tokens = QoraLikeLexer().Lexing("import \"lib.qor\";").TokensForParsing;

        Assert.Equal(new[] { "import", "\"lib.qor\"", ";" }, tokens.Select(t => t.Data).ToArray());
        Assert.Equal(TokenType.Literal.StringLiteral, tokens[1].PatternInfo.Terminal.TokenType);
    }

    [Fact]
    public void String_literal_keeps_inner_spaces()
    {
        var tokens = QoraLikeLexer().Lexing("\"a b c\"").TokensForParsing;

        Assert.Equal(new[] { "\"a b c\"" }, tokens.Select(t => t.Data).ToArray());
    }

    [Fact]
    public void Empty_string_literal_lexes_as_one_token()
    {
        var tokens = QoraLikeLexer().Lexing("\"\"").TokensForParsing;

        Assert.Equal(new[] { "\"\"" }, tokens.Select(t => t.Data).ToArray());
    }

    [Fact]
    public void Char_literal_lexes_as_one_token()
    {
        var tokens = QoraLikeLexer().Lexing("x = 'a';").TokensForParsing;

        Assert.Equal(new[] { "x", "=", "'a'", ";" }, tokens.Select(t => t.Data).ToArray());
        Assert.Equal(TokenType.Literal.CharLiteral, tokens[2].PatternInfo.Terminal.TokenType);
    }

    [Fact]
    public void Division_operator_is_not_disturbed()
    {
        var tokens = QoraLikeLexer().Lexing("x = 1 / 2;").TokensForParsing;

        Assert.Equal(new[] { "x", "=", "1", "/", "2", ";" }, tokens.Select(t => t.Data).ToArray());
    }

    [Fact]
    public void Line_comment_is_not_disturbed()
    {
        var lexed = QoraLikeLexer().Lexing("// comment");

        Assert.Single(lexed.TokensForView);
        Assert.Equal("// comment", lexed.TokensForView[0].Data);
        Assert.Empty(lexed.TokensForParsing);
    }

    [Fact]
    public void Real_number_still_beats_dot_operator()
    {
        var tokens = QoraLikeLexer().Lexing("0.5").TokensForParsing;

        Assert.Equal(new[] { "0.5" }, tokens.Select(t => t.Data).ToArray());
    }

    // The literal's char class excludes \r\n, so an unterminated string must not swallow the
    // rest of the line (or file); the orphan quote surfaces as a not-defined token instead.
    [Fact]
    public void Unterminated_string_fails_as_not_defined_token()
    {
        var tokens = QoraLikeLexer().Lexing("import \"lib.qor;\n").TokensForParsing;

        Assert.Contains(tokens, t => t.PatternInfo.Terminal is NotDefined);
    }

    // A LineComment-typed terminal is a Comment too: it must be filtered from the parsing
    // stream exactly like the base Comment type (it used to slip through the == filter).
    [Fact]
    public void Line_comment_subtype_is_filtered_from_parsing_stream()
    {
        var lexer = new Lexer();
        lexer.AddTokenRule(new Terminal(TokenType.Operator.NormalOperator, "/", false));
        lexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Comment.LineComment, "//.*$", false, true));

        var lexed = lexer.Lexing("//hi");

        Assert.Single(lexed.TokensForView);
        Assert.Empty(lexed.TokensForParsing);
    }

    [Fact]
    public void Broken_regex_pattern_is_rejected_at_registration()
    {
        var lexer = new Lexer();
        var broken = new Terminal(TokenType.Literal.StringLiteral, "(unclosed", "broken", true, true);

        Assert.Throws<ArgumentException>(() => lexer.AddTokenRule(broken));
    }

    // A named capture group inside a pattern would corrupt the group-to-pattern index mapping
    // the lexer uses to attribute matches, so it is rejected up front.
    [Fact]
    public void Named_group_in_pattern_is_rejected_at_registration()
    {
        var lexer = new Lexer();
        var named = new Terminal(TokenType.Literal.StringLiteral, "(?<s>\"[^\"]*\")", "named", true, true);

        Assert.Throws<ArgumentException>(() => lexer.AddTokenRule(named));
    }

    [Fact]
    public void Lookbehind_is_not_mistaken_for_a_named_group()
    {
        var lexer = new Lexer();

        lexer.AddTokenRule(new Terminal(TokenType.Literal.StringLiteral, "(?<=x)\"[^\"]*\"", "look", true, true));
    }

    // '(?<' as literal text inside a character class defines no capture group, so it must
    // pass validation (the group check counts real groups instead of scanning for text).
    [Fact]
    public void Group_like_text_inside_a_character_class_is_accepted()
    {
        var lexer = new Lexer();

        lexer.AddTokenRule(new Terminal(TokenType.Literal.StringLiteral, "[(?<]+", "punct", true, true));
    }

    // A backreference to the validation wrapper's group name must not slip through
    // validation only to crash later inside the combined rule at tokenize time.
    [Fact]
    public void Backreference_to_an_undefined_group_is_rejected_at_registration()
    {
        var lexer = new Lexer();
        var backref = new Terminal(TokenType.Literal.StringLiteral, "\"\\k<t>\"", "backref", true, true);

        Assert.Throws<ArgumentException>(() => lexer.AddTokenRule(backref));
    }

    // An empty-Value terminal is skipped by GetTokenizeRule, so registering one next to
    // real rules must stay harmless as it always was (no validation crash on the empty
    // pattern, and the other rules keep lexing).
    [Fact]
    public void Empty_value_terminal_still_registers_without_error()
    {
        var lexer = new Lexer();
        lexer.AddTokenRule(new Terminal(TokenType.Operator.NormalOperator, ";", false));
        lexer.AddTokenRule(new Terminal(TokenType.Keyword, ""));

        var tokens = lexer.Lexing(";").TokensForParsing;

        Assert.Equal(new[] { ";" }, tokens.Select(t => t.Data).ToArray());
    }

    // Operator values are escaped with Regex.Escape now: letters and digits stay literal
    // instead of becoming broken escapes (\i) or backreferences (\2).
    [Fact]
    public void Operator_values_with_letters_or_digits_lex_literally()
    {
        var lexer = new Lexer();
        lexer.AddTokenRule(new Terminal(TokenType.Operator, "id"));
        lexer.AddTokenRule(new Terminal(TokenType.Operator, "2"));

        var tokens = lexer.Lexing("id2").TokensForParsing;

        Assert.Equal(new[] { "id", "2" }, tokens.Select(t => t.Data).ToArray());
    }

    [Fact]
    public void Punctuation_operator_regex_is_still_escaped()
    {
        var plusPlus = new Terminal(TokenType.Operator.NormalOperator, "++", false);

        Assert.Equal(@"\+\+", plusPlus.RegexExpression);
    }
}
