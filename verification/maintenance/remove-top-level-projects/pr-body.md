Remove the unused top-level AJStudio WPF application and CommandPrompt.Compiler CLI. Both project builds failed, and neither had incoming project references or calls from build scripts, workflows, or other source. Delete their source files and remove their legacy solution entries, build configurations, and folder mappings.

The default 11-project solution is unchanged. The historical solution now contains 28 projects; the repository contains 31 C# projects. Documentation describes the removal and preserves the original author's design and source history in Git. Remaining projects and package/web versions are unchanged.

Validation:
- Root Debug and Release builds passed with zero errors.
- Both removed source directories are absent, including local generated files.
- All remaining solution paths resolve; no new dangling references were introduced.
- The legacy Release build still fails with the five existing Common namespace/WpfControls XAML errors, as expected.
- git diff --check passed.
