# Release history

The engine, web application, MCP service, and built-in grammar baselines use independent version numbers. The entries below describe each version's scope and changes.

## Janglim engine 0.5.0 — 2026-10-21

Engine 0.5.0 adds parser-driven lexical input, longest-match token selection and ECMAScript 2026-based JavaScript language APIs. The package still targets .NET Standard 2.1; the web Playground remains 1.1.0 and MCP remains 0.1.0.

### Parser-driven lexing

- Add LexicalGoal rule groups, LexicalPolicy selection, ITokenSource, TokenCursor and TokenRead. Create a policy from rule groups, prepare a source with Lexer.CreateTokenSource(input, policy), then call LALRParser.Parsing(source).
- Pass parsing context to the policy; pass only the returned rule group and input checkpoint to the source. Peek returns a token and its Next checkpoint, while the actual cursor advances only on Shift.
- Preserve source trivia, previous-token context, lexical state and separate cursor/history/lookahead values for prediction candidates, including candidates that read different token boundaries.
- Keep the existing token-list input API. Source-based parsing stops at the first error and explicitly rejects custom error recovery and general conflict backtracking settings.

### Token selection and compatibility

- Share longest returned match selection across Lexer.Lexing, incremental tokenization and parser-driven sources. Equal-length matches follow token-category priority and registration order, including skip rules.
- Remove automatic word-boundary wrapping from token regexes. Grammars that relied on implicit \b must author the required boundary explicitly.
- Preserve each regex's .NET alternation and greedy/lazy semantics. The existing single combined-regex Tokenizer.Tokenize(string, ...) path retains ordered alternatives.
- In the registered-rule lexing paths, diagnose empty matches with EmptyTokenMatchException and apply a one-second limit per compiled token regex. MCP reports empty-token matches as invalid_grammar/zero_length_token and regex timeouts as runtime_limit.

### JavaScript language layer

- Add JavaScriptExpressionGrammar, JavaScriptScriptGrammar and JavaScriptModuleGrammar in Janglim.FrontEnd.Grammars.JavaScript. Consumers check Parsing success and Validate diagnostics together.
- Embed six EBNF sources and Unicode 17 identifier data; include the Unicode data license in the NuGet package.
- Support expressions, strict Script statements/declarations, Module syntax, contextual slash handling, templates, destructuring and bindings, ordinary/async/arrow functions, generators/async generators, object/class methods and accessors, classes and private names.
- Include ASI, import/export and attributes, dynamic import, import.meta, top-level await and for-await-of with function/body context checks.
- Validate regular-expression pattern bodies in ordinary/u/v modes on the final tree, plus lexical flags, declaration scope, parameters, assignment targets and other documented early errors.
- Keep non-strict Script and full specification/Test262 syntax conformance as follow-up work. MCP's built-in grammar catalog remains c11-core.

### Documentation and release preparation

- Update package description, discovery tags, package/repository READMEs, release notes and manual installation versions to 0.5.0.
- Add Korean implementation guides for parser-driven lexing and JavaScript expressions, statements, classes, modules, function kinds and regular expressions. Explain cursor fields, ownership and Peek results in three coordinated source-position diagrams.
- Preserve the human author's engine-design attribution and distinguish the AI-assisted JavaScript language layer and documentation.
- Validate the versioned source and package before pushing. Publication still requires master: feature-branch pushes and PRs run verification only; master verification feeds NuGet publication and Playground deployment.

Local validation on 2026-10-21 passed 4,976 engine tests (one known SLR test skipped), 69 MCP tests, 22 UI tests and 12 release-infrastructure tests. Debug/Release solution builds, public Playground/MCP publish builds, packaging and DocFX succeeded. A separate net8.0 package consumer passed 80 checks, and both JavaScript README examples ran unchanged. Local Docker was inactive; container verification remains required in CI. See [release notes](RELEASE_NOTES.md) for artifacts and details.

## Janglim engine 0.4.0 — 2026-10-19

Engine `0.4.0` adds EBNF token kinds, preserved source trivia, host-registered predicates, and prediction for a specific predicate conflict. The engine continues to target .NET Standard 2.1, with `LALRParser` as its supported parsing path.

### EBNF and lexer APIs

- Add `Identifier`, `StringLiteral`, `CharLiteral`, `NumberLiteral`, `Comment`, and `Reject` token kinds to EBNF-text definitions. Existing definitions without a kind retain `Identifier` behavior.
- Use raw regexes for literal/number token kinds, skip declared comments, and expose `EbnfGrammar.IsRejectedToken()` for lexical rejection checks. The MCP adapter applies those checks before accepting input.
- Preserve skipped source text and UTF-16 offsets in `TokenCell.LeadingTrivia` and `TokenData.LeadingTrivia`; expose `HasLineBreakBefore` and `LexingData.TrailingTrivia`.
- Recognize CR, LF, U+2028, and U+2029 in leading trivia, including line breaks inside comments. Rebuild trivia after incremental edits so whitespace-only or comment-only edits update predicate context.

