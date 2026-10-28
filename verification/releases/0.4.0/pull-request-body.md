Janglim grammars can now inspect source trivia and use registered C# conditions at token boundaries. This release adds targeted conflict prediction and rejects zero-input grammar paths before parser construction, and packages the engine as **0.4.0**.

## Changes

- Add typed EBNF tokens, exact leading trivia and line-break information, and pure host-registered `{Name()}?` predicates.
- Predict the first action for an original two-way Shift / predicate epsilon Reduce conflict, with explicit ambiguity, cycle, failure, and resource-limit diagnostics. Keep the existing policy for other conflicts.
- Validate nullable repetition and predicate-bearing zero-token left recursion; preserve ordinary LR left recursion and token-consuming repetition. Include focused regression tests and HTML behavior documentation.
- Add the isolated remote MCP service, its five tools, and the read-only, versioned C11 Core grammar catalog with regression examples. Include MCP tests and publish/container builds in release verification.
- Update package descriptions, discovery tags, READMEs, release notes, changelog, and translated version references. Preserve the original author's design and implementation attribution.

Registered callbacks and predicate conflict prediction are .NET host features. The current web and MCP readers use default options. Parser-driven contextual lexing and complete JS/TS grammars remain separate design work.

## Local validation

- Default solution Debug and Release builds: passed, zero errors; five existing unused-field/event warnings in each build.
- Parser tests: 692 passed; one existing SLR known-issue test skipped.
- MCP protocol and worker tests: 51 passed.
- UI regression tests: 22 passed. Release infrastructure tests: 12 passed.
- Public frontend build, Playground publish build, and MCP publish build: passed.
- NuGet package: metadata and README verified; all nine engine assemblies are version 0.4.0.0, with 40 localized resource assemblies.
- Independent net8.0 package consumer: README examples, typed tokens, Reject handling, and exact comment/newline trivia passed.
- Local container build was unavailable because Docker Desktop's Linux engine was stopped; the PR workflow performs this check on Linux.

## Release behavior

Merging into `master` starts the existing verified-artifact release workflow: publish Janglim 0.4.0 to NuGet, then deploy the verified Playground artifact. Playground remains independently versioned at 1.1.0. MCP remains 0.1.0 and its production deployment stays separate.
