using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using Janglim;
using Janglim.FrontEnd;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;

// This consumer has a single NuGet PackageReference and no repository ProjectReference.
// JavaScript is parsed and validated, never evaluated.
var checks = new List<object>();
int count = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new InvalidOperationException("FAILED: " + name);
    count++;
    checks.Add(new { name, passed = true });
    Console.WriteLine("PASS " + name);
}

// 1. The existing README quick start still works through the packaged API.
var quick = EbnfGrammarReader.Read("""
    Expr   : Expr '+' Term | Term ;
    Term   : Term '*' Factor | Factor ;
    Factor : '(' Expr ')' | id ;
    id     := "[a-zA-Z]+" ;
    """);
Check(quick.Success, "README EBNF grammar construction");
var quickLexer = new Lexer();
foreach (var terminal in quick.Grammar.TerminalSet) quickLexer.AddTokenRule(terminal);
var quickParser = new LALRParser(quick.Grammar);
Check(quickParser.Parsing(quickLexer.Lexing("a + a * a").TokensForParsing).Success,
    "README eager-lexer arithmetic example");
Check(!quickParser.Parsing(quickLexer.Lexing("a + * a").TokensForParsing).Success,
    "README arithmetic syntax error");

// 2. Longest actual match wins; equal matches retain keyword priority.
var maximalLexer = new Lexer();
var identifier = new Terminal(TokenType.Identifier, "[$A-Za-z_][$0-9A-Za-z_]*", "identifier", true, true);
var keyword = new Terminal(TokenType.Keyword, "if", "IF");
maximalLexer.AddTokenRule(identifier);
maximalLexer.AddTokenRule(keyword);
foreach (char whitespace in " \t\r\n")
    maximalLexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Delimiter, whitespace.ToString(), false));
var eager = maximalLexer.Lexing("if ifx if$").TokensForParsing.ToArray();
Check(eager.Select(token => token.Data).SequenceEqual(new[] { "if", "ifx", "if$" }),
    "maximal munch preserves if/ifx/if$ boundaries");
Check(ReferenceEquals(eager[0].PatternInfo.Terminal, keyword)
    && eager.Skip(1).All(token => ReferenceEquals(token.PatternInfo.Terminal, identifier)),
    "maximal munch keyword priority with identifier registered first");
var maximalGoal = new LexicalGoal(new[] { identifier, keyword });
var maximalSource = maximalLexer.CreateTokenSource("if ifx if$", new LexicalPolicy(maximalGoal));
var maximalCursor = maximalSource.Start;
var contextual = new List<TokenData>();
for (int i = 0; i < 4; i++)
{
    var read = maximalSource.Peek(maximalCursor, maximalGoal);
    if (read.Token.Kind is EndMarker) break;
    contextual.Add(read.Token);
    maximalCursor = read.Next;
}
Check(contextual.Select(token => token.Input).SequenceEqual(new[] { "if", "ifx", "if$" }),
    "contextual source uses maximal munch too");

// 3. Public LexicalGoal -> LexicalPolicy -> token source and immutable Peek cursors.
var contextualGrammar = EbnfGrammarReader.Read("""
    S : id '=' Expr ';' ;
    Expr : Expr '/' Atom | Atom ;
    Atom : id | rx ;
    id := Identifier "[a-z]+" ;
    rx := StringLiteral "/[a-z]+/[a-z]*" ;
    """);
Check(contextualGrammar.Success, "parser-driven EBNF grammar construction");
var contextualTerminals = contextualGrammar.Grammar.TerminalSet
    .Where(terminal => terminal is not EndMarker && terminal is not Epsilon).ToArray();
