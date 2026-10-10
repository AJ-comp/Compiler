# Release history

Engine and web releases use independent version numbers. The entries below describe each version's scope and changes.

## Janglim Playground 1.1.0

The first step toward Janglim Studio adds local workspace files while retaining the existing Playground and parser engine.

- Save the current EBNF grammar and one input string to a tagged, versioned `.janglim.json` file.
- Open a workspace file to restore its source and parse it again with the existing engine.
- Keep the current work when a file is invalid or uses an unsupported format version; confirm replacement of unsaved edits inline before opening another workspace.
- Read and write workspace files locally in the browser, with no account or server storage. Parse results are rebuilt instead of serialized.

The engine package remains at 0.3.0. This web update does not add parser code generation or change the parsing algorithm.

## Janglim engine 0.3.0

The first non-prerelease package in the 0.3 line targets the supported `LALRParser` workflow on .NET Standard 2.1. The package is installed as `Janglim`; its engine assemblies remain bundled in one NuGet package.

### Included capabilities

- Define grammars in C# or EBNF text, with sequence, choice, optional symbols, and repetition.
- Inspect LALR ACTION/GOTO tables, FIRST/FOLLOW sets, item sets, conflict reports, parse trees, ASTs, and execution traces.
- Use grammar normalization, incremental reparsing, pluggable error recovery, and optional conflict backtracking through the C# engine API.
- Use the existing string/character literal token types and lexer-pattern validation from the 0.3 previews.
- Render parse-tree and AST debug dumps in O(n) using a shared `StringBuilder`, preserving the emitted text from 0.3.0-preview.2.

### Release changes

- Remove the prerelease suffix from package metadata; update the package description, discovery tags, README, and release notes.
- Verify engine tests, UI tests, NuGet packaging, and the public web build before publication. Every master push runs the automated release pipeline; pull requests and other branches run verification only.
- Document LALR as the supported parser, the remaining experimental variants, and the 0.x compatibility policy.

This release does not introduce a new parsing algorithm or promise the API compatibility of a future 1.0 release. The public API may evolve during 0.x, including introspection results and grammar-builder state. SLR has a known table-generation issue; CLR is incomplete. Grammar construction uses process-global state and must run one instance at a time. EBNF-text named regex tokens use word-pattern matching; the C# API supports raw-regex string/character literal tokens.

## Janglim Playground 1.0.0

The web application at [janglim.mythosia.net](https://janglim.mythosia.net/) provides a browser-local EBNF and LALR workspace. Its web version does not change the engine's API compatibility policy.

### Included capabilities

- React and shadcn/ui workspace with Monaco EBNF editing, light/dark themes, examples, and responsive layouts, connected to the existing C# WebAssembly engine.
- Interactive parse trees, searchable parsing tables, parser stacks, conflict details, and execution traces with step navigation and playback.
- Select a tree node to highlight the corresponding grammar symbols and consumed input, including repeated tokens at their actual source offsets.
- Fourteen interface languages: English, Korean, Japanese, Russian, Simplified Chinese, Traditional Chinese (Taiwan), Ukrainian, Vietnamese, German, French, Thai, Spanish, Portuguese, and Italian.
- Persisted explicit language choice, automatic browser-language matching, and English fallback. Chinese script and regional tags select the appropriate writing system.
- Preserve grammar, input, parse result, trace position, and selected node when switching interface languages.
- Use the shared language resolver for initial loading, page metadata, authored interface text, and accessibility messages.

### Release changes

- Replace the Preview badge with a version badge derived from the frontend package version.
- Set web project and frontend metadata to 1.0.0; add descriptions, keywords, and a canonical URL.
- Run language regression tests and engine tests before deployment; verify a public build that does not require licensed component-registry credentials.
- Restore two omitted bundled Tailwind dependency records in the lockfile so clean `npm ci` installs succeed without changing existing dependency versions.

The optional licensed loading animation is a local build enhancement. Public checkouts use the included loading indicator. Grammar symbols, parser action keywords, and original engine diagnostics retain their technical values. Some Monaco built-in commands remain in their original language. Large grammars and full traces can affect responsiveness; background parsing and cancellation remain future improvements.

## 0.3 preview history

- **0.3.0-preview.2:** O(n) parse-tree/AST debug rendering with unchanged output.
- **0.3.0-preview.1:** raw-regex string/character literal token types and lexer-pattern validation.

The feature lists above describe the release baseline; they do not claim that all capabilities were newly introduced in 0.3.0 or Playground 1.0.0.
