using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;

namespace Janglim.Mcp;

/// <summary>
/// Read-only release assets embedded in the service assembly. Caller-supplied IDs are
/// matched exactly against the catalog, never used as filesystem or resource paths.
/// The immutable text is a baseline; client edits are sent as independent worker requests.
/// </summary>
public static class GrammarCatalog
{
    private static readonly Lazy<GrammarPreset> C11Core = new(CreateC11Core);

    public static GrammarListResponse List()
    {
        var preset = C11Core.Value;
        return new("ok", [new(preset.Id, preset.Name, preset.Version, preset.Language,
            preset.Standard, preset.Description, preset.SupportedFeatures, preset.Limitations, preset.Sha256)]);
    }

    public static GrammarPresetResponse Get(string? grammarId)
        => string.Equals(grammarId, "c11-core", StringComparison.Ordinal)
            ? new("ok", C11Core.Value)
            : new("unknown_grammar", Message: "Unknown built-in grammar ID. Use list_grammars to discover available IDs.");

    private static GrammarPreset CreateC11Core()
    {
        using var stream = typeof(GrammarCatalog).Assembly.GetManifestResourceStream("Janglim.Mcp.Grammars.c11-core.ebnf")
            ?? throw new InvalidOperationException("The built-in C11 Core grammar is missing from the service assembly.");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        // A reproducible snapshot/hash across Windows, Linux, and container builds.
        var grammar = reader.ReadToEnd().ReplaceLineEndings("\n");
        if (grammar.Length is 0 or > McpLimits.MaxGrammarChars)
            throw new InvalidOperationException("The built-in C11 Core grammar exceeds the service's grammar limit.");
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(grammar)));
        return new("c11-core", "C11 Core", "1.0.0", "C", "C11",
            "An editable C11 core syntax baseline for grammar experiments, not a complete C compiler.",
            ["Built-in types, declarations, function definitions and prototypes",
             "Pointers, arrays, function declarators, structs, unions and enums",
             "Expressions with C precedence and associativity, casts and initializers",
             "Control flow, nearest-if else binding and labels",
             "Numeric, string and character literals, comments and lexical rejection guards"],
            ["No preprocessing, macros, includes or line splicing",
             "No typedef declarations or context-sensitive typedef-name resolution",
             "No semantic evaluation, type checking or parser source generation",
             "No _Atomic, _Generic, _Alignas, _Imaginary or K&R function definitions",
             "ASCII identifiers only; no digraphs, trigraphs or compiler-specific extensions"],
            hash, grammar);
    }
}

public record GrammarPresetMetadata(string Id, string Name, string Version, string Language,
    string Standard, string Description, ImmutableArray<string> SupportedFeatures,
    ImmutableArray<string> Limitations, string Sha256);

public sealed record GrammarPreset(string Id, string Name, string Version, string Language,
    string Standard, string Description, ImmutableArray<string> SupportedFeatures,
    ImmutableArray<string> Limitations, string Sha256, string Grammar)
    : GrammarPresetMetadata(Id, Name, Version, Language, Standard, Description,
        SupportedFeatures, Limitations, Sha256);

public sealed record GrammarListResponse(string Status, ImmutableArray<GrammarPresetMetadata> Grammars);
public sealed record GrammarPresetResponse(string Status, GrammarPreset? Preset = null, string? Message = null);
