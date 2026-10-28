using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;

var output = args[0];
Directory.CreateDirectory(output);
var assembly = typeof(LALRParser).Assembly;
var api = assembly.GetTypes().Where(ExposedType).OrderBy(TypeName).SelectMany(Api).Order().ToArray();
File.WriteAllLines(Path.Combine(output, "api.txt"), api);
if (args.Contains("--api-only")) return;

var grammarWatch = Stopwatch.StartNew();
var language = new JavaScriptGrammar(mode: JavaScriptMode.Strict, inputKind: JavaScriptInputKind.Script);
grammarWatch.Stop();
GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
var allocatedBefore = GC.GetTotalAllocatedBytes(true);
var parserWatch = Stopwatch.StartNew();
var parser = new LALRParser(language.Grammar, false);
parserWatch.Stop();
var allocated = GC.GetTotalAllocatedBytes(true) - allocatedBefore;

var core = parser.Canonical.IndexStateDic.ToDictionary(p => p.Key, p =>
    Hash(string.Join("\n", p.Value.Select(item => item.ToString()).Order())));
var lookaheads = parser.Canonical.LookAheadTable.Select(p =>
    core[p.Key.Item1] + " | " + p.Key.Item2 + " | " +
    string.Join(", ", p.Value.Select(t => t.ToString()).Order())).Order().ToArray();
var tables = parser.ParsingTable.Entries.Select(e =>
    core[e.State] + " | " + e.Symbol + " | " + ActionText(e.Action)).Order().ToArray();
File.WriteAllLines(Path.Combine(output, "lookaheads.txt"), lookaheads);
File.WriteAllLines(Path.Combine(output, "table.txt"), tables);
File.WriteAllText(Path.Combine(output, "summary.json"), JsonSerializer.Serialize(new {
    utc = DateTime.UtcNow,
    parserAssemblySha256 = FileHash(assembly.Location),
    grammarAssemblySha256 = FileHash(typeof(JavaScriptGrammar).Assembly.Location),
    apiLines = api.Length, apiSha256 = Hash(string.Join("\n", api)),
    states = parser.Canonical.IndexStateDic.Count,
    lrItems = parser.Canonical.IndexStateDic.Sum(p => p.Value.Count),
    lookaheadEntries = lookaheads.Length,
    lookaheadSha256 = Hash(string.Join("\n", lookaheads)),
    tableEntries = tables.Length,
    tableSha256 = Hash(string.Join("\n", tables)),
    grammarMs = grammarWatch.Elapsed.TotalMilliseconds,
    parserMs = parserWatch.Elapsed.TotalMilliseconds,
    parserAllocatedBytes = allocated
}, new JsonSerializerOptions { WriteIndented = true }));

string ActionText(ParseAction action) => action switch {
    ParseAction.Shift s => "Shift " + core[s.State],
    ParseAction.Goto g => "Goto " + core[g.State],
    ParseAction.Reduce r => "Reduce " + r.IsEpsilon + " " + r.Production.ToGrammarString(),
    ParseAction.Accept a => "Accept " + a.Production.ToGrammarString(),
    _ => action.ToString()
};
static string FileHash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
static bool ExposedType(Type t) => t.IsNested
    ? ExposedType(t.DeclaringType!) && (t.IsNestedPublic || t.IsNestedFamily || t.IsNestedFamORAssem)
    : t.IsPublic;
static bool ExposedMethod(MethodBase m) => m.IsPublic || m.IsFamily || m.IsFamilyOrAssembly;
static string TypeName(Type t) => t.IsGenericParameter ? "!" + t.GenericParameterPosition + ":" + t.Name :
    t.IsArray ? TypeName(t.GetElementType()!) + "[" + new string(',', t.GetArrayRank()-1) + "]" :
    t.IsByRef ? TypeName(t.GetElementType()!) + "&" :
    t.IsPointer ? TypeName(t.GetElementType()!) + "*" :
    t.IsGenericType ? (t.GetGenericTypeDefinition().FullName ?? t.Name) + "<" + string.Join(",", t.GetGenericArguments().Select(TypeName)) + ">" :
    t.FullName ?? t.Name;
static string GenericParameters(Type[] types) => string.Join(";", types.Where(t => t.IsGenericParameter).Select(t =>
    TypeName(t) + ":" + t.GenericParameterAttributes + ":" + string.Join(",", t.GetGenericParameterConstraints().Select(TypeName).Order())));
static string Parameters(ParameterInfo[] ps) => string.Join(",", ps.Select(p =>
    p.Attributes + " " + TypeName(p.ParameterType) + " " + p.Name +
    (p.HasDefaultValue ? "=" + Convert.ToString(p.DefaultValue, CultureInfo.InvariantCulture) : "") +
    " req(" + string.Join(",", p.GetRequiredCustomModifiers().Select(TypeName)) + ")" +
    " opt(" + string.Join(",", p.GetOptionalCustomModifiers().Select(TypeName)) + ")"));
static IEnumerable<string> Api(Type t) {
    var prefix = TypeName(t) + " | ";
    yield return prefix + "TYPE " + t.Attributes + " BASE " + (t.BaseType == null ? "" : TypeName(t.BaseType)) +
        " INTERFACES " + string.Join(",", t.GetInterfaces().Select(TypeName).Order()) + " GENERICS " + GenericParameters(t.GetGenericArguments());
    const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
    foreach (var m in t.GetMethods(flags).Where(ExposedMethod))
        yield return prefix + "METHOD " + m.Attributes + " " + m.CallingConvention + " " + TypeName(m.ReturnType) + " " + m.Name +
            "(" + Parameters(m.GetParameters()) + ") GENERICS " + GenericParameters(m.GetGenericArguments());
    foreach (var c in t.GetConstructors(flags).Where(ExposedMethod))
        yield return prefix + "CTOR " + c.Attributes + "(" + Parameters(c.GetParameters()) + ")";
    foreach (var f in t.GetFields(flags).Where(f => f.IsPublic || f.IsFamily || f.IsFamilyOrAssembly))
        yield return prefix + "FIELD " + f.Attributes + " " + TypeName(f.FieldType) + " " + f.Name +
            (f.IsLiteral ? "=" + Convert.ToString(f.GetRawConstantValue(), CultureInfo.InvariantCulture) : "");
    foreach (var p in t.GetProperties(flags).Where(p => p.GetAccessors(true).Any(ExposedMethod)))
        yield return prefix + "PROPERTY " + TypeName(p.PropertyType) + " " + p.Name + "(" + Parameters(p.GetIndexParameters()) + ")";
    foreach (var e in t.GetEvents(flags).Where(e => (e.AddMethod != null && ExposedMethod(e.AddMethod)) || (e.RemoveMethod != null && ExposedMethod(e.RemoveMethod))))
        yield return prefix + "EVENT " + TypeName(e.EventHandlerType!) + " " + e.Name;
}
