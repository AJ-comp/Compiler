using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;

// This diagnostic adapter loads an already-built JavaScript support assembly.
// It has no project/reference dependency and never builds or changes the engine.
if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: RegExpAudit <Parse.FrontEnd.Grammars.Ebnf.dll>");
    return 2;
}
string path = Path.GetFullPath(args[0]);
string folder = Path.GetDirectoryName(path)!;
AssemblyLoadContext.Default.Resolving += (_, name) =>
{
    string dependency = Path.Combine(folder, name.Name + ".dll");
    return File.Exists(dependency)
        ? AssemblyLoadContext.Default.LoadFromAssemblyPath(dependency) : null;
};
var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
var validator = assembly.GetType(
    "Janglim.FrontEnd.Grammars.JavaScript.JavaScriptRegExpValidation", throwOnError: true)!;
var readerType = validator.GetNestedType("Reader", BindingFlags.NonPublic)!;
var read = readerType.GetMethod("Read", BindingFlags.Instance | BindingFlags.NonPublic)!;
string? line;
while ((line = Console.ReadLine()) != null)
{
    var test = JsonSerializer.Deserialize<PatternCase>(line,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    bool accepted = false;
    string? error = null;
    string? unexpectedException = null;
    string? bodySha256 = null;
    try
    {
        // JSON's Unicode-scalar handling must not normalize unmatched UTF-16
        // surrogates. Base64 carries the exact code units of constructor strings.
        byte[] bytes = Convert.FromBase64String(test.BodyUtf16);
        if (bytes.Length % 2 != 0) throw new InvalidDataException("Odd UTF-16 byte count.");
        var units = new char[bytes.Length / 2];
        for (int index = 0; index < units.Length; index++)
            units[index] = (char)(bytes[index * 2] | bytes[index * 2 + 1] << 8);
        string body = new string(units);
        var restored = new byte[body.Length * 2];
        for (int index = 0; index < body.Length; index++)
        {
            restored[index * 2] = (byte)body[index];
            restored[index * 2 + 1] = (byte)(body[index] >> 8);
        }
        bodySha256 = Convert.ToHexString(SHA256.HashData(restored)).ToLowerInvariant();
        var reader = Activator.CreateInstance(readerType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null, args: new object[] { body, test.Flags }, culture: null);
        read.Invoke(reader, null);
        accepted = true;
    }
    catch (TargetInvocationException exception)
    {
        var cause = exception.InnerException ?? exception;
        error = cause.Message;
        // An implementation crash must remain distinguishable from an intended
        // syntax diagnostic, even when the reference engine also rejects a case.
        if (cause.GetType().Name != "SyntaxFailure") unexpectedException = cause.GetType().FullName;
    }
    catch (Exception exception)
    {
        error = exception.Message;
        unexpectedException = exception.GetType().FullName;
    }
    Console.WriteLine(JsonSerializer.Serialize(new
    {
        test.Id, accepted, error, unexpectedException, bodySha256
    }));
}
return 0;

internal sealed record PatternCase(string Id, string BodyUtf16, string Flags);
