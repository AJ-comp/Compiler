using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;

var read = EbnfGrammarReader.Read("""
    S : id '=' Expr ';' ;
    Expr : Expr '+' Term | Term ;
    Term : Term '/' Atom | Atom ;
    Atom : id | num | rx ;
    id := Identifier "[a-z]+" ;
    num := NumberLiteral "[0-9]+" ;
    rx := StringLiteral "/[a-z]+/[a-z]*" ;
    """);
if (!read.Success) throw new Exception(string.Join("; ", read.Errors));
var grammar = read.Grammar;
var terminals = grammar.TerminalSet.Where(t => t is not EndMarker && t is not Epsilon).ToArray();
var regex = terminals.Single(t => t.Caption == "rx");
var slash = terminals.Single(t => t.Value == "/");
var common = terminals.Where(t => !ReferenceEquals(t, regex) && !ReferenceEquals(t, slash));
var div = new LexicalGoal(common.Append(slash));
var regexp = new LexicalGoal(common.Append(regex));
var policy = new LexicalPolicy(div, regexp);
var lexer = new Lexer();
foreach (var terminal in terminals) lexer.AddTokenRule(terminal);
var parser = new LALRParser(grammar, true);

var demoSource = lexer.CreateTokenSource("result = a / b;", policy);
var first = demoSource.Peek(demoSource.Start, div);
var second = demoSource.Peek(first.Next, div);
if (first.Token.Input != "result" || second.Token.Input != "=")
    throw new Exception("The documented direct Peek example returned unexpected tokens.");
if (!parser.Parsing(demoSource).Success)
    throw new Exception("Parsing after the direct Peek example failed.");
Console.WriteLine("Direct Peek example and parser-owned parsing: OK");

Check("result = a / b;", "result = a / b ;",
    [div, div, regexp, div, regexp, div, div]);
Check("result = /ab/g;", "result = /ab/g ;",
    [div, div, regexp, div, div]);
var token = new TokenCell(3, 1, 1, "x", null, " \n ");
if (token.LeadingTrivia.Text != " \n " || !token.HasLineBreakBefore)
    throw new Exception("External custom-source trivia construction failed.");
Console.WriteLine("Public constructor and source interface: OK");

void Check(string text, string expected, LexicalGoal[] expectedGoals)
{
    Console.WriteLine("INPUT: " + text);
    var source = new CountingSource(lexer.CreateTokenSource(text, policy));
    if (!ReferenceEquals(source.Policy, policy)) throw new Exception("The source lost its configured policy.");
    var result = parser.Parsing(source);
    if (!result.Success) throw new Exception(parser.FiredException?.ToString());
    var shifted = result.Trace.Steps.Where(s => s.Action is ParseAction.Shift)
        .Select(s => s.Lookahead.Input).ToArray();
    if (string.Join(" ", shifted) != expected || source.Reads != shifted.Length + 1)
        throw new Exception("Unexpected token boundaries or repeated reads.");
    if (!source.SelectedGoals.SequenceEqual(expectedGoals))
        throw new Exception("Unexpected goal selection sequence.");
    Console.WriteLine(text + " -> " + string.Join(" | ", shifted));
}

sealed class CountingSource(ITokenSource inner) : ITokenSource
{
    public int Reads { get; private set; }
    public List<LexicalGoal> SelectedGoals { get; } = new();
    public TokenCursor Start => inner.Start;
    public int Length => inner.Length;
    public LexicalPolicy Policy => inner.Policy;
    public TokenRead Peek(TokenCursor start, LexicalGoal goal)
    {
        Reads++;
        SelectedGoals.Add(goal);
        var read = inner.Peek(start, goal);
        int goalIndex = Array.FindIndex(Policy.Goals.ToArray(), item => ReferenceEquals(item, goal));
        Console.WriteLine($"  {start.SourceOffset}: goal[{goalIndex}] -> '{read.Token.Input}'");
        return read;
    }
}
