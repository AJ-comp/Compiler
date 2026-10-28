$ErrorActionPreference = 'Stop'
$taskDir = Join-Path $PSScriptRoot 'public-nuget'
$taskDlls = Get-ChildItem -LiteralPath $taskDir -Filter '*.dll'
$taskVersions = @($taskDlls | ForEach-Object {
    $taskName = [Reflection.AssemblyName]::GetAssemblyName($_.FullName)
    [pscustomobject]@{ name = $_.Name; version = $taskName.Version.ToString(); passed = $taskName.Version.ToString() -eq '0.6.0.0' }
})
$taskAssembly = [Reflection.Assembly]::LoadFile((Join-Path $taskDir 'Parse.FrontEnd.Grammars.Ebnf.dll'))
$taskResources = @($taskAssembly.GetManifestResourceNames() | Where-Object { $_.EndsWith('.ebnf') } | Sort-Object)
$taskReport = [pscustomobject]@{
    engineAssemblyCount = $taskVersions.Count
    assemblies = $taskVersions
    ebnfResourceCount = $taskResources.Count
    resources = $taskResources
    passed = $taskVersions.Count -eq 9 -and ($taskVersions | Where-Object { -not $_.passed }).Count -eq 0 -and $taskResources.Count -eq 8
}
$taskReport | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $taskDir 'assembly-verification.json') -Encoding utf8
$taskReport | ConvertTo-Json -Depth 8
if (-not $taskReport.passed) { exit 1 }
