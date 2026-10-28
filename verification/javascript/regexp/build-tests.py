import json
from pathlib import Path

groups = {}
def add(name, expected, literals):
    groups.setdefault((name, expected), []).extend(literals)

add('Atoms_alternatives_and_quantifiers', True, [
 r'/a/', r'/a|b/', r'/a||b/', r'/(?:)/', r'/|/', r'/a|/', r'/|a/',
 r'/./', r'/a*/', r'/a+/', r'/a?/', r'/a*?/', r'/a+?/', r'/a??/',
 r'/a{0}/', r'/a{2}/', r'/a{2,}/', r'/a{2,4}/', r'/a{0,0}?/',
 r'/a{0002,0004}/', r'/(?:ab|cd){2,4}/', r'/(a(b(c)))+/',
 r'/a{999999999999999999999999999999999999999}/u',
 r'/a{999999999999999999999999999999999999999,}/v',
 r'/a{100000000000000000000000000000000000000,200000000000000000000000000000000000000}/u',
 r'/(?:x(?:y|z)*)?/', r'/a\{2\}/u', r'/[{}]+/u',
 r'/a{2/', r'/a{2,/', r'/a{,2}/', r'/a{z}/', r'/a}/', r'/{/',
 r'/]/', r'/a{2,1x}/', r'/a{1, 2}/',
])
add('Atoms_alternatives_and_quantifiers', False, [
 r'/(?:*a)/', r'/+a/', r'/?a/', r'/a**/', r'/a++/', r'/a?+/', r'/a{1}*/',
 r'/a{1}+/', r'/a{1}?+/', r'/a{3,2}/', r'/a{3,2}/u', r'/a{3,2}/v',
 r'/a{0003,0002}/', r'/{1}/', r'/|*/', r'/(?:a|*)/',
 r'/a{999999999999999999999999999999999999998,999999999999999999999999999999999999997}/u',
 r'/a{1000000000000000000000000000000000000000,999999999999999999999999999999999999999}/v',
 r'/a{2/u', r'/a{2,/u', r'/a{,2}/u', r'/a{z}/u', r'/a}/u', r'/{/u',
 r'/]/u', r'/a{2,1x}/u', r'/a{1, 2}/u', r'/{,}/v', r'/a{1,,2}/v',
])

add('Groups_and_assertions', True, [
 r'/()/', r'/(())/', r'/(?:a)/', r'/(?=a)a/', r'/(?!a)b/',
 r'/(?<=a)b/', r'/(?<!a)b/', r'/(?<=a|bc)d/u', r'/(?<=a*)b/',
 r'/^a$/', r'/\ba\B/', r'/(?:^|$)/', r'/(?=a)?/', r'/(?!a)*/',
 r'/(?=a)+/', r'/(?=a){2}/', r'/(?=a)??/', r'/(?:\b)*/u',
 r'/(?<name>a)/', r'/(?<π>a)/u', r'/(?<$x>a)/', r'/(?<_x>a)/',
 r'/(?<\u0061>a)/u', r'/(?<\u{61}>a)/u', r'/(?<\u{10400}>a)/u',
 r'/(?<a\u200Cb>x)/u', r'/(?<a\u200Db>x)/u',
 r'/(?<\uD835\uDC00>a)/u', r'/(?<𝒜>a)/v',
 r'/(?<a>(?:x|y))/', r'/(?<a>x)|(?:(?<a>y)|z)/v',
])
add('Groups_and_assertions', False, [
 r'/(/', r'/)/', r'/(a))/', r'/((a)/', r'/(?a)/', r'/(?#comment)/',
 r'/(?P<name>a)/', r'/(?<name>a/', r'/(?<name)/', r'/(?<>a)/',
 r'/(?<1>a)/', r'/(?<a-b>x)/', r'/(?<a b>x)/', r'/(?<\u0030>x)/u',
 r'/(?<\uD800>x)/u', r'/(?<\u{110000}>x)/u', r'/(?<\u{}>x)/u',
 r'/(?<\x61>x)/u', r'/(?<😀>a)/u', r'/(?=a)?/u', r'/(?=a)*/v',
 r'/(?<\u{D835}\u{DC00}>a)/u', r'/(?<\uDC00\uD800>a)/u',
 r'/(?<=a)?/', r'/(?<!a)*/', r'/^*/', r'/$+/', r'/\b?/', r'/\B{2}/u',
])