### Host-registered predicates and conflict prediction

- Register pure C# conditions through `EbnfReaderOptions.RegisterPredicate()` and call them from EBNF as `{Name()}?`. Conditions receive the previous token, next token, and input index through `PredicateContext`.
- Evaluate conditions at a zero-token grammar position. False conditions exclude that path; missing registrations, malformed calls, and callback exceptions produce explicit diagnostics.
- Predict only when the original table cell contains exactly one Shift and one predicate-bearing epsilon Reduce, and both remain viable after condition evaluation. Other conflicts retain their existing policy.
- Copy prediction state and advance lookahead until one initial action survives. Return that original action to the actual parser without committing prediction stacks or input positions.
- Report ambiguity when multiple initial actions accept the same complete input. Bound stack depth, configurations, and transitions independently of lookahead length, and report exhausted limits instead of choosing an action.
- Keep callback registration and this prediction API in the .NET host workflow. Playground 1.1.0 and MCP 0.1.0 read EBNF with default options; MCP parsing and example checks require conflict-free tables.

### Grammar validation

- Reject `*` and `+` when the repeated body can finish without consuming a token; retain repetitions that require token consumption.
- Add `PredicateLeftRecursionValidation` and `PredicateLeftRecursionException` to reject a predicate-bearing path that re-enters the same nonterminal before consuming input. Preserve ordinary LR left recursion, predicates after recursive entry, and token-consuming recursive paths.
- Analyze indirect references, nullable prefixes, and predicate information on the same empty path. Include the rule name, predicate name, and closed rule path in diagnostics, checking unused named rules as well.
- Run structural validation before normalization and table construction, without invoking registered conditions. Link the class summary to its [HTML behavior document](docs/ko/predicate-left-recursion-validation.html).

### Repository, C11 Core, and MCP scope

- Include the [C11 Core syntax profile and regression examples](grammars/c/README.md). The `c11-core` baseline is version `1.0.0` and covers documented declarations, functions, expressions, statements, and literals with ASCII identifiers. Its documented extension areas include preprocessing, typedef-name resolution, semantic checking, selected C11 constructs, and compiler extensions.
- Expose `list_grammars`, `get_grammar`, `validate_grammar`, `parse_input`, and `run_tests` through the independently versioned MCP `0.1.0` service over stateless Streamable HTTP.
- Embed the read-only baseline with its version, scope, and SHA-256 of LF-normalized UTF-8 text. Clients edit returned copies; requests cannot replace the catalog or persist user grammar projects.
- Execute engine jobs in separate worker processes to isolate grammar-builder globals. Enforce request/output, concurrency, and execution limits; terminate work on timeout or cancellation.
- Include engine and MCP tests, MCP publish/container builds, NuGet packaging, and public Playground builds in release verification. The release workflow publishes verified NuGet and Playground artifacts; MCP production deployment uses its dedicated Azure script.

### Compatibility and verification

The Playground remains at `1.1.0`; the MCP service remains at `0.1.0`. Their versions are independent of the engine package. Public APIs may evolve during `0.x`; pin versions and review changes when upgrading. SLR retains a known table-generation defect and CLR remains incomplete. In-process grammar construction uses global builder state and must run sequentially.

Grammar preparation now rejects non-consuming `*`/`+` bodies and predicate-bearing zero-token left recursion. Review token consumption in existing repetition bodies and recursive prefixes when upgrading.

Local validation on 2026-10-19 with the 0.4.0 metadata passed: 692 engine tests with one known SLR test skipped, 51 MCP tests, 22 UI tests, and 12 release-infrastructure tests. Debug and Release solution builds and local `dotnet publish` builds for the public Playground and MCP server succeeded. Both solution configurations retain five existing unused-field/event warnings and zero errors.

The package at `artifacts/release-0.4.0/packages/Janglim.0.4.0.nupkg` includes nine engine DLLs at version `0.4.0.0` and 40 localized resource DLLs. Package metadata and the embedded README were checked. A separate `net8.0` consumer installed the local package and verified the README quickstart, true/false predicates, five typed-token inputs, lexical rejection, and exact comment/newline trivia. The local container build was not run because the Docker engine was inactive; container verification remains a CI step.

Check the corresponding [Actions run](https://github.com/AJ-comp/Compiler/actions) for package publication and deployment results. Detailed local validation results and separate design work are recorded in [RELEASE_NOTES.md](RELEASE_NOTES.md).

Janglim's core structure remains based on the human author's design and implementation. AI-assisted documentation, UI, release preparation, and separately authored grammar examples preserve that authorship context.

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
