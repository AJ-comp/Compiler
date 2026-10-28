using System.Reflection;
using Janglim.FrontEnd;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas.LR;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;

const string grammar = """
S : {Always()}? C ;
C : P {Always()}? | P {Never()}? 'e' 'x' '?' S ':' S ;
P : R {Always()}? ;
R : id {Always()}? | id {Never()}? 'i' S ;
id := "[a-z]+" ;
""";
int alwaysCalls=0, neverCalls=0;
var options=new EbnfReaderOptions().RegisterPredicate("Always",_=>{alwaysCalls++;return true;})
 .RegisterPredicate("Never",_=>{neverCalls++;return false;});
var read=EbnfGrammarReader.Read(grammar,options);
if(!read.Success)throw new InvalidOperationException(string.Join(";",read.Errors));
var parser=new LALRParser(read.Grammar,false);
var lexer=new Lexer();foreach(var terminal in read.Grammar.TerminalSet)lexer.AddTokenRule(terminal);
var result=parser.Parsing(lexer.Lexing("a").TokensForParsing);
Console.WriteLine(grammar);
Console.WriteLine("Input: a; expected: accept; observed: "+result.Success);
Console.WriteLine("Predicate evaluations before failure: Always="+alwaysCalls+", Never="+neverCalls);
foreach(var unit in result.SelectMany(block=>block.Units))Console.WriteLine(unit.InputValue+": "+unit.Action+" "+unit.ErrorMessage);
var table=(LRParsingTable)parser.ParsingTable;
var eof=new EndMarker();
foreach(var state in new[]{4,5,6}) {
 Console.WriteLine("ACTION[I"+state+", EOF]="+(table[state].MatchedValueSet.TryGetValue(eof,out var actions)?string.Join(";",actions):"MISSING"));
 foreach(var cell in table[state].MatchedValueSet.Where(pair=>pair.Key is Terminal))Console.WriteLine("  ["+cell.Key+"] "+string.Join(";",cell.Value));
}

// Independent diagnostic model. It only reads the already built LR(0) graph.
// It does not modify the parser, its table, production objects or source files.
var ff=new FirstFollowAnalyzer();ff.CalculateAllFollow(read.Grammar.NonTerminalMultiples);
var predEx=typeof(CanonicalRelation).GetMethod("PredEx",BindingFlags.Instance|BindingFlags.NonPublic)!;
var nodes=parser.Canonical.IndexStateDic.SelectMany(state=>state.Value.Select(item=>(State:state.Key,Item:item))).ToArray();
var first=new Dictionary<(int,LRItem),HashSet<Terminal>>();
var edges=new Dictionary<(int,LRItem),List<(int,LRItem)>>();
foreach(var node in nodes) {
 var key=(node.State,node.Item);first[key]=new HashSet<Terminal>();edges[key]=new List<(int,LRItem)>();
 if(node.Item.SingleNT.Name=="Accept"){first[key].Add(eof);continue;}
 var predecessors=(IEnumerable<CanonicalState>)predEx.Invoke(parser.Canonical,new object[]{parser.Canonical.IndexStateDic[node.State],node.Item})!;
 var target=node.Item.SingleNT.ToNonTerminal();
 foreach(var predecessor in predecessors)foreach(var parent in predecessor) {
  if(parent.MarkSymbol!=target)continue;
  var suffix=ff.First(parent.SymbolListAfterMarkSymbol);
  foreach(var terminal in suffix)if(terminal is not Epsilon)first[key].Add(terminal);
  if(suffix.Count==0||suffix.IsNullAble)edges[key].Add((predecessor.StateNumber,parent));
 }
}
int rounds=0;bool changed;
do{changed=false;rounds++;foreach(var node in nodes){var key=(node.State,node.Item);int old=first[key].Count;foreach(var dependency in edges[key])first[key].UnionWith(first[dependency]);changed|=first[key].Count!=old;}}while(changed);
Console.WriteLine("Independent fixed-point rounds: "+rounds);
foreach(var node in nodes) {
 var key=(node.State,node.Item);
 if(!parser.Canonical.LookAheadTable.TryGetValue(key,out var original)||original.Contains(eof))continue;
 foreach(var dependency in edges[key]) {
  if(parser.Canonical.LookAheadTable.TryGetValue(dependency,out var parent)&&parent.Contains(eof))
   Console.WriteLine("STALE DEPENDENCY: I"+node.State+" "+node.Item+" original="+original+" depends on I"+dependency.Item1+" "+dependency.Item2+" original="+parent);
 }
}
foreach(var node in nodes.Where(n=>n.State is 4 or 5 or 6 && n.Item.SingleNT.Predicate?.Name=="Always")) {
 parser.Canonical.LookAheadTable.TryGetValue((node.State,node.Item),out var actual);
 Console.WriteLine("I"+node.State+" "+node.Item+"; original LA="+actual+"; reference LA={"+string.Join(",",first[(node.State,node.Item)])+"}");
}
if(result.Success)throw new InvalidOperationException("The expected baseline failure did not reproduce.");
var targetNode=nodes.Single(n=>n.State==6&&n.Item.SingleNT.Predicate?.Name=="Always");
if(table[6].MatchedValueSet.ContainsKey(eof)||!first[(6,targetNode.Item)].Contains(eof))
 throw new InvalidOperationException("The missing-EOF lookahead evidence was not established.");
Console.WriteLine("CONFIRMED: EOF action is absent before predicate evaluation; fixed-point propagation recovers EOF.");
