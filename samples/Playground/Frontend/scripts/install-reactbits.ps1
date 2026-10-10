param([string]$VaultName = 'kv-cmpbx-d147bfc2df')
$ErrorActionPreference = 'Stop'
$taskKey = $null
try {
    $taskKey = az keyvault secret show --vault-name $VaultName --name react-bits-license-key --query value --output tsv
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($taskKey)) { throw 'Unable to read the React Bits key from Azure.' }
    $taskRegistry = Invoke-RestMethod -Uri 'https://pro.reactbits.dev/api/r/starter/preloader-tw.json' -Headers @{ Authorization = "Bearer $taskKey" }
    $taskSource = $taskRegistry.files | Where-Object { $_.path -match 'preloader\.tsx$' } | Select-Object -First 1
    if (-not $taskSource -or -not $taskSource.content) { throw 'The official registry did not return a preloader component.' }
    $taskDestination = Join-Path (Split-Path $PSScriptRoot -Parent) '.licensed'
    New-Item -ItemType Directory -Path $taskDestination -Force | Out-Null
    '*' | Set-Content -LiteralPath (Join-Path $taskDestination '.gitignore') -Encoding utf8
    $taskContent = $taskSource.content.Replace('NodeJS.Timeout', 'ReturnType<typeof setTimeout>')
    $taskContent | Set-Content -LiteralPath (Join-Path $taskDestination 'preloader.tsx') -Encoding utf8
    $taskRegistry | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath (Join-Path $taskDestination 'preloader.registry.json') -Encoding utf8
    Write-Host 'Installed the authorized React Bits animation into the ignored .licensed directory.'
} finally { $taskKey = $null }
