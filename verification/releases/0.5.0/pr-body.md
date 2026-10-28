Janglim 0.5.0 adds contextual token requests so a lexical policy can choose the rule group before the source reads the next token. Registered-rule lexing now chooses the longest returned match, with stable equal-length priority and explicit regex boundaries.

The JavaScript language layer provides expression, strict Script and Module entry points for ECMAScript 2026-based syntax, including templates, destructuring, functions/async/generators, object methods, classes, modules and regular-expression body validation. Consumers must check both Parsing and Validate results. Non-strict syntax and full specification conformance remain follow-up work.

Include all six JavaScript grammar sources in the MCP container build context and ship the Unicode notice with package, MCP and web output.

Update package description/tags, repository and package READMEs, release notes, changelog and manual installation versions. Add implementation guides and coordinated cursor diagrams, preserving the human author's engine-design attribution.

Validation:
- Debug and Release solution builds passed; 4,976 engine tests passed with one known SLR issue skipped.
- 69 MCP, 22 UI and 12 release-infrastructure tests passed.
- Parallel-validator checks use a captured completed tree, retaining worker counts, repetitions, diagnostics and token-identity assertions without concurrently extracting the legacy parser-result stack.
- Public Playground and MCP publish builds, NuGet packaging and DocFX succeeded.
- An independent net8.0 package consumer passed 80 checks; both JavaScript README examples ran unchanged.
- Package checks confirmed six EBNF resources, Unicode licensing, nine engine DLLs at 0.5.0.0 and 40 localized DLLs.
- Local Docker was inactive; the required container build runs in CI.

Compatibility: token regexes no longer receive implicit word boundaries. Review patterns that relied on the old boundary/shorter-match behavior. Source-based parsing stops at the first error and uses the existing token-list path for custom recovery/general backtracking.

Engine 0.5.0 is independent of Playground 1.1.0 and MCP 0.1.0. Merging to master triggers verification, NuGet publication and Playground deployment. Manual changes deploy through the separate Pages workflow; the remote MCP retains its dedicated deployment procedure.

