[CmdletBinding()]
param(
    [string]$ApprovedBy = 'Ahmed ELbamby',
    [string]$ApprovedOn = '2026-07-21'
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
$candidateRoot = Join-Path $repositoryRoot 'artifacts\visual-candidates\Spec003'
$v2Root = Join-Path $PSScriptRoot 'v2\Spec003'
$globalManifestPath = Join-Path $PSScriptRoot 'baseline-manifest.json'
$routes = @('ADM-10', 'STF-05', 'STU-09')
$states = @('primary', 'error')
$viewports = @(375, 768, 1280, 1920)
$approvalSource = 'Explicit user approval in the current Codex task turn for all demo visual candidates and the unified design.'

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
        browserName = 'Playwright Firefox'
        browserBuild = '151.0 via Microsoft.Playwright 1.61.0'
        engine = 'Gecko 151'
    }
    webkit = @{
        browserName = 'Playwright WebKit (not Safari)'
        browserBuild = 'WebKit 26.5 via Microsoft.Playwright 1.61.0'
        engine = 'WebKit 26.5 bundled by Microsoft.Playwright 1.61.0'
    }
}

$newGlobalEntries = @()
foreach ($route in $routes) {
    foreach ($state in $states) {
        $sourceDirectory = Join-Path $candidateRoot "$route\$state"
        if (-not (Test-Path -LiteralPath $sourceDirectory)) {
            throw "Candidate directory is missing: $sourceDirectory"
        }

        $targetDirectory = Join-Path $v2Root "$route\$state"
        New-Item -ItemType Directory -Path $targetDirectory -Force | Out-Null
        $targets = @()
        foreach ($browser in @('chrome', 'edge', 'firefox', 'webkit')) {
            foreach ($viewport in $viewports) {
                $source = Join-Path $sourceDirectory "$browser-$viewport.png"
                if (-not (Test-Path -LiteralPath $source) -or (Get-Item -LiteralPath $source).Length -eq 0) {
                    throw "Approved candidate is missing or empty: $source"
                }

                $file = "$browser-$viewport-$state.png"
                $destination = Join-Path $targetDirectory $file
                Copy-Item -LiteralPath $source -Destination $destination -Force
                $hash = (Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash.ToLowerInvariant()
                $metadata = $browserMetadata[$browser]
                $targets += [ordered]@{
                    browser = $browser
                    browserName = $metadata.browserName
                    browserBuild = $metadata.browserBuild
                    engine = $metadata.engine
                    osImage = 'Microsoft Windows 11 Pro 10.0.26200 x64'
                    viewport = $viewport
                    tokenVersion = 'design-token-set/2.0.0'
                    fixtureVersion = 'frontend-fixture/2.0'
                    file = $file
                    sha256 = $hash
                    artifactSha256 = $hash
                }
            }
        }

        $routeManifest = [ordered]@{
            version = "$($route.ToLowerInvariant())-visual-baselines/2.0.0"
            baselineGeneration = 'v2'
            routeId = $route
            state = $state
            fixtureVersion = 'frontend-fixture/2.0'
            tokenVersion = 'design-token-set/2.0.0'
            status = 'approved'
            automaticReplacementAllowed = $false
            approvalAuthority = $ApprovedBy
            approvedBy = $ApprovedBy
            approvedOn = $ApprovedOn
            approvalSource = $approvalSource
            osImage = 'Microsoft Windows 11 Pro 10.0.26200 x64'
            targets = $targets
        }
        $routeManifestPath = Join-Path $targetDirectory 'baseline-targets.json'
        $routeManifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $routeManifestPath -Encoding utf8
        $manifestHash = (Get-FileHash -LiteralPath $routeManifestPath -Algorithm SHA256).Hash.ToLowerInvariant()

        $newGlobalEntries += [ordered]@{
            routeId = $route
            state = $state
            baselineGeneration = 'v2'
            browserName = 'Google Chrome Stable; Microsoft Edge Stable; Playwright Firefox; Playwright WebKit (not Safari)'
            browserBuild = 'Chrome 150.0.7871.125; Edge 150.0.4078.83; Firefox 151.0; WebKit 26.5 via Microsoft.Playwright 1.61.0'
            engine = 'Blink/Chromium 150; Gecko 151; WebKit 26.5'
            osImage = 'Microsoft Windows 11 Pro 10.0.26200 x64'
            viewport = $viewports
            tokenVersion = 'design-token-set/2.0.0'
            fixtureVersion = 'frontend-fixture/2.0'
            artifactSha256 = $manifestHash
            approvedBy = $ApprovedBy
            approvedOn = $ApprovedOn
            approvalSource = $approvalSource
            targetManifest = "v2/Spec003/$route/$state/baseline-targets.json"
            targetCount = 16
        }
    }
}

$global = Get-Content -LiteralPath $globalManifestPath -Raw | ConvertFrom-Json -AsHashtable
$global.version = 'visual-baselines/2.0.0'
$global.status = 'approved-routes-with-governed-v2-additions'
$global.requiredEvidenceFields = @(
    'routeId', 'state', 'browserName', 'browserBuild', 'engine', 'osImage',
    'viewport', 'tokenVersion', 'fixtureVersion', 'artifactSha256',
    'approvedBy', 'approvedOn'
)
$global.baselines = @($global.baselines | Where-Object {
    -not ($_.baselineGeneration -eq 'v2' -and $_.routeId -in $routes)
}) + $newGlobalEntries
$global | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $globalManifestPath -Encoding utf8

Write-Host "Promoted $($newGlobalEntries.Count * 16) approved PNGs into governed v2 baselines without changing v1 route folders."
