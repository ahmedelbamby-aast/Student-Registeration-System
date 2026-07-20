[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$baselineRoot = Join-Path $PSScriptRoot 'Baselines'
$globalManifestPath = Join-Path $baselineRoot 'baseline-manifest.json'
$approvedOn = '2026-07-19'
$approvedBy = 'Ahmed ELbamby'
$currentSpec003Routes = @(
    'STU-02', 'STU-03', 'STU-04', 'STU-05', 'STU-06', 'STU-07',
    'ADM-01', 'ADM-05', 'ADM-06', 'ADM-07', 'ADM-08', 'ADM-09',
    'STF-01', 'STF-02', 'STF-03', 'STF-04'
)

$browserMetadata = @{
    chrome = @{
        browserName = 'Google Chrome Stable'
        browserBuild = '150.0.7871.125'
        engine = 'Blink/Chromium 150.0.7871.125'
    }
    edge = @{
        browserName = 'Microsoft Edge Stable'
        browserBuild = '150.0.4078.83'
        engine = 'Blink/Chromium 150 (Edge 150.0.4078.83)'
    }
    firefox = @{
        browserName = 'Playwright Firefox 151.0 visual capture; Mozilla Firefox 152.0.6 current-stable smoke'
        browserBuild = '151.0 visual; 152.0.6 stable smoke'
        engine = 'Gecko 151 visual; Gecko 152.0.6 stable smoke'
    }
    webkit = @{
        browserName = 'Playwright WebKit (not Safari)'
        browserBuild = 'WebKit 26.5 via Microsoft.Playwright 1.61.0'
        engine = 'WebKit 26.5 bundled by Microsoft.Playwright 1.61.0'
    }
}

$globalBrowserName = 'Google Chrome Stable; Microsoft Edge Stable; Playwright Firefox 151.0 visual capture + Mozilla Firefox 152.0.6 current-stable smoke; Playwright WebKit (not Safari)'
$globalBrowserBuild = '150.0.7871.125; 150.0.4078.83; 151.0 visual / 152.0.6 stable smoke; WebKit 26.5 via Microsoft.Playwright 1.61.0'
$globalEngine = 'Blink/Chromium 150.0.7871.125; Blink/Chromium 150 (Edge 150.0.4078.83); Gecko 151 visual / Gecko 152.0.6 stable smoke; WebKit 26.5 bundled by Microsoft.Playwright 1.61.0'

$global = Get-Content -Raw -LiteralPath $globalManifestPath | ConvertFrom-Json
foreach ($record in $global.baselines) {
    $targetManifestPath = Join-Path $baselineRoot $record.targetManifest
    $targetManifest = Get-Content -Raw -LiteralPath $targetManifestPath | ConvertFrom-Json

    $targetManifest.status = 'approved'
    $targetManifest.automaticReplacementAllowed = $false
    $targetManifest.approvalAuthority = $approvedBy
    $targetManifest.approvedBy = $approvedBy
    $targetManifest.approvedOn = $approvedOn

    foreach ($target in $targetManifest.targets) {
        $metadata = $browserMetadata[$target.browser]
        if ($null -eq $metadata) {
            throw "Unsupported browser '$($target.browser)' in $targetManifestPath."
        }

        $artifactPath = Join-Path (Split-Path $targetManifestPath -Parent) $target.file
        if (-not (Test-Path -LiteralPath $artifactPath -PathType Leaf)) {
            throw "Missing baseline artifact: $artifactPath"
        }

        $hash = (Get-FileHash -LiteralPath $artifactPath -Algorithm SHA256).Hash.ToLowerInvariant()
        $target.browserName = $metadata.browserName
        $target.browserBuild = $metadata.browserBuild
        $target.engine = $metadata.engine
        $target.sha256 = $hash
        $target.artifactSha256 = $hash
    }

    $targetManifest | ConvertTo-Json -Depth 10 |
        Set-Content -LiteralPath $targetManifestPath -Encoding utf8

    $record.state = $targetManifest.state
    $record.browserName = $globalBrowserName
    $record.browserBuild = $globalBrowserBuild
    $record.engine = $globalEngine
    $record.osImage = $targetManifest.osImage
    $record.tokenVersion = $targetManifest.tokenVersion
    $record.fixtureVersion = $targetManifest.fixtureVersion
    $record.approvedBy = $approvedBy
    $record.approvedOn = $approvedOn
    $record.targetCount = @($targetManifest.targets).Count

    $isCurrentSpec003Approval =
        $record.targetManifest.StartsWith('Spec003/', [StringComparison]::Ordinal) -and
        $currentSpec003Routes -contains $record.routeId -and
        $targetManifest.state -eq 'denied'
    if ($isCurrentSpec003Approval) {
        $combinedText = ($targetManifest.targets | ForEach-Object {
            "$($_.file):$($_.sha256)"
        }) -join "`n"
        $combinedHash = [Convert]::ToHexString(
            [Security.Cryptography.SHA256]::HashData(
                [Text.Encoding]::UTF8.GetBytes($combinedText)))
        $record.artifactSha256 = $combinedHash.ToLowerInvariant()
    }
    else {
        $record.artifactSha256 =
            (Get-FileHash -LiteralPath $targetManifestPath -Algorithm SHA256).Hash.ToLowerInvariant()
    }
}

$global | ConvertTo-Json -Depth 10 |
    Set-Content -LiteralPath $globalManifestPath -Encoding utf8

Write-Output "Refreshed $($global.baselines.Count) approved route manifests."
