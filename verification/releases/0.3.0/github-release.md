Janglim 0.3.0 is the first non-prerelease package in the 0.3 line. It supports the LALR parser workflow on .NET Standard 2.1 and carries forward the existing C#/EBNF grammar engine, introspection, normalization, incremental reparsing, and pluggable recovery features.

```shell
dotnet add package Janglim --version 0.3.0
```

[NuGet package](https://www.nuget.org/packages/Janglim/0.3.0) · [Playground](https://janglim.mythosia.net/) · [Manual](https://aj-comp.github.io/Compiler/)

Playground 1.0.0 introduces the React and shadcn/ui workspace connected to the C# WebAssembly engine: interactive trees with linked grammar/input highlighting, parsing tables, execution playback, themes, responsive layouts, and 14 interface languages. Saved explicit language choices take priority over browser-language detection; unsupported languages fall back to English.

The engine and Playground use independent versions. LALRParser is the supported parser; SLR and CLR remain experimental and incomplete. The engine's public API may evolve during 0.x. Large grammars can affect browser responsiveness. Public builds use the included loading indicator and require no licensed component sources or credentials.

Validation: engine tests 140 passed with one existing SLR skip; UI tests 15 passed; release helper tests 12 passed; clean installation, public web build, packaging, and SHA-256 checks passed on Linux. NuGet publication and Azure deployment use the artifacts produced by the same verified release run.

See the [changelog](https://github.com/AJ-comp/Compiler/blob/v0.3.0/CHANGELOG.md) and [Korean release notes](https://github.com/AJ-comp/Compiler/blob/v0.3.0/RELEASE_NOTES.md) for details. The core engine was designed and written by its human author; AI assistance with documentation, UI, and release preparation preserves that authorship.
