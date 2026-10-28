using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;
var language = new JavaScriptScriptGrammar();
var parser = new LALRParser(language.Grammar, false);
if (!language.GrammarText.Contains("ClassStaticBlock")) throw new Exception("Embedded class grammar missing.");
var examples = new[] {
    ("class C {}", true),
    ("const C=class extends Base {constructor(...args){super(...args);} method(){return super.value;}};", true),
    ("class C {#value=1; get value(){return this.#value;} set value(value){this.#value=value;}}", true),
    ("class C {static get #value(){} static set #value(value){} static has(o){return #value in o;}}", true),
    ("class C {x=/a/; static y=()=>({value:1}); static {this.y();}} /b/.test(text);", true),
    ("class C {static static\nfoo(){} get value(){return /a/;}}", true),
    ("class C {field=()=>{}\ninstanceof(){} get value(){}}", true),
    ("class C {field=()=>{}\nin(){} get value(){}}", true),
    ("class C {field=()=>{}\ninstanceof=/a/; get value(){}}", true),
    ("class C {field=value\ninstanceof Type; get value(){}}", true),
    ("class C {method(){return class D {key=arguments;};}}", false),
    ("class C {#x; method(o){return o?.#x;}}", true),
    ("class C {#x; method(){return class D {read(o){return o.#x;}};}}", true),
    ("class C {field=`value ${class D {nested=/a/;}}`;}", true),
    ("class C {static {function f(){} function f(){} var f;}}", true),
    ("class C {field=new.target; static {new.target;}}", true),
    ("class C {static field=await;}", true),
    ("class C {constructor(){} constructor(){}}", false),
    ("class C {static prototype(){}}", false),
    ("class C {constructor=0;}", false),
    ("class C {#x; static #x;}", false),
    ("class C {method(){return this.#missing;}}", false),
    ("class C {#x; method(o){delete (o.#x);}}", false),
    ("class C {method(){super();}}", false),
    ("class C {field=()=>arguments;}", false),
    ("class C {static {return;}}", false),
    ("class C {static {await;}}", false),
    ("class C {set x(value,){}}", false),
    ("class C {method(a,a){}}", false),
    ("class C {method(a=0){'use strict';}}", false),
    ("class C {get static\nfoo(){}}", false)
};
foreach (var (input, expected) in examples)
{
    var result = parser.Parsing(language.CreateTokenSource(input));
    bool valid = result.Success && language.Validate(result.ToParseTree).Count == 0;
    if (valid != expected) throw new Exception("Script package mismatch: " + input + " " + parser.FiredException);
}
var expression = new JavaScriptExpressionGrammar();
var expressionParser = new LALRParser(expression.Grammar, false);
foreach (string input in new[] { "class {}", "(class {method(){return new.target;}}) / divisor", "() => class {#x; read(){return this.#x;}}" })
{
    var result = expressionParser.Parsing(expression.CreateTokenSource(input));
    if (!result.Success || expression.Validate(result.ToParseTree).Count != 0) throw new Exception("Expression package mismatch: " + input);
}
Console.WriteLine($"PASS: {examples.Length + 3} package-only consumer cases; embedded class grammar, lexical context and static validation.");
