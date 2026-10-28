using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Tokenize;
using System.Text.RegularExpressions;
var text=File.ReadAllText("artifacts/typescript/type-probe/minimized.ebnf");
var options=new EbnfReaderOptions();
foreach(var name in Regex.Matches(text,@"\{([A-Za-z_]\w*)\(\)\}\?").Select(m=>m.Groups[1].Value).Distinct())
 options.RegisterPredicate(name,c=>name.StartsWith("TSNot")||name=="TSNoTypeReferenceSuffix"||name=="NoLineBreak");
List<string> Split(string s){var parts=new List<string>();int start=0;char quote='\0';for(int i=0;i<s.Length;i++){char c=s[i];if(quote!='\0'){if(c==quote){if(i+1<s.Length&&s[i+1]==quote)i++;else quote='\0';}}else if(c=='\''||c=='"')quote=c;else if(c=='|'){parts.Add(s.Substring(start,i-start).Trim());start=i+1;}}parts.Add(s.Substring(start).Trim());return parts;}
var rules=Regex.Matches(text,@"(?ms)^(?<name>\w+)\s*(?<assign>:=|:)\s*(?<body>.*?)(?=^\w+\s*(?::=|:)|\z)").Select(m=>new Rule(m.Groups["name"].Value,m.Groups["assign"].Value,Split(m.Groups["body"].Value.Substring(0,m.Groups["body"].Value.LastIndexOf(';'))))).ToList();
var keep=new Dictionary<string,string>{{"TSTypeEntry","TSType"},{"TSType","{TSNotFunctionTypeStart()}? TSConditionalType"},{"TSConditionalType","TSUnionType {TSNotConditionalTypeStart()}?"},{"TSUnionType","TSIntersectionType"},{"TSIntersectionType","TSTypeOperator"},{"TSTypeOperator","TSPostfixType"},{"TSPostfixType","TSPostfixCore {TSNotPostfixTypeStart()}?"},{"TSPostfixCore","TSPrimaryType"},{"TSPrimaryType","TSTypeReference"},{"TSTypeReference","TSQualifiedName {TSNoTypeReferenceSuffix()}?"},{"TSQualifiedName","TSBindingIdentifier"},{"TSBindingIdentifier","IdentifierName"}};
string Make()=>string.Join("\n",rules.Select(r=>r.Name+" "+r.Assign+" "+string.Join(" | ",r.Alts)+" ;"));
bool Fails(){var r=EbnfGrammarReader.Read(Make(),options);if(!r.Success)return false;try{var p=new LALRParser(r.Grammar,false);var l=new Lexer();foreach(var t in r.Grammar.TerminalSet)l.AddTokenRule(t);return !p.Parsing(l.Lexing("A").TokensForParsing).Success;}catch{return false;}}
Console.WriteLine("STARTFAIL "+Fails());
bool changed;do{changed=false;foreach(var r in rules){if(r.Assign!=":"||!keep.TryGetValue(r.Name,out var safe))continue;for(int i=r.Alts.Count-1;i>=0;i--){if(r.Alts[i].Trim()==safe)continue;var alt=r.Alts[i];r.Alts.RemoveAt(i);if(Fails()){changed=true;Console.WriteLine("REMOVED "+r.Name+" : "+alt);}else r.Alts.Insert(i,alt);}}}while(changed);
var reachable=new HashSet<string>{"TSTypeEntry"};do{changed=false;foreach(var r in rules.Where(r=>reachable.Contains(r.Name))){if(r.Assign==":=")continue;foreach(var alt in r.Alts){var stripped=Regex.Replace(alt,@"'[^']*'|""(?:""""|[^""])*""|\{[^}]*\}\?", "");foreach(Match word in Regex.Matches(stripped,@"[A-Za-z_]\w*"))if(rules.Any(n=>n.Name==word.Value))changed|=reachable.Add(word.Value);}}}while(changed);
rules.RemoveAll(r=>!reachable.Contains(r.Name));var final=Make();File.WriteAllText("artifacts/typescript/minimize/repro.ebnf",final);Console.WriteLine(final);Console.WriteLine("FINALFAIL "+Fails());
var result=EbnfGrammarReader.Read(final,options);var parser=new LALRParser(result.Grammar,false);var lex=new Lexer();foreach(var t in result.Grammar.TerminalSet)lex.AddTokenRule(t);var parsed=parser.Parsing(lex.Lexing("A").TokensForParsing);foreach(var unit in parsed.SelectMany(b=>b.Units))Console.WriteLine(unit.InputValue+" : "+unit.Action+" "+unit.ErrorMessage);
record Rule(string Name,string Assign,List<string> Alts);
