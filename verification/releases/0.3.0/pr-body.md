This releases Janglim 0.3.0 without a prerelease suffix and introduces Playground 1.0.0 at https://janglim.mythosia.net/. The engine keeps the existing LALR implementation and its 0.x API policy; SLR and CLR remain experimental.

The Playground now uses a React and shadcn/ui workspace connected to the C# WebAssembly engine. It includes interactive tree/source highlighting, table and trace inspection, playback, responsive layouts, themes, and 14 interface languages with saved explicit choice, browser-language matching, and English fallback. Package/project descriptions, keywords, installation guides, README, changelog, and release notes are updated while preserving human-author attribution.

Merging this PR activates the existing master-push pipeline: verify and pack, publish the new engine version through NuGet Trusted Publishing, then deploy the verified static-site artifact to Azure. Licensed sources and credentials are excluded; public CI uses the included loading indicator.

Validation completed before this PR:
- Engine Release tests: 140 passed, one existing SLR issue skipped.
- Language regression tests: 15 passed, including Node.js 22.18.0.
- Clean dependency installation, strict TypeScript/Vite public build, and .NET web publish passed.
- Packaged NuGet metadata and bundled engine assemblies checked; the README example ran successfully against the actual package in a separate project.
- Browser verification of version display, successful parsing, and linked grammar/input highlighting passed.

The complete committed sources passed [PR verification on Linux](https://github.com/AJ-comp/Compiler/actions/runs/38076730430): engine 140 passed / one existing skip, UI 15 passed, helper 12 passed, clean dependency installation, strict TypeScript/Vite public build, .NET web publish, and Janglim 0.3.0 packaging with verified SHA-256. The branch-push verification also passed.
