using Janglim.FrontEnd;
using Janglim;
using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;

var cases = new (string Name,string Text)[] {
 ("predicate-original", "S : {Always()}? C ; C : P {Always()}? | P {Never()}? 'e' 'x' '?' S ':' S ; P : R {Always()}? ; R : id {Always()}? | id {Never()}? 'i' S ; id := \"[a-z]+\" ;"),
 ("plain-no-predicates", "S : C ; C : P | P 'e' 'x' '?' S ':' S ; P : R ; R : id | id 'i' S ; id := \"[a-z]+\" ;"),
 ("explicit-epsilon", "S : G1 C ; C : P G2 | P G3 'e' 'x' '?' S ':' S ; P : R G4 ; R : id G5 | id G6 'i' S ; G1 : ; G2 : ; G3 : ; G4 : ; G5 : ; G6 : ; id := \"[a-z]+\" ;"),
 ("plain-short-cycle", "S : C ; C : P | P 'e' S ':' S ; P : R ; R : 'a' | 'a' 'i' S ;"),
 ("plain-recursive-chain", "S : C ; C : P | '(' S ')' ; P : R ; R : 'a' | 'a' 'i' S ;"),
 ("explicit-epsilon-no-r-tail", "S : G1 C ; C : P G2 | P G3 'e' 'x' '?' S ':' S ; P : R G4 ; R : 'a' G5 ; G1 : ; G2 : ; G3 : ; G4 : ; G5 : ;"),
 ("epsilon-only-SC", "S : G1 C ; C : P G2 | P G3 'e' S ':' S ; P : 'a' ; G1 : ; G2 : ; G3 : ;"),
 ("epsilon-only-tail", "S : C ; C : P G2 | P G3 'e' S ':' S ; P : R G4 ; R : 'a' G5 ; G2 : ; G3 : ; G4 : ; G5 : ;")
};
foreach(var c in cases) {
 try {
  var options = new EbnfReaderOptions().RegisterPredicate("Always", _=>true).RegisterPredicate("Never", _=>false);
  var read=EbnfGrammarReader.Read(c.Text,options);
  Console.WriteLine("CASE "+c.Name+" READ="+read.Success);
  if(!read.Success){Console.WriteLine(string.Join(";",read.Errors));continue;}
  Run(c.Name,read.Grammar,"a");
 }catch(Exception e){Console.WriteLine(c.Name+" EXCEPTION "+e);}
}
Run("direct-tree-no-predicates", new PlainGrammar(false), "a");
Run("direct-tree-explicit-epsilon", new PlainGrammar(true), "a");
Run("direct-tree-epsilon-no-r-tail", new PlainGrammar(true,false), "a");