var rx = contextualTerminals.Single(terminal => terminal.Caption == "rx");
var slash = contextualTerminals.Single(terminal => terminal.Value == "/");
var common = contextualTerminals.Where(terminal => !ReferenceEquals(terminal, rx) && !ReferenceEquals(terminal, slash));
var div = new LexicalGoal(common.Append(slash));
var regExp = new LexicalGoal(common.Append(rx));
var policy = new LexicalPolicy(div, regExp);
var contextualLexer = new Lexer();
foreach (var terminal in contextualGrammar.Grammar.TerminalSet) contextualLexer.AddTokenRule(terminal);
var source = contextualLexer.CreateTokenSource("result = /ab/g;", policy);
Check(ReferenceEquals(source.Policy, policy) && source.Policy.Goals.Count == 2,
    "source retains the configured LexicalPolicy");
var first = source.Peek(source.Start, div);
var second = source.Peek(first.Next, div);
var current = second.Next;
Check(current.SourceOffset == 8 && current.TokenIndex == 2 && current.PreviousToken.Input == "=",
    "current cursor before Peek points to source offset 8");
var regexRead = source.Peek(current, regExp);
Check(regexRead.Token.Input == "/ab/g" && regexRead.Token.StartIndex == 9 && regexRead.Token.EndIndex == 13
    && regexRead.Token.LeadingTrivia.Text == " ", "Peek returns the regex token and exact source trivia");
Check(current.SourceOffset == 8 && current.TokenIndex == 2 && current.PreviousToken.Input == "=",
    "Peek leaves the current cursor unchanged");
Check(regexRead.Next.SourceOffset == 14 && regexRead.Next.TokenIndex == 3
    && regexRead.Next.PreviousToken.Input == "/ab/g" && ReferenceEquals(current.SourceIdentity, regexRead.Next.SourceIdentity),
    "Peek returns a separate Next checkpoint at source offset 14");
var divisionRead = source.Peek(current, div);
Check(divisionRead.Token.Input == "/" && divisionRead.Next.SourceOffset == 10,
    "the same cursor supports an independent Div read");
current = regexRead.Next;
Check(source.Peek(current, div).Token.Input == ";", "adopting Next advances to the semicolon");
var contextualParser = new LALRParser(contextualGrammar.Grammar);
Check(contextualParser.Parsing(source).Success, "parser-driven automatic RegExp selection");
Check(contextualParser.Parsing(contextualLexer.CreateTokenSource("result = left / right;", policy)).Success,
    "parser-driven automatic Div selection");

// 4. All three public JavaScript entry goals parse and then validate static errors.
var goals = new Dictionary<string, (LALRParser Parser, Func<string, ITokenSource> Source,
    Func<ParseTreeSymbol, IReadOnlyList<JavaScriptDiagnostic>> Validate)>();
