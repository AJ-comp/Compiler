param(
    [string]$ArtifactDirectory = "$PSScriptRoot/ci-master-playground",
    [string]$Commit = '1b8f85f13c063a855358a1d5bac3769fec2f84e0'
)

$ErrorActionPreference = 'Stop'
$artifactRoot = (Resolve-Path -LiteralPath $ArtifactDirectory).Path
$downloadDirectory = Join-Path $PSScriptRoot 'production-web-files'
New-Item -ItemType Directory -Path $downloadDirectory -Force | Out-Null
$paths = @('index.html', 'ui/playground.js', 'ui/style.css', 'ui/locale.js', 'licenses/Unicode-LICENSE.txt')
$paths += Get-ChildItem -LiteralPath (Join-Path $artifactRoot '_framework') -File |
    Where-Object { $_.Name -match '^(Parse\.|AJ\.Common\.|Janglim\.Playground\.).*\.wasm$|^blazor\.webassembly\..*\.js$' } |
    ForEach-Object { '_framework/' + $_.Name }

$results = foreach ($relativePath in $paths) {
    $expectedPath = Join-Path $artifactRoot $relativePath
    $downloadPath = Join-Path $downloadDirectory ($relativePath -replace '/', '__')
    $url = "https://janglim.mythosia.net/${relativePath}?release=$Commit"
    $response = Invoke-WebRequest -Uri $url -UseBasicParsing -OutFile $downloadPath -PassThru -TimeoutSec 30
    $expectedHash = (Get-FileHash -LiteralPath $expectedPath -Algorithm SHA256).Hash.ToLowerInvariant()
    $actualHash = (Get-FileHash -LiteralPath $downloadPath -Algorithm SHA256).Hash.ToLowerInvariant()
    [pscustomobject]@{
        Path = $relativePath
        Url = $url
        Status = [int]$response.StatusCode
        ContentType = [string]$response.Headers['Content-Type']
        ExpectedBytes = (Get-Item -LiteralPath $expectedPath).Length
        ActualBytes = (Get-Item -LiteralPath $downloadPath).Length
        ExpectedSha256 = $expectedHash
        ActualSha256 = $actualHash
        MatchesVerifiedArtifact = ($expectedHash -eq $actualHash)
    }
}

$report = [pscustomobject]@{
    VerifiedAtUtc = [DateTime]::UtcNow.ToString('o')
    Commit = $Commit
    WorkflowRun = 39531085294
    CheckedFiles = @($results).Count
    AllMatch = (@($results | Where-Object { -not $_.MatchesVerifiedArtifact }).Count -eq 0)
    Results = @($results)
}
$report | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'public-playground-verification.json') -Encoding UTF8
$report | Select-Object Commit, WorkflowRun, CheckedFiles, AllMatch | ConvertTo-Json -Compress
if (-not $report.AllMatch) {
    $results | Where-Object { -not $_.MatchesVerifiedArtifact } | Select-Object Path, ExpectedSha256, ActualSha256 | ConvertTo-Json
    exit 1
}
