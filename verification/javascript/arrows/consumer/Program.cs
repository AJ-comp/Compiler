using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;
var language=new JavaScriptScriptGrammar();
var parser=new LALRParser(language.Grammar);
if(!language.GrammarText.Contains("ArrowFunction")) throw new Exception("Updated embedded grammar is missing.");
var examples=new[] {
 ("const f=x=>x;",true), ("const f=()=>1;",true),
 ("const f=(a,b,)=>a+b;",true),
 ("const f=({name='guest'},[first]=values,...rest)=>`${name}:${first}`;",true),
 ("const f=(...[first,...rest])=>first;",true), ("const f=(...{length})=>length;",true),
 ("const f=first=>second=>first+second;",true),
 ("const f=()=>({name:1});",true), ("const f=()=>{return 1;};",true),
 ("const f=()=>{}\n(value);",true), ("const f=()=>{}\n[value];",true),
 ("const f=()=>{}\n`value`;",true), ("const f=()=>{}\n/a/.test(value);",true),
 ("const f=()=>{}\n+value;",true), ("let f=()=>{}\n,g=1;",true),
 ("const f=x=>x\n(value);",true), ("(x=>{})(value);",true),
 ("const f=x=>/a/.test(x);const g=x=>x/divisor;",true),
 ("const f=x=>function(){} /2;",true),
 ("const f=({__proto__:a,__proto__:b})=>a;",true),
 ("const f=(a=({b=1}=other))=>a;",true),
 ("for((a=b in object)=>a;;);",true), ("for(x=>{x in object;};;);",true),
 ("const f=()=>{return\nvalue=>value;};",true),
 ("const f=()=>{function inner(){return 1;}return inner();};",true),
 ("const f=await=>await; const g=async=>async;",true),
 ("const f=(same,same)=>same;",false), ("const f=eval=>eval;",false),
 ("const f=({a:object.value})=>a;",false), ("const f=((a))=>a;",false),
 ("const f=(...rest,)=>rest;",false), ("const f=(a=0)=>{'use strict';};",false),
 ("const f=a=>{let a;};",false), ("const f=a\n=>a;",false),
 ("const f=()=>{} /a/.test(value);",false), ("const f=()=>{}\n.foo;",false),
 ("const f=()=>({a=1});",false), ("const f=(a={b=1})=>a;",false),
 ("const f=()=>{break;};",false), ("for(x=>x in object;;);",false),
 ("const value=();",false), ("const value=(a,);",false), ("const value=(...a);",false),
 ("const f=()=>`bad\\xZ`;",false)
};
foreach(var (input,expected) in examples)
{
 var result=parser.Parsing(language.CreateTokenSource(input));
 bool actual=result.Success && language.Validate(result.ToParseTree).Count==0;
 if(actual!=expected) throw new Exception("Script package mismatch: "+input+" "+parser.FiredException);
}
var expression=new JavaScriptExpressionGrammar();
var ep=new LALRParser(expression.Grammar);
foreach(var input in new[]{"x=>x", "()=>({value:1})", "({a=1})=>a", "x=>y=>x+y", "(x=>x)(value)"})
{
 var result=ep.Parsing(expression.CreateTokenSource(input));
 if(!result.Success || expression.Validate(result.ToParseTree).Count!=0) throw new Exception("Expression package mismatch: "+input);
}
Console.WriteLine($"PASS: {examples.Length+5} package-only consumer cases; arrow grammar, parameter analysis and lexical context state loaded from NuGet.");
