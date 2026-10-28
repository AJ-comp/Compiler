The default solution included outdated WPF/IDE and CLI projects, so a root solution build failed. AJPGS.sln now contains the nine engine/package projects, parser tests, and Playground. The previous 30-project graph is preserved in legacy/AJPGS.Legacy.sln with rebased paths; source files and project files remain unchanged.

Release verification now builds the default solution in both Debug and Release before the existing tests, NuGet packaging, and Playground publishing. README and release/legacy notes document prerequisites, public builds, and the historical solution's remaining build failures. Package and web versions remain unchanged.

Validation:
- Root dotnet build (Debug) and dotnet build -c Release passed with zero errors.
- Parser tests: 140 passed, one existing SLR skip.
- UI tests: 22 passed.
- actionlint and git diff --check passed.
- All solution project paths resolve; the legacy archive retains the original project GUIDs, configuration mappings, and folder relationships.
