using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers;
using Janglim.FrontEnd.Parsers.LR;

// This is a syntax/early-error runner: it never evaluates JavaScript or resolves imports.
// The external corpus preparation tool supplies strict/module variants and expectations.
var options = Options.Parse(args);
var jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(options.Output))!);
using var output = new StreamWriter(options.Output, append: options.Append, new UTF8Encoding(false)) { AutoFlush = true };
using var failures = new StreamWriter(options.Failures, append: options.Append, new UTF8Encoding(false)) { AutoFlush = true };
var profiles = new Dictionary<(JavaScriptInputKind, JavaScriptMode, bool), Profile>();
var totals = new Dictionary<string, int>(StringComparer.Ordinal);
var timer = Stopwatch.StartNew();
int selected = 0, processed = 0;
foreach (var line in File.ReadLines(options.Input, Encoding.UTF8))
{
    if (string.IsNullOrWhiteSpace(line)) continue;
    var test = JsonSerializer.Deserialize<Case>(line, jsonOptions)
        ?? throw new InvalidDataException("A corpus line is null.");
    var kind = Enum.Parse<JavaScriptInputKind>(test.InputKind, ignoreCase: true);
    var mode = Enum.Parse<JavaScriptMode>(test.Mode, ignoreCase: true);
    string profileName = kind == JavaScriptInputKind.Module ? "module" : mode == JavaScriptMode.Strict ? "script-strict" : "script-nonstrict";
    if (options.Profile != null && profileName != options.Profile) continue;
    if (selected++ < options.Skip) continue;
    if (options.Limit > 0 && processed >= options.Limit) break;
    var key = (kind, mode, test.WebCompatibility);
    if (!profiles.TryGetValue(key, out var profile))
    {
        Console.Error.WriteLine($"Preparing {profileName} (web={test.WebCompatibility}) ...");
        var language = new JavaScriptGrammar(mode, test.WebCompatibility, kind);
        profile = new Profile(language, new LALRParser(language.Grammar, false));
        profiles.Add(key, profile);
        Console.Error.WriteLine($"Ready {profileName} at {timer.Elapsed}.");
    }

    // A stuck case must not be counted as a valid rejection. The coordinator can resume
    // after this recorded case in a fresh process; no background parser is reused.
    // A CPU-bound parser can occupy the only available pool worker. In that case
    // a Task.Delay/WhenAny continuation may not run promptly enough to stop it.
    // Use an independent worker and a native wait on the coordinator thread so
    // the timeout does not depend on the thread pool scheduling a continuation.
    var caseClock = Stopwatch.StartNew();
    var work = Task.Factory.StartNew(() => Evaluate(test, profile),
        CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    using var completed = ((IAsyncResult)work).AsyncWaitHandle;
    Result result;
    if (!completed.WaitOne(options.TimeoutMs))
    {
        result = Result.Timeout(test, caseClock.Elapsed.TotalMilliseconds);
        Record(test, result);
        WriteSummary();
        output.Flush(); failures.Flush();
        Environment.Exit(124);
        return;
    }
    result = work.GetAwaiter().GetResult();
    Record(test, result);
    if (processed % 250 == 0)
        Console.Error.WriteLine($"{processed} cases, {totals.GetValueOrDefault("pass")} passed, {processed - totals.GetValueOrDefault("pass")} mismatches/errors; {timer.Elapsed}.");
    if (options.StopOnFailure && result.Outcome != "pass") break;
}
WriteSummary();
Environment.ExitCode = totals.Keys.Any(key => key != "pass") ? 1 : 0;

void Record(Case test, Result result)
{
    output.WriteLine(JsonSerializer.Serialize(result, jsonOptions));
    if (result.Outcome != "pass")
        failures.WriteLine(JsonSerializer.Serialize(new { test, result }, jsonOptions));
    totals[result.Outcome] = totals.GetValueOrDefault(result.Outcome) + 1;
    processed++;
}

void WriteSummary()
{
    var summary = new { options.Input, options.Profile, options.Skip, processed, outcomes = totals, elapsedSeconds = timer.Elapsed.TotalSeconds, syntaxOnly = true };
    var text = JsonSerializer.Serialize(summary, new JsonSerializerOptions(jsonOptions) { WriteIndented = true });
    File.WriteAllText(options.Summary, text, new UTF8Encoding(false));
    Console.Error.WriteLine(text);
}

static Result Evaluate(Case test, Profile profile)
{
    var timer = Stopwatch.StartNew();
    try
    {
        var parsed = profile.Parser.Parsing(profile.Language.CreateTokenSource(test.Source));
        if (!parsed.Success)
        {
            var exception = profile.Parser.FiredException;
            bool syntax = exception == null || Chain(exception).Any(item =>
                item is ParsingException || item is JavaScriptSyntaxException || item.GetType().Name == "PredictionFailedException");
            return Result.Create(test, false, syntax ? "parse" : "error", null, exception, timer.Elapsed.TotalMilliseconds);
        }
        var diagnostics = profile.Language.Validate(parsed.ToParseTree);
        return Result.Create(test, diagnostics.Count == 0, diagnostics.Count == 0 ? "accepted" : "early-error",
            diagnostics, null, timer.Elapsed.TotalMilliseconds);
    }
    catch (Exception exception)
    {
        return Result.Create(test, false, "error", null, exception, timer.Elapsed.TotalMilliseconds);
    }
}

static IEnumerable<Exception> Chain(Exception exception)
{
    for (Exception? item = exception; item != null; item = item.InnerException) yield return item;
}

sealed record Profile(JavaScriptGrammar Language, LALRParser Parser);
sealed class Case
{
    public string Id { get; set; } = "";
    public string Path { get; set; } = "";
    public string Variant { get; set; } = "";
    public string InputKind { get; set; } = "Script";
    public string Mode { get; set; } = "NonStrict";
    public bool WebCompatibility { get; set; } = true;
    public bool ExpectAccept { get; set; }
    public string Source { get; set; } = "";
    public string[] Features { get; set; } = Array.Empty<string>();
    public string? Esid { get; set; }
    public string? SourceSha256 { get; set; }
    public string? OriginalSha256 { get; set; }
}

sealed class Result
{
    public string Id { get; init; } = "";
    public string Path { get; init; } = "";
    public string Variant { get; init; } = "";
    public string InputKind { get; init; } = "";
    public string Mode { get; init; } = "";
    public bool ExpectedAccept { get; init; }
    public bool Accepted { get; init; }
    public string Stage { get; init; } = "";
    public string Outcome { get; init; } = "";
    public double ElapsedMs { get; init; }
    public string[]? Diagnostics { get; init; }
    public string[]? ExceptionChain { get; init; }
    public string? SourceSha256 { get; init; }

    public static Result Create(Case test, bool accepted, string stage, IReadOnlyList<JavaScriptDiagnostic>? diagnostics, Exception? exception, double elapsed)
        => new()
        {
            Id = test.Id, Path = test.Path, Variant = test.Variant, InputKind = test.InputKind, Mode = test.Mode,
            ExpectedAccept = test.ExpectAccept, Accepted = accepted, Stage = stage,
            Outcome = stage == "error" ? "error" : accepted == test.ExpectAccept ? "pass" : accepted ? "unexpected-accept" : "unexpected-reject",
            ElapsedMs = elapsed,
            Diagnostics = diagnostics?.Select(d => $"{d.Code}@{d.SourceOffset}+{d.Length}: {d.Message}").ToArray(),
            ExceptionChain = exception == null ? null : Exceptions(exception).ToArray(),
            SourceSha256 = test.SourceSha256
        };
    public static Result Timeout(Case test, double elapsedMs)
        => new() { Id = test.Id, Path = test.Path, Variant = test.Variant, InputKind = test.InputKind, Mode = test.Mode,
            ExpectedAccept = test.ExpectAccept, Accepted = false, Stage = "timeout", Outcome = "timeout", ElapsedMs = elapsedMs, SourceSha256 = test.SourceSha256 };
    private static IEnumerable<string> Exceptions(Exception exception)
    {
        for (Exception? item = exception; item != null; item = item.InnerException)
            yield return item.GetType().FullName + ": " + item.Message;
    }
}

sealed record Options(string Input, string Output, string Failures, string Summary, string? Profile, int Skip, int Limit, int TimeoutMs, bool Append, bool StopOnFailure)
{
    public static Options Parse(string[] args)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--append" || args[i] == "--stop-on-failure") { values[args[i]] = "true"; continue; }
            if (!args[i].StartsWith("--", StringComparison.Ordinal) || i + 1 >= args.Length)
                throw new ArgumentException("Use --input cases.jsonl --output results.jsonl [--profile script-strict|script-nonstrict|module].");
            string key = args[i]; values[key] = args[++i];
        }
        string Required(string key) => values.TryGetValue(key, out string? value) ? value : throw new ArgumentException("Missing " + key);
        string output = Required("--output");
        return new Options(Required("--input"), output, values.GetValueOrDefault("--failures", output + ".failures.jsonl"),
            values.GetValueOrDefault("--summary", output + ".summary.json"), values.GetValueOrDefault("--profile"),
            int.Parse(values.GetValueOrDefault("--skip", "0")), int.Parse(values.GetValueOrDefault("--limit", "0")),
            int.Parse(values.GetValueOrDefault("--timeout-ms", "30000")), values.ContainsKey("--append"), values.ContainsKey("--stop-on-failure"));
    }
}