add('Backreferences_and_duplicate_named_groups', True, [
 r'/(a)\1/', r'/\1(a)/u', r'/(a)\1/u', r'/(a)(b)\2\1/v',
 r'/(?<name>a)\k<name>/u', r'/\k<name>(?<name>a)/u',
 r'/(?<\u0061>x)\k<a>/u', r'/(?<a>x)\k<\u0061>/u',
 r'/(?<π>x)\k<π>/u', r'/(?<x>a)|(?<x>b)/',
 r'/(?:(?<x>a)|(?<x>b))\k<x>/u', r'/(?:(?<x>a)|(?<x>b))*/v',
 r'/(?<x>a)|(?<x>b)|(?<x>c)/v', r'/(?:(?<x>a)|b)|(?<x>c)/u',
 r'/(?:(?<x>a)|b)(?:(?<y>c)|(?<y>d))/u',
 r'/(?<x>a)|(?:(?<y>b)(?<x>c))/',
 r'/\k<unknown>/', r'/\k/', r'/\1/', r'/\8/', r'/\9/',
 r'/\11/', r'/(a)\11/', r'/(a)\2/', r'/\123/', r'/[\1]/',
 r'/[(?<name>)]\k<unknown>/', r'/\k<missing>[(?<name>)]/',
 r'/(?:(?<a>x)|(?<a>y)){2}/u',
 r'/\0/u', r'/\0/v', r'/\0a/u',
])
add('Backreferences_and_duplicate_named_groups', False, [
 r'/\1/u', r'/\8/u', r'/\9/v', r'/(a)\2/u', r'/(a)\11/u',
 r'/\99999999999999999999999999999/u', r'/[\1]/u', r'/[\8]/v',
 r'/\01/u', r'/\00/v', r'/\09/u',
 r'/(?<x>a)(?<x>b)/', r'/(?<x>a)(?<\u0078>b)/u',
 r'/(?<x>a(?<x>b))/', r'/(?<a>(?<a>x)|y)/',
 r'/(?:(?<x>a)|b)(?:(?<x>c)|d)/u',
 r'/(?<x>a)|(?<x>b)(?<x>c)/v', r'/(?=(?<x>a))(?<x>b)/u',
 r'/\k<unknown>/u', r'/(?<x>a)\k<unknown>/', r'/(?<x>a)\k/',
 r'/(?<x>a)\k<>/', r'/(?<x>a)\k<x/', r'/\k<1>/u',
 r'/(?<a>x)\k<\x61>/u', r'/\k/u',
])

add('Escapes_across_legacy_and_Unicode_modes', True, [
 r'/\n\r\t\v\f\b\B/', r'/\d\D\s\S\w\W/u', r'/\cA\cz/',
 r'/\x00\xFF/u', r'/\u0000\uFFFF/u', r'/\u{0}\u{10FFFF}/u',
 r'/\u{0000000000000000000000000000000000000061}/v',
 r'/\uD800/u', r'/\uDC00/v', r'/\uD83D\uDE00/u',
 r'/\u{D800}/u', r'/\u{DFFF}/v', r'/\^\$\\\.\*\+\?\(\)\[\]\{\}\|\//u',
 r'/\q/', r'/\a/', r'/\_/', r'/\-/', r'/\:/', r'/\c/',
 r'/\c1/', r'/\x/', r'/\xG0/', r'/\u/', r'/\uG000/', r'/\u{61}/',
 r'/\p{Letter}/', r'/\P{bad}/',
])
add('Escapes_across_legacy_and_Unicode_modes', False, [
 r'/\q/u', r'/\a/v', r'/\_/u', r'/\-/u', r'/\:/v',
 r'/\c/u', r'/\c1/u', r'/\x/u', r'/\xG0/u', r'/\x1/u',
 r'/\u/u', r'/\uG000/v', r'/\u123/u', r'/\u{}/u',
 r'/\u{110000}/u', r'/\u{FFFFFFFFFFFFFFFFFFFFFF}/v',
 r'/\u{-1}/u', r'/\u{1_0}/v', r'/\u{61/u',
])

