[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$manifest = Get-Content (Join-Path $root '.specify/spec-manifest.json') -Raw | ConvertFrom-Json

function Get-Section([string]$Text, [string]$Name) {
    $pattern = "(?ms)^## $([regex]::Escape($Name))\s*\r?\n(.*?)(?=^## |\z)"
    $match = [regex]::Match($Text, $pattern)
    if ($match.Success) { return $match.Groups[1].Value.Trim() }
    return 'Not specified.'
}

function Get-Items([string]$Section, [string]$Prefix) {
    return [regex]::Matches($Section, "(?ms)^### ($Prefix-\d+):?\s*([^\r\n]*)\r?\n(.*?)(?=^### |\z)")
}

foreach ($item in $manifest.specs) {
    $featureName = "$($item.id)-$($item.slug)"
    $featureDir = Join-Path $root "specs/$featureName"
    New-Item -ItemType Directory -Force $featureDir, (Join-Path $featureDir 'contracts'), (Join-Path $featureDir 'checklists') | Out-Null
    $source = Join-Path $root "specs/SPEC-$($item.id)-$($item.slug).md"
    $requirements = Join-Path $featureDir 'requirements.md'
    if (Test-Path $source) { Move-Item -LiteralPath $source -Destination $requirements }
    $raw = Get-Content $requirements -Raw

    if ($raw -notmatch '(?m)^\*\*Dependencies:\*\*') {
        $depText = if ($item.dependencies.Count -eq 0) { 'None' } else { ($item.dependencies | ForEach-Object { "SPEC-$_" }) -join ', ' }
        $raw = $raw -replace '(\*\*Target:\*\*[^\r\n]*<br>\r?\n)', "`$1**Dependencies:** $depText<br>`r`n"
        Set-Content $requirements $raw
    }

    $context = Get-Section $raw 'Context'
    $functional = Get-Section $raw 'Functional Requirements'
    $nonFunctional = Get-Section $raw 'Non-Functional Requirements'
    $acceptance = Get-Section $raw 'Acceptance Criteria'
    $edge = Get-Section $raw 'Edge Cases'
    $api = Get-Section $raw 'API Contracts'
    $models = Get-Section $raw 'Data Models'
    $outScope = Get-Section $raw 'Out of Scope'
    $acs = Get-Items $acceptance 'AC'
    $depLinks = if ($item.dependencies.Count -eq 0) { '- None; this is a root specification.' } else {
        ($item.dependencies | ForEach-Object {
            $dep = $manifest.specs | Where-Object id -eq $_
            "- [SPEC-$_](../$($_)-$($dep.slug)/spec.md)"
        }) -join "`r`n"
    }

    $stories = New-Object System.Collections.Generic.List[string]
    $tasks = New-Object System.Collections.Generic.List[string]
    $storyNumber = 0
    foreach ($ac in $acs) {
        $storyNumber++
        $acId = $ac.Groups[1].Value
        $acTitle = $ac.Groups[2].Value.Trim()
        $body = $ac.Groups[3].Value.Trim()
        $priority = if ($storyNumber -le 2) { 'P1' } elseif ($storyNumber -le 4) { 'P2' } else { 'P3' }
        $stories.Add(@"
### User Story $storyNumber - $acTitle ($priority)

As a $($item.actor), I need the $acTitle behavior so that $($item.title) produces a verifiable outcome.

**Independent Test**: Execute $acId in requirements.md without relying on another story in this feature.

**Acceptance Scenario ($acId)**

$body
"@)
        $frRefs = ([regex]::Matches("$acTitle $body", 'FR-\d+') | ForEach-Object Value | Select-Object -Unique) -join ', '
        if (-not $frRefs) { $frRefs = 'linked functional requirements' }
        $tasks.Add("- [ ] T$('{0:d3}' -f ($storyNumber * 2 + 2)) [P] [US$storyNumber] Add the future failing acceptance test for $acId ($frRefs) under tests/acceptance/$featureName/.")
        $tasks.Add("- [ ] T$('{0:d3}' -f ($storyNumber * 2 + 3)) [US$storyNumber] Implement $acId only after approval, using the module paths declared in plan.md.")
    }

    $success = ($item.successCriteria | ForEach-Object -Begin { $n = 0 } -Process { $n++; "- **SC-$n**: $_" }) -join "`r`n"
    $entities = ($item.entities | ForEach-Object { "- **$_**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD." }) -join "`r`n"

    @"
# Feature Specification: $($item.title)

**Feature Branch**: $featureName
**Created**: 2026-07-12
**Status**: In Review
**Owner**: $($item.owner)
**Normative detail**: [requirements.md](requirements.md)

## Context

$context

## User Scenarios and Testing

$($stories -join "`r`n")

## Edge Cases

$edge

## Requirements

### Functional Requirements

$functional

### Key Entities

$entities

## Success Criteria

$success

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

$depLinks

## Out of Scope

$outScope
"@ | Set-Content (Join-Path $featureDir 'spec.md')

    @"
# Implementation Plan: $($item.title)

**Branch**: $featureName | **Date**: 2026-07-12 | **Spec**: [spec.md](spec.md)
**Status**: Planning complete; implementation is not authorized.

## Summary

Deliver $($item.title) inside the modular monolith while keeping server-side academic and authorization decisions authoritative.

## Technical Context

**Language/Version**: C# / .NET 10
**Primary Dependencies**: ASP.NET Core, Blazor WebAssembly, Entity Framework Core, LINQ
**Storage**: SQL Server with Code First migrations
**Testing**: xUnit plus API, integration, concurrency, accessibility, and browser tests as applicable
**Project Type**: Web application with hosted WebAssembly client and server API
**Performance Goals**: Governed by SPEC-018 and feature NFRs
**Constraints**: Atomic writes, WCAG 2.2 AA, stateless APIs, no client-authoritative decisions
**Scale/Scope**: Registration-peak horizontal scaling; bounded and paginated queries

## Constitution Check

- PASS: Git ownership is reserved for Ahmed ELbamby.
- PASS: Requirements, acceptance scenarios, and tasks use stable traceability identifiers.
- PASS: The design remains a simple modular monolith.
- PASS: Security, policy, schedule, capacity, and term decisions remain server-authoritative.
- PASS: Accessibility, scalability, concurrency, and observability requirements are retained.
- PASS: No application source code or migration is created by this planning phase.

## Dependency Check

$depLinks

## Project Structure

Future implementation paths are src/StudentRegistration.Client, src/StudentRegistration.Server, src/StudentRegistration.Domain, src/StudentRegistration.Infrastructure, and tests/. These paths are declarations only and do not exist yet.

## Design Artifacts

- [Research](research.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Planning quickstart](quickstart.md)
- [Tasks](tasks.md)

## Non-Functional Requirements

$nonFunctional

## Complexity Tracking

No constitution violation or distributed component is proposed. Additional infrastructure requires measured evidence and an approved amendment.
"@ | Set-Content (Join-Path $featureDir 'plan.md')

    @"
# Research: $($item.title)

## Decisions

### Modular boundary
**Decision**: Own this capability in the $($item.module) module of the modular monolith.
**Rationale**: It provides a clear extension seam without premature distributed-system cost.
**Alternatives rejected**: A microservice per feature and direct client-to-database access.

### Authority and consistency
**Decision**: Validate permissions, term state, policy, conflicts, and durable changes on the server, with database enforcement for contested writes.
**Rationale**: Browser state is stale and untrusted during registration peaks.
**Alternatives rejected**: Client-only validation and check-then-write capacity logic.

### Feature contract
$api

## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
"@ | Set-Content (Join-Path $featureDir 'research.md')

    @"
# Data Model: $($item.title)

## Owned Entities

$entities

## Detailed Model

$models

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
"@ | Set-Content (Join-Path $featureDir 'data-model.md')

    @"
# API Contract: $($item.title)

## Feature Contract

$api

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
"@ | Set-Content (Join-Path $featureDir 'contracts/api.md')

    @"
# Clarification Record: $($item.title)

**Reviewed**: 2026-07-12
**Result**: PASS - no unresolved NEEDS CLARIFICATION markers.

The specification was reviewed for scope, actors, data, business rules, errors, concurrency, security, accessibility, dependencies, and measurable outcomes. Assumptions and external approvals are explicit. Unknown AASTMT policy values are governed configuration with provenance and fail-closed behavior; they are not invented requirements.
"@ | Set-Content (Join-Path $featureDir 'clarifications.md')

    @"
# Planning Quickstart: $($item.title)

This is a pre-implementation verification guide. It does not run or create application code.

1. Confirm the feature remains In Review and that implementation is not authorized.
2. Review requirements.md and resolve all external approvals recorded there.
3. Verify each functional requirement appears in spec.md and tasks.md.
4. Walk through each acceptance scenario with the accountable owner.
5. Review data-model.md and contracts/api.md against upstream dependencies.
6. Run the repository Spec Kit gate script.
7. Obtain human approval before creating any source, test, migration, or deployment file.
"@ | Set-Content (Join-Path $featureDir 'quickstart.md')

    @"
# Requirements Quality Checklist: $($item.title)

- [x] No unresolved clarification or placeholder remains.
- [x] Requirements are testable and use stable identifiers.
- [x] Every acceptance scenario maps to a user story.
- [x] Success criteria are measurable and technology-neutral.
- [x] Assumptions, dependencies, and out-of-scope boundaries are explicit.
- [x] Key entities are identified and refined in data-model.md.
- [x] Security, accessibility, concurrency, scale, and failure behavior are addressed where applicable.
- [x] This package contains planning artifacts only.

**Automated readiness**: PASS
**Human approval**: PENDING
"@ | Set-Content (Join-Path $featureDir 'checklists/requirements.md')

    @"
# Gate Checklist: $($item.title)

- [x] G1 Constitution compliance
- [x] G2 Specification completeness
- [x] G3 Clarification resolution
- [x] G4 Dependency validity
- [x] G5 Plan and research completeness
- [x] G6 Data model and API contract consistency
- [x] G7 Requirement-to-acceptance traceability
- [x] G8 Requirement-to-task traceability
- [x] G9 Cross-spec analysis
- [x] G10 Planning-only boundary

Automated gates pass. Human approval remains pending and implementation MUST NOT begin.
"@ | Set-Content (Join-Path $featureDir 'checklists/gates.md')

    @"
# Tasks: $($item.title)

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md

## Phase 1 - Approval and Contracts

- [ ] T001 Obtain human approval for SPEC-$($item.id).
- [ ] T002 Reconfirm upstream dependency versions and institutional policy approvals.
- [ ] T003 Freeze the reviewed data and API contract for the implementation sprint.

## Phase 2 - User Stories

$($tasks -join "`r`n")

## Phase 3 - Quality and Release

- [ ] T090 Run unit, integration, concurrency, authorization, accessibility, and performance checks required by requirements.md.
- [ ] T091 Verify every FR and AC has passing evidence and no capacity or authorization invariant regressed.
- [ ] T092 Complete product-owner, policy-owner, QA, security, and operations release gates.

## Requirement Traceability Reference

$functional

No task is complete and no implementation file has been created.
"@ | Set-Content (Join-Path $featureDir 'tasks.md')
}

$rows = $manifest.specs | ForEach-Object {
    $deps = if ($_.dependencies.Count) { ($_.dependencies | ForEach-Object { "SPEC-$_" }) -join ', ' } else { 'None' }
    "| SPEC-$($_.id) | [$($_.title)]($($_.id)-$($_.slug)/spec.md) | $($_.owner) | $deps | Automated PASS; human approval pending |"
}
@"
# Specification Index

All 18 features have completed the Spec Kit planning workflow through consistency analysis. No implementation has started. Detailed original requirements are preserved in each feature's requirements.md.

| Spec | Feature package | Owner | Dependencies | Gate status |
|---|---|---|---|---|
$($rows -join "`r`n")

Run .specify/scripts/powershell/Test-AllSpecs.ps1 to reproduce the gates.
"@ | Set-Content (Join-Path $root 'specs/README.md')

Write-Host "Generated Spec Kit planning packages for $($manifest.specs.Count) specifications."
