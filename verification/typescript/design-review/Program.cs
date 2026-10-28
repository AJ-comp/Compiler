using Janglim.FrontEnd;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Tokenize;

var probes = new (string Name, string Grammar, string[] Inputs)[] {
  ("predicate-tail-nullable", """
S : 'x' Tail ;
Tail : {End()}? | {More()}? 'y' ;
""", ["x", "xy"]),
  ("lookahead-fixedpoint-repro", """
S : {Always()}? C ;
C : P {Always()}? | P {Never()}? 'e' 'x' '?' S ':' S ;
P : R {Always()}? ;
R : id {Always()}? | id {Never()}? 'i' S ;
id := "[a-z]+" ;
""", ["a"]),
  ("qualified-reference-predicate-end", """
S : Type ;
Type : Qual {NotLess()}? | Qual {Less()}? '<' Type '>' ;
Qual : Name | Qual '.' Name ;
Name : id ;
id := "[a-z]+" ;
""", ["a", "a<b>", "a.b", "a.b<c<d>>"]),
  ("nullable-recursive-type-chain", """
S : Type ;
Type : Conditional | '(' Name ':' Type ')' '=>' Type ;
Conditional : Union {NotExtends()}? | Union {Extends()}? 'extends' Type '?' Type ':' Type ;
Union : Intersection | Union '|' Intersection ;
Intersection : Operator | Intersection '&' Operator ;
Operator : Postfix | 'keyof' Operator ;
Postfix : Core {NotBracket()}? ;
Core : Ref | Core Suffix ;
Suffix : {Bracket()}? '[' ']' ;
Ref : Qual {NotLess()}? | Qual {Less()}? '<' Type '>' | Qual 'is' Type ;
Qual : Name | Qual '.' Name ;
Name : id ;
id := "[a-z]+" ;
""", ["a", "a<b>", "a.b", "a.b<c<d>>", "a[]"]),
  ("generic-call-v-relational-naive", """
S : Expr ;
Expr : Rel ;
Rel : Call | Rel '<' Call | Rel '>' Call ;
Call : Primary | Call '(' Expr ')' | Primary TypeArgs '(' Expr ')' ;
Primary : id | '(' Expr ')' ;
TypeArgs : '<' Type '>' ;
Type : id | id TypeArgs ;
id := "[a-z]+" ;
""", ["f<t>(x)", "f<a<b>>(x)", "a<b>c", "a<b"]),
  ("typed-arrow-naive", """
S : Expr ;
Expr : id | '(' Expr ')' | Arrow ;
Arrow : '(' Params ')' '=>' Expr ;
Params : Param | Params ',' Param ;
Param : id | id ':' Type ;
Type : id ;
id := "[a-z]+" ;
""", ["(x)=>x", "(x:t)=>x", "(x)"]),
  ("generic-call-complementary-guards", """
S : Expr ;
Expr : Rel ;
Rel : Call | Rel '<' Call | Rel '>' Call ;
Call : Primary CallEnd ;
CallEnd : {NoGeneric()}? | {Generic()}? '<' Type '>' '(' Expr ')' ;
Primary : id | '(' Expr ')' ;
Type : id | id '<' Type '>' ;
id := "[a-z]+" ;
""", ["f<t>(x)", "f<a<b>>(x)", "a<b>c", "a<b"]),
  ("typed-arrow-cover", """
S : Expr ;
Expr : id | Parens | Parens '=>' Expr ;
Parens : '(' Covers ')' ;
Covers : Cover | Covers ',' Cover ;
Cover : Expr | id ':' Type ;
Type : id ;
id := "[a-z]+" ;
""", ["(x)=>x", "(x:t)=>x", "(x)", "(x:t)"]),
  ("type-operators-naive", """
S : Type ;
Type : id | Type '|' Type | Type '&' Type | Type '[' ']' | Type 'extends' Type '?' Type ':' Type ;
id := "[a-z]+" ;
""", ["a|b&c", "a extends b ? c : d"]),
  ("type-operators-layered", """
S : Type ;
Type : Union | Union 'extends' Union '?' Type ':' Type ;
Union : Intersection | Union '|' Intersection ;
Intersection : Postfix | Intersection '&' Postfix ;
Postfix : id | Postfix '[' ']' ;
id := "[a-z]+" ;
""", ["a|b&c", "a extends b ? c : d"])
};
foreach(var probe in probes) {
  string currentInput="";
  bool IsGeneric(Janglim.FrontEnd.RegularGrammar.PredicateContext context) {
    int start=context.NextToken.StartIndex;
    return start >= 0 && start < currentInput.Length && System.Text.RegularExpressions.Regex.IsMatch(currentInput.Substring(start), @"^<[a-z<>]+>\(");
  }
  var options=new EbnfReaderOptions().RegisterPredicate("Generic", IsGeneric).RegisterPredicate("NoGeneric", context=>!IsGeneric(context)).RegisterPredicate("End",c=>c.IsEndOfInput).RegisterPredicate("More",c=>c.NextToken.Input=="y").RegisterPredicate("Less",c=>c.NextToken.Input=="<").RegisterPredicate("NotLess",c=>c.NextToken.Input!="<").RegisterPredicate("Bracket",c=>c.NextToken.Input=="[").RegisterPredicate("NotBracket",c=>c.NextToken.Input!="[").RegisterPredicate("Extends",c=>c.NextToken.Input=="extends").RegisterPredicate("NotExtends",c=>c.NextToken.Input!="extends").RegisterPredicate("Always",c=>true).RegisterPredicate("Never",c=>false);
  var read=EbnfGrammarReader.Read(probe.Grammar, options);
  Console.WriteLine("PROBE "+probe.Name+" grammar="+read.Success);
  if(!read.Success) { foreach(var e in read.Errors) Console.WriteLine(e); continue; }
  var parser=new LALRParser(read.Grammar,false);
  Console.WriteLine(parser.ConflictReport);
  var lexer=new Lexer();
  foreach(var terminal in read.Grammar.TerminalSet) lexer.AddTokenRule(terminal);
  if(probe.Name=="lookahead-fixedpoint-repro") {
    var table=(Janglim.FrontEnd.Parsers.Collections.LRParsingTable)parser.ParsingTable;
    foreach(var state in parser.Canonical.IndexStateDic) {
      Console.WriteLine("STATE I"+state.Key+" EOF="+(table[state.Key].MatchedValueSet.TryGetValue(new Janglim.FrontEnd.RegularGrammar.EndMarker(),out var actions)?string.Join(";",actions):"MISSING"));
      foreach(var item in state.Value)Console.WriteLine("  "+item+" LA="+(parser.Canonical.LookAheadTable.TryGetValue((state.Key,item),out var la)?la.ToString():"<none>"));
    }
  }
  foreach(var input in probe.Inputs) {
    currentInput=input;
    try { var result=parser.Parsing(lexer.Lexing(input).TokensForParsing); Console.WriteLine(input+" => "+result.Success+" "+parser.FiredException); if(probe.Name=="lookahead-fixedpoint-repro")foreach(var u in result.SelectMany(b=>b.Units))Console.WriteLine(u.InputValue+" : "+u.Action+" "+u.ErrorMessage); }
    catch(Exception e) {Console.WriteLine(input+" => "+e);}
  }
}