add('Character_classes_and_ranges', True, [
 r'/[]/', r'/[^]/', r'/[abc]/', r'/[^abc]/u', r'/[a-zA-Z0-9_]/v',
 r'/[-a]/u', r'/[a-]/u', r'/[\-]/v', r'/[\]\[]/u', r'/[\b]/u',
 r'/[\0]/v', r'/[\cA]/u', r'/[\c_]/', r'/[\c0]/', r'/[\c]/',
 r'/[\x41-\u005A]/u', r'/[\u{1F600}-\u{1F64F}]/u',
 r'/[\uD800-\uDFFF]/u', r'/[😀-🙏]/u', r'/[a-a]/u',
 r'/[\uD83D\uDE00-\uD83D\uDE4F]/u',
 r'/[\d-a]/', r'/[a-\d]/', r'/[\s-\w]/',
 r'/[a&&b]/', r'/[&&]/u', r'/[!!]/u',
 r'/[()[\]{}|\/]/', r'/[\p{L}\P{N}]/u',
])
add('Character_classes_and_ranges', False, [
 r'/[z-a]/', r'/[z-a]/u', r'/[9-0]/v', r'/[\u005A-\x41]/u',
 r'/[\u{1F64F}-\u{1F600}]/u', r'/[\d-a]/u', r'/[a-\d]/v',
 r'/[\s-\w]/u', r'/[\p{L}-a]/u', r'/[a-\p{L}]/u',
 r'/[\c_]/u', r'/[\c0]/v', r'/[\c]/u', r'/[\B]/u', r'/[\q]/u',
 r'/[\u{}]/u', r'/[\01]/u', r'/[\8]/u', r'/[\-a-\-]/u',
 r'/[\u{D83D}\u{DE00}-\u{D83D}\u{DE4F}]/u',
 r'/[\p{L}-a]/', r'/[a--]/',
])

add('Unicode_properties', True, [
 r'/\p{L}/u', r'/\p{Letter}/v', r'/\p{Lu}/u', r'/\P{Lowercase_Letter}/u',
 r'/\p{General_Category=Decimal_Number}/u', r'/\p{gc=Nd}/v',
 r'/\p{Script=Greek}/u', r'/\p{sc=Grek}/u', r'/\p{Script_Extensions=Hira}/v',
 r'/\p{scx=Katakana}/u', r'/\p{ASCII}/u', r'/\P{Any}/v', r'/\p{Assigned}/u',
 r'/\p{Alphabetic}/u', r'/\p{White_Space}/v', r'/\p{space}/u', r'/\p{ID_Start}/u',
 r'/\p{ID_Continue}/u', r'/\p{Emoji}/v', r'/\p{Extended_Pictographic}/u',
 r'/\p{RGI_Emoji}/v', r'/\p{Basic_Emoji}/v', r'/\p{Emoji_Keycap_Sequence}/v',
 r'/\p{RGI_Emoji_Modifier_Sequence}/v', r'/\p{RGI_Emoji_Flag_Sequence}/v',
 r'/\p{RGI_Emoji_Tag_Sequence}/v', r'/\p{RGI_Emoji_ZWJ_Sequence}/v',
 r'/[\p{RGI_Emoji}]/v', r'/[\p{ASCII}&&\p{Letter}]/v',
])
add('Unicode_properties', False, [
 r'/\p{}/u', r'/\p{Unknown}/u', r'/\p{letter}/u', r'/\p{ASCII_Hex_digit}/v',
 r'/\p{Script=greek}/u', r'/\p{script=Greek}/u', r'/\p{Greek}/u',
 r'/\p{gc=Greek}/v', r'/\p{sc=Letter}/u', r'/\p{GeneralCategory=Letter}/u',
 r'/\p{General_Category = Letter}/u', r'/\p{General_Category=Uppercase Letter}/v',
 r'/\p{IsGreek}/u', r'/\p{InBasic_Latin}/u', r'/\p{Age=17.0}/v',
 r'/\p{ASCII=Yes}/u', r'/\p{Emoji=Yes}/v', r'/\p{WSpace}/u',
 r'/\p{RGI_Emoji}/u', r'/\P{RGI_Emoji}/v', r'/[^\p{RGI_Emoji}]/v',
 r'/\p{L/u', r'/\pL/u', r'/\p/u', r'/\P/u',
])

