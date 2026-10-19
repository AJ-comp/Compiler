[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[0-9a-fA-F-]{36}$')]
    [string] $SubscriptionId,
    [ValidatePattern('^[a-zA-Z0-9_-]+$')]
    [string] $ResourceGroup = 'janglim-mcp-rg',
    [string] $Location = 'koreacentral',
    [ValidatePattern('^[a-z][a-z0-9-]*[a-z0-9]$')]
    [string] $AppName = 'janglim-mcp',
    [string] $LocalImage = 'janglim-mcp:local',
    [switch] $UseExistingImage
)

$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$workDirectory = Join-Path $repoRoot 'services/Janglim.Mcp/obj/azure-deploy'
[IO.Directory]::CreateDirectory($workDirectory) | Out-Null
$apiKey = $null
$accessToken = $null
$deploymentBody = $null

function Invoke-Azure {
    param([string[]] $Arguments)
    $result = & az @Arguments --subscription $SubscriptionId --only-show-errors
    if ($LASTEXITCODE -ne 0) { throw 'Azure operation failed; see the CLI diagnostic above.' }
    return $result
}

function Assert-NativeSuccess {
    if ($LASTEXITCODE -ne 0) { throw 'Build or registry operation failed.' }
}

try {
    foreach ($template in @('foundation', 'app')) {
        & az bicep build --file (Join-Path $PSScriptRoot "$template.bicep") --outfile (Join-Path $workDirectory "$template.json") --only-show-errors
        Assert-NativeSuccess
    }
    Invoke-Azure -Arguments @('group', 'create', '--name', $ResourceGroup, '--location', $Location, '--tags', 'project=Janglim', 'component=MCP', '--output', 'none')
    $foundationJson = Invoke-Azure -Arguments @('deployment', 'group', 'create', '--resource-group', $ResourceGroup, '--name', 'janglim-mcp-foundation', '--template-file', (Join-Path $workDirectory 'foundation.json'), '--query', 'properties.outputs', '--output', 'json')
    $foundation = ($foundationJson -join "`n") | ConvertFrom-Json
    $registryName = $foundation.registryName.value
    $registryServer = $foundation.registryServer.value

    if (-not $UseExistingImage) {
        & docker build --file (Join-Path $repoRoot 'services/Janglim.Mcp/Dockerfile') --tag $LocalImage $repoRoot
        Assert-NativeSuccess
    }
    Invoke-Azure -Arguments @('acr', 'login', '--name', $registryName)
    $imageTag = [DateTime]::UtcNow.ToString('yyyyMMddHHmmssfff')
    $remoteImage = "${registryServer}/janglim/mcp:${imageTag}"
    & docker tag $LocalImage $remoteImage
    Assert-NativeSuccess
    & docker push $remoteImage
    Assert-NativeSuccess
    $digest = (Invoke-Azure -Arguments @('acr', 'repository', 'show', '--name', $registryName, '--image', "janglim/mcp:${imageTag}", '--query', 'digest', '--output', 'tsv')).Trim()
    $immutableImage = "${registryServer}/janglim/mcp@${digest}"

    # Preserve the existing client credential on redeployment. No credential is
    # printed or passed in a command-line argument, image, or template output.
    $existingApp = Invoke-Azure -Arguments @('containerapp', 'list', '--resource-group', $ResourceGroup, '--query', "[?name=='$AppName'].name | [0]", '--output', 'tsv')
    if ($existingApp) {
        $apiKey = (Invoke-Azure -Arguments @('containerapp', 'secret', 'list', '--name', $AppName, '--resource-group', $ResourceGroup, '--show-values', '--query', "[?name=='mcp-api-key'].value | [0]", '--output', 'tsv')).Trim()
        if (-not $apiKey) { throw 'The existing app has no mcp-api-key; restore it before redeploying.' }
    } else {
        $randomBytes = New-Object byte[] 48
        $random = [Security.Cryptography.RandomNumberGenerator]::Create()
        try { $random.GetBytes($randomBytes) } finally { $random.Dispose() }
        $apiKey = [Convert]::ToBase64String($randomBytes)
        [Array]::Clear($randomBytes, 0, $randomBytes.Length)
    }

    # Submit the secure ARM parameter in memory. No temporary credential file,
    # command-line credential, or credential-bearing deployment output exists.
    $deploymentRequest = @{
        properties = @{
            mode = 'Incremental'
            template = ([IO.File]::ReadAllText((Join-Path $workDirectory 'app.json')) | ConvertFrom-Json)
            parameters = @{
                appName = @{ value = $AppName }
                image = @{ value = $immutableImage }
                apiKey = @{ value = $apiKey }
            }
        }
    }
    $deploymentBody = $deploymentRequest | ConvertTo-Json -Depth 100 -Compress
    $accessToken = (Invoke-Azure -Arguments @('account', 'get-access-token', '--resource', 'https://management.azure.com/', '--query', 'accessToken', '--output', 'tsv')).Trim()
    $deploymentUrl = "https://management.azure.com/subscriptions/$SubscriptionId/resourcegroups/$ResourceGroup/providers/Microsoft.Resources/deployments/janglim-mcp-app?api-version=2022-09-01"
    try {
        Invoke-RestMethod -Method Put -Uri $deploymentUrl -Headers @{ Authorization = "Bearer $accessToken" } -ContentType 'application/json' -Body $deploymentBody -TimeoutSec 90 | Out-Null
    } catch {
        throw 'ARM rejected the app deployment. Inspect its deployment status in Azure; credentials are omitted from this diagnostic.'
    }
    $deploymentRequest = $null
    $deploymentBody = $null
    $accessToken = $null
    Invoke-Azure -Arguments @('deployment', 'group', 'wait', '--resource-group', $ResourceGroup, '--name', 'janglim-mcp-app', '--created', '--interval', '10', '--timeout', '900')
    $outputsJson = Invoke-Azure -Arguments @('deployment', 'group', 'show', '--resource-group', $ResourceGroup, '--name', 'janglim-mcp-app', '--query', 'properties.outputs', '--output', 'json')
    $outputs = ($outputsJson -join "`n") | ConvertFrom-Json
    $deployment = [ordered]@{
        resourceGroup = $ResourceGroup
        appName = $AppName
        registryName = $registryName
        image = $immutableImage
        mcpUrl = $outputs.mcpUrl.value
        healthUrl = $outputs.healthUrl.value
        secretName = 'mcp-api-key'
        deployedAtUtc = [DateTime]::UtcNow.ToString('o')
    }
    $deploymentJson = $deployment | ConvertTo-Json
    [IO.File]::WriteAllText((Join-Path $workDirectory 'deployment.json'), $deploymentJson)
    $deploymentJson
} finally {
    $apiKey = $null
    $accessToken = $null
    $deploymentBody = $null
    $deploymentRequest = $null
}
