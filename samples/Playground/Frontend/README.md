# Janglim Playground UI

Playground 1.0.0 is the web release alongside Janglim engine 0.3.0. The web and NuGet versions are independent. See the [release history](../../../CHANGELOG.md) and [release notes](../../../RELEASE_NOTES.md) for scope and limitations.

React and shadcn/ui present the existing browser-local Blazor WebAssembly engine. `Home.razor` owns the host and exposes `Parse`; no parser implementation is duplicated in JavaScript. A completed run opens its final trace step. The tree, stack, ACTION/GOTO table, conflicts and execution trace all come from the same C# result.

From the repository root:

```powershell
dotnet run --project samples/Playground/Janglim.Playground.csproj --launch-profile http
```

The project automatically restores and builds the UI. Node.js 22.18+ is required to run the TypeScript-based language tests. The generated files in `wwwroot/ui` are ignored and rebuilt for publishing.

Run `npm test` in this directory before publishing. The release verification workflow also runs the engine tests, packs the NuGet artifact, and publishes the public web build. Deploy and NuGet publish workflows run the relevant tests before changing their destinations.

## Interface languages

The interface supports English, Korean, Japanese, Russian, Simplified Chinese, Traditional Chinese (Taiwan), Ukrainian, Vietnamese, German, French, Thai, Spanish, Portuguese, and Italian. An explicit choice in the header is stored in `localStorage` under `janglim-language` and takes precedence over browser preferences. Automatic mode follows the first supported language in `navigator.languages`, including regional variants, with English as the fallback. Chinese script tags take precedence: `zh-Hant` selects Traditional Chinese and `zh-Hans` selects Simplified Chinese. Without a script tag, Taiwan, Hong Kong, and Macao regional tags select Traditional Chinese; other Chinese tags select Simplified Chinese. When storage is unavailable, a manual choice remains active for the current page.

Changing the interface language preserves the editor contents, parsing result, trace position, and selected tree node. Authored interface and accessibility messages are translated; grammar symbols and original parser diagnostics remain intact. The initial loading frame uses the same language resolver as React.

## UI sources and optional licensed animation

- The workspace adapts the free [Shadcnblocks Application Shell 1](https://www.shadcnblocks.com/block/application-shell1), with Janglim navigation and content.
- Controls use the official MIT-licensed shadcn/ui components.
- A private React Bits animation can be used from `.licensed/preloader.tsx`. This folder is ignored because [React Bits license terms](https://pro.reactbits.dev/license) restrict publishing component source in public repositories. Public checkouts build with the included loading indicator and retain all parser features.

The existing Azure key is in `kv-cmpbx-d147bfc2df`, secret `react-bits-license-key`. Its Starter entitlement includes animated components; Pro application blocks require a different entitlement. Keys are never runtime UI dependencies and must never use a `VITE_` environment variable.

To download the optional animation into this developer's local checkout using their existing Azure CLI sign-in:

```powershell
powershell -NoProfile -File samples/Playground/Frontend/scripts/install-reactbits.ps1
```

Use `PLAYGROUND_PUBLIC_BUILD=1` when explicitly validating the public fallback. The Azure CI build works without component-registry credentials. Generated bundles may be used as an application output; provider component source remains private.