add('Unicode_set_operations_and_strings', True, [
 r'/[]/v', r'/[^]/v', r'/[abc]/v', r'/[[a-z][0-9]]/v',
 r'/[a&&b]/v', r'/[a--b]/v', r'/[[a-z]&&[^aeiou]]/v',
 r'/[[a-z]--[aeiou]]/v', r'/[a&&b&&c]/v', r'/[a--b--c]/v',
 r'/[\q{ab|cd}]/v', r'/[\q{}]/v', r'/[\q{a|}]/v', r'/[\q{a|b}]/v',
 r'/[\q{\n|\u{1F600}}]/v', r'/[\q{ab}&&\q{ab|cd}]/v',
 r'/[\q{ab}--\q{cd}]/v', r'/[^\q{a|b}]/v',
 r'/[^a&&\q{ab}]/v', r'/[^a--\q{ab}]/v', r'/[^\q{ab}&&a]/v',
 r'/[^\q{a}]/v', r'/[^\q{😀}]/v', r'/[^\q{\uD83D\uDE00}]/v',
 r'/[a\&b]/v', r'/[\(\)\[\]\{\}\|\/\-]/v', r'/[^^]/v',
 r'/[\!\#\$\%\&\*\+\,\.\:\;\<\=\>\?\@\^\`\~]/v',
])
add('Unicode_set_operations_and_strings', False, [
 r'/[a&&]/v', r'/[&&a]/v', r'/[a--]/v', r'/[--a]/v',
 r'/[a&&b--c]/v', r'/[a--b&&c]/v', r'/[ab&&c]/v', r'/[a&&bc]/v',
 r'/[a&&&b]/v', r'/[a---b]/v', r'/[a-b-c]/v', r'/[-a]/v', r'/[a-]/v',
 r'/[()]/v', r'/[{}]/v', r'/[|]/v', r'/[[a]/v',
 r'/[!!]/v', r'/[##]/v', r'/[$$]/v', r'/[%%]/v', r'/[**]/v',
 r'/[++]/v', r'/[,,]/v', r'/[..]/v', r'/[::]/v', r'/[;;]/v',
 r'/[<<]/v', r'/[==]/v', r'/[>>]/v', r'/[??]/v', r'/[@@]/v',
 r'/[``]/v', r'/[~~]/v', r'/[\q{a|b}]/u',
 r'/\q{ab}/v', r'/[\q{ab]/v', r'/[\q{(a)}]/v', r'/[\q{a-b}]/v',
 r'/[^\q{ab}]/v', r'/[^\q{}]/v', r'/[^\p{RGI_Emoji}--a]/v',
 r'/[^\q{ab}&&\q{ab}]/v', r'/[\q{ab}-z]/v', r'/[a-\q{ab}]/v',
 r'/[^\q{ab}--\q{ab}]/v', r'/[^\q{\u{D83D}\u{DE00}}]/v',
])

add('Modifier_groups', True, [
 r'/(?i:a)/', r'/(?m:^a$)/u', r'/(?s:.)/v', r'/(?ims:a)/',
 r'/(?-i:a)/i', r'/(?i-m:a)/m', r'/(?im-s:a)/s', r'/(?i-ms:a)/',
 r'/(?-ims:a)/ims', r'/(?i:a(?-i:b)c)/', r'/(?i:(?<a>x))\k<a>/u',
 r'/(?i:)/', r'/(?s:.){2}/v', r'/(?i:(a))\1/u', r'/(?i-:a)/',
])
add('Modifier_groups', False, [
 r'/(?i)/', r'/(?i)a/', r'/(?i:a/', r'/(?i:a))/',
 r'/(?ii:a)/', r'/(?i-i:a)/', r'/(?i-ii:a)/', r'/(?g:a)/',
 r'/(?d:a)/', r'/(?u:a)/u', r'/(?v:a)/v', r'/(?y:a)/',
 r'/(?a:a)/', r'/(?-:a)/', r'/(?--i:a)/',
 r'/(?i-m-s:a)/', r'/(?I:a)/',
])

# Correct accidental duplicate entries while retaining a deliberately independent
# expectation review. Node checks are generation-time syntax checks, not a test dependency.
corpus = []
for (name, expected), literals in groups.items():
    for literal in dict.fromkeys(literals):
        corpus.append(dict(category=name, expected=expected, literal=literal))
Path('artifacts/regexp').mkdir(parents=True, exist_ok=True)
Path('artifacts/regexp/corpus.json').write_text(json.dumps(corpus, ensure_ascii=False, indent=2), encoding='utf-8')
print(len(corpus))
