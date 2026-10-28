using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;

var read = EbnfGrammarReader.Read("""
    Expression : IdentifierName | 'true' ;
    IdentifierName := Identifier "[$A-Za-z_][$0-9A-Za-z_]*" ;
    """);
if (!read.Success) throw new Exception(string.Join("; ", read.Errors));
var lexer = new Lexer();
foreach (var terminal in read.Grammar.TerminalSet) lexer.AddTokenRule(terminal);
var goal = new LexicalGoal(read.Grammar.TerminalSet.Where(t => t is not EndMarker && t is not Epsilon));
var policy = new LexicalPolicy(goal);
foreach (var text in new[] { "name", "$name", "name$", "true$" })
{
    var source = lexer.CreateTokenSource(text, policy);
    var readToken = source.Peek(source.Start, goal);
    Console.WriteLine($"{text} => first kind={readToken.Token.Kind.Caption}, text='{readToken.Token.Input}', nextOffset={readToken.Next.SourceOffset}");
}
