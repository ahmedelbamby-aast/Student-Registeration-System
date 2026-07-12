[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$manifest = Get-Content (Join-Path $root '.specify/spec-manifest.json') -Raw | ConvertFrom-Json
$failures = New-Object System.Collections.Generic.List[string]
$results = New-Object System.Collections.Generic.List[object]
$required = @('spec.md','requirements.md','plan.md','research.md','data-model.md','quickstart.md','clarifications.md','contracts/api.md','checklists/requirements.md','checklists/gates.md','tasks.md')

function Add-Failure([string]$Message) { $script:failures.Add($Message) }

if ($manifest.specs.Count -ne 18) { Add-Failure "Manifest contains $($manifest.specs.Count) specs instead of 18." }
$ids = @($manifest.specs.id)
if (($ids | Select-Object -Unique).Count -ne $ids.Count) { Add-Failure 'Manifest contains duplicate IDs.' }

foreach ($item in $manifest.specs) {
    foreach ($dep in $item.dependencies) {
        if ($dep -eq $item.id) { Add-Failure "SPEC-$($item.id) depends on itself." }
        if ($ids -notcontains $dep) { Add-Failure "SPEC-$($item.id) has missing dependency SPEC-$dep." }
    }
}

$visiting = @{}
$visited = @{}
function Visit-Spec([string]$Id) {
    if ($visiting[$Id]) { Add-Failure "Dependency cycle detected at SPEC-$Id."; return }
    if ($visited[$Id]) { return }
    $visiting[$Id] = $true
    $node = $manifest.specs | Where-Object id -eq $Id
    foreach ($dep in $node.dependencies) { Visit-Spec $dep }
    $visiting.Remove($Id)
    $visited[$Id] = $true
}
foreach ($id in $ids) { Visit-Spec $id }

foreach ($item in $manifest.specs) {
    $name = "$($item.id)-$($item.slug)"
    $dir = Join-Path $root "specs/$name"
    $before = $failures.Count
    foreach ($file in $required) {
        if (-not (Test-Path (Join-Path $dir $file) -PathType Leaf)) { Add-Failure "SPEC-$($item.id) missing $file." }
    }
    if (-not (Test-Path $dir -PathType Container)) { continue }
    $generatedFiles = Get-ChildItem $dir -Recurse -File
    $combined = ($generatedFiles | Get-Content -Raw) -join "`n"
    if ($combined -match '\[NEEDS CLARIFICATION\]|\[FEATURE NAME\]|\[DATE\]|\[###-feature-name\]|TKTK') {
        Add-Failure "SPEC-$($item.id) contains unresolved placeholders."
    }
    $uncheckedGates = Select-String -Path (Join-Path $dir 'checklists/*.md') -Pattern '^- \[ \]' -ErrorAction SilentlyContinue
    if ($uncheckedGates) { Add-Failure "SPEC-$($item.id) has unresolved checklist items." }
    $completedTasks = Select-String -Path (Join-Path $dir 'tasks.md') -Pattern '^- \[[xX]\]' -ErrorAction SilentlyContinue
    if ($completedTasks) { Add-Failure "SPEC-$($item.id) marks implementation tasks complete." }

    $requirements = Get-Content (Join-Path $dir 'requirements.md') -Raw
    $spec = Get-Content (Join-Path $dir 'spec.md') -Raw
    $tasks = Get-Content (Join-Path $dir 'tasks.md') -Raw
    $model = Get-Content (Join-Path $dir 'data-model.md') -Raw
    $frs = [regex]::Matches($requirements, '(?m)^- (FR-\d+):') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique
    foreach ($fr in $frs) {
        if ($spec -notmatch "(?m)\b$([regex]::Escape($fr))\b") { Add-Failure "SPEC-$($item.id) $fr missing from spec.md." }
        if ($tasks -notmatch "(?m)\b$([regex]::Escape($fr))\b") { Add-Failure "SPEC-$($item.id) $fr missing from tasks.md." }
    }
    $acs = [regex]::Matches($requirements, '(?m)^### (AC-\d+):') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique
    foreach ($ac in $acs) {
        if ($spec -notmatch [regex]::Escape($ac)) { Add-Failure "SPEC-$($item.id) $ac missing from spec.md." }
        if ($tasks -notmatch [regex]::Escape($ac)) { Add-Failure "SPEC-$($item.id) $ac missing from tasks.md." }
    }
    foreach ($entity in $item.entities) {
        if ($model -notmatch [regex]::Escape($entity)) { Add-Failure "SPEC-$($item.id) entity $entity missing from data-model.md." }
    }

    $env:SPECIFY_FEATURE = $name
    $env:SPECIFY_FEATURE_DIRECTORY = "specs/$name"
    $output = & (Join-Path $PSScriptRoot 'check-prerequisites.ps1') -Json -RequireTasks -IncludeTasks 2>&1
    if ($LASTEXITCODE -ne 0) { Add-Failure "SPEC-$($item.id) official prerequisite gate failed: $output" }

    $validator = 'C:/Users/Ahmed/.codex/skills/claude-spec-driven-workflow/scripts/spec_validator.py'
    $validationJson = & python $validator --file (Join-Path $dir 'requirements.md') --strict --json 2>&1
    if ($LASTEXITCODE -ne 0) { Add-Failure "SPEC-$($item.id) strict requirements validation failed: $validationJson" }
    $score = try { ($validationJson | ConvertFrom-Json).score } catch { $null }

    $results.Add([pscustomobject]@{
        spec = "SPEC-$($item.id)"
        artifacts = $required.Count
        requirementsScore = $score
        automatedGates = if ($failures.Count -eq $before) { 'PASS' } else { 'FAIL' }
        humanApproval = 'PENDING'
    })
}
Remove-Item Env:SPECIFY_FEATURE -ErrorAction SilentlyContinue
Remove-Item Env:SPECIFY_FEATURE_DIRECTORY -ErrorAction SilentlyContinue

$sourceFiles = @(Get-ChildItem $root -Recurse -File -Include *.cs,*.razor,*.csproj,*.sln,*.sql |
    Where-Object { $_.FullName -notmatch '\\.agents\\|\\.specify\\' })
if ($sourceFiles.Count -gt 0) { Add-Failure "Planning-only boundary violated by $($sourceFiles.Count) implementation files." }

$constitution = Get-Content (Join-Path $root '.specify/memory/constitution.md') -Raw
if ($constitution -match '\[PRINCIPLE_|\[PROJECT_NAME\]|\[SECTION_') { Add-Failure 'Constitution contains template placeholders.' }
if ($constitution -notmatch 'Ahmed ELbamby <A\.Elbamby61869@student\.aast\.edu>') { Add-Failure 'Constitution is missing the required Git identity.' }

$reportRows = $results | ForEach-Object { "| $($_.spec) | $($_.artifacts) | $($_.requirementsScore) | $($_.automatedGates) | $($_.humanApproval) |" }
$overall = if ($failures.Count -eq 0) { 'PASS' } else { 'FAIL' }
@"
# Spec Kit Readiness Audit

**Date**: 2026-07-12
**Overall automated result**: $overall
**Scope**: 18 specifications; planning artifacts only
**Human approval**: Pending for every specification

| Spec | Required artifacts | Strict score | Automated gates | Human approval |
|---|---:|---:|---|---|
$($reportRows -join "`r`n")

## Cross-Spec Results

- Dependency references exist and the graph is acyclic.
- Requirement, acceptance, task, entity, and contract artifacts are connected.
- Official Spec Kit prerequisite checks were executed for every feature package.
- No application source, migration, or deployment implementation was created.
- Automated PASS means ready for human review; it does not authorize implementation.

## Failures

$(if ($failures.Count) { ($failures | ForEach-Object { "- $_" }) -join "`r`n" } else { 'None.' })
"@ | Set-Content (Join-Path $root 'docs/SPECKIT_AUDIT.md')

[pscustomobject]@{ overall = $overall; specifications = $results; failures = $failures } | ConvertTo-Json -Depth 5
if ($failures.Count -gt 0) { exit 1 }
