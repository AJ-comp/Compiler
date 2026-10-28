using System;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Tokenize;
using System.Linq;

VerifyQuickStart();
VerifyPredicateExample();
VerifyTypedTokens();
Console.WriteLine("Package consumer checks passed: README examples, typed EBNF, trivia, and assembly versions.");

void VerifyQuickStart()
{
var read = EbnfGrammarReader.Read(@"
    Expr   : Expr '+' Term | Term ;
    Term   : Term '*' Factor | Factor ;
    Factor : '(' Expr ')' | id ;
    id     := ""[a-zA-Z]+"" ;");

if (!read.Success)
    throw new InvalidOperationException(string.Join(Environment.NewLine, read.Errors));

var grammar = read.Grammar;
var lexer = new Lexer();
foreach (var t in grammar.TerminalSet) lexer.AddTokenRule(t);

var parser = new LALRParser(grammar);
var result = parser.Parsing(lexer.Lexing("a + a * a").TokensForParsing);

Console.WriteLine(result.Success);   // True
    if (!result.Success) throw new Exception("README quick start failed.");
    if (typeof(LALRParser).Assembly.GetName().Version != new Version(0, 4, 0, 0))
        throw new Exception("The consumer did not load the 0.4.0 parser assembly.");
}

void VerifyPredicateExample()
{
var options = new EbnfReaderOptions()
    .RegisterPredicate("NoLineBreak", context => !context.NextToken.HasLineBreakBefore);
var read = EbnfGrammarReader.Read(@"
    Throw : 'throw' {NoLineBreak()}? id ';' ;
    id := ""[a-zA-Z_][a-zA-Z_0-9]*"" ;
    comment := Comment ""/\*[\s\S]*?\*/"" ;", options);

if (!read.Success)
    throw new InvalidOperationException(string.Join(Environment.NewLine, read.Errors));

var grammar = read.Grammar;
var lexer = new Lexer();
foreach (var terminal in grammar.TerminalSet) lexer.AddTokenRule(terminal);
var parser = new LALRParser(grammar);

bool Parse(string source) => parser.Parsing(lexer.Lexing(source).TokensForParsing).Success;

Console.WriteLine(Parse("throw value;"));    // True
Console.WriteLine(Parse("throw\nvalue;"));  // False
    if (!Parse("throw value;") || Parse("throw\nvalue;") || Parse("throw /*note\n*/ value;"))
        throw new Exception("README predicate behavior did not match the examples.");
    var value = lexer.Lexing("throw /*note\n*/ value;").TokensForParsing.Single(t => t.Data == "value");
    if (!value.HasLineBreakBefore || value.LeadingTrivia.Text != " /*note\n*/ ")
        throw new Exception("Package consumer did not receive exact comment/newline trivia.");
}

void VerifyTypedTokens()
{
    var typed = EbnfGrammarReader.Read(System.IO.File.ReadAllText(System.IO.Path.Combine(AppContext.BaseDirectory, "typed.ebnf")));
    if (!typed.Success) throw new Exception(string.Join("; ", typed.Errors));
    var grammar = typed.Grammar;
    var lexer = new Lexer();
    foreach (var terminal in grammar.TerminalSet) lexer.AddTokenRule(terminal);
    var parser = new LALRParser(grammar);
    foreach (var source in new[] { "\"hello\"", "'x'", "1.25", "name", "/* comment */ \"ok\"" })
        if (!parser.Parsing(lexer.Lexing(source).TokensForParsing).Success)
            throw new Exception("README typed token example failed: " + source);
    if (!lexer.Lexing("/* unterminated").TokensForParsing.Any(t => grammar.IsRejectedToken(t.PatternInfo.Terminal)))
        throw new Exception("README Reject rule did not mark the invalid comment.");
}