for(int prefix=0;prefix<2;prefix++)for(int rsuffix=0;rsuffix<2;rsuffix++)for(int csuffix=0;csuffix<2;csuffix++)for(int rstart=0;rstart<2;rstart++) {
 var csecond=(prefix==0?new[]{"P","G3"}:new[]{"G3"}).Concat(new[]{"e","x","?","S",":","S"}).Concat(csuffix==1?new[]{"k"}:Array.Empty<string>()).ToArray();
 var rsecond=new[]{rstart==0?"a":"b","G6","i","S"}.Concat(rsuffix==1?new[]{"j"}:Array.Empty<string>()).ToArray();
 var rules=new Dictionary<string,string[][]>{["S"]=new[]{new[]{"G1","C"}},["C"]=new[]{new[]{"P","G2"},csecond},["P"]=new[]{new[]{"R","G4"}},["R"]=new[]{new[]{"a","G5"},rsecond}};
 for(int i=1;i<=6;i++)rules["G"+i]=new[]{Array.Empty<string>()};
 var gr=new SpecGrammar(rules);var p=new LALRParser(gr,false);var lx=new Lexer();foreach(var t in gr.TerminalSet)lx.AddTokenRule(t);var pr=p.Parsing(lx.Lexing("a").TokensForParsing);
 Console.WriteLine($"MATRIX prefix={prefix} rsuffix={rsuffix} csuffix={csuffix} rstart={rstart} success={pr.Success} conflicts={p.Conflicts.Count}");
 if(!pr.Success&&p.Conflicts.Count==0){Console.WriteLine("ZERO CONFLICT REJECTION "+string.Join("\n",rules.Select(kv=>kv.Key+" : "+string.Join(" | ",kv.Value.Select(a=>string.Join(" ",a)))+" ;")));}
}
for(int shape=0;shape<3;shape++)for(int mask=0;mask<64;mask++) {
 string[] W(params string[] words)=>words.Where(w=>!w.StartsWith("G")||((mask>>(int.Parse(w.Substring(1))-1))&1)==1).ToArray();
 var r2=shape==2?W("b","G6","S"):W("b","G6","i","S");
 var c2=shape==0?W("G3","e","x","?","S",":","S"):W("G3","e","S");
 var specs=new Dictionary<string,string[][]>{["S"]=new[]{W("G1","C")},["C"]=new[]{W("P","G2"),c2},["P"]=new[]{W("R","G4")},["R"]=new[]{W("a","G5"),r2}};
 for(int i=1;i<=6;i++)if(((mask>>(i-1))&1)==1)specs["G"+i]=new[]{Array.Empty<string>()};
 var gr=new SpecGrammar(specs);var p=new LALRParser(gr,false);var lx=new Lexer();foreach(var t in gr.TerminalSet)lx.AddTokenRule(t);var pr=p.Parsing(lx.Lexing("a").TokensForParsing);
 if(!pr.Success&&p.Conflicts.Count==0)Console.WriteLine($"SMALL-FAIL shape={shape} mask={mask} rules="+string.Join(" / ",specs.Select(kv=>kv.Key+":"+string.Join("|",kv.Value.Select(a=>string.Join(" ",a))))));
}
var tiny=new Dictionary<string,string[][]>[] {
 new(){["S"]=new[]{new[]{"G","C"}},["C"]=new[]{new[]{"R"},new[]{"e","S"}},["R"]=new[]{new[]{"a"},new[]{"b","S"}},["G"]=new[]{Array.Empty<string>()}},
 new(){["S"]=new[]{new[]{"G","C"}},["C"]=new[]{new[]{"a"},new[]{"b","S"},new[]{"e","S"}},["G"]=new[]{Array.Empty<string>()}},
 new(){["S"]=new[]{new[]{"G","C"}},["C"]=new[]{new[]{"a"},new[]{"b","S"}},["G"]=new[]{Array.Empty<string>()}},
 new(){["S"]=new[]{new[]{"G","C"}},["C"]=new[]{new[]{"R"}},["R"]=new[]{new[]{"a"},new[]{"b","S"}},["G"]=new[]{Array.Empty<string>()}},
 new(){["S"]=new[]{new[]{"C"}},["C"]=new[]{new[]{"R"},new[]{"e","S"}},["R"]=new[]{new[]{"a"},new[]{"b","S"}}}
};
for(int i=0;i<tiny.Length;i++)Run("tiny-"+i,new SpecGrammar(tiny[i]),"a");

