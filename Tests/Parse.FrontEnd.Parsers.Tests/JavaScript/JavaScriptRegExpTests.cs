using System;
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

    [Theory]
    [InlineData("/a/")]
    [InlineData("/a|b/")]
    [InlineData("/a||b/")]
    [InlineData("/(?:)/")]
    [InlineData("/|/")]
    [InlineData("/a|/")]
    [InlineData("/|a/")]
    [InlineData("/./")]
    [InlineData("/a*/")]
    [InlineData("/a+/")]
    [InlineData("/a?/")]
    [InlineData("/a*?/")]
    [InlineData("/a+?/")]
    [InlineData("/a??/")]
    [InlineData("/a{0}/")]
    [InlineData("/a{2}/")]
    [InlineData("/a{2,}/")]
    [InlineData("/a{2,4}/")]
    [InlineData("/a{0,0}?/")]
    [InlineData("/a{0002,0004}/")]
    [InlineData("/(?:ab|cd){2,4}/")]
    [InlineData("/(a(b(c)))+/")]
    [InlineData("/a{999999999999999999999999999999999999999}/u")]
    [InlineData("/a{999999999999999999999999999999999999999,}/v")]
    [InlineData("/a{100000000000000000000000000000000000000,200000000000000000000000000000000000000}/u")]
    [InlineData("/(?:x(?:y|z)*)?/")]
    [InlineData("/a\\{2\\}/u")]
    [InlineData("/[{}]+/u")]
    [InlineData("/a{2/")]
    [InlineData("/a{2,/")]
    [InlineData("/a{,2}/")]
    [InlineData("/a{z}/")]
    [InlineData("/a}/")]
    [InlineData("/{/")]
    [InlineData("/]/")]
    [InlineData("/a{2,1x}/")]
    [InlineData("/a{1, 2}/")]
    public void Atoms_alternatives_and_quantifiers_are_accepted(string input)
        => Accept(input);

    [Theory]
    [InlineData("/(?:*a)/")]
    [InlineData("/+a/")]
    [InlineData("/?a/")]
    [InlineData("/a**/")]
    [InlineData("/a++/")]
    [InlineData("/a?+/")]
    [InlineData("/a{1}*/")]
    [InlineData("/a{1}+/")]
    [InlineData("/a{1}?+/")]
    [InlineData("/a{3,2}/")]
    [InlineData("/a{3,2}/u")]
    [InlineData("/a{3,2}/v")]
    [InlineData("/a{0003,0002}/")]
    [InlineData("/{1}/")]
    [InlineData("/|*/")]
    [InlineData("/(?:a|*)/")]
    [InlineData("/a{999999999999999999999999999999999999998,999999999999999999999999999999999999997}/u")]
    [InlineData("/a{1000000000000000000000000000000000000000,999999999999999999999999999999999999999}/v")]
    [InlineData("/a{2/u")]
    [InlineData("/a{2,/u")]
    [InlineData("/a{,2}/u")]
    [InlineData("/a{z}/u")]
    [InlineData("/a}/u")]
    [InlineData("/{/u")]
    [InlineData("/]/u")]
    [InlineData("/a{2,1x}/u")]
    [InlineData("/a{1, 2}/u")]
    [InlineData("/{,}/v")]
    [InlineData("/a{1,,2}/v")]
    public void Atoms_alternatives_and_quantifiers_are_rejected(string input)
        => Reject(input);

    [Theory]
    [InlineData("/()/")]
    [InlineData("/(())/")]
    [InlineData("/(?:a)/")]
    [InlineData("/(?=a)a/")]
    [InlineData("/(?!a)b/")]
    [InlineData("/(?<=a)b/")]
    [InlineData("/(?<!a)b/")]
    [InlineData("/(?<=a|bc)d/u")]
    [InlineData("/(?<=a*)b/")]
    [InlineData("/^a$/")]
    [InlineData("/\\ba\\B/")]
    [InlineData("/(?:^|$)/")]
    [InlineData("/(?=a)?/")]
    [InlineData("/(?!a)*/")]
    [InlineData("/(?=a)+/")]
    [InlineData("/(?=a){2}/")]
    [InlineData("/(?=a)??/")]
    [InlineData("/(?:\\b)*/u")]
    [InlineData("/(?<name>a)/")]
    [InlineData("/(?<π>a)/u")]
    [InlineData("/(?<$x>a)/")]
    [InlineData("/(?<_x>a)/")]
    [InlineData("/(?<\\u0061>a)/u")]
    [InlineData("/(?<\\u{61}>a)/u")]
    [InlineData("/(?<\\u{10400}>a)/u")]
    [InlineData("/(?<a\\u200Cb>x)/u")]
    [InlineData("/(?<a\\u200Db>x)/u")]
    [InlineData("/(?<\\uD835\\uDC00>a)/u")]
    [InlineData("/(?<𝒜>a)/v")]
    [InlineData("/(?<a>(?:x|y))/")]
    [InlineData("/(?<a>x)|(?:(?<a>y)|z)/v")]
    public void Groups_and_assertions_are_accepted(string input)
        => Accept(input);

    [Theory]
    [InlineData("/(/")]
    [InlineData("/)/")]
    [InlineData("/(a))/")]
    [InlineData("/((a)/")]
    [InlineData("/(?a)/")]
    [InlineData("/(?#comment)/")]
    [InlineData("/(?P<name>a)/")]
    [InlineData("/(?<name>a/")]
    [InlineData("/(?<name)/")]
    [InlineData("/(?<>a)/")]
    [InlineData("/(?<1>a)/")]
    [InlineData("/(?<a-b>x)/")]
    [InlineData("/(?<a b>x)/")]
    [InlineData("/(?<\\u0030>x)/u")]
    [InlineData("/(?<\\uD800>x)/u")]
    [InlineData("/(?<\\u{110000}>x)/u")]
    [InlineData("/(?<\\u{}>x)/u")]
    [InlineData("/(?<\\x61>x)/u")]
    [InlineData("/(?<😀>a)/u")]
    [InlineData("/(?=a)?/u")]
    [InlineData("/(?=a)*/v")]
    [InlineData("/(?<\\u{D835}\\u{DC00}>a)/u")]
    [InlineData("/(?<\\uDC00\\uD800>a)/u")]
    [InlineData("/(?<=a)?/")]
    [InlineData("/(?<!a)*/")]
    [InlineData("/^*/")]
    [InlineData("/$+/")]
    [InlineData("/\\b?/")]
    [InlineData("/\\B{2}/u")]
    public void Groups_and_assertions_are_rejected(string input)
        => Reject(input);

    [Theory]
    [InlineData("/(a)\\1/")]
    [InlineData("/\\1(a)/u")]
    [InlineData("/(a)\\1/u")]
    [InlineData("/(a)(b)\\2\\1/v")]
    [InlineData("/(?<name>a)\\k<name>/u")]
    [InlineData("/\\k<name>(?<name>a)/u")]
    [InlineData("/(?<\\u0061>x)\\k<a>/u")]
    [InlineData("/(?<a>x)\\k<\\u0061>/u")]
    [InlineData("/(?<π>x)\\k<π>/u")]
    [InlineData("/(?<x>a)|(?<x>b)/")]
    [InlineData("/(?:(?<x>a)|(?<x>b))\\k<x>/u")]
    [InlineData("/(?:(?<x>a)|(?<x>b))*/v")]
    [InlineData("/(?<x>a)|(?<x>b)|(?<x>c)/v")]
    [InlineData("/(?:(?<x>a)|b)|(?<x>c)/u")]
    [InlineData("/(?:(?<x>a)|b)(?:(?<y>c)|(?<y>d))/u")]
    [InlineData("/(?<x>a)|(?:(?<y>b)(?<x>c))/")]
    [InlineData("/\\k<unknown>/")]
    [InlineData("/\\k/")]
    [InlineData("/\\1/")]
    [InlineData("/\\8/")]
    [InlineData("/\\9/")]
    [InlineData("/\\11/")]
    [InlineData("/(a)\\11/")]
    [InlineData("/(a)\\2/")]
    [InlineData("/\\123/")]
    [InlineData("/[\\1]/")]
    [InlineData("/[(?<name>)]\\k<unknown>/")]
    [InlineData("/\\k<missing>[(?<name>)]/")]
    [InlineData("/(?:(?<a>x)|(?<a>y)){2}/u")]
    [InlineData("/\\0/u")]
    [InlineData("/\\0/v")]
    [InlineData("/\\0a/u")]
    public void Backreferences_and_duplicate_named_groups_are_accepted(string input)
        => Accept(input);

    [Theory]
    [InlineData("/\\1/u")]
    [InlineData("/\\8/u")]
    [InlineData("/\\9/v")]
    [InlineData("/(a)\\2/u")]
    [InlineData("/(a)\\11/u")]
    [InlineData("/\\99999999999999999999999999999/u")]
    [InlineData("/[\\1]/u")]
    [InlineData("/[\\8]/v")]
    [InlineData("/\\01/u")]
    [InlineData("/\\00/v")]
    [InlineData("/\\09/u")]
    [InlineData("/(?<x>a)(?<x>b)/")]
    [InlineData("/(?<x>a)(?<\\u0078>b)/u")]
    [InlineData("/(?<x>a(?<x>b))/")]
    [InlineData("/(?<a>(?<a>x)|y)/")]
    [InlineData("/(?:(?<x>a)|b)(?:(?<x>c)|d)/u")]
    [InlineData("/(?<x>a)|(?<x>b)(?<x>c)/v")]
    [InlineData("/(?=(?<x>a))(?<x>b)/u")]
    [InlineData("/\\k<unknown>/u")]
    [InlineData("/(?<x>a)\\k<unknown>/")]
    [InlineData("/(?<x>a)\\k/")]
    [InlineData("/(?<x>a)\\k<>/")]
    [InlineData("/(?<x>a)\\k<x/")]
    [InlineData("/\\k<1>/u")]
    [InlineData("/(?<a>x)\\k<\\x61>/u")]
    [InlineData("/\\k/u")]
    public void Backreferences_and_duplicate_named_groups_are_rejected(string input)
        => Reject(input);

    [Theory]
    [InlineData("/\\n\\r\\t\\v\\f\\b\\B/")]
    [InlineData("/\\d\\D\\s\\S\\w\\W/u")]
    [InlineData("/\\cA\\cz/")]
    [InlineData("/\\x00\\xFF/u")]
    [InlineData("/\\u0000\\uFFFF/u")]
    [InlineData("/\\u{0}\\u{10FFFF}/u")]
    [InlineData("/\\u{0000000000000000000000000000000000000061}/v")]
    [InlineData("/\\uD800/u")]
    [InlineData("/\\uDC00/v")]
    [InlineData("/\\uD83D\\uDE00/u")]
    [InlineData("/\\u{D800}/u")]
    [InlineData("/\\u{DFFF}/v")]
    [InlineData("/\\^\\$\\\\\\.\\*\\+\\?\\(\\)\\[\\]\\{\\}\\|\\//u")]
    [InlineData("/\\q/")]
    [InlineData("/\\a/")]
    [InlineData("/\\_/")]
    [InlineData("/\\-/")]
    [InlineData("/\\:/")]
    [InlineData("/\\c/")]
    [InlineData("/\\c1/")]
    [InlineData("/\\x/")]
    [InlineData("/\\xG0/")]
    [InlineData("/\\u/")]
    [InlineData("/\\uG000/")]
    [InlineData("/\\u{61}/")]
    [InlineData("/\\p{Letter}/")]
    [InlineData("/\\P{bad}/")]
    public void Escapes_across_legacy_and_Unicode_modes_are_accepted(string input)
        => Accept(input);

    [Theory]
    [InlineData("/\\q/u")]
    [InlineData("/\\a/v")]
    [InlineData("/\\_/u")]
    [InlineData("/\\-/u")]
    [InlineData("/\\:/v")]
    [InlineData("/\\c/u")]
    [InlineData("/\\c1/u")]
    [InlineData("/\\x/u")]
    [InlineData("/\\xG0/u")]
    [InlineData("/\\x1/u")]
    [InlineData("/\\u/u")]
    [InlineData("/\\uG000/v")]
    [InlineData("/\\u123/u")]
    [InlineData("/\\u{}/u")]
    [InlineData("/\\u{110000}/u")]
    [InlineData("/\\u{FFFFFFFFFFFFFFFFFFFFFF}/v")]
    [InlineData("/\\u{-1}/u")]
    [InlineData("/\\u{1_0}/v")]
    [InlineData("/\\u{61/u")]
    public void Escapes_across_legacy_and_Unicode_modes_are_rejected(string input)
        => Reject(input);

    [Theory]
    [InlineData("/[]/")]
    [InlineData("/[^]/")]
    [InlineData("/[abc]/")]
    [InlineData("/[^abc]/u")]
    [InlineData("/[a-zA-Z0-9_]/v")]
    [InlineData("/[-a]/u")]
    [InlineData("/[a-]/u")]
    [InlineData("/[\\-]/v")]
    [InlineData("/[\\]\\[]/u")]
    [InlineData("/[\\b]/u")]
    [InlineData("/[\\0]/v")]
    [InlineData("/[\\cA]/u")]
    [InlineData("/[\\c_]/")]
    [InlineData("/[\\c0]/")]
    [InlineData("/[\\c]/")]
    [InlineData("/[\\x41-\\u005A]/u")]
    [InlineData("/[\\u{1F600}-\\u{1F64F}]/u")]
    [InlineData("/[\\uD800-\\uDFFF]/u")]
    [InlineData("/[😀-🙏]/u")]
    [InlineData("/[a-a]/u")]
    [InlineData("/[\\uD83D\\uDE00-\\uD83D\\uDE4F]/u")]
    [InlineData("/[\\d-a]/")]
    [InlineData("/[a-\\d]/")]
    [InlineData("/[\\s-\\w]/")]
    [InlineData("/[a&&b]/")]
    [InlineData("/[&&]/u")]
    [InlineData("/[!!]/u")]
    [InlineData("/[()[\\]{}|\\/]/")]
    [InlineData("/[\\p{L}\\P{N}]/u")]
    public void Character_classes_and_ranges_are_accepted(string input)
        => Accept(input);

    [Theory]
    [InlineData("/[z-a]/")]
    [InlineData("/[z-a]/u")]
    [InlineData("/[9-0]/v")]
    [InlineData("/[\\u005A-\\x41]/u")]
    [InlineData("/[\\u{1F64F}-\\u{1F600}]/u")]
    [InlineData("/[\\d-a]/u")]
    [InlineData("/[a-\\d]/v")]
    [InlineData("/[\\s-\\w]/u")]
    [InlineData("/[\\p{L}-a]/u")]
    [InlineData("/[a-\\p{L}]/u")]
    [InlineData("/[\\c_]/u")]
    [InlineData("/[\\c0]/v")]
    [InlineData("/[\\c]/u")]
    [InlineData("/[\\B]/u")]
    [InlineData("/[\\q]/u")]
    [InlineData("/[\\u{}]/u")]
    [InlineData("/[\\01]/u")]
    [InlineData("/[\\8]/u")]
    [InlineData("/[\\-a-\\-]/u")]
    [InlineData("/[\\u{D83D}\\u{DE00}-\\u{D83D}\\u{DE4F}]/u")]
    [InlineData("/[\\p{L}-a]/")]
    [InlineData("/[a--]/")]
    public void Character_classes_and_ranges_are_rejected(string input)
        => Reject(input);

    [Theory]
    [InlineData("/\\p{L}/u")]
    [InlineData("/\\p{Letter}/v")]
    [InlineData("/\\p{Lu}/u")]
    [InlineData("/\\P{Lowercase_Letter}/u")]
    [InlineData("/\\p{General_Category=Decimal_Number}/u")]
    [InlineData("/\\p{gc=Nd}/v")]
    [InlineData("/\\p{Script=Greek}/u")]
    [InlineData("/\\p{sc=Grek}/u")]
    [InlineData("/\\p{Script_Extensions=Hira}/v")]
    [InlineData("/\\p{scx=Katakana}/u")]
    [InlineData("/\\p{ASCII}/u")]
    [InlineData("/\\P{Any}/v")]
    [InlineData("/\\p{Assigned}/u")]
    [InlineData("/\\p{Alphabetic}/u")]
    [InlineData("/\\p{White_Space}/v")]
    [InlineData("/\\p{space}/u")]
    [InlineData("/\\p{ID_Start}/u")]
    [InlineData("/\\p{ID_Continue}/u")]
    [InlineData("/\\p{Emoji}/v")]
    [InlineData("/\\p{Extended_Pictographic}/u")]
    [InlineData("/\\p{RGI_Emoji}/v")]
    [InlineData("/\\p{Basic_Emoji}/v")]
    [InlineData("/\\p{Emoji_Keycap_Sequence}/v")]
    [InlineData("/\\p{RGI_Emoji_Modifier_Sequence}/v")]
    [InlineData("/\\p{RGI_Emoji_Flag_Sequence}/v")]
    [InlineData("/\\p{RGI_Emoji_Tag_Sequence}/v")]
    [InlineData("/\\p{RGI_Emoji_ZWJ_Sequence}/v")]
    [InlineData("/[\\p{RGI_Emoji}]/v")]
    [InlineData("/[\\p{ASCII}&&\\p{Letter}]/v")]
    public void Unicode_properties_are_accepted(string input)
        => Accept(input);

    [Theory]
    [InlineData("/\\p{}/u")]
    [InlineData("/\\p{Unknown}/u")]
    [InlineData("/\\p{letter}/u")]
    [InlineData("/\\p{ASCII_Hex_digit}/v")]
    [InlineData("/\\p{Script=greek}/u")]
    [InlineData("/\\p{script=Greek}/u")]
    [InlineData("/\\p{Greek}/u")]
    [InlineData("/\\p{gc=Greek}/v")]
    [InlineData("/\\p{sc=Letter}/u")]
    [InlineData("/\\p{GeneralCategory=Letter}/u")]
    [InlineData("/\\p{General_Category = Letter}/u")]
    [InlineData("/\\p{General_Category=Uppercase Letter}/v")]
    [InlineData("/\\p{IsGreek}/u")]
    [InlineData("/\\p{InBasic_Latin}/u")]
    [InlineData("/\\p{Age=17.0}/v")]
    [InlineData("/\\p{ASCII=Yes}/u")]
    [InlineData("/\\p{Emoji=Yes}/v")]
    [InlineData("/\\p{WSpace}/u")]
    [InlineData("/\\p{RGI_Emoji}/u")]
    [InlineData("/\\P{RGI_Emoji}/v")]
    [InlineData("/[^\\p{RGI_Emoji}]/v")]
    [InlineData("/\\p{L/u")]
    [InlineData("/\\pL/u")]
    [InlineData("/\\p/u")]
    [InlineData("/\\P/u")]
    public void Unicode_properties_are_rejected(string input)
        => Reject(input);

    [Theory]
    [InlineData("/[]/v")]
    [InlineData("/[^]/v")]
    [InlineData("/[abc]/v")]
    [InlineData("/[[a-z][0-9]]/v")]
    [InlineData("/[a&&b]/v")]
    [InlineData("/[a--b]/v")]
    [InlineData("/[[a-z]&&[^aeiou]]/v")]
    [InlineData("/[[a-z]--[aeiou]]/v")]
    [InlineData("/[a&&b&&c]/v")]
    [InlineData("/[a--b--c]/v")]
    [InlineData("/[\\q{ab|cd}]/v")]
    [InlineData("/[\\q{}]/v")]
    [InlineData("/[\\q{a|}]/v")]
    [InlineData("/[\\q{a|b}]/v")]
    [InlineData("/[\\q{\\n|\\u{1F600}}]/v")]
    [InlineData("/[\\q{ab}&&\\q{ab|cd}]/v")]
    [InlineData("/[\\q{ab}--\\q{cd}]/v")]
    [InlineData("/[^\\q{a|b}]/v")]
    [InlineData("/[^a&&\\q{ab}]/v")]
    [InlineData("/[^a--\\q{ab}]/v")]
    [InlineData("/[^\\q{ab}&&a]/v")]
    [InlineData("/[^\\q{a}]/v")]
    [InlineData("/[^\\q{😀}]/v")]
    [InlineData("/[^\\q{\\uD83D\\uDE00}]/v")]
    [InlineData("/[a\\&b]/v")]
    [InlineData("/[\\(\\)\\[\\]\\{\\}\\|\\/\\-]/v")]
    [InlineData("/[^^]/v")]
    [InlineData("/[\\!\\#\\$\\%\\&\\*\\+\\,\\.\\:\\;\\<\\=\\>\\?\\@\\^\\`\\~]/v")]
    public void Unicode_set_operations_and_strings_are_accepted(string input)
        => Accept(input);

    [Theory]
    [InlineData("/[a&&]/v")]
    [InlineData("/[&&a]/v")]
    [InlineData("/[a--]/v")]
    [InlineData("/[--a]/v")]
    [InlineData("/[a&&b--c]/v")]
    [InlineData("/[a--b&&c]/v")]
    [InlineData("/[ab&&c]/v")]
    [InlineData("/[a&&bc]/v")]
    [InlineData("/[a&&&b]/v")]
    [InlineData("/[a---b]/v")]
    [InlineData("/[a-b-c]/v")]
    [InlineData("/[-a]/v")]
    [InlineData("/[a-]/v")]
    [InlineData("/[()]/v")]
    [InlineData("/[{}]/v")]
    [InlineData("/[|]/v")]
    [InlineData("/[[a]/v")]
    [InlineData("/[!!]/v")]
    [InlineData("/[##]/v")]
    [InlineData("/[$$]/v")]
    [InlineData("/[%%]/v")]
    [InlineData("/[**]/v")]
    [InlineData("/[++]/v")]
    [InlineData("/[,,]/v")]
    [InlineData("/[..]/v")]
    [InlineData("/[::]/v")]
    [InlineData("/[;;]/v")]
    [InlineData("/[<<]/v")]
    [InlineData("/[==]/v")]
    [InlineData("/[>>]/v")]
    [InlineData("/[??]/v")]
    [InlineData("/[@@]/v")]
    [InlineData("/[``]/v")]
    [InlineData("/[~~]/v")]
    [InlineData("/[\\q{a|b}]/u")]
    [InlineData("/\\q{ab}/v")]
    [InlineData("/[\\q{ab]/v")]
    [InlineData("/[\\q{(a)}]/v")]
    [InlineData("/[\\q{a-b}]/v")]
    [InlineData("/[^\\q{ab}]/v")]
    [InlineData("/[^\\q{}]/v")]
    [InlineData("/[^\\p{RGI_Emoji}--a]/v")]
    [InlineData("/[^\\q{ab}&&\\q{ab}]/v")]
    [InlineData("/[\\q{ab}-z]/v")]
    [InlineData("/[a-\\q{ab}]/v")]
    [InlineData("/[^\\q{ab}--\\q{ab}]/v")]
    [InlineData("/[^\\q{\\u{D83D}\\u{DE00}}]/v")]
    public void Unicode_set_operations_and_strings_are_rejected(string input)
        => Reject(input);

    [Theory]
    [InlineData("/(?i:a)/")]
    [InlineData("/(?m:^a$)/u")]
    [InlineData("/(?s:.)/v")]
    [InlineData("/(?ims:a)/")]
    [InlineData("/(?-i:a)/i")]
    [InlineData("/(?i-m:a)/m")]
    [InlineData("/(?im-s:a)/s")]
    [InlineData("/(?i-ms:a)/")]
    [InlineData("/(?-ims:a)/ims")]
    [InlineData("/(?i:a(?-i:b)c)/")]
    [InlineData("/(?i:(?<a>x))\\k<a>/u")]
    [InlineData("/(?i:)/")]
    [InlineData("/(?s:.){2}/v")]
    [InlineData("/(?i:(a))\\1/u")]
    [InlineData("/(?i-:a)/")]
    public void Modifier_groups_are_accepted(string input)
        => Accept(input);

    [Theory]
    [InlineData("/(?i)/")]
    [InlineData("/(?i)a/")]
    [InlineData("/(?i:a/")]
    [InlineData("/(?i:a))/")]
    [InlineData("/(?ii:a)/")]
    [InlineData("/(?i-i:a)/")]
    [InlineData("/(?i-ii:a)/")]
    [InlineData("/(?g:a)/")]
    [InlineData("/(?d:a)/")]
    [InlineData("/(?u:a)/u")]
    [InlineData("/(?v:a)/v")]
    [InlineData("/(?y:a)/")]
    [InlineData("/(?a:a)/")]
    [InlineData("/(?-:a)/")]
    [InlineData("/(?--i:a)/")]
    [InlineData("/(?i-m-s:a)/")]
    [InlineData("/(?I:a)/")]
    public void Modifier_groups_are_rejected(string input)
        => Reject(input);

    // These script names and aliases were added in Unicode 17.0.0.
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
        // Equivalent token bundles may carry different branch facts; select a representative.
        var regex = source.Policy.Goals.First(goal =>
            goal.Terminals.Any(terminal => terminal.Caption == "RegularExpressionLiteral") &&
            !goal.Terminals.Any(terminal => terminal.Caption == "TemplateTail"));
        var division = source.Policy.Goals.First(goal =>
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

[Collection(JavaScriptScriptCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptRegExp")]
public sealed class JavaScriptRegExpScriptIntegrationTests
{
    private readonly JavaScriptScriptFixture _fixture;
    public JavaScriptRegExpScriptIntegrationTests(JavaScriptScriptFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("const pattern = /(?<name>a)\\k<name>/u;")]
    [InlineData("let value = /[\\p{ASCII}&&\\p{Letter}]/v;")]
    [InlineData("/a|b/; /[\\q{ab|cd}]/v;")]
    [InlineData("if (value) /(?i:a)/.test(value);")]
    [InlineData("if (value) {} /(?<x>a)|(?<x>b)/u.test(value);")]
    [InlineData("function f() { return /(?<=prefix)value/u; }")]
    [InlineData("const f = () => /(?:a{2})/;")]
    [InlineData("const f = () => { return /\\p{RGI_Emoji}/v; };")]
    [InlineData("const f = () => {}; /(?i:a)/;")]
    [InlineData("const f = ({value = /[a-z]/u} = {}) => value;")]
    [InlineData("const [first = /a+/u] = values;")]
    [InlineData("let {value: local = /[a--b]/v} = object;")]
    [InlineData("class A { field = /[a-z]/u; static value = /(?-i:a)/i; }")]
    [InlineData("class A { #value = /(?<name>a)/u; method() { return this.#value; } }")]
    [InlineData("class A { static { /[\\q{ab}]/v; } }")]
    [InlineData("class A extends Base { method() { return /a+/; } }")]
    [InlineData("const value = `${/(?<name>a)/u}`;")]
    [InlineData("const value = tag`outer ${/[[a-z]--[aeiou]]/v} tail`;")]
    [InlineData("import(path, {with: {pattern: /[a-z]/u}});")]
    [InlineData("for (const value of /a/.source) {}")]
    [InlineData("for (let value = /(?:a)/; value; value = /b/) {}")]
    [InlineData("switch (value) { case /[a-z]/u: break; default: /a/; }")]
    [InlineData("try {} catch (error) { /a{1,2}/u; } finally { /b/; }")]
    [InlineData("({[/a/.source]: /b/});")]
    [InlineData("value / /(?<name>a)\\k<name>/u.source;")]
    [InlineData("value /= /a/.source;")]
    [InlineData("value\n/other/divisor;")]
    [InlineData("function f() { return\n/[a-z]/u; }")]
    public void Script_accepts_RegExp_bodies_in_nested_JavaScript_contexts(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("const pattern = /(/;")]
    [InlineData("let value = /\\q/u;")]
    [InlineData("/a/; /[z-a]/u;")]
    [InlineData("if (value) /a{3,2}/.test(value);")]
    [InlineData("if (value) {} /(?<x>a)(?<x>b)/u.test(value);")]
    [InlineData("function f() { return /(?g:a)/; }")]
    [InlineData("const f = () => /(?=a)*/u;")]
    [InlineData("const f = () => { return /\\P{RGI_Emoji}/v; };")]
    [InlineData("const f = () => {}; /\\k<missing>/u;")]
    [InlineData("const f = ({value = /[!!]/v} = {}) => value;")]
    [InlineData("const [first = /\\p{greek}/u] = values;")]
    [InlineData("let {value: local = /[a&&]/v} = object;")]
    [InlineData("class A { field = /\\8/u; }")]
    [InlineData("class A { #value = /(?<>a)/u; }")]
    [InlineData("class A { static { /[^\\q{ab}]/v; } }")]
    [InlineData("class A extends Base { method() { return /a++/; } }")]
    [InlineData("const value = `${/a{2,1}/u}`;")]
    [InlineData("const value = tag`outer ${/[\\d-a]/u} tail`;")]
    [InlineData("import(path, {with: {pattern: /\\p{Invalid}/u}});")]
    [InlineData("for (const value of /\\9/u.source) {}")]
    [InlineData("for (let value = /a}/u; value; value = /b/) {}")]
    [InlineData("switch (value) { case /[a-\\d]/u: break; }")]
    [InlineData("try {} catch (error) { /(?-:a)/; }")]
    [InlineData("({[/a/.source]: /(?<a>x)(?<a>y)/});")]
    public void Script_rejects_RegExp_bodies_in_nested_JavaScript_contexts(string input)
    {
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

}

[Collection(JavaScriptModuleCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptRegExp")]
public sealed class JavaScriptRegExpModuleIntegrationTests
{
    private readonly JavaScriptModuleFixture _fixture;
    public JavaScriptRegExpModuleIntegrationTests(JavaScriptModuleFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("export const pattern = /(?<name>a)\\k<name>/u;")]
    [InlineData("export default /[\\q{ab|cd}]/v;")]
    [InlineData("export default class { field = /[a-z]/u; }")]
    [InlineData("export default function() { return /(?i:a)/; }")]
    [InlineData("export function f(value = /(?<=a)b/u) { return value; }")]
    [InlineData("const pattern = /\\p{RGI_Emoji}/v; export {pattern};")]
    [InlineData("import value from \"module\"; /(?<name>a)/u.test(value);")]
    [InlineData("import \"module\"; /[a--b]/v;")]
    [InlineData("await /(?<name>a)\\k<name>/u;")]
    [InlineData("for await (const value of source) { /[a-z]/u; }")]
    [InlineData("const value = import.meta.url + /a/.source;")]
    [InlineData("import(/a/.source);")]
    [InlineData("export class A { static { /(?s:.)/; } }")]
    [InlineData("export const f = () => /(?<name>a)|(?<name>b)/v;")]
    [InlineData("const value = `${/[\\p{Letter}&&\\p{ASCII}]/v}`; export {value};")]
    public void Module_accepts_RegExp_bodies_in_nested_JavaScript_contexts(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("export const pattern = /(/;")]
    [InlineData("export default /[z-a]/u;")]
    [InlineData("export default class { field = /\\p{Unknown}/u; }")]
    [InlineData("export default function() { return /\\q/u; }")]
    [InlineData("export function f(value = /(?<=a)*/) { return value; }")]
    [InlineData("const pattern = /[^\\q{ab}]/v; export {pattern};")]
    [InlineData("import value from \"module\"; /(?<name>a)(?<name>b)/u.test(value);")]
    [InlineData("import \"module\"; /[a&&b--c]/v;")]
    [InlineData("await /(?<name>a)\\k<missing>/u;")]
    [InlineData("for await (const value of source) { /[\\d-a]/u; }")]
    [InlineData("const value = import.meta.url + /a{3,2}/.source;")]
    [InlineData("import(/\\p{RGI_Emoji}/u.source);")]
    [InlineData("export class A { static { /(?ii:.)/; } }")]
    [InlineData("export const f = () => /(?<name>a)(?<name>b)/v;")]
    [InlineData("const value = `${/\\8/u}`; export {value};")]
    public void Module_rejects_RegExp_bodies_in_nested_JavaScript_contexts(string input)
    {
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

}
