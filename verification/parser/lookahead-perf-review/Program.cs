using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;

// Standalone construction measurement, not a product change or a test-suite run.
// Both bundles use the same frozen ECMAScript grammar and supporting DLLs;
// only Parse.FrontEnd.Parsers.dll differs between old and new bundles.
var label = args.Length == 0 ? "unspecified" : args[0];
var grammarWatch = Stopwatch.StartNew();
var language = new JavaScriptGrammar(mode: JavaScriptMode.Strict, inputKind: JavaScriptInputKind.Script);
grammarWatch.Stop();
GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
long before = GC.GetTotalAllocatedBytes(true);
var parserWatch = Stopwatch.StartNew();
var parser = new LALRParser(language.Grammar, false);
parserWatch.Stop();
long allocated = GC.GetTotalAllocatedBytes(true) - before;
Console.WriteLine(JsonSerializer.Serialize(new {
    label,
    measuredUtc = DateTime.UtcNow,
    grammarMs = grammarWatch.Elapsed.TotalMilliseconds,
    parserMs = parserWatch.Elapsed.TotalMilliseconds,
    parserAllocatedBytes = allocated,
    states = parser.Canonical.IndexStateDic.Count,
    lrItems = parser.Canonical.IndexStateDic.Sum(pair => pair.Value.Count),
    lookaheadEntries = parser.Canonical.LookAheadTable.Count,
    parserAssemblySha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(LALRParser).Assembly.Location))).ToLowerInvariant(),
    grammarAssemblySha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(JavaScriptGrammar).Assembly.Location))).ToLowerInvariant()
}));
