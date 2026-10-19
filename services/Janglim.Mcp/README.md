# Janglim MCP

A .NET 10 ASP.NET Core service that exposes the Janglim LALR engine to Claude Code,
Codex, and other MCP clients through stateless Streamable HTTP.

| Tool | Purpose |
| --- | --- |
| `list_grammars` | List the built-in baselines, versions, scope, and SHA-256 fingerprints without their EBNF text. |
| `get_grammar` | Return one built-in EBNF baseline and its metadata; use `grammarId: "c11-core"`. |
| `validate_grammar` | Read EBNF, build the LALR table, and report grammar errors and table conflicts. |
| `parse_input` | Parse an input string and return positioned tokens, diagnostics, and an optional syntax tree. |
| `run_tests` | Check positive and negative examples against one EBNF grammar. |

The service accepts text; it does not read or write the caller's files, execute semantic
actions, or generate parser source code. AI reasoning and project edits remain in the MCP client.

## Built-in C baseline

The service embeds [`c11-core.ebnf`](../../grammars/c/c11-core.ebnf) at build time as the
`c11-core` baseline, version `1.0.0`. Its [documented scope](../../grammars/c/README.md)
excludes preprocessing, `typedef` name resolution, and semantic checks; it is not a full C compiler.

`list_grammars` returns `status` and a `grammars` metadata array. `get_grammar` returns
`status` and `preset`; `preset.grammar` is the EBNF text. Both include the baseline's
`id`, `name`, `version`, `language`, `standard`, `description`, `supportedFeatures`,
`limitations`, and `sha256`. An unknown `grammarId` returns `status: "unknown_grammar"`.
The returned EBNF uses LF line endings; `sha256` is the lowercase SHA-256 hex digest
of those exact UTF-8 bytes.

The built-in baseline is read-only. The client modifies a returned copy in its conversation
or local project and sends the copy's `grammar` text to `validate_grammar`, `parse_input`,
or `run_tests`. These requests cannot change the baseline or another user's copy.
The server does not persist user grammar copies or projects. Only a controlled repository
change and service release can update the built-in baseline.

## Remote service

The service is deployed to Azure Container Apps in Korea Central:

The deployed service includes `c11-core` version `1.0.0` and both preset lookup tools.
Remote checks verify its canonical hash, client-copy edits, and unchanged baseline retrieval.

- [MCP endpoint](https://janglim-mcp.wonderfulsand-6b5e00a1.koreacentral.azurecontainerapps.io/mcp)
- [Health endpoint](https://janglim-mcp.wonderfulsand-6b5e00a1.koreacentral.azurecontainerapps.io/health)

Set `JANGLIM_MCP_API_KEY` in the MCP client's environment to the operator-provided Bearer key.
Remote clients do not need the .NET SDK. See [the MCP guide](../../docs/MCP.md) for Codex and
Claude Code configuration, operator-only key retrieval, and cold-start timeouts.

## Run locally

From the repository root, with the .NET 10 SDK installed:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project services/Janglim.Mcp/Janglim.Mcp.csproj --no-launch-profile -- --urls http://localhost:5080
```

MCP endpoint: `http://localhost:5080/mcp`. Health endpoint: `http://localhost:5080/health`.
Development without an API key accepts only TCP connections from loopback.

## Container

Build from the repository root:

```shell
docker build -f services/Janglim.Mcp/Dockerfile -t janglim-mcp .
```

The container runs as a non-root user on port `8080` in Production. Set
`JanglimMcp__ApiKey` to a secret of 32–256 characters and set `AllowedHosts` to
the exact host names through which the service will be reached. Supply the secret at
runtime; never put it in the image, repository, or Playground frontend.

## Azure operations

[`infra/Deploy-Azure.ps1`](infra/Deploy-Azure.ps1) provisions the dedicated resources and
deploys an immutable image digest, preserving the existing API key on redeployment. The key is
submitted as an in-memory secure ARM parameter; the script writes no local credential file.
The app uses 1 CPU and 2 GiB per replica, with a minimum of 0 and maximum of 2 replicas.
The dedicated Basic container registry and log storage may incur costs while the app is scaled to zero.

[`scripts/smoke_remote.py`](scripts/smoke_remote.py) verifies the HTTPS service using
`JANGLIM_MCP_API_KEY` and prints only check results. The GitHub Actions release pipeline builds
and tests MCP but does not automatically redeploy this service. See
[the Korean MCP guide](../../docs/MCP.md) for the deployment and verification commands.
