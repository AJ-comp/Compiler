using Janglim.FrontEnd;
using Janglim;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Tokenize;
using Janglim.FrontEnd.RegularGrammar;
using System.Reflection;
using System.Text.RegularExpressions;

const string jsCache="artifacts/typescript/type-probe/javascript-cache.ebnf";
string jsText;
if(File.Exists(jsCache))jsText=File.ReadAllText(jsCache);
else {jsText=new JavaScriptGrammar().GrammarText;File.WriteAllText(jsCache,jsText);}
var ident=(string)typeof(JavaScriptGrammar).Assembly.GetType("Janglim.FrontEnd.Grammars.JavaScript.JavaScriptTokenPatterns").GetField("IdentifierName", BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);
var fragment=File.ReadAllText("grammars/typescript/typescript-types.ebnf").Replace("{{IdentifierName}}",ident.Replace("\"","\"\""));
// One token kind for the isolated grammar-shape probe. Production uses the distinct
// type goal and aliases, exercised in root's integration tests.
fragment=fragment.Replace("TSTypeIdentifierName", "IdentifierName");
fragment=Regex.Replace(fragment,@"(?m)^IdentifierName :=[^\r\n]*", "");
fragment=Regex.Replace(fragment,@"(?m)^TSTypeClose :=[^\r\n]*", "").Replace("TSTypeClose", "'>'");
var text="TSTypeEntry : TSType ;\n"+fragment+"\n"+jsText;
if(args.Contains("--minimal"))text="TSTypeEntry : TSType ;\n"+fragment+"\n"+"""
AssignmentExpression : IdentifierName ;
IdentifierName := "[A-Za-z_][A-Za-z0-9_]*" ;
ReservedWord : 'class' ;
StringLiteral := "'(?:[^'\\]|\\.)*'" ;
NumericLiteral := "[0-9]+n?" ;
NoSubstitutionTemplate := "`[^`$]*`" ;
TemplateHead := "`[^`]*\$\{" ;
TemplateMiddle := "\}[^`]*\$\{" ;
TemplateTail := "\}[^`]*`" ;
NoLineTerminator : {NoLineBreak()}? ;
""";
string input="";
string Rest(PredicateContext c)=>c.NextToken.StartIndex >= 0 && c.NextToken.StartIndex < input.Length?input.Substring(c.NextToken.StartIndex):"";
bool Fun(PredicateContext c) {
 string s=Rest(c); if(s.StartsWith('<') || Regex.IsMatch(s,@"^(?:abstract\s+)?new\b"))return true;
 if(!s.StartsWith('('))return false;
 int depth=0; for(int i=0;i<s.Length;i++){if(s[i]=='(')depth++;if(s[i]==')'&&--depth==0)return Regex.IsMatch(s.Substring(i+1),@"^\s*=>");}
 return false;
}
bool Mapped(PredicateContext c)=>Regex.IsMatch(Rest(c),@"^\{\s*(?:[+-]?\s*readonly\s*)?\[\s*[\w$]+\s+in\b");
bool Named(PredicateContext c)=>Regex.IsMatch(Rest(c),@"^(?:\.\.\.\s*)?[\w$]+\s*\??\s*:");
bool Index(PredicateContext c)=>Regex.IsMatch(Rest(c),@"^(?:readonly\s+)?\[\s*[\w$]+\s*:");
var options=new EbnfReaderOptions();
foreach(var name in Regex.Matches(text,@"\{([A-Za-z_]\w*)\(\)\}\?").Select(m=>m.Groups[1].Value).Distinct()) {
  Func<PredicateContext,bool> callback=name switch {
  "TSFunctionTypeStart"=>Fun,"TSNotFunctionTypeStart"=>c=>!Fun(c),
  "TSMappedTypeStart"=>Mapped,"TSNotMappedTypeStart"=>c=>!Mapped(c),
  "TSNamedTupleStart"=>Named,"TSNotNamedTupleStart"=>c=>!Named(c),
  "TSIndexSignatureStart"=>Index,"TSNotIndexSignatureStart"=>c=>!Index(c),
  "TSConditionalTypeStart"=>c=>!c.NextToken.HasLineBreakBefore&&c.NextToken.Input=="extends",
  "TSNotConditionalTypeStart"=>c=>c.NextToken.HasLineBreakBefore||c.NextToken.Input!="extends",
  "TSPostfixTypeStart"=>c=>!c.NextToken.HasLineBreakBefore&&c.NextToken.Input=="[",
  "TSNotPostfixTypeStart"=>c=>c.NextToken.HasLineBreakBefore||c.NextToken.Input!="[",
  "TSTypeArgumentsStart"=>c=>!c.NextToken.HasLineBreakBefore&&c.NextToken.Input=="<",
  "TSNotTypeArgumentsStart"=>c=>c.NextToken.HasLineBreakBefore||c.NextToken.Input!="<",
  "TSTypePredicateStart"=>c=>!c.NextToken.HasLineBreakBefore&&c.NextToken.Input=="is",
  "TSNotTypePredicateStart"=>c=>c.NextToken.HasLineBreakBefore||c.NextToken.Input!="is",
  "TSNoTypeReferenceSuffix"=>c=>!( !c.NextToken.HasLineBreakBefore&&(c.NextToken.Input=="<"||c.NextToken.Input=="is"||c.PreviousToken?.Input=="asserts"&&(c.NextToken.Kind.Caption=="IdentifierName"||c.NextToken.Input=="this"))),
  "TSTypeMemberEnd"=>c=>c.NextToken.Input!=";"&&c.NextToken.Input!=","&&(c.IsEndOfInput||c.NextToken.Input=="}"||c.NextToken.HasLineBreakBefore),
  "NoLineBreak"=>c=>!c.NextToken.HasLineBreakBefore,
  _=>c=>true};
  options.RegisterPredicate(name,callback);
}
var read=EbnfGrammarReader.Read(text,options);
if(args.Contains("--minimize")) {
 input="A";
 var rules=Regex.Matches(text,@"(?ms)^(?<name>\w+)\s*(?<assign>:=|:)\s*(?<body>.*?)(?=^\w+\s*(?::=|:)|\z)").Select(m=>(Name:m.Groups["name"].Value,Assign:m.Groups["assign"].Value,Text:m.Value)).ToArray();
 var protectedNames=new HashSet<string>(new[]{"TSTypeEntry","TSType","TSConditionalType","TSUnionType","TSIntersectionType","TSTypeOperator","TSPostfixType","TSPostfixCore","TSPrimaryType","TSTypeReference","TSQualifiedName","TSBindingIdentifier","IdentifierName"});
 var candidates=rules.Where(r=>r.Assign==":"&&!protectedNames.Contains(r.Name)).Select(r=>r.Name).ToArray();
 string Make(HashSet<string> disabled)=>string.Join("\n",rules.Select(r=>disabled.Contains(r.Name)?r.Name+" : '!disabled_"+r.Name+"' ;":r.Text));
 bool Fails(HashSet<string> disabled) {
  var g=EbnfGrammarReader.Read(Make(disabled),options);if(!g.Success)return false;
  try{var p=new LALRParser(g.Grammar,false);var l=new Lexer();foreach(var t in g.Grammar.TerminalSet)l.AddTokenRule(t);return !p.Parsing(l.Lexing("A").TokensForParsing).Success;}
  catch(Exception e){Console.WriteLine("INVALID "+e.GetType().Name);return false;}
 }
 var disabled=new HashSet<string>();Console.WriteLine("MINIMIZE fullFail="+Fails(disabled)+" candidates="+candidates.Length);
 int chunk=Math.Max(1,candidates.Length/2);
 while(true){bool change=false;for(int i=0;i<candidates.Length;i+=chunk){var proposed=new HashSet<string>(disabled);foreach(var n in candidates.Skip(i).Take(chunk))proposed.Add(n);if(proposed.Count==disabled.Count)continue;if(Fails(proposed)){disabled=proposed;change=true;Console.WriteLine("DISABLED "+disabled.Count+" chunk="+chunk);File.WriteAllText("artifacts/typescript/type-probe/minimized.ebnf",Make(disabled));}}
 if(chunk==1&&!change)break;if(!change)chunk=Math.Max(1,chunk/2);
 }
 Console.WriteLine("RETAINED "+string.Join(",",candidates.Where(n=>!disabled.Contains(n))));return;
}
Console.WriteLine("GRAMMAR "+read.Success);
if(!read.Success){foreach(var e in read.Errors)Console.WriteLine(e);return;}
if(args.Contains("--first")) {
 var first=new FirstFollowAnalyzer();first.CalculateAllFollow(read.Grammar.NonTerminalMultiples);
 foreach(var n in read.Grammar.NonTerminalMultiples.Where(n=>n.Name is "TSConditionalTail" or "TSConditionalType" or "TSTypeReferenceTail" or "TSPostfixTail" || n.Name.StartsWith("__predicate")))
 Console.WriteLine(n.Name+" FIRST="+first.FirstSet(n)+" FOLLOW="+first.Follow(n)+" RULES="+string.Join(" || ",n.Select(p=>p.ToGrammarString()+" / PRED="+p.Predicate?.Name)));
 return;
}
foreach(var p in read.Grammar.NonTerminalMultiples.SelectMany(n=>n)) {
 try {for(int i=0;i<=p.Count;i++)_=new Janglim.FrontEnd.Parsers.Datas.LR.LRItem(p,(sbyte)i).GetHashCode();}
 catch(OverflowException){Console.WriteLine("HASH OVERFLOW "+p+" hash="+p.GetHashCode());}
}
var parser=new LALRParser(read.Grammar,false);
Console.WriteLine("CONFLICTS "+parser.Conflicts.Count);
File.WriteAllText("artifacts/typescript/type-probe/expanded.ebnf",text);
var lexer=new Lexer();foreach(var t in read.Grammar.TerminalSet.Where(t=>t.Value is not ">>" and not ">>>" and not ">=" and not ">>=" and not ">>>="))lexer.AddTokenRule(t);
foreach(char c in " \t\r\n")lexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Delimiter,c.ToString(),false));
string[] inputs=["string","A","A.B<C<D>>","A|B&C[]","T extends U ? X : Y", "T extends infer U extends X ? U : never", "infer U extends X ? Y : Z", "keyof T", "readonly [string, number?]", "[name: string, count?: number, ...rest: number[]]", "[A, B?, ...C[]]", "[...A, B]", "(T)", "((x:T)=>U)","(x:T)=>U","<T extends U = V>(x:T)=>T", "abstract new <T>(x:T)=>C<T>", "{name:string; value?:T}", "{readonly [key:string]:T}", "{[Symbol.iterator]():Iterator<T>}", "{<T>(value:T):T; new(value:T):C;}", "{get value():T; set value(v:T);}", "{[K in keyof T as `get${K}`]?: T[K]}", "typeof Foo.bar<T>", "import('x').Foo<T>", "typeof import('x', {with:{'resolution-mode':'import'}}).Foo", "(x:any)=>x is string", "(x:any)=>asserts x is string", "`a${T}b${U}`", "any.Foo", "asserts", "unknown | (()=>string)"];
foreach(var s in inputs){input=s;var lexed=lexer.Lexing(input).TokensForParsing;var parsed=parser.Parsing(lexed);Console.WriteLine((parsed.Success?"PASS ":"FAIL ")+input+(parsed.Success?"":" => "+parser.FiredException?.Message+" ERR "+string.Join(";",parsed.AllErrors.Select(e=>e.Message+" "+string.Join(",",e.ErrTokens.Select(t=>t.Input+"@"+t.StartIndex))))));if(!parsed.Success)foreach(var unit in parsed.SelectMany(b=>b.Units).TakeLast(8))Console.WriteLine(unit.InputValue+" : "+unit.Action+" "+unit.ErrorMessage);}
