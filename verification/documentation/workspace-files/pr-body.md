The Playground now saves the current EBNF grammar and one input string to a portable `.janglim.json` file. Opening it restores both editors and runs the existing parser again, so users can keep working on their own grammar across sessions.

Files use a tagged, versioned JSON format and stay on the device. Invalid, oversized, or unsupported files leave the current work intact; unsaved changes require confirmation before replacement. Import clears stale tree selection and parsing results. The File menu, feedback, and confirmation are translated in all 14 supported languages.

Web version is 1.1.0; the engine package remains 0.3.0. README and changelog describe the workflow. No code generation or parsing-algorithm changes are included.

Validation:
- Clean npm installation, 22 UI/file tests, strict TypeScript/Vite build, and public .NET publish passed.
- Real browser download/reopen restored the source and rebuilt a successful parse. Cancel/confirm, stale selection clearing, invalid JSON, unsupported versions, and keyboard focus were checked.
- 320px layout keeps the new File menu and controls within the viewport.