static void Run(string name, Grammar grammar, string input) {
 var parser=new LALRParser(grammar,false);
 var lexer=new Lexer();foreach(var t in grammar.TerminalSet)lexer.AddTokenRule(t);
 var tokens=lexer.Lexing(input).TokensForParsing;
 var result=parser.Parsing(tokens);
 Console.WriteLine(name+" RESULT="+result.Success+" CONFLICTS="+parser.Conflicts.Count+" NT="+grammar.NonTerminalMultiples.Count+" STATES="+parser.Canonical.IndexStateDic.Count);
 Console.WriteLine(parser.ConflictReport);
 Console.WriteLine("TOKENS="+string.Join(",",tokens.Select(t=>t.ToString())));
 foreach(var block in result)foreach(var unit in block.Units)Console.WriteLine(unit.InputValue+" "+unit.Action+" "+unit.ErrorMessage);
 foreach(var state in parser.Canonical.IndexStateDic)
  foreach(var item in state.Value.Where(i=>i.IsReachedHandle))
   Console.WriteLine("LA I"+state.Key+" "+item+" "+item.LookAhead);
 Console.WriteLine("ASSEMBLY "+typeof(LALRParser).Assembly.Location);
 if(name=="tiny-0") {
  var manual=new TokenCell(0,0,0,"a",null);
  typeof(TokenCell).GetProperty("PatternInfo")!.SetValue(manual,new TokenPatternInfo(1,grammar.TerminalSet.Single(t=>t.Value=="a")));
  var manualResult=parser.Parsing(new[]{manual});
  Console.WriteLine("MANUAL TOKEN NO LEXER RESULT="+manualResult.Success+" INPUT="+manual.Data+" TERMINAL="+manual.PatternInfo.Terminal.Caption);
  foreach(var block in manualResult)foreach(var unit in block.Units)Console.WriteLine("MANUAL "+unit.InputValue+" "+unit.Action+" "+unit.ErrorMessage);
 }
}

sealed class PlainGrammar : Grammar {
 public override NonTerminal EbnfRoot => StartSymbol;
 public PlainGrammar(bool epsilon, bool rtail=true) {
  var names = epsilon ? new[]{"S","C","P","R","G1","G2","G3","G4","G5","G6"}:new[]{"S","C","P","R"};
  var nts=names.ToDictionary(n=>n,n=>new NonTerminal(n,n=="S"));
  foreach(var nt in nts.Values){keyManager.AllocateUniqueKey(nt);NonTerminalMultiples.Add(nt);}
  SetStartSymbol(nts["S"]);
  var vals=new[]{"a","e","x","?",":","i"};
  var ts=vals.ToDictionary(v=>v,v=>new Terminal(TokenType.Operator,v));
  foreach(var t in ts.Values){keyManager.AllocateUniqueKey(t);TerminalSet.Add(t);}
  var s=nts["S"];var c=nts["C"];var p=nts["P"];var r=nts["R"];
  if(epsilon){
   s.Add(nts["G1"],c);c.Add(p,nts["G2"]);c.Add(p,nts["G3"],ts["e"],ts["x"],ts["?"],s,ts[":"],s);p.Add(r,nts["G4"]);
   r.Add(ts["a"],nts["G5"]);if(rtail)r.Add(ts["a"],nts["G6"],ts["i"],s);
   foreach(var n in names.Skip(4))nts[n].Add(new Epsilon());
  }else{
   s.Add(c);c.Add(p);c.Add(p,ts["e"],ts["x"],ts["?"],s,ts[":"],s);p.Add(r);r.Add(ts["a"]);r.Add(ts["a"],ts["i"],s);
  }
 }
}

sealed class SpecGrammar : Grammar {
 public override NonTerminal EbnfRoot => StartSymbol;
 public SpecGrammar(Dictionary<string,string[][]> rules) {
  var ns=rules.Keys.ToDictionary(n=>n,n=>new NonTerminal(n,n=="S"));
  var ts=rules.Values.SelectMany(v=>v).SelectMany(v=>v).Where(v=>!ns.ContainsKey(v)).Distinct().ToDictionary(v=>v,v=>new Terminal(TokenType.Operator,v));
  foreach(var n in ns.Values){keyManager.AllocateUniqueKey(n);NonTerminalMultiples.Add(n);}foreach(var t in ts.Values){keyManager.AllocateUniqueKey(t);TerminalSet.Add(t);}SetStartSymbol(ns["S"]);
  foreach(var rule in rules)foreach(var alt in rule.Value)ns[rule.Key].Add(alt.Length==0?new Symbol[]{new Epsilon()}:alt.Select(v=>ns.ContainsKey(v)?(Symbol)ns[v]:ts[v]).ToArray());
 }
}
