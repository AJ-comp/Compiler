using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;

var language = new JavaScriptScriptGrammar();
var parser = new LALRParser(language.Grammar);
if (!language.GrammarText.Contains("FormalParameters") || !language.GrammarText.Contains("ForStatement"))
    throw new Exception("Embedded binding/statement grammar is missing.");
var cases = new[] {
    ("function read({name = 'guest'}, ...rest) { return name; } const result = read(user);", true),
    ("const [...[first, second]] = values;", true),
    ("const {name, ...other} = user;", true),
    ("const sum = function(a, b = 0){ return a + b; };", true),
    ("a\n(b)", true), ("a++\n(b)", true), ("value\n/other/divisor;", true),
    ("function f(){return\nvalue;}", true), ("function f(){return\n;}", true),
    ("let a\n(b)", true), ("let a = value\n(b)", true),
    ("if (ready) /ab/.test(text); call(value) / divisor;", true),
    ("function f(){} /a/.test(value);", true), ("const f=function(){} / 2;", true),
    ("const x=flag?{if:f(x)/2}:function(){} /2;", true),
    ("outer:while(x){break outer\n/a/.test(x);}", true),
    ("for (let i=0; i<10; i++) step(i);", true),
    ("for (const {name} of users) { if(name==='') continue; display(name); }", true),
    ("for ([a, b] of pairs) display(a,b);", true),
    ("for (let a=(b in c);;);", true),
    ("try { process(value); } catch({message}) {display(message);} finally {finish();}", true),
    ("try{}catch{call(value)/2;}", true),
    ("switch(value){case 0: break; default: debugger;}", true),
    ("function f(a=0){('use strict');return `${({a=1}=source)}`;}", true),
    ("do{}while(false) next();", true),
    ("var a; {let a;} function f(a){{let a;}}", true),
    ("function f(a,a){}", false), ("function f({a}){'use strict';}", false),
    ("function f(a){let a;}", false), ("let a; {var a;}", false),
    ("const missing;", false), ("let eval;", false), ("const {a: target.name}=value;", false),
    ("const [...rest,]=values;", false), ("function f(...rest=[]){}", false),
    ("function f(){throw\nerror;}", false), ("return value;", false),
    ("if(x) let a=1;", false), ("for(let i=0\ni<3;i++);", false),
    ("for(a?.b in object);", false), ("for(call() of values);", false),
    ("for(async of values);", false), ("for([...rest,] of values);", false),
    ("for(let a=b in c;;);", false), ("continue;", false),
    ("switch(x){default:;default:;}", false),
    ("({a={b=1}}=source);", false), ("a?.b\n`text`;", false)
};
foreach (var (input, expected) in cases)
{
    var parsed=parser.Parsing(language.CreateTokenSource(input));
    bool actual=parsed.Success && language.Validate(parsed.ToParseTree).Count==0;
    if(actual!=expected) throw new Exception("Script package mismatch: "+input+" "+parser.FiredException);
}
var expressions=new JavaScriptExpressionGrammar();
var expressionParser=new LALRParser(expressions.Grammar);
foreach(var input in new[]{"function({a=1}){return a;}","[a,...rest]=values","`${value / divisor}`"})
{
    var parsed=expressionParser.Parsing(expressions.CreateTokenSource(input));
    if(!parsed.Success || expressions.Validate(parsed.ToParseTree).Count!=0) throw new Exception("Expression package mismatch: "+input);
}
Console.WriteLine($"PASS: {cases.Length + 3} package-only consumer cases; Script and expression APIs; all three embedded EBNF resources loaded.");