var expression = new JavaScriptExpressionGrammar();
goals.Add("Expression", (new LALRParser(expression.Grammar), expression.CreateTokenSource, expression.Validate));
var script = new JavaScriptScriptGrammar();
goals.Add("Script", (new LALRParser(script.Grammar), script.CreateTokenSource, script.Validate));
var module = new JavaScriptModuleGrammar();
goals.Add("Module", (new LALRParser(module.Grammar), module.CreateTokenSource, module.Validate));
void JavaScript(string goal, string input, bool accepted, bool requireParse = false, string? diagnosticCode = null)
{
    var runner = goals[goal];
    var parsed = runner.Parser.Parsing(runner.Source(input));
    var diagnostics = parsed.Success ? runner.Validate(parsed.ToParseTree) : Array.Empty<JavaScriptDiagnostic>();
    if (requireParse) Check(parsed.Success, goal + " structural parse: " + input);
    Check((parsed.Success && diagnostics.Count == 0) == accepted,
        goal + (accepted ? " accepted: " : " rejected: ") + input);
    if (diagnosticCode != null)
        Check(diagnostics.Any(item => item.Code == diagnosticCode), goal + " diagnostic " + diagnosticCode + ": " + input);
}
JavaScript("Expression", "a + /ab/g.test(value)", true, true);
JavaScript("Expression", "a +", false);
JavaScript("Expression", "({ get value(x) {} })", false, true);
JavaScript("Script", "const {value} = source; function f([first], ...rest) { return first; }", true, true);
JavaScript("Script", "const value = ;", false);
JavaScript("Script", "let value; let value;", false, true);
JavaScript("Module", "import {value as item} from 'module'; export const result = await import('other'); export {item};", true, true);
JavaScript("Module", "export const value = ;", false);
JavaScript("Module", "export {missing};", false, true);
JavaScript("Script", "async function f() { return await work(); }", true, true);
JavaScript("Script", "const f = async ({value}) => await value;", true, true);
JavaScript("Script", "function* f() { yield 1; yield* values; }", true, true);
JavaScript("Script", "async function* f() { yield await work(); }", true, true);
JavaScript("Expression", "({ run() {}, async work() { return await task; }, *items() { yield 1; }, async *stream() { yield await task; }, get value() { return 1; }, set value(v) { this._value = v; } })", true, true);
JavaScript("Script", "async function f(await) {}", false);
JavaScript("Script", "function f() { yield 1; }", false);
JavaScript("Expression", "/(?<name>a)\\k<name>/u", true, true);
JavaScript("Expression", "/[\\p{Letter}&&\\p{ASCII}]/v", true, true);
JavaScript("Expression", "/a{3,2}/u", false, true, "regexp_syntax");
JavaScript("Expression", "/(?<name>a)\\k<missing>/u", false, true, "regexp_syntax");
JavaScript("Module", "export const pattern = /[z-a]/u;", false, true, "regexp_syntax");

// 5. Package resources, license and bundled engine versions are checked independently.
var grammarAssembly = typeof(JavaScriptExpressionGrammar).Assembly;
var resources = grammarAssembly.GetManifestResourceNames();
foreach (string name in new[] { "Expressions", "Bindings", "Statements", "Classes", "Modules", "Functions" })
{
    string expected = "Janglim.JavaScript.Ecma2026." + name + ".ebnf";
    Check(resources.Contains(expected), "embedded grammar resource " + name);
    using var stream = grammarAssembly.GetManifestResourceStream(expected);
    Check(stream != null && stream.Length > 0, "nonempty grammar resource " + name);
}
string packagePath = args.Length > 0 ? args[0] : Path.GetFullPath("../packages/Janglim.0.5.0.nupkg");
using (var package = ZipFile.OpenRead(packagePath))
{
    var license = package.GetEntry("licenses/Unicode-LICENSE.txt");
    Check(license != null && license.Length > 0, "Unicode license is bundled in the NuGet package");
    var engineDlls = package.Entries.Where(entry => entry.FullName.StartsWith("lib/netstandard2.1/", StringComparison.Ordinal)
        && entry.FullName.EndsWith(".dll", StringComparison.Ordinal) && entry.FullName.Count(character => character == '/') == 2)
        .OrderBy(entry => entry.Name).ToArray();
    Check(engineDlls.Length == 9, "NuGet package contains the expected nine engine DLLs");
    string extract = Path.Combine(Path.GetDirectoryName(packagePath)!, "inspection");
    Directory.CreateDirectory(extract);
    foreach (var entry in engineDlls)
    {
        string assemblyPath = Path.Combine(extract, entry.Name);
        entry.ExtractToFile(assemblyPath, true);
        var identity = AssemblyName.GetAssemblyName(assemblyPath);
        Check(identity.Version == new Version(0, 5, 0, 0), "engine assembly version: " + entry.Name);
    }
    var readme = package.GetEntry("README.md");
    Check(readme != null && readme.Length > 0, "NuGet package embeds README.md");
}
Console.WriteLine($"Completed {count} independent package-consumer checks.");
File.WriteAllText("consumer-verification.json", JsonSerializer.Serialize(new
{
    packageVersion = "0.5.0", packagePath, verifiedAtUtc = DateTime.UtcNow, total = count, checks
}, new JsonSerializerOptions { WriteIndented = true }));
