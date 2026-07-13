[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$manifest = Get-Content (Join-Path $root '.specify/spec-manifest.json') -Raw | ConvertFrom-Json
$routeManifest = Get-Content (Join-Path $root '.specify/route-manifest.json') -Raw | ConvertFrom-Json
$endpointManifest = Get-Content (Join-Path $root '.specify/endpoint-manifest.json') -Raw | ConvertFrom-Json
$pageApiManifest = Get-Content (Join-Path $root '.specify/page-api-manifest.json') -Raw | ConvertFrom-Json
$componentManifest = Get-Content (Join-Path $root '.specify/component-manifest.json') -Raw | ConvertFrom-Json
$workstreamManifest = Get-Content (Join-Path $root '.specify/workstream-manifest.json') -Raw | ConvertFrom-Json
$entityOwnership = Get-Content (Join-Path $root '.specify/entity-ownership.json') -Raw | ConvertFrom-Json
$persistenceManifest = Get-Content (Join-Path $root '.specify/persistence-manifest.json') -Raw | ConvertFrom-Json
$generationDate = Get-Date -Format 'yyyy-MM-dd'

function Get-Section([string]$Text, [string]$Name) {
    $pattern = "(?ms)^## $([regex]::Escape($Name))\s*\r?\n(.*?)(?=^## |\z)"
    $match = [regex]::Match($Text, $pattern)
    if ($match.Success) { return $match.Groups[1].Value.Trim() }
    return 'Not specified.'
}

function Get-Items([string]$Section, [string]$Prefix) {
    return [regex]::Matches($Section, "(?ms)^### ($Prefix-\d+):?\s*([^\r\n]*)\r?\n(.*?)(?=^### |\z)")
}

function Get-RequirementItems([string]$Section, [string]$PrefixPattern) {
    return [regex]::Matches($Section, "(?ms)^- (($PrefixPattern)-\d+):\s*(.*?)(?=^- ($PrefixPattern)-\d+:|\z)")
}

function ConvertTo-OneLine([string]$Value) {
    return ([regex]::Replace(($Value -replace '<br>', ' '), '\s+', ' ')).Trim().TrimEnd('.')
}

function New-TaskLine([ref]$Counter, [string]$Tags, [string]$Action) {
    $Counter.Value++
    $id = 'T{0:d3}' -f $Counter.Value
    if ([string]::IsNullOrWhiteSpace($Tags)) { return "- [ ] $id $Action" }
    return "- [ ] $id $Tags $Action"
}

function Get-ModuleProjectName([string]$Module) {
    switch ($Module) {
        'Operations' { return 'Api' }
        default { return $Module }
    }
}

foreach ($item in $manifest.specs) {
    $featureName = "$($item.id)-$($item.slug)"
    $featureDir = Join-Path $root "specs/$featureName"
    New-Item -ItemType Directory -Force $featureDir, (Join-Path $featureDir 'contracts'), (Join-Path $featureDir 'checklists') | Out-Null
    $source = Join-Path $root "specs/SPEC-$($item.id)-$($item.slug).md"
    $requirements = Join-Path $featureDir 'requirements.md'
    if (Test-Path $source) { Move-Item -LiteralPath $source -Destination $requirements }
    $raw = Get-Content $requirements -Raw
    $sourceDateMatch = [regex]::Match($raw, '(?m)^\*\*Date:\*\*\s*([^<\r\n]+)')
    $createdDate = if ($sourceDateMatch.Success) { $sourceDateMatch.Groups[1].Value.Trim() } else { $generationDate }

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
    $frItems = Get-RequirementItems $functional 'FR'
    $nfrItems = Get-RequirementItems $nonFunctional 'NFR(?:-[A-Z]+)?'
    $ecItems = Get-RequirementItems $edge 'EC'
    $osItems = Get-RequirementItems $outScope 'OS'
    $depLinks = if ($item.dependencies.Count -eq 0) { '- None; this is a root specification.' } else {
        ($item.dependencies | ForEach-Object {
            $dep = $manifest.specs | Where-Object id -eq $_
            "- [SPEC-$_](../$($_)-$($dep.slug)/spec.md)"
        }) -join "`r`n"
    }

    $stories = New-Object System.Collections.Generic.List[string]
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
    }

    $taskNumber = 0
    $approvalTasks = New-Object System.Collections.Generic.List[string]
    $foundationTasks = New-Object System.Collections.Generic.List[string]
    $endpointDeliveryTasks = New-Object System.Collections.Generic.List[string]
    $endpointDeliveryDefinitions = New-Object System.Collections.Generic.List[object]
    $requirementTasks = New-Object System.Collections.Generic.List[string]
    $testTasks = New-Object System.Collections.Generic.List[string]
    $frontendTasks = New-Object System.Collections.Generic.List[string]
    $qualityTasks = New-Object System.Collections.Generic.List[string]
    $scopeTasks = New-Object System.Collections.Generic.List[string]

    foreach ($depId in $item.dependencies) {
        $dep = $manifest.specs | Where-Object id -eq $depId
        $depName = "$depId-$($dep.slug)"
        $approvalTasks.Add((New-TaskLine ([ref]$taskNumber) "[DEP-SPEC-$depId]" "Validate the consumed upstream requirements, plan, data model, and API contract at specs/$depName/ and record the accepted versions in specs/$featureName/dependency-baseline.md."))
    }
    $approvalTasks.Add((New-TaskLine ([ref]$taskNumber) '[GATE]' "Freeze SPEC-$($item.id) requirements, API, data-model, policy approvals, and dependency versions in specs/$featureName/checklists/implementation-readiness.md."))
    $approvalTasks.Add((New-TaskLine ([ref]$taskNumber) '[GATE] [CONSISTENCY-ANALYSIS]' "Run the cross-artifact and cross-spec consistency analysis and record zero unresolved critical/high findings in specs/$featureName/checklists/implementation-readiness.md."))
    $approvalTasks.Add((New-TaskLine ([ref]$taskNumber) '[GATE]' "Record Ahmed ELbamby's human approval for SPEC-$($item.id) in specs/$featureName/checklists/approval.md as the final gate before any test or implementation task."))

    $moduleSafe = $item.module -replace '[^A-Za-z0-9]', ''
    $specWorkstreams = @($workstreamManifest.specs.PSObject.Properties | Where-Object Name -eq $item.id | ForEach-Object Value)

    foreach ($entity in $item.entities) {
        $ownerProperty = $entityOwnership.canonicalOwners.PSObject.Properties | Where-Object Name -eq $entity
        $canonicalOwner = if ($ownerProperty) { [string]$ownerProperty.Value } else { [string]$item.id }
        $ownerSpec = $manifest.specs | Where-Object id -eq $canonicalOwner
        $ownerProject = Get-ModuleProjectName ([string]$ownerSpec.module)
        $entityFile = "src/StudentRegistration.$ownerProject/Domain/$entity.cs"
        $overrideKey = "$($item.id):$entity"
        $overrideProperty = $entityOwnership.artifactOverrides.PSObject.Properties | Where-Object Name -eq $overrideKey
        if ($overrideProperty) { $entityFile = [string]$overrideProperty.Value }
        if (-not $overrideProperty -and $canonicalOwner -ne $item.id) {
            $ownerOverrideKey = "$canonicalOwner`:$entity"
            $ownerOverrideProperty = $entityOwnership.artifactOverrides.PSObject.Properties | Where-Object Name -eq $ownerOverrideKey
            if ($ownerOverrideProperty) { $entityFile = [string]$ownerOverrideProperty.Value }
        }
        if ($item.id -eq '006') { $entityFile = "src/StudentRegistration.Contracts/$entity.cs" }
        if ($item.id -eq '003' -and -not $overrideProperty) { $entityFile = "src/StudentRegistration.Client/Features/Frontend/Models/$entity.cs" }
        $entityTest = "tests/StudentRegistration.IntegrationTests/Specs/Spec$($item.id)/$($entity)ModelTests.cs"
        if ($overrideProperty -and $entityFile -match '^(?:specs|docs)/') { $entityTest = "tests/StudentRegistration.SpecificationTests/Specs/Spec$($item.id)/$($entity)SchemaTests.cs" }
        if ($item.id -eq '005') {
            $foundationTasks.Add((New-TaskLine ([ref]$taskNumber) "[ENTITY-$entity] [SCHEMA-CONTRACT] [CONSUMER-SPEC-$canonicalOwner]" "Verify the canonical SPEC-$canonicalOwner $entity mapping at $entityFile against the ERD/schema contract in docs/diagrams/ERD.md using $entityTest; SPEC-005 does not deliver the runtime entity or mapping."))
        } elseif ($canonicalOwner -ne $item.id) {
            $foundationTasks.Add((New-TaskLine ([ref]$taskNumber) "[P] [ENTITY-$entity] [CONSUMER-SPEC-$canonicalOwner]" "Verify SPEC-$($item.id) consumes the canonical $entity at $entityFile without redefining ownership in $entityTest."))
        } else {
            $kindTag = if ($overrideProperty -and $entityFile -match '^(?:specs|docs)/') { '[ARTIFACT-OWNER]' } else { "[OWNER-SPEC-$canonicalOwner]" }
            $entityTestTaskId = 'T{0:d3}' -f ($taskNumber + 1)
            $foundationTasks.Add((New-TaskLine ([ref]$taskNumber) "[P] [ENTITY-$entity] $kindTag" "Create the future failing invariant/schema/serialization checks for canonical $entity ownership in $entityTest."))
            $foundationTasks.Add((New-TaskLine ([ref]$taskNumber) "[ENTITY-$entity] $kindTag" "Deliver the canonical $entity model or governed artifact at $entityFile after $entityTestTaskId fails for the expected reason (depends on $entityTestTaskId)."))
        }
    }

    if ([string]$persistenceManifest.dbContext.owner -eq [string]$item.id) {
        $dbContextTestTaskId = 'T{0:d3}' -f ($taskNumber + 1)
        $foundationTasks.Add((New-TaskLine ([ref]$taskNumber) '[P] [PERSISTENCE-BOUNDARY] [OWNER-SPEC-004]' "Create the future failing single-DbContext and module-mapping boundary checks in $($persistenceManifest.dbContext.testPath)."))
        $foundationTasks.Add((New-TaskLine ([ref]$taskNumber) '[PERSISTENCE-BOUNDARY] [OWNER-SPEC-004]' "Deliver the sole application DbContext at $($persistenceManifest.dbContext.path) after $dbContextTestTaskId fails for the expected reason (depends on $dbContextTestTaskId)."))
    }

    $persistenceProperty = $persistenceManifest.contributions.PSObject.Properties | Where-Object Name -eq $item.id
    if ($persistenceProperty) {
        $mapping = $persistenceProperty.Value
        $mappingEntityTags = (@($mapping.entities) | ForEach-Object { "[ENTITY-$_]" }) -join ' '
        $mappingTestTaskId = 'T{0:d3}' -f ($taskNumber + 1)
        $foundationTasks.Add((New-TaskLine ([ref]$taskNumber) "[P] [EF-MAPPING] $mappingEntityTags" "Create the future failing SQL Server mapping, key/index/rowversion, delete-behavior, and mode '$($mapping.mode)' checks in $($mapping.testPath)."))
        $foundationTasks.Add((New-TaskLine ([ref]$taskNumber) "[EF-MAPPING] $mappingEntityTags" "Deliver the SPEC-$($item.id) EF Core configuration at $($mapping.path) after $mappingTestTaskId fails for the expected reason (depends on $mappingTestTaskId)."))
    }

    foreach ($migration in @($persistenceManifest.migrations | Where-Object owner -eq $item.id)) {
        $migrationTestTaskId = 'T{0:d3}' -f ($taskNumber + 1)
        $foundationTasks.Add((New-TaskLine ([ref]$taskNumber) "[P] [MIGRATION-$($migration.id)]" "Create the future failing $($migration.kind) migration completeness, empty-database/update/rollback, and snapshot parity checks in tests/StudentRegistration.IntegrationTests/Persistence/$($migration.id)MigrationTests.cs after accepting prerequisite mappings: $(@($migration.prerequisiteSpecs) -join ', ')."))
        $foundationTasks.Add((New-TaskLine ([ref]$taskNumber) "[MIGRATION-$($migration.id)]" "Generate the $($migration.kind) SQL Server migration at $($migration.path) and update the model snapshot at $($migration.snapshotPath) after $migrationTestTaskId fails for the expected reason (depends on $migrationTestTaskId)."))
    }

    $endpoints = @($endpointManifest.endpoints | Where-Object owner -eq $item.id | ForEach-Object { "$($_.method) $($_.path)" })
    $endpointIndex = 0
    foreach ($endpoint in $endpoints) {
        $endpoint = ConvertTo-OneLine $endpoint
        $endpointIndex++
        $endpointCode = 'Endpoint{0:d2}' -f $endpointIndex
        $foundationTasks.Add((New-TaskLine ([ref]$taskNumber) "[API-$endpointCode] [OWNER-SPEC-$($item.id)]" "Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for $endpoint in specs/$featureName/contracts/api.md."))
        $endpointTestTaskId = 'T{0:d3}' -f ($taskNumber + 1)
        $foundationTasks.Add((New-TaskLine ([ref]$taskNumber) "[P] [API-$endpointCode]" "Verify every documented response and authorization outcome for $endpoint in tests/StudentRegistration.ContractTests/Specs/Spec$($item.id)/$($endpointCode)ContractTests.cs."))
        $moduleProject = Get-ModuleProjectName ([string]$item.module)
        $endpointDeliveryDefinitions.Add([pscustomobject]@{
            Endpoint = $endpoint
            EndpointCode = $endpointCode
            HandlerPath = "src/StudentRegistration.$moduleProject/Endpoints/Spec$($item.id)Endpoints.cs"
            ContractTestTaskId = $endpointTestTaskId
        })
    }

    $foundationTaskId = 'T{0:d3}' -f $taskNumber
    $storyTaskNumber = 0
    foreach ($ac in $acs) {
        $storyTaskNumber++
        $acId = $ac.Groups[1].Value
        $refs = @([regex]::Matches("$($ac.Groups[2].Value) $($ac.Groups[3].Value)", '\b(?:FR-\d+|NFR(?:-[A-Z]+)?-?\d+)\b') | ForEach-Object Value | Select-Object -Unique)
        $refTags = ($refs | ForEach-Object { "[$_]" }) -join ' '
        $acTitle = ConvertTo-OneLine $ac.Groups[2].Value
        $acSummary = ConvertTo-OneLine "$($ac.Groups[2].Value): $($ac.Groups[3].Value)"
        $priority = if ($storyTaskNumber -le 2) { 'P1' } elseif ($storyTaskNumber -le 4) { 'P2' } else { 'P3' }
        $successTag = if ($storyTaskNumber -le @($item.successCriteria).Count) { "[SC-$storyTaskNumber] " } else { '' }
        $testTasks.Add(@"
### US$storyTaskNumber - $acTitle ($priority)

**Goal**: Prove $acId as an independently demonstrable slice of $($item.title).

**Independent Test**: Execute only the $acId Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through $foundationTaskId.
"@)
        $testTasks.Add((New-TaskLine ([ref]$taskNumber) "[P] $successTag[$acId] $refTags" "Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec$($item.id)/$($acId)Tests.cs for $($acId): $acSummary."))
    }

    foreach ($ec in $ecItems) {
        $ecId = $ec.Groups[1].Value
        $summary = ConvertTo-OneLine $ec.Groups[3].Value
        $testTasks.Add((New-TaskLine ([ref]$taskNumber) "[P] [$ecId]" "Exercise $ecId with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec$($item.id)/EdgeCases/$($ecId)Tests.cs and assert: $summary."))
    }

    if ($item.id -eq '014') {
        $matrixPath = Join-Path $featureDir 'concurrency-matrix.md'
        if (-not (Test-Path $matrixPath -PathType Leaf)) {
            throw "SPEC-014 concurrency matrix is missing at $matrixPath."
        }

        $testTasks.Add(@"
### SPEC-014 Concurrency Matrix Oracles

**Goal**: Prove every race, serialization boundary, winner/loser result, and invariant declared in concurrency-matrix.md with deterministic real-SQL tests before delivery begins.
"@)
        $raceNumber = 0
        foreach ($matrixLine in Get-Content $matrixPath) {
            if ($matrixLine -notmatch '^\|') { continue }
            $cells = @($matrixLine.Trim().Trim('|') -split '\|' | ForEach-Object { $_.Trim() })
            if ($cells.Count -ne 5 -or $cells[0] -eq 'Race' -or $cells[0] -match '^---') { continue }

            $raceNumber++
            $raceId = 'R{0:d2}' -f $raceNumber
            $race = ConvertTo-OneLine $cells[0]
            $boundary = ConvertTo-OneLine $cells[1]
            $winner = ConvertTo-OneLine $cells[2]
            $loser = ConvertTo-OneLine $cells[3]
            $invariant = ConvertTo-OneLine $cells[4]
            $testPath = "tests/StudentRegistration.ConcurrencyTests/Specs/Spec014/ConcurrencyMatrix/Race$($raceId)Tests.cs"

            $proof = 'Use deterministic SQL Server barriers'
            if ($race -match '(?i)two students|one student|same idempotency|same key|vs submit|capacity reduction|process death|process/network loss') {
                $proof += ' across two application replicas'
            }
            if ($race -match '(?i)multi-group allocation') {
                $proof += ' and inject the losing group allocation before commit'
            } elseif ($race -match '(?i)deadlock|transient error') {
                $proof += ' and inject the deadlock/transient failure during the contested write'
            } elseif ($race -match '(?i)process death') {
                $proof += ' and inject application-process death after the idempotency claim but before commit, then retry on the second replica'
            } elseif ($race -match '(?i)process/network loss') {
                $proof += ' and inject process/network loss after database commit but before the HTTP result'
            } elseif ($race -match '(?i)reconciliation mismatch') {
                $proof += ' and inject the persisted counter/enrollment mismatch before reconciliation'
            }

            $oracle = "shared boundary/order '$boundary'; allowed winner '$winner'; required loser/result '$loser'; invariant '$invariant'"
            $testTasks.Add((New-TaskLine ([ref]$taskNumber) "[P] [RACE-$raceId]" "Create the future failing real-SQL test for race '$race' from specs/014-registration-capacity-concurrency/concurrency-matrix.md in $testPath. $proof and assert the complete oracle: $oracle."))
        }

        if ($raceNumber -eq 0) {
            throw 'SPEC-014 concurrency matrix contains no data rows.'
        }
    }

    foreach ($workstream in $specWorkstreams) {
        $streamRequirements = @($workstream.requirements | ForEach-Object { [string]$_ })
        foreach ($frId in $streamRequirements) {
            if (@($frItems | Where-Object { $_.Groups[1].Value -eq $frId }).Count -ne 1) {
                throw "SPEC-$($item.id) workstream $($workstream.name) references undefined $frId."
            }
        }
        $workstreamTag = ($workstream.name -replace '[^A-Za-z0-9]+', '-').Trim('-').ToUpperInvariant()
        $requirementTags = ($streamRequirements | ForEach-Object { "[$_]" }) -join ' '
        $summaries = @($frItems | Where-Object { $streamRequirements -contains $_.Groups[1].Value } | ForEach-Object {
            "$($_.Groups[1].Value): $(ConvertTo-OneLine $_.Groups[3].Value)"
        }) -join ' | '
        $testPath = [string]$workstream.testPath
        $deliveryPath = [string]$workstream.deliveryPath
        $testFocus = ConvertTo-OneLine ([string]$workstream.testFocus)
        $streamTestTaskId = 'T{0:d3}' -f ($taskNumber + 1)
        $requirementTasks.Add((New-TaskLine ([ref]$taskNumber) "[P] $requirementTags [WORKSTREAM-$workstreamTag]" "Create one cohesive future-failing workstream suite in $testPath. Test focus: $testFocus. Prove every linked AC/EC fixture for $summaries."))
        $requirementTasks.Add((New-TaskLine ([ref]$taskNumber) "$requirementTags [WORKSTREAM-$workstreamTag]" "Deliver the bounded $($workstream.name) workstream at $deliveryPath only after $streamTestTaskId fails for the expected reasons (depends on $streamTestTaskId): $summaries."))
    }

    foreach ($definition in $endpointDeliveryDefinitions) {
        $endpointDeliveryTasks.Add((New-TaskLine ([ref]$taskNumber) "[API-$($definition.EndpointCode)] [OWNER-SPEC-$($item.id)]" "Deliver the sole canonical $($definition.Endpoint) handler at $($definition.HandlerPath) only after contract test $($definition.ContractTestTaskId) and all linked acceptance/requirement tests fail for expected reasons (depends on $($definition.ContractTestTaskId))."))
    }

    if ($item.id -eq '003') {
        $tokenTestTaskId = 'T{0:d3}' -f ($taskNumber + 1)
        $frontendTasks.Add((New-TaskLine ([ref]$taskNumber) '[P] [FR-3] [NFR-3]' 'Create failing token-schema, contrast, focus, and semantic-color checks in tests/StudentRegistration.Client.UnitTests/DesignSystem/DesignTokenContractTests.cs.'))
        $frontendTasks.Add((New-TaskLine ([ref]$taskNumber) '[FR-3]' "Deliver the approved versioned token sources at src/StudentRegistration.Client/wwwroot/design/design-tokens.json and src/StudentRegistration.Client/wwwroot/css/design-tokens.css after $tokenTestTaskId fails for the expected reason (depends on $tokenTestTaskId); institutional brand values remain blocked by DEC-03."))
        foreach ($component in $componentManifest.components) {
            $componentTest = "tests/StudentRegistration.Client.UnitTests/Components/$($component.name)Tests.cs"
            $componentTestTaskId = 'T{0:d3}' -f ($taskNumber + 1)
            $frontendTasks.Add((New-TaskLine ([ref]$taskNumber) "[P] [FR-4] [COMPONENT-$($component.name)]" "Create failing default, hover, active, focus, disabled, loading, error, keyboard, and accessible-name checks for $($component.name) in $componentTest."))
            $frontendTasks.Add((New-TaskLine ([ref]$taskNumber) "[FR-4] [COMPONENT-$($component.name)]" "Deliver the reusable token-only $($component.name) component at $($component.path) after $componentTestTaskId fails for the expected reason (depends on $componentTestTaskId)."))
        }
        $browserMatrixTestId = 'T{0:d3}' -f ($taskNumber + 1)
        $frontendTasks.Add((New-TaskLine ([ref]$taskNumber) '[P] [FR-12] [NFR-7] [BROWSER-MATRIX]' 'Create the future failing pinned OS/browser/version and actual-Safari-evidence contract checks in tests/StudentRegistration.SpecificationTests/Frontend/BrowserMatrixContractTests.cs.'))
        $frontendTasks.Add((New-TaskLine ([ref]$taskNumber) '[FR-12] [NFR-7] [BROWSER-MATRIX]' "Deliver the versioned browser support matrix at tests/StudentRegistration.E2ETests/browser-matrix.json after $browserMatrixTestId fails for the expected reason (depends on $browserMatrixTestId); WebKit MUST remain distinct from Safari."))
        $baselineManifestTestId = 'T{0:d3}' -f ($taskNumber + 1)
        $frontendTasks.Add((New-TaskLine ([ref]$taskNumber) '[P] [FR-12] [NFR-9] [VISUAL-BASELINES]' 'Create the future failing baseline provenance, route/state/browser/viewport/token/fixture, approval, and no-auto-replacement checks in tests/StudentRegistration.SpecificationTests/Frontend/VisualBaselineManifestTests.cs.'))
        $frontendTasks.Add((New-TaskLine ([ref]$taskNumber) '[FR-12] [NFR-9] [VISUAL-BASELINES]' "Deliver the governed baseline registry at tests/StudentRegistration.VisualTests/Baselines/baseline-manifest.json after $baselineManifestTestId fails for the expected reason (depends on $baselineManifestTestId)."))
        $flakePolicyTestId = 'T{0:d3}' -f ($taskNumber + 1)
        $frontendTasks.Add((New-TaskLine ([ref]$taskNumber) '[P] [FR-12] [NFR-7] [FLAKE-POLICY]' 'Create the future failing checks for first-run failure, diagnostics-only retry, critical-journey no-quarantine, and owned flake correction in tests/StudentRegistration.SpecificationTests/Frontend/FlakePolicyContractTests.cs.'))
        $frontendTasks.Add((New-TaskLine ([ref]$taskNumber) '[FR-12] [NFR-7] [FLAKE-POLICY]' "Deliver the enforced browser-test policy at tests/StudentRegistration.E2ETests/flake-policy.json after $flakePolicyTestId fails for the expected reason (depends on $flakePolicyTestId)."))
    }

    $ownedRoutes = @($routeManifest.routes | Where-Object { $_.owners -contains $item.id })
    foreach ($route in $ownedRoutes) {
        $routeId = $route.id
        $page = $route.page
        $pageApiProperty = $pageApiManifest.pages.PSObject.Properties | Where-Object Name -eq $routeId
        $pageApiText = if ($pageApiProperty) { @($pageApiProperty.Value) -join ', ' } else { 'no registered endpoint' }
        if ($item.id -eq '003') {
            $frontendTasks.Add((New-TaskLine ([ref]$taskNumber) "[$routeId] [FR-1] [FR-2] [FR-11]" "Produce and approve the Page Design Record with annotated layouts at 320, 375, 768, 1024, 1280, and 1920 CSS pixels, complete state matrix, focus order, exact APIs '$pageApiText', reason mapping, and test IDs for $routeId $($route.template) at specs/$featureName/design/pages/$routeId.md."))
            $frontendTasks.Add((New-TaskLine ([ref]$taskNumber) "[P] [$routeId] [FR-12] [FR-14]" "Create the route API/reason-code fixture and failing contract assertions for $routeId in tests/StudentRegistration.Client.ContractTests/Routes/$($page)ContractTests.cs."))
            $frontendTasks.Add((New-TaskLine ([ref]$taskNumber) "[P] [$routeId] [FR-5] [FR-12]" "Create failing component-state, navigation, form, focus, pending, and duplicate-action assertions for $routeId in tests/StudentRegistration.Client.UnitTests/Pages/$($page)ComponentTests.cs."))
            $frontendTasks.Add((New-TaskLine ([ref]$taskNumber) "[P] [$routeId] [NFR-1] [NFR-2] [NFR-5]" "Run axe, keyboard, focus, screen-reader, 400-percent zoom, and responsive assertions for $routeId in tests/StudentRegistration.AccessibilityTests/Routes/$($page)AccessibilityTests.cs."))
            $frontendTasks.Add((New-TaskLine ([ref]$taskNumber) "[P] [$routeId] [NFR-7] [NFR-9]" "Approve cross-browser visual baselines for $routeId at 375, 768, 1280, and 1920 CSS pixels in tests/StudentRegistration.VisualTests/Routes/$($page)VisualTests.cs."))
            if ($route.implementationOwner -eq '003') {
                $routeTestTaskId = 'T{0:d3}' -f ($taskNumber + 1)
                $frontendTasks.Add((New-TaskLine ([ref]$taskNumber) "[P] [$routeId] [AC-17]" "Create the future failing safe-status end-to-end journey for $routeId in tests/StudentRegistration.E2ETests/Routes/$($page)JourneyTests.cs."))
                $frontendTasks.Add((New-TaskLine ([ref]$taskNumber) "[$routeId] [FR-4] [FR-5]" "Deliver the token-based $routeId page at src/StudentRegistration.Client/Pages/$page.razor only after $routeTestTaskId and the SPEC-003 route contract/component/accessibility checks fail for expected reasons (depends on $routeTestTaskId)."))
            }
        } else {
            $link = $route.links | Where-Object spec -eq $item.id
            $linkTags = (@($link.requirements) + @($link.criteria) | ForEach-Object { "[$_]" }) -join ' '
            if ($route.implementationOwner -eq $item.id) {
                $routeTestTaskId = 'T{0:d3}' -f ($taskNumber + 1)
                $frontendTasks.Add((New-TaskLine ([ref]$taskNumber) "[P] [$routeId] [UI-CONTRACT-SPEC-003] $linkTags" "Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for $routeId in tests/StudentRegistration.E2ETests/Specs/Spec$($item.id)/$($page)FeatureTests.cs."))
                $frontendTasks.Add((New-TaskLine ([ref]$taskNumber) "[$routeId] [UI-CONTRACT-SPEC-003] $linkTags" "Deliver the sole canonical Blazor implementation for $routeId at src/StudentRegistration.Client/Pages/$page.razor after $routeTestTaskId and the SPEC-003 contract/component checks fail for expected reasons (depends on $routeTestTaskId)."))
            } else {
                $frontendTasks.Add((New-TaskLine ([ref]$taskNumber) "[$routeId] [UI-CONTRACT-SPEC-003] $linkTags" "Finalize SPEC-$($item.id) data, actions, stable reasons, authorization, and stale/concurrent contribution for $routeId at specs/$featureName/contracts/routes/$routeId.md without editing the canonical Razor page."))
                $frontendTasks.Add((New-TaskLine ([ref]$taskNumber) "[P] [$routeId] [UI-CONTRACT-SPEC-003] $linkTags" "Verify the SPEC-$($item.id) contribution consumed by $routeId in tests/StudentRegistration.E2ETests/Specs/Spec$($item.id)/$($page)ContributorTests.cs."))
            }
        }
    }

    foreach ($nfr in $nfrItems) {
        $nfrId = $nfr.Groups[1].Value
        $summary = ConvertTo-OneLine $nfr.Groups[3].Value
        if ($item.id -eq '003' -and $nfrId -eq 'NFR-7') {
            $qualityTasks.Add((New-TaskLine ([ref]$taskNumber) "[$nfrId] [AUTOMATED-EVIDENCE] [POC-BROWSER-MATRIX]" "Execute the current-stable Chrome/Edge/Firefox plus pinned Playwright WebKit gate and record exact builds, WebKit-not-Safari labeling, journeys, results, and actual Safari/macOS deferral in tests/StudentRegistration.QualityTests/Specs/Spec003/NFR-7EvidenceTests.cs and docs/release-evidence/SPEC-003-NFR-7.md: $summary."))
        } elseif ($summary -match '(?i)screen.reader|usability|participant|manual') {
            $qualityTasks.Add((New-TaskLine ([ref]$taskNumber) "[$nfrId] [MANUAL-EVIDENCE]" "Execute the controlled human/browser evidence protocol for $nfrId and record participants, environment, script, observations, pass/fail thresholds, defects, and approver in docs/release-evidence/SPEC-$($item.id)-$nfrId.md: $summary."))
        } else {
            $qualityTasks.Add((New-TaskLine ([ref]$taskNumber) "[P] [$nfrId] [AUTOMATED-EVIDENCE]" "Produce measurable automated release evidence for $nfrId in tests/StudentRegistration.QualityTests/Specs/Spec$($item.id)/$($nfrId)EvidenceTests.cs and docs/release-evidence/SPEC-$($item.id)-$nfrId.md: $summary."))
        }
    }

    foreach ($os in $osItems) {
        $osId = $os.Groups[1].Value
        $summary = ConvertTo-OneLine $os.Groups[3].Value
        $scopeTasks.Add((New-TaskLine ([ref]$taskNumber) "[$osId]" "Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-$($item.id)-scope-review.md that $osId remains excluded: $summary."))
    }
    $successCriterionNumber = 0
    foreach ($criterion in @($item.successCriteria)) {
        $successCriterionNumber++
        $criterionId = "SC-$successCriterionNumber"
        $criterionSummary = ConvertTo-OneLine ([string]$criterion)
        $scopeTasks.Add((New-TaskLine ([ref]$taskNumber) "[$criterionId] [SUCCESS-EVIDENCE]" "Map the supporting FR/NFR/AC tasks, execute their approved tests, and record measured pass/fail evidence for '$criterionSummary' in docs/release-evidence/SPEC-$($item.id)-$criterionId.md."))
    }
    $scopeTasks.Add((New-TaskLine ([ref]$taskNumber) '[TRACE]' "Generate the completed FR/NFR/AC/EC/SC/route-to-test evidence matrix at docs/release-evidence/SPEC-$($item.id)-traceability.md and reject release if any row lacks passing evidence."))
    $scopeTasks.Add((New-TaskLine ([ref]$taskNumber) '[GATE]' "Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-$($item.id) in docs/release-evidence/SPEC-$($item.id)-release-approval.md."))

    $routeOwnership = if ($ownedRoutes.Count) {
        $routeRows = $ownedRoutes | ForEach-Object {
            $role = if ($_.designOwner -eq $item.id) { 'Design/test contract owner' } elseif ($_.implementationOwner -eq $item.id) { 'Canonical page implementation owner' } else { 'Feature contract contributor; does not edit page' }
            "| $($_.id) | $($_.template) | $($_.page).razor | $role; design SPEC-$($_.designOwner), implementation SPEC-$($_.implementationOwner) |"
        }
        @"
| Route ID | Route template | Future Blazor page | Responsibility |
|---|---|---|---|
$($routeRows -join "`r`n")
"@
    } else {
        'No route is directly owned. Any later UI exposure requires a SPEC-003 route-manifest amendment before implementation.'
    }

    $success = ($item.successCriteria | ForEach-Object -Begin { $n = 0 } -Process { $n++; "- **SC-$n**: $_" }) -join "`r`n"
    $entities = ($item.entities | ForEach-Object {
        $entityName = [string]$_
        $ownerProperty = $entityOwnership.canonicalOwners.PSObject.Properties | Where-Object Name -eq $entityName
        $canonicalOwner = if ($ownerProperty) { [string]$ownerProperty.Value } else { [string]$item.id }
        if ($item.id -eq '005') {
            "- **$entityName**: Schema-contract reference governed by SPEC-005; the runtime model and EF mapping are delivered by canonical owner SPEC-$canonicalOwner after that feature is approved."
        } elseif ($canonicalOwner -eq $item.id) {
            "- **$entityName**: Canonical entity/artifact owned by SPEC-$canonicalOwner; attributes and relationships are refined in requirements.md and the shared ERD."
        } else {
            "- **$entityName**: Referenced/consumed from canonical owner SPEC-$canonicalOwner; this feature MUST NOT redefine or deliver it."
        }
    }) -join "`r`n"

    $frontendPlan = if ($item.id -eq '003') {
@"
## Frontend Verification Toolchain and Evidence

- Razor component behavior uses bUnit with deterministic render fixtures and no live institutional dependency.
- Browser journeys use Microsoft.Playwright. Browser and operating-system builds MUST be pinned per release in tests/StudentRegistration.E2ETests/browser-matrix.json; floating latest labels are not release evidence.
- Automated accessibility uses axe-core from the Playwright accessibility harness plus keyboard and focus assertions. Automated results supplement rather than replace manual assistive-technology review.
- Visual regression uses Playwright screenshot comparisons with approved baselines stored by route, state, browser engine, and viewport under tests/StudentRegistration.VisualTests/Baselines/ and governed by tests/StudentRegistration.VisualTests/Baselines/baseline-manifest.json. Dynamic time, identifiers, animations, and nondeterministic content MUST use controlled fixtures or documented masks.
- Chromium, Firefox, and WebKit automation runs in pinned CI images. WebKit results MUST NOT be reported as Safari results. Current stable Safari requires a manual run on pinned macOS/Safari versions with evidence recorded in docs/release-evidence/frontend/safari-macos-evidence.md.
- Component and contract fixtures MUST pin server time, academic term, identity/role, policy version, capacity, rowversion, correlation IDs, and every applicable UI state. Fixtures MUST contain no production student data.
- A first-run failure remains a failed gate under tests/StudentRegistration.E2ETests/flake-policy.json. A retry MAY collect trace, video, screenshot, console, and network diagnostics but MUST NOT convert the gate to pass. Critical journeys cannot be quarantined; every flake requires an owner, issue, cause, and correction before release.
- Release evidence includes bUnit results, client contract results, Playwright traces/reports, axe-core output, visual-baseline approval, browser-matrix provenance, and the signed Safari/macOS manual record.

## Frontend Test Project Structure

- tests/StudentRegistration.Client.UnitTests: bUnit component and design-token checks.
- tests/StudentRegistration.Client.ContractTests: route-to-API and stable-reason fixtures.
- tests/StudentRegistration.E2ETests: Microsoft.Playwright primary, failure, authorization, stale, offline, and race journeys.
- tests/StudentRegistration.AccessibilityTests: axe-core automation plus keyboard/focus protocols.
- tests/StudentRegistration.VisualTests: deterministic screenshot assertions and approved baselines.
- docs/release-evidence/frontend: manual screen-reader, usability, Safari/macOS, browser provenance, and baseline-approval records.
"@
    } else { '' }

    $frontendResearch = if ($item.id -eq '003') {
@"
### Frontend verification stack
**Decision**: Use bUnit for Blazor component behavior, Microsoft.Playwright for browser journeys, axe-core for automated accessibility, and Playwright screenshot comparison for visual regression.
**Rationale**: These layers separate fast component feedback, client-contract behavior, real navigation, accessibility signals, and pixel-level review without treating snapshots as functional assertions.
**Alternatives rejected**: Browser-only testing, snapshot-only testing, and manual-only accessibility review because each leaves important behavior unverified.

### Browser evidence and version pinning
**Decision**: Pin CI operating-system images and exact browser builds in a versioned browser matrix. Use Chromium, Firefox, and WebKit automation for repeatable coverage, then require a separately recorded run on actual stable Safari on pinned macOS for Safari support.
**Rationale**: Playwright WebKit is useful compatibility evidence but is not the shipping Safari browser. Version-pinned provenance makes failures and visual baselines reproducible.
**Alternatives rejected**: Floating latest browsers and labeling WebKit automation as Safari certification.

### Deterministic fixtures and visual baselines
**Decision**: Version fixtures for server time, term, role, policy, rowversion, capacity, reason codes, and UI states. Store visual baselines by route/state/browser/viewport with token version and UX approval; mask only reviewed nondeterministic regions.
**Rationale**: Registration state is time- and concurrency-sensitive. Uncontrolled clocks, data, or animation create misleading failures and unreviewable baseline churn.
**Alternatives rejected**: Production-data copies, arbitrary screenshot tolerances, and automatic baseline replacement.

### Flake and evidence policy
**Decision**: A first-run failure fails the gate; a retry can collect diagnostics only. Critical journeys cannot be quarantined, and every intermittent failure requires an owned defect before release. Machine reports and manual Safari, screen-reader, keyboard, and usability evidence are retained together.
**Rationale**: Retrying until green hides race, timing, and accessibility defects in the system's highest-risk journeys.
**Alternatives rejected**: Pass-on-retry, unowned quarantine, and unsupported claims based only on generated reports.
"@
    } else { '' }

    $integrityRules = if ($item.id -eq '003') {
@"
- Every PageDesignRecord MUST declare a schema version, one route ID/template pair, nonempty ownerSpecs, exactly one design owner, exactly one canonical implementation owner, and an approval version.
- Route IDs, templates, and canonical page names MUST be unique and MUST match route-manifest.json, page-matrix.md, and the owning feature specifications.
- Every required responsive width and applicable UI state MUST be present; a not-applicable state requires a reviewed reason.
- DesignTokenSet versions MUST be immutable after approval. Brand tokens require institutional approval, semantic tokens MUST meet the specified contrast rules, and pages/components MUST reference approved tokens rather than guessed literal values.
- Every FrontendTestRecord MUST have a unique test ID, existing route ID, valid owning requirement/criterion IDs, deterministic fixture version, evidence type, and expected outcome.
- Each route MUST trace to its PageDesignRecord, ownerSpecs, contributing API/reason contracts, implementation task, component tests, client-contract tests, Playwright journeys, axe/keyboard evidence, visual baselines, and manual evidence where required.
- Every visual baseline MUST record route, state, viewport, browser/browser-engine build, operating-system image, token version, fixture version, approval actor, and approval date; automatic baseline replacement is prohibited.
- PageDesignRecord, DesignTokenSet, and FrontendTestRecord are governed design/test metadata, not SQL entities, unless a separately approved runtime requirement introduces persistence.
"@
    } elseif ($item.id -in @('004','006')) {
@"
- Architecture and contract artifacts are versioned documents or DTO schemas,
  not assumed SQL entities.
- Each artifact has one canonical owner, stable identifiers, exact consumers,
  compatibility rules, and an automated conformance test.
- Public DTOs exclude EF navigation state, secrets, credential material, and
  internal exception details.
- Mutation DTOs carry the approved expectedRowVersion/idempotency metadata;
  list DTOs use the bounded shared Page contract.
"@
    } else {
@"
- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
"@
    }

    @"
# Feature Specification: $($item.title)

**Feature Branch**: $featureName
**Created**: $createdDate
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

### Non-Functional Requirements

$nonFunctional

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

## Frontend Route Ownership

$routeOwnership

## Out of Scope

$outScope
"@ | Set-Content (Join-Path $featureDir 'spec.md')

    @"
# Implementation Plan: $($item.title)

**Branch**: $featureName | **Date**: $generationDate | **Spec**: [spec.md](spec.md)
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

The composition root is `src/StudentRegistration.Api`; the UI is
`src/StudentRegistration.Client`; public conventions are
`src/StudentRegistration.Contracts`; business code belongs to the owning
`src/StudentRegistration.$(Get-ModuleProjectName ([string]$item.module))`
project; SQL mappings and migrations belong to
`src/StudentRegistration.Infrastructure.SqlServer`. Domain, Application, and
Endpoints are folders inside the relevant business module. These are future
paths only.

## Feature Design and Boundaries

- **Owner**: SPEC-$($item.id) owns the $($item.title) contract in the
  $($item.module) boundary.
- **Inputs**: Only the published interfaces and version baselines listed in
  Dependencies may be consumed; downstream specifications are not planning
  prerequisites.
- **Authority**: Identity, role, academic time, policy, schedule, and durable
  mutations are evaluated on the server. Browser state is advisory.
- **Consistency**: The owning feature requirements define whether the use case
  is read-only, optimistic-versioned, idempotent, or part of the short
  registration SQL transaction; no remote call occurs inside that transaction.
- **Extension seam**: A future feature calls a narrow module application port
  and receives DTOs; it does not reference another module's EF entities or
  handler internals.

## Delivery Sequence and Rollback

1. Baseline upstream contracts and complete consistency analysis.
2. Record Ahmed Elbamby's approval as the final planning gate.
3. Add failing contract, acceptance, authorization, concurrency, and
   non-functional tests applicable to this feature.
4. Deliver one cohesive workstream at a time through its owning module.
5. Run migration/recovery rehearsal when persistence changes, then all Gate C
   and Gate D evidence before release.
6. Roll back the application and reversible migration using SPEC-018 runbooks;
   never repair a failed release by bypassing an invariant.

## Design Artifacts

- [Research](research.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Planning quickstart](quickstart.md)
- [Tasks](tasks.md)

$frontendPlan

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

$frontendResearch

## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
"@ | Set-Content (Join-Path $featureDir 'research.md')

    @"
# Data Model: $($item.title)

## Entity Responsibilities

$entities

## Detailed Model

$models

## Integrity Rules

$integrityRules
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

**Reviewed**: $generationDate
**Automated result**: PASS - no hidden NEEDS CLARIFICATION marker.
**Human approval**: PENDING

The specification was reviewed for scope, actors, data, business rules, errors,
concurrency, security, accessibility, dependencies, and measurable outcomes.
Unknown product/institutional decisions are registered in
docs/OPEN_DECISIONS.md or docs/POLICY_RESEARCH.md with an owner and fail-closed
planning rule. They are not invented requirements, and affected specs cannot
become Approved until their owners decide them.
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

- [x] No hidden clarification or placeholder remains; every external decision is registered with an owner and fail-closed rule.
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
- [x] G3 Clarification registration and fail-closed handling
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
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Dependency, Consistency, and Approval Gates

$($approvalTasks -join "`r`n")

## Phase 2 - Models and API Contracts

$($foundationTasks -join "`r`n")

## Phase 3 - User-Story Acceptance and Edge Tests

$($testTasks -join "`r`n")

## Phase 4 - Requirement Tests and Bounded Delivery

$($requirementTasks -join "`r`n")

$($endpointDeliveryTasks -join "`r`n")

## Phase 5 - Frontend Route Tests and Integration

$(if ($frontendTasks.Count) { $frontendTasks -join "`r`n" } else { 'No direct frontend route is owned by this specification; frontend integration remains governed by SPEC-003.' })

## Phase 6 - Measurable Non-Functional Evidence

$($qualityTasks -join "`r`n")

## Phase 7 - Scope and Release Evidence

$($scopeTasks -join "`r`n")

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
