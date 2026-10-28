using Janglim.FrontEnd.Grammars.TypeScript;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Tokenize;
using Janglim.FrontEnd.Parsers.LR;

var language = new TypeScriptGrammar(TypeScriptInputKind.Script, JavaScriptMode.Strict);
var parser = new LALRParser(language.Grammar, false);
foreach (var input in args.Length == 0 ? new[] { "/ab/!.test(text);", "function f(value: unknown) { return value as User; }" } : args)
{
 Console.WriteLine("INPUT " + input);
 var result = parser.Parsing(new TraceSource(language.CreateTokenSource(input)));
 Console.WriteLine("PARSE " + result.Success + " " + parser.FiredException);
}

sealed class TraceSource : ITokenSource
{
 private readonly ITokenSource source;
 private readonly LexicalPolicy policy;
 public TraceSource(ITokenSource source)
 {
  this.source = source;
  var resolve = typeof(LexicalPolicy).GetMethod("Resolve", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
  policy = new LexicalPolicy(source.Policy.Goals, context =>
  {
   var chosen = (LexicalGoal)resolve.Invoke(source.Policy, new object[] {context})!;
   Console.WriteLine("EXPECT " + context.SourceOffset + " " + string.Join(",", context.ExpectedTerminals.Select(t=>t.Caption).Where(c=>c is "IdentifierName" or "TSTypeIdentifierName" or "TSTypeOpen" or "TSTypeClose" or "{" or "}" or "RegularExpressionLiteral" or "/")));
   return chosen;
  });
 }
 public TokenCursor Start => source.Start;
 public int Length => source.Length;
 public LexicalPolicy Policy => policy;
 public TokenRead Peek(TokenCursor start, LexicalGoal goal)
 {
  var read = source.Peek(start, goal);
  string role = goal.Terminals.Any(t=>t.Caption=="TSTypeIdentifierName") ? "TYPE" : goal.Terminals.Any(t=>t.Caption=="TSNonNullToken") ? "POSTFIX" : "RUNTIME";
  Console.WriteLine("READ " + start.SourceOffset + " " + role + " " + read.Token.Input + " " + read.Token.Kind.Caption);
  return read;
 }
}
