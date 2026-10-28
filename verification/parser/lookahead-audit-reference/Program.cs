using System.Reflection;
using System.Text.Json;
using Janglim;
using Janglim.FrontEnd;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Parsers.Datas.LR;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;

var grammarText = """
S : {Always()}? C ;
C : P {Always()}? | P {Never()}? 'e' 'x' '?' S ':' S ;
P : R {Always()}? ;
R : id {Always()}? | id {Never()}? 'i' S ;
id := "[a-z]+" ;
""";
bool plainSmall=args.Length!=0&&args[0]=="--plain-small";
if(args.Length!=0&&!plainSmall)grammarText=File.ReadAllText(args[0]);
int alwaysCalls=0,neverCalls=0;
var options = new EbnfReaderOptions().RegisterPredicate("Always", _=>{alwaysCalls++;return true;})
    .RegisterPredicate("Never", _=>{neverCalls++;return false;});
Grammar grammar;
if(plainSmall){grammar=new SmallGrammar();grammarText="S : G C ; C : R | 'e' S ; R : 'a' | 'b' S ; G : epsilon ;";}
else {var read=EbnfGrammarReader.Read(grammarText,options);if(!read.Success)throw new Exception(string.Join(";",read.Errors));grammar=read.Grammar;}
var parser=new LALRParser(grammar,false);
var markField=typeof(LRItem).GetField("markIndex",BindingFlags.NonPublic|BindingFlags.Instance)!;
string Sym(Symbol symbol)=>symbol is EndMarker?"$":symbol is Epsilon?"EPS":symbol is Terminal t?"T:"+t.Caption+":"+t.Value:"N:"+((NonTerminal)symbol).Name;
string RuleKey(NonTerminalSingle rule)=>rule.Name+"->"+string.Join(" ",rule.Where(s=>s is not Epsilon).Select(Sym));
var allItems=parser.Canonical.IndexStateDic.SelectMany(s=>s.Value.Select(i=>(state:s.Key,item:i))).ToArray();
var rules=allItems.Select(x=>x.item.SingleNT).GroupBy(RuleKey).Select(g=>g.First()).OrderBy(RuleKey).ToArray();
var ids=rules.Select((rule,index)=>(rule,index)).ToDictionary(x=>RuleKey(x.rule),x=>x.index);
var ff=new FirstFollowAnalyzer();ff.CalculateAllFollow(grammar.NonTerminalMultiples);
var lexer=new Lexer();foreach(var terminal in grammar.TerminalSet)lexer.AddTokenRule(terminal);
var parsed=parser.Parsing(lexer.Lexing("a").TokensForParsing);
var ntNames=rules.Select(r=>r.Name).ToHashSet();
var collisions=allItems.GroupBy(x=>x.item.GetHashCode()).Select(g=>new {hash=g.Key,identities=g.Select(x=>ids[RuleKey(x.item.SingleNT)]+":"+markField.GetValue(x.item)).Distinct().ToArray()}).Where(g=>g.identities.Length>1).ToArray();
var table=(LRParsingTable)parser.ParsingTable;
var data = new {
    grammarText,
    sourceAssembly=typeof(LALRParser).Assembly.Location,
    grammarAssembly=typeof(EbnfGrammarReader).Assembly.Location,
    rules=rules.Select((r,id)=>new{id,lhs="N:"+r.Name,rhs=r.Where(s=>s is not Epsilon).Select(Sym).ToArray(),predicate=r.Predicate?.Name,hash=r.GetHashCode()}).ToArray(),
    states=parser.Canonical.IndexStateDic.OrderBy(s=>s.Key).Select(s=>new{id=s.Key,items=s.Value.Select(item=>new{
        rule=ids[RuleKey(item.SingleNT)],dot=System.Convert.ToInt32(markField.GetValue(item)),hash=item.GetHashCode(),
        lookahead=parser.Canonical.LookAheadTable.TryGetValue((s.Key,item),out var la)?la.Select(Sym).Order().ToArray():null
    }).OrderBy(i=>i.rule).ThenBy(i=>i.dot).ToArray()}).ToArray(),
    first=grammar.NonTerminalMultiples.ToDictionary(nt=>"N:"+nt.Name,nt=>ff.First(nt).Select(Sym).Order().ToArray()),
    follow=grammar.NonTerminalMultiples.ToDictionary(nt=>"N:"+nt.Name,nt=>ff.Follow(nt).Select(Sym).Order().ToArray()),
    collisions,
    parse=new{input="a",success=parsed.Success,alwaysCalls,neverCalls,exception=parser.FiredException?.ToString(),conflicts=parser.ConflictReport.ToString(),
        steps=parsed.SelectMany(b=>b.Units).Select(u=>new{token=u.InputValue?.ToString(),action=u.Action.ToString(),error=u.ErrorMessage}).ToArray()},
    eofActions=table.Select((row,id)=>new{id,actions=row.MatchedValueSet.TryGetValue(new EndMarker(),out var actions)?actions.Select(a=>a.ToString()).ToArray():Array.Empty<string>()}).ToArray()
};
Console.WriteLine(JsonSerializer.Serialize(data,new JsonSerializerOptions{WriteIndented=true}));

sealed class SmallGrammar : Grammar {
    public override NonTerminal EbnfRoot=>StartSymbol;
    public SmallGrammar(){
        var n=new[]{"S","C","R","G"}.ToDictionary(s=>s,s=>new NonTerminal(s,s=="S"));
        var t=new[]{"a","b","e"}.ToDictionary(s=>s,s=>new Terminal(TokenType.Operator,s));
        foreach(var x in n.Values){keyManager.AllocateUniqueKey(x);NonTerminalMultiples.Add(x);}
        foreach(var x in t.Values){keyManager.AllocateUniqueKey(x);TerminalSet.Add(x);}
        SetStartSymbol(n["S"]);
        n["S"].Add(n["G"],n["C"]);
        n["C"].Add(n["R"]);n["C"].Add(t["e"],n["S"]);
        n["R"].Add(t["a"]);n["R"].Add(t["b"],n["S"]);
        n["G"].Add(new Epsilon());
    }
}
