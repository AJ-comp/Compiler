using System.Reflection;
using System.Text.Json;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Tokenize;

var language = new JavaScriptGrammar(JavaScriptMode.NonStrict, webCompatibility: true);
var parser = new LALRParser(language.Grammar);
var select = (Func<LexicalContext, LexicalGoal>)typeof(LexicalPolicy).GetField("_selector", BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(language.Policy)!;
string line;
while ((line = Console.ReadLine()) != null) {
  var row = JsonSerializer.Deserialize<JsonElement>(line);
  string input = row.GetProperty("source").GetString()!;
  var observed = new List<object>();
  var policy = new LexicalPolicy(language.Policy.Goals, context => {
    var result = select(context);
    if (context.PreviousToken?.Input == "await") {
      observed.Add(new { offset=context.SourceOffset, previous=context.PreviousToken.StartIndex, state=context.State, expected=context.ExpectedTerminals.Select(t=>t.Caption+":"+t.Value).Order().ToArray(), goal=language.Policy.Goals.ToList().IndexOf(result) });
    }
    return result;
  });
  var source = new Source(language.CreateTokenSource(input), policy);
  var parse = parser.Parsing(source);
  Console.WriteLine(JsonSerializer.Serialize(new { id=row.GetProperty("id").GetString(), source=input, parse=parse.Success, diagnostics=parse.Success ? language.Validate(parse.ToParseTree).Select(d=>d.Code).ToArray() : Array.Empty<string>(), observed }));
}
sealed class Source : ITokenSource {
  readonly ITokenSource inner;
  public Source(ITokenSource source, LexicalPolicy policy) { inner=source; Policy=policy; }
  public TokenCursor Start => inner.Start;
  public int Length => inner.Length;
  public LexicalPolicy Policy { get; }
  public TokenRead Peek(TokenCursor cursor, LexicalGoal goal) => inner.Peek(cursor,goal);
}
