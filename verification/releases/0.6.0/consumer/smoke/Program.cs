using System.Reflection;
using System.Text.Json;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Grammars.TypeScript;
using Janglim.FrontEnd.Parsers;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.Tokenize;

var results = new List<object>();
int failures = 0;
void Check(string id, bool pass, object? detail = null)
{
    results.Add(new {id, pass, detail});
    Console.WriteLine($"{(pass ? "PASS" : "FAIL")} {id}");
    if (!pass) failures++;
}
void Cases(string group, EbnfGrammar grammar, Func<string, ITokenSource> source,
    Func<ParseTreeSymbol,IReadOnlyList<JavaScriptDiagnostic>> validate,
    params (string Id, string Input, bool Accept)[] cases)
{
    var parser = new LALRParser(grammar, false);
    foreach (var item in cases)
    {
        try
        {
            var parsed = parser.Parsing(source(item.Input));
            var diagnostics = parsed.Success ? validate(parsed.ToParseTree) : Array.Empty<JavaScriptDiagnostic>();
            bool accepted = parsed.Success && parsed.AllErrors.Count() == 0 && diagnostics.Count == 0;
            var fired = parser.FiredException;
            bool internalFailure = !parsed.Success && fired != null && fired is not ParsingException &&
                fired is not JavaScriptSyntaxException && fired.GetType().Name != "PredictionFailedException";
            Check(group+"/"+item.Id, accepted == item.Accept && !internalFailure,
                new {expectedAccept=item.Accept, accepted, parseSuccess=parsed.Success,
                    diagnostics=diagnostics.Select(d=>d.Code).ToArray(),
                    exception=fired?.GetType().Name, input=item.Input});
        }
        catch(Exception ex) { Check(group+"/"+item.Id,false,new {exception=ex.ToString(),input=item.Input}); }
    }
}

string[] dllNames = {"Janglim","AJ.Common","Parse","Parse.FrontEnd","Parse.FrontEnd.Grammars",
    "Parse.FrontEnd.Parsers","Parse.FrontEnd.Grammars.Ebnf","Parse.FrontEnd.ErrorHandler","Parse.FrontEnd.Support"};
foreach(var name in dllNames)
{
    var file=Path.Combine(AppContext.BaseDirectory,name+".dll");
    var version=AssemblyName.GetAssemblyName(file).Version?.ToString();
    Check("assembly/"+name,version=="0.6.0.0",new {file,version});
}
var resources=typeof(TypeScriptGrammar).Assembly.GetManifestResourceNames().Where(n=>n.EndsWith(".ebnf")).OrderBy(n=>n).ToArray();
Check("embedded-ebnf-resources",resources.Length==8,new {resources});

var read=EbnfGrammarReader.Read("Expr : Expr '+' Term | Term ; Term : id ; id := \"[a-zA-Z]+\" ;");
Check("ebnf/read",read.Success);
if(read.Success)
{
    var lexer=new Lexer();
    foreach(var token in read.Grammar.TerminalSet) lexer.AddTokenRule(token);
    var parser=new LALRParser(read.Grammar,false);
    Check("ebnf/accept",parser.Parsing(lexer.Lexing("a + b").TokensForParsing).Success);
    Check("ebnf/reject",!parser.Parsing(lexer.Lexing("a +").TokensForParsing).Success);
}
var bad=EbnfGrammarReader.Read("S : U ; U : U 'x' ;");
Check("ebnf/productivity-diagnostic",!bad.Success,new {errors=bad.Errors.Select(e=>e.ToString()).ToArray()});

var jsStrict=new JavaScriptGrammar();
Cases("javascript-strict",jsStrict.Grammar,jsStrict.CreateTokenSource,jsStrict.Validate,
    ("function","async function load({path}) { return /ab/g.test(await read(path)); }",true),
    ("with-rejected","with (obj) { value; }",false),
    ("duplicate-parameter","function f(x, x) {}",false));
var jsLoose=new JavaScriptGrammar(JavaScriptMode.NonStrict);
Cases("javascript-nonstrict",jsLoose.Grammar,jsLoose.CreateTokenSource,jsLoose.Validate,
    ("with","with (obj) { value; }",true),
    ("duplicate-parameter","function f(x, x) {}",true),
    ("strict-directive","'use strict'; with (obj) { value; }",false));
var jsWeb=new JavaScriptGrammar(JavaScriptMode.NonStrict,webCompatibility:true);
Cases("javascript-annexb",jsWeb.Grammar,jsWeb.CreateTokenSource,jsWeb.Validate,
    ("html-comment","<!-- browser comment\nvar x = 1;",true),
    ("if-function","if (ok) function f() {}",true));
var jsModule=new JavaScriptGrammar(inputKind:JavaScriptInputKind.Module);
Cases("javascript-module",jsModule.Grammar,jsModule.CreateTokenSource,jsModule.Validate,
    ("import-export","import {value} from './x.js'; export const result = value;",true),
    ("duplicate-export","const value = 1; export {value}; export {value};",false),
    ("module-with","with (obj) {}",false));

var ts=new TypeScriptGrammar();
Cases("typescript-script",ts.Grammar,ts.CreateTokenSource,ts.Validate,
    ("class-readme","class Box<T> { constructor(public value: T) {} }",true),
    ("type-declarations","interface User { name: string; } type Maybe<T> = T | null; let user: Maybe<User>;",true),
    ("as","let result = value as User;",true),
    ("satisfies","let result = value satisfies User;",true),
    ("nonnull","value!.property;",true),
    ("abstract-class","abstract class C<T> { abstract run(value: T): T; }",true),
    ("nested-generics","let value: Array<Map<string, number>>;",true),
    ("malformed-annotation","let value: = 1;",false),
    ("parameter-order","function f(a?: number, b: number) {}",false),
    ("getter-arity","declare class C { get value(next: number): number; }",false));
var tsType=new TypeScriptGrammar(TypeScriptInputKind.Type);
Cases("typescript-type",tsType.Grammar,tsType.CreateTokenSource,tsType.Validate,
    ("union","User | null",true),
    ("mapped","{ [K in keyof T]?: T[K] }",true),
    ("conditional","T extends U ? X : Y",true),
    ("invalid-union","string |",false));
var tsModule=new TypeScriptGrammar(TypeScriptInputKind.Module);
Cases("typescript-module",tsModule.Grammar,tsModule.CreateTokenSource,tsModule.Validate,
    ("type-export","export type Name = string;",true),
    ("class-export","export class C { constructor(public value: number) {} }",true),
    ("strict-with","with (obj) {}",false));
var tsExpression=new TypeScriptGrammar(TypeScriptInputKind.Expression);
Cases("typescript-expression",tsExpression.Grammar,tsExpression.CreateTokenSource,tsExpression.Validate,
    ("as","value as User",true),
    ("satisfies","value satisfies User",true),
    ("nonnull","value!.property",true),
    ("incomplete-assertion","value as",false));

var report=new {package="Janglim",version="0.6.0",runtime=Environment.Version.ToString(),
    checks=results.Count,failures,passed=failures==0,results};
File.WriteAllText(args[0],JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));
return failures==0?0:1;
