using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
var assemblyPath = Path.GetFullPath(args[0]);
var folder = Path.GetDirectoryName(assemblyPath)!;
AssemblyLoadContext.Default.Resolving += (_, name) => {
    var path = Path.Combine(folder, name.Name + ".dll");
    return File.Exists(path) ? AssemblyLoadContext.Default.LoadFromAssemblyPath(path) : null;
};
var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
var outer = assembly.GetType("Janglim.FrontEnd.Grammars.JavaScript.JavaScriptRegExpValidation", true)!;
var type = outer.GetNestedType("Reader", BindingFlags.NonPublic)!;
var read = type.GetMethod("Read", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
string? line;
while ((line = Console.ReadLine()) != null) {
    var test = JsonSerializer.Deserialize<Test>(line, new JsonSerializerOptions {PropertyNameCaseInsensitive=true})!;
    bool accepted = false;
    string? error = null;
    try {
        var reader = Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null, args: new object[] { test.Body, test.Flags }, culture: null);
        read.Invoke(reader, null);
        accepted = true;
    } catch (TargetInvocationException exception) { error = exception.InnerException?.Message ?? exception.Message; }
    catch (Exception exception) { error = exception.GetType().Name + ": " + exception.Message; }
    Console.WriteLine(JsonSerializer.Serialize(new {test.Id, test.Body, test.Flags, accepted, error}));
}
record Test(string Id,string Body,string Flags);
