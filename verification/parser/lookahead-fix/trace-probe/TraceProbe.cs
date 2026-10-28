using System.Reflection;
using System.Text.Json;
using Janglim;
using Janglim.FrontEnd;
using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas.LR;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;

bool simple = args.Contains("simple");
bool intermediate = args.Contains("intermediate");
Grammar grammar = simple || intermediate ? new RequeueGrammar(intermediate) : new SmallGrammar();
var parser = new LALRParser(grammar, false);
var c = parser.Canonical;
var original = c.LookAheadTable.ToDictionary(p=>p.Key,p=>new TerminalSet(p.Value));
var mark = typeof(LRItem).GetField("markIndex",BindingFlags.Instance|BindingFlags.NonPublic)!;
string SymbolName(Symbol s) => s is EndMarker ? "EOF" : s is Epsilon ? "ε" : s is Terminal t ? "'"+t.Value+"'" : ((NonTerminal)s).Name;
string ItemName(LRItem item) {
    var symbols = item.SingleNT.Where(s=>s is not Epsilon).Select(SymbolName).ToList();
    int dot=System.Convert.ToInt32(mark.GetValue(item));
    symbols.Insert(Math.Min(dot,symbols.Count),"•");
    return item.SingleNT.Name + " → " + string.Join(" ",symbols);
}
string Key((int State,LRItem Item) n) => "I"+n.State+" · "+ItemName(n.Item);
string Set(TerminalSet s)=>"{"+string.Join(", ",s.Select(SymbolName).Order())+"}";
c.LookAheadTable.Clear();
var edges=(Dictionary<(int State,LRItem Item),List<((int State,LRItem Item) Target,TerminalSet First,bool Inherit)>>)
    typeof(CanonicalRelation).GetMethod("BuildLookAheadDependencies",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(c,Array.Empty<object>())!;
var pending=new Queue<(int State,LRItem Item)>();
var queued=new HashSet<(int State,LRItem Item)>();
foreach(var e in c.LookAheadTable)if(e.Value.Count>0){pending.Enqueue(e.Key);queued.Add(e.Key);}
var seed=pending.Select(Key).ToArray();
var rounds=new List<object>();int round=0;
while(pending.Count>0){
    var queueBefore=pending.Select(Key).ToArray();
    var source=pending.Dequeue();queued.Remove(source);var updates=new List<object>();
    foreach(var d in edges[source]){
        var set=c.LookAheadTable[d.Target];var before=Set(set);int count=set.Count;
        if(d.First!=null)set.UnionWith(d.First);
        if(d.Inherit)set.UnionWith(c.LookAheadTable[source]);
        bool changed=set.Count>count;bool added=changed&&queued.Add(d.Target);
        if(added)pending.Enqueue(d.Target);
        updates.Add(new {target=Key(d.Target),first=d.First==null?null:Set(d.First),inherit=d.Inherit,before,after=Set(set),changed,enqueued=added});
    }
    rounds.Add(new {round=++round,queueBefore,source=Key(source),lookahead=Set(c.LookAheadTable[source]),updates,queue=pending.Select(Key).ToArray()});
}
if(original.Any(e=>!c.LookAheadTable[e.Key].SetEquals(e.Value)))throw new Exception("Replicated trace differs from actual product.");
var lexer=new Lexer();foreach(var t in grammar.TerminalSet)lexer.AddTokenRule(t);
var accepted=(intermediate?new[]{"az!","bcdaz?"}:simple?new[]{"a!","bcda?"}:new[]{"a"}).Select(input=>new{input,success=parser.Parsing(lexer.Lexing(input).TokensForParsing).Success}).ToArray();
Console.WriteLine(JsonSerializer.Serialize(new {seed,rounds,accepted,final=c.LookAheadTable.Select(e=>new {item=Key(e.Key),lookahead=Set(e.Value)}).ToArray()},new JsonSerializerOptions{WriteIndented=true}));

sealed class SmallGrammar:Grammar {
    public override NonTerminal EbnfRoot=>StartSymbol;
    public SmallGrammar(){
        var ns=new[]{"S","C","R","G"}.ToDictionary(s=>s,s=>new NonTerminal(s,s=="S"));
        var ts=new[]{"a","b","e"}.ToDictionary(s=>s,s=>new Terminal(TokenType.Operator,s));
        foreach(var n in ns.Values){keyManager.AllocateUniqueKey(n);NonTerminalMultiples.Add(n);}
        foreach(var t in ts.Values){keyManager.AllocateUniqueKey(t);TerminalSet.Add(t);}
        SetStartSymbol(ns["S"]);
        ns["S"].Add(ns["G"],ns["C"]);ns["C"].Add(ns["R"]);ns["C"].Add(ts["e"],ns["S"]);
        ns["R"].Add(ts["a"]);ns["R"].Add(ts["b"],ns["S"]);ns["G"].Add(new Epsilon());
    }
}

sealed class RequeueGrammar:Grammar {
    public override NonTerminal EbnfRoot=>StartSymbol;
    public RequeueGrammar(bool intermediate=false){
        var ns=new[]{"S","A"}.ToDictionary(s=>s,s=>new NonTerminal(s,s=="S"));
        var ts=new[]{"a","b","c","d","!","?","z"}.ToDictionary(s=>s,s=>new Terminal(TokenType.Operator,s));
        foreach(var n in ns.Values){keyManager.AllocateUniqueKey(n);NonTerminalMultiples.Add(n);}
        foreach(var t in ts.Values){keyManager.AllocateUniqueKey(t);TerminalSet.Add(t);}
        SetStartSymbol(ns["S"]);
        ns["S"].Add(ns["A"],ts["!"]);ns["S"].Add(ts["b"],ts["c"],ts["d"],ns["A"],ts["?"]);
        if(intermediate)ns["A"].Add(ts["a"],ts["z"]);else ns["A"].Add(ts["a"]);
    }
}
