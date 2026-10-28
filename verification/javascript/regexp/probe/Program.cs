using System.Reflection;
using System.Text.Json;
using Janglim.FrontEnd.Grammars.JavaScript;
var type = typeof(JavaScriptExpressionGrammar).Assembly.GetType("Janglim.FrontEnd.Grammars.JavaScript.JavaScriptRegExpValidation").GetNestedType("Reader", BindingFlags.NonPublic);
var constructor = type.GetConstructor(BindingFlags.NonPublic|BindingFlags.Instance,null,new[]{typeof(string),typeof(string)},null);
var read = type.GetMethod("Read",BindingFlags.NonPublic|BindingFlags.Instance);
using var data = JsonDocument.Parse(File.ReadAllText(args[0]));
int total = 0, bad = 0, unexpected = 0;
foreach(var row in data.RootElement.EnumerateArray())
{
    string body = row.GetProperty("body").GetString();
    string flags = row.GetProperty("flags").GetString();
    bool expected = row.GetProperty("expected").GetBoolean();
    string group = row.GetProperty("group").GetString();
    bool valid = true;
    string error = null;
    try { read.Invoke(constructor.Invoke(new object[]{body,flags}),null); }
    catch(TargetInvocationException e)
    {
        valid = false;error=e.InnerException?.Message;
        if(e.InnerException?.GetType().Name!="SyntaxFailure") { unexpected++;Console.WriteLine("UNEXPECTED "+e.InnerException); }
    }
    total++;
    if(valid!=expected)
    {
        bad++;
        Console.WriteLine(JsonSerializer.Serialize(new {group,body,flags,expected,valid,error}));
    }
}
Console.WriteLine($"Compared {total}; mismatches {bad}; unexpected exceptions {unexpected}.");
Environment.ExitCode = bad==0 && unexpected==0 ? 0 : 1;
