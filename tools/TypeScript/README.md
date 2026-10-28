# TypeScript grammar reference and development oracles

The implementation target is **TypeScript 7.0.2**, the latest stable release
verified on **2026-10-21**. The `7.1.0-beta` release from October 20 is excluded.
The [official 7.0 announcement](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0/)
describes the native port and explicitly says that 7.0 does not expose a compiler
API. Its release parser/scanner are therefore the authoritative implementation
reference for this work. The older TypeScript language specification is not a
complete specification of the current language.

`baseline.json` pins the original native source, release archives, original test
corpus submodule, older comparison API and optional portable Go toolchain by
revision and SHA-256. All dependencies remain development tools under ignored
`artifacts/typescript`; Janglim does not depend on these programs at runtime.

## Reproduce the parser oracle

With Go 1.26.5 available:

```powershell
python tools/TypeScript/prepare_oracles.py `
  --go C:/path/to/go1.26.5/bin/go.exe --include-corpus
```

The preparation script checks archive bytes before extraction, retains the
upstream licenses, builds the adapter inside the pinned Go module, and writes
`artifacts/typescript/oracle-provenance.json`. It does not expand the upstream
baseline output directory, which contains tens of thousands of unrelated files.

The adapter reads one JSON object per line and writes one result per line:

```json
{"id":"annotation","kind":"ts","source":"let count: number = 1;"}
{"id":"tsx","kind":"tsx","source":"const element = <div>{value}</div>;"}
{"id":"ambient","kind":"dts","source":"declare const version: string;"}
```

Supported input kinds are `ts`, `tsx`, `dts`, `declaration`, `mts`, and `cts`.
`fileName` can explicitly supply an upstream virtual filename. Source hashes
cover the UTF-8 bytes supplied to the native parser. Native diagnostic offsets
are **UTF-8 byte offsets**; the older JavaScript API uses **UTF-16 code units**.
Offsets must be normalized before comparing non-ASCII diagnostic locations.

## Keep parser, grammar, and type errors separate

The native adapter returns **parser diagnostics only**. It does not evaluate
input, resolve imports, run the type checker, or emit code.

For example:

```typescript
let count: = 1;                              // Parser error: type expected.
function f(x?: string, y: number) {}          // Grammar error: required after optional.
let name: string = 1;                        // Type assignment error.
```

Only the first example is rejected by `parser.ParseSourceFile(...).Diagnostics()`.
The second is reported by the native compiler's checker as grammar error 1016.
The third requires type checking. Consequently, a zero parser-diagnostic count
does not by itself establish that all grammar restrictions passed.

The compiler's `internal/checker/grammarchecks.go` also contains checks that
consult inferred types, compiler options, or emit targets. Neither that file's
diagnostic names nor all `tsc` errors should be treated as a blanket syntax-error
whitelist. Each grammar restriction needs an explicit source-based classification.

`typescript6-parse-oracle.cjs` provides a **comparison** adapter for the pinned
6.0.3 JavaScript API:

```powershell
Get-Content cases.jsonl | node tools/TypeScript/typescript6-parse-oracle.cjs `
  artifacts/typescript/typescript6/package/lib/typescript.js
```

It cannot replace the 7.0.2 reference. A verified difference is:

```typescript
let f: function(string): void;
```

The 6.0.3 parser accepts this Closure-style function type without parser
diagnostics; the 7.0.2 parser rejects it. The official native `CHANGES.md` records
the removal of these Closure/JSDoc forms from ordinary type annotations.

## Official corpus

The native release pins its `_submodules/TypeScript` corpus to
`4d4f005c8541e0255a9d8791205fdce326e462bc`. That revision contains:

| Directory | Test files |
| --- | ---: |
| `tests/cases/compiler` | 6,537 |
| `tests/cases/conformance` | 5,908 |
| `tests/cases/fourslash` | 6,568 |

The native release archive also contains 317 compiler cases under
`testdata/tests/cases/compiler`. These include native additions and overrides.
Cases must be merged by their relative test path, with native overrides taking
precedence, rather than blindly adding both archive counts.

Compiler/conformance files are not all successful programs. They include expected
errors, compiler-option directives and multi-file virtual projects (`@Filename`).
A corpus runner must split virtual files, preserve their extensions and source
bytes, and distinguish parser/grammar errors from type-checking and resolution
errors. Fourslash files describe editor-service scenarios and need their own
harness; they are not source files to feed directly to a language parser.

## Syntax boundaries that affect the language layer

- `.ts` angle-bracket assertions and `.tsx` JSX are different parse modes.
  A generic arrow such as `<T,>(x: T) => x` disambiguates its type parameters in TSX;
  an ordinary `<T>(x: T) => x` can be read as JSX there.
- Declaration files retain ambient grammar restrictions. Parsing `.d.ts` as an
  ordinary `.ts` file loses that context.
- Standard decorators and legacy `experimentalDecorators` share token spellings
  but differ in allowed positions, including parameter decorators.
- The native scanner exposes rescan operations for `<`, `>`, slash tokens,
  template continuations and JSX text/attributes. In particular, nested generic
  closers must be distinguishable from shift operators such as `>>` and `>>>`.
  These are language input-policy decisions, not a reason to change Janglim's core.
- TypeScript 7.0.2 includes TypeScript-supported proposal syntax such as `using`.
  Its acceptance set must be checked against the TypeScript release, rather than
  inferred only from the underlying ECMAScript 2026 grammar.
- The new `declare module "*" with { ... }` and source-phase import forms described
  by [7.1 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-1-beta/)
  belong to the excluded prerelease baseline.
