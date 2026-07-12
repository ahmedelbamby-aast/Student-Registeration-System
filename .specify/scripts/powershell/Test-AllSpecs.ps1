[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$manifest = Get-Content (Join-Path $root '.specify/spec-manifest.json') -Raw | ConvertFrom-Json
$routeManifest = Get-Content (Join-Path $root '.specify/route-manifest.json') -Raw | ConvertFrom-Json
$endpointManifest = Get-Content (Join-Path $root '.specify/endpoint-manifest.json') -Raw | ConvertFrom-Json
$componentManifest = Get-Content (Join-Path $root '.specify/component-manifest.json') -Raw | ConvertFrom-Json
$workstreamManifest = Get-Content (Join-Path $root '.specify/workstream-manifest.json') -Raw | ConvertFrom-Json
$entityOwnership = Get-Content (Join-Path $root '.specify/entity-ownership.json') -Raw | ConvertFrom-Json
$auditDate = Get-Date -Format 'yyyy-MM-dd'
$failures = New-Object System.Collections.Generic.List[string]
$results = New-Object System.Collections.Generic.List[object]
$required = @('spec.md','requirements.md','plan.md','research.md','data-model.md','quickstart.md','clarifications.md','contracts/api.md','checklists/requirements.md','checklists/gates.md','tasks.md')

function Add-Failure([string]$Message) { $script:failures.Add($Message) }
function One-Line([string]$Value) { return ([regex]::Replace($Value, '\s+', ' ')).Trim() }
function Has-Property([object]$Value, [string]$Name) {
    return $null -ne $Value -and $null -ne $Value.PSObject.Properties[$Name]
}
function Assert-RequiredProperties([string]$Subject, [object]$Value, [string[]]$Names) {
    foreach ($name in $Names) {
        if (-not (Has-Property $Value $name)) {
            Add-Failure "$Subject is missing required property $name."
            continue
        }
        $propertyValue = $Value.PSObject.Properties[$name].Value
        if ($propertyValue -is [string] -and [string]::IsNullOrWhiteSpace($propertyValue)) {
            Add-Failure "$Subject has empty required property $name."
        }
    }
}
function Test-ExactFuturePath([string]$Value) {
    return $Value -match '(?:src|tests|specs|docs|\.github)/[^\s,;]+\.[A-Za-z0-9]+' -or
        $Value -match '(?<![A-Za-z0-9_./-])global\.json(?![A-Za-z0-9_./-])'
}
function Get-TaskNumber([object]$TaskMatch) {
    return [int]([regex]::Match($TaskMatch.Groups[1].Value, '\d+$').Value)
}
function Test-TaskTags([string]$Body, [string[]]$Tags) {
    foreach ($tag in $Tags) {
        if ($Body -notmatch "\[$([regex]::Escape($tag))\]") { return $false }
    }
    return $true
}
function Get-EntityArtifactPath([string]$SpecId, [string]$Entity) {
    $item = $manifest.specs | Where-Object id -eq $SpecId
    $ownerProperty = $entityOwnership.canonicalOwners.PSObject.Properties | Where-Object Name -eq $Entity
    $canonicalOwner = if ($ownerProperty) { [string]$ownerProperty.Value } else { $SpecId }
    $ownerSpec = $manifest.specs | Where-Object id -eq $canonicalOwner
    $ownerModule = ([string]$ownerSpec.module) -replace '[^A-Za-z0-9]', ''
    $path = "src/StudentRegistration.Domain/Modules/$ownerModule/$Entity.cs"
    $overrideKey = "$SpecId`:$Entity"
    $overrideProperty = $entityOwnership.artifactOverrides.PSObject.Properties | Where-Object Name -eq $overrideKey
    if ($overrideProperty) { return [string]$overrideProperty.Value }
    if ($SpecId -eq '005') { return "src/StudentRegistration.Infrastructure/Persistence/Configurations/$($Entity)Configuration.cs" }
    if ($SpecId -eq '006') { return "src/StudentRegistration.Contracts/$Entity.cs" }
    if ($SpecId -eq '003') { return "src/StudentRegistration.Client/Features/Frontend/Models/$Entity.cs" }
    if ($canonicalOwner -ne $SpecId) {
        $ownerOverrideKey = "$canonicalOwner`:$Entity"
        $ownerOverride = $entityOwnership.artifactOverrides.PSObject.Properties | Where-Object Name -eq $ownerOverrideKey
        if ($ownerOverride) { return [string]$ownerOverride.Value }
    }
    return $path
}
function Get-Ids([string]$Text, [string]$Pattern) {
    return @([regex]::Matches($Text, $Pattern) | ForEach-Object { $_.Groups[1].Value })
}
function Assert-Sequential([string]$SpecId, [string]$Kind, [string[]]$Values) {
    if (($Values | Select-Object -Unique).Count -ne $Values.Count) {
        Add-Failure "SPEC-$SpecId contains duplicate $Kind identifiers."
        return
    }
    $numbers = @($Values | ForEach-Object { [int]([regex]::Match($_, '\d+$').Value) })
    for ($i = 0; $i -lt $numbers.Count; $i++) {
        if ($numbers[$i] -ne ($i + 1)) {
            Add-Failure "SPEC-$SpecId $Kind identifiers are not sequential at $($Values[$i])."
            return
        }
    }
}

if ($manifest.specs.Count -ne 18) { Add-Failure "Manifest contains $($manifest.specs.Count) specs instead of 18." }
$ids = @($manifest.specs.id)
if (($ids | Select-Object -Unique).Count -ne $ids.Count) { Add-Failure 'Manifest contains duplicate IDs.' }
$specSlugs = @($manifest.specs.slug)
$specTitles = @($manifest.specs.title)
if (($specSlugs | Select-Object -Unique).Count -ne $specSlugs.Count) { Add-Failure 'Spec manifest contains duplicate slugs.' }
if (($specTitles | Select-Object -Unique).Count -ne $specTitles.Count) { Add-Failure 'Spec manifest contains duplicate titles.' }
foreach ($item in $manifest.specs) {
    Assert-RequiredProperties "SPEC manifest entry $($item.id)" $item @('id','slug','title','owner','actor','module','dependencies','entities','successCriteria')
    if ([string]$item.id -notmatch '^\d{3}$') { Add-Failure "Spec manifest ID $($item.id) is not three digits." }
    if ([string]$item.slug -notmatch '^[a-z0-9]+(?:-[a-z0-9]+)*$') { Add-Failure "SPEC-$($item.id) slug is not canonical kebab-case." }
    if (@($item.entities).Count -eq 0) { Add-Failure "SPEC-$($item.id) has no declared entities/artifacts." }
    if (@($item.successCriteria).Count -eq 0) { Add-Failure "SPEC-$($item.id) has no success criteria." }
    if ((@($item.dependencies) | Select-Object -Unique).Count -ne @($item.dependencies).Count) { Add-Failure "SPEC-$($item.id) contains duplicate dependencies." }
    if ((@($item.entities) | Select-Object -Unique).Count -ne @($item.entities).Count) { Add-Failure "SPEC-$($item.id) contains duplicate entities." }
}

if (-not (Has-Property $endpointManifest 'version') -or -not (Has-Property $endpointManifest 'endpoints')) {
    Add-Failure 'Endpoint manifest must contain version and endpoints.'
}
$endpointKeys = New-Object System.Collections.Generic.List[string]
foreach ($endpoint in @($endpointManifest.endpoints)) {
    Assert-RequiredProperties 'Endpoint manifest entry' $endpoint @('method','path','owner')
    $method = ([string]$endpoint.method).ToUpperInvariant()
    $key = "$method $($endpoint.path)"
    $endpointKeys.Add($key)
    if ($method -notin @('GET','POST','PUT','PATCH','DELETE')) { Add-Failure "Endpoint $key uses an unsupported method." }
    if ([string]$endpoint.path -notmatch '^/api/[A-Za-z0-9_{}?=&/\-]+$') { Add-Failure "Endpoint $key has an invalid canonical API path." }
    if ($ids -notcontains [string]$endpoint.owner) { Add-Failure "Endpoint $key has missing owner SPEC-$($endpoint.owner)." }
}
if (($endpointKeys | Select-Object -Unique).Count -ne $endpointKeys.Count) { Add-Failure 'Endpoint manifest contains duplicate method/path keys.' }

if (-not (Has-Property $componentManifest 'version') -or -not (Has-Property $componentManifest 'components')) {
    Add-Failure 'Component manifest must contain version and components.'
}
$componentNames = @($componentManifest.components.name)
$componentPaths = @($componentManifest.components.path)
if (($componentNames | Select-Object -Unique).Count -ne $componentNames.Count) { Add-Failure 'Component manifest contains duplicate names.' }
if (($componentPaths | Select-Object -Unique).Count -ne $componentPaths.Count) { Add-Failure 'Component manifest contains duplicate paths.' }
$requiredComponentNames = @(
    'AppShell','RoleNavigation','Button','AppLink','FormField','ValidationSummary',
    'SearchFilter','EntityCard','StatusBadge','StatePanel','Alert',
    'ConfirmationDialog','DataTable','Pagination','GroupCard',
    'CapacityIndicator','ScheduleCalendar','ScheduleList','ConflictPanel',
    'ReceiptSummary'
)
foreach ($requiredComponent in $requiredComponentNames) {
    if ($componentNames -notcontains $requiredComponent) { Add-Failure "Component manifest omits FR-4 component category $requiredComponent." }
}
foreach ($component in @($componentManifest.components)) {
    Assert-RequiredProperties 'Component manifest entry' $component @('name','path')
    if ([string]$component.name -notmatch '^[A-Za-z][A-Za-z0-9]*$') { Add-Failure "Component $($component.name) has an invalid name." }
    if ([string]$component.path -notmatch '^src/StudentRegistration\.Client/.+\.razor$') { Add-Failure "Component $($component.name) has invalid Razor path $($component.path)." }
}

if (-not (Has-Property $workstreamManifest 'version') -or -not (Has-Property $workstreamManifest 'specs')) {
    Add-Failure 'Workstream manifest must contain version and specs.'
}
$workstreamSpecIds = @($workstreamManifest.specs.PSObject.Properties.Name)
if (($workstreamSpecIds | Select-Object -Unique).Count -ne $workstreamSpecIds.Count) { Add-Failure 'Workstream manifest contains duplicate spec keys.' }
if ((Compare-Object @($ids | Sort-Object) @($workstreamSpecIds | Sort-Object))) { Add-Failure 'Workstream manifest spec keys do not exactly match the spec manifest.' }
foreach ($specId in $workstreamSpecIds) {
    $streams = @($workstreamManifest.specs.PSObject.Properties[$specId].Value)
    if ($streams.Count -eq 0) { Add-Failure "SPEC-$specId has no workstreams."; continue }
    $streamNames = @($streams.name)
    $streamTestPaths = @($streams.testPath)
    if (($streamNames | Select-Object -Unique).Count -ne $streamNames.Count) { Add-Failure "SPEC-$specId has duplicate workstream names." }
    if (($streamTestPaths | Select-Object -Unique).Count -ne $streamTestPaths.Count) { Add-Failure "SPEC-$specId has duplicate workstream test paths." }
    foreach ($stream in $streams) {
        Assert-RequiredProperties "SPEC-$specId workstream" $stream @('name','requirements','deliveryPath','testPath','testFocus')
        if (@($stream.requirements).Count -eq 0) { Add-Failure "SPEC-$specId workstream $($stream.name) has no FR mapping." }
        if ((@($stream.requirements) | Select-Object -Unique).Count -ne @($stream.requirements).Count) { Add-Failure "SPEC-$specId workstream $($stream.name) repeats an FR." }
        if (-not (Test-ExactFuturePath ([string]$stream.deliveryPath))) { Add-Failure "SPEC-$specId workstream $($stream.name) has invalid deliveryPath $($stream.deliveryPath)." }
        if ([string]$stream.testPath -notmatch '^tests/.+\.[A-Za-z0-9]+$') { Add-Failure "SPEC-$specId workstream $($stream.name) has invalid testPath $($stream.testPath)." }
        if ([string]$stream.testFocus -notmatch '\S+\s+\S+') { Add-Failure "SPEC-$specId workstream $($stream.name) has an underspecified testFocus." }
    }
}

if (-not (Has-Property $entityOwnership 'version') -or -not (Has-Property $entityOwnership 'canonicalOwners') -or -not (Has-Property $entityOwnership 'artifactOverrides')) {
    Add-Failure 'Entity ownership manifest must contain version, canonicalOwners, and artifactOverrides.'
}
$entityDeclarations = New-Object System.Collections.Generic.List[object]
foreach ($item in $manifest.specs) {
    foreach ($entity in @($item.entities)) { $entityDeclarations.Add([pscustomobject]@{ entity = [string]$entity; spec = [string]$item.id }) }
}
foreach ($group in ($entityDeclarations | Group-Object entity)) {
    $ownerProperty = $entityOwnership.canonicalOwners.PSObject.Properties[$group.Name]
    if ($group.Count -gt 1 -and $null -eq $ownerProperty) { Add-Failure "Shared entity $($group.Name) has no canonical owner."; continue }
    $ownerId = if ($ownerProperty) { [string]$ownerProperty.Value } else { [string]$group.Group[0].spec }
    if ($ids -notcontains $ownerId) { Add-Failure "Entity $($group.Name) has missing canonical owner SPEC-$ownerId."; continue }
    if (-not ($group.Group | Where-Object spec -eq $ownerId)) { Add-Failure "Canonical owner SPEC-$ownerId does not declare entity $($group.Name)." }
}
foreach ($property in $entityOwnership.canonicalOwners.PSObject.Properties) {
    if (-not ($entityDeclarations | Where-Object entity -eq $property.Name)) { Add-Failure "Entity ownership declares unknown entity $($property.Name)." }
}
foreach ($property in $entityOwnership.artifactOverrides.PSObject.Properties) {
    if ($property.Name -notmatch '^(\d{3}):(.+)$') { Add-Failure "Artifact override $($property.Name) has an invalid key."; continue }
    $overrideSpec = $Matches[1]
    $overrideEntity = $Matches[2]
    $declaringSpec = $manifest.specs | Where-Object id -eq $overrideSpec
    if ($null -eq $declaringSpec -or @($declaringSpec.entities) -notcontains $overrideEntity) { Add-Failure "Artifact override $($property.Name) does not reference a declared spec entity." }
    if (-not (Test-ExactFuturePath ([string]$property.Value))) { Add-Failure "Artifact override $($property.Name) has invalid path $($property.Value)." }
}

if ($routeManifest.routes.Count -ne 27) { Add-Failure "Route manifest contains $($routeManifest.routes.Count) routes instead of 27." }
$routeIds = @($routeManifest.routes.id)
if (($routeIds | Select-Object -Unique).Count -ne 27) { Add-Failure 'Route manifest route IDs are not unique.' }
$routeTemplates = @($routeManifest.routes.template)
$routePages = @($routeManifest.routes.page)
if (($routeTemplates | Select-Object -Unique).Count -ne $routeTemplates.Count) { Add-Failure 'Route manifest templates are not unique.' }
if (($routePages | Select-Object -Unique).Count -ne $routePages.Count) { Add-Failure 'Route manifest page names are not unique.' }
foreach ($route in $routeManifest.routes) {
    Assert-RequiredProperties "Route $($route.id)" $route @('id','template','page','designOwner','implementationOwner','owners','links')
    if ([string]$route.id -notmatch '^(?:AUTH|STU|ADM|STF|SYS)-\d{2}$') { Add-Failure "Route ID $($route.id) is invalid." }
    if ([string]$route.page -notmatch '^[A-Za-z][A-Za-z0-9]*Page$') { Add-Failure "Route $($route.id) page name is invalid." }
    if ($ids -notcontains [string]$route.designOwner) { Add-Failure "$($route.id) has missing design owner SPEC-$($route.designOwner)." }
    if ($ids -notcontains [string]$route.implementationOwner) { Add-Failure "$($route.id) has missing implementation owner SPEC-$($route.implementationOwner)." }
    if ($route.owners -notcontains [string]$route.designOwner) { Add-Failure "$($route.id) owners omit its design owner." }
    if ($route.owners -notcontains [string]$route.implementationOwner) { Add-Failure "$($route.id) owners omit its implementation owner." }
    if ($route.owners -notcontains '003') { Add-Failure "$($route.id) is not governed by SPEC-003." }
    if ($route.owners.Count -lt 2) { Add-Failure "$($route.id) has no feature owner in addition to SPEC-003." }
    if ((@($route.owners) | Select-Object -Unique).Count -ne @($route.owners).Count) { Add-Failure "$($route.id) contains duplicate owners." }
    foreach ($owner in $route.owners) {
        if ($ids -notcontains $owner) { Add-Failure "$($route.id) references missing owner SPEC-$owner." }
    }
    foreach ($link in @($route.links)) {
        Assert-RequiredProperties "$($route.id) link" $link @('spec','requirements','criteria')
        if ($ids -notcontains [string]$link.spec) { Add-Failure "$($route.id) links missing SPEC-$($link.spec)."; continue }
        if ($route.owners -notcontains [string]$link.spec) { Add-Failure "$($route.id) link SPEC-$($link.spec) is not a route owner/contributor." }
        $linkedSpecId = [string]$link.spec
        $linkedItem = $manifest.specs | Where-Object id -eq $linkedSpecId
        $linkedDir = Join-Path $root "specs/$($linkedItem.id)-$($linkedItem.slug)"
        $linkedRequirements = if (Test-Path (Join-Path $linkedDir 'requirements.md')) { Get-Content (Join-Path $linkedDir 'requirements.md') -Raw } else { '' }
        $definedFrs = Get-Ids $linkedRequirements '(?m)^- (FR-\d+):'
        $definedAcs = Get-Ids $linkedRequirements '(?m)^### (AC-\d+):'
        if (@($link.requirements).Count -eq 0 -or @($link.criteria).Count -eq 0) { Add-Failure "$($route.id) link to SPEC-$($link.spec) must contain FR and AC references." }
        foreach ($fr in @($link.requirements)) { if ($definedFrs -notcontains [string]$fr) { Add-Failure "$($route.id) links undefined SPEC-$($link.spec)/$fr." } }
        foreach ($ac in @($link.criteria)) { if ($definedAcs -notcontains [string]$ac) { Add-Failure "$($route.id) links undefined SPEC-$($link.spec)/$ac." } }
    }
}

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

function Test-ReachesRoot([string]$Id, [hashtable]$Seen) {
    if ($Id -eq '001') { return $true }
    if ($Seen[$Id]) { return $false }
    $Seen[$Id] = $true
    $node = $manifest.specs | Where-Object id -eq $Id
    foreach ($dep in $node.dependencies) {
        if (Test-ReachesRoot $dep $Seen) { return $true }
    }
    return $false
}
foreach ($id in $ids) {
    if (-not (Test-ReachesRoot $id @{})) { Add-Failure "SPEC-$id is not connected to root SPEC-001." }
}

$taskRegistry = New-Object System.Collections.Generic.List[object]
foreach ($taskSpec in $manifest.specs) {
    $taskFeatureName = "$($taskSpec.id)-$($taskSpec.slug)"
    $taskFile = Join-Path $root "specs/$taskFeatureName/tasks.md"
    if (-not (Test-Path $taskFile -PathType Leaf)) { continue }
    $taskText = Get-Content $taskFile -Raw
    foreach ($taskMatch in [regex]::Matches($taskText, '(?m)^- \[ \] (T\d{3})\s+(.+)$')) {
        $taskRegistry.Add([pscustomobject]@{
            spec = [string]$taskSpec.id
            id = $taskMatch.Groups[1].Value
            number = Get-TaskNumber $taskMatch
            body = $taskMatch.Groups[2].Value
            value = $taskMatch.Value
        })
    }
}

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
    if ($combined -match '\[NEEDS CLARIFICATION\]|\[FEATURE NAME\]|\[DATE\]|\[###-feature-name\]|TKTK|\bTODO\b') {
        Add-Failure "SPEC-$($item.id) contains unresolved placeholders."
    }
    $uncheckedGates = Select-String -Path (Join-Path $dir 'checklists/*.md') -Pattern '^- \[ \]' -ErrorAction SilentlyContinue
    if ($uncheckedGates) { Add-Failure "SPEC-$($item.id) has unresolved planning checklist items." }

    $requirements = Get-Content (Join-Path $dir 'requirements.md') -Raw
    $spec = Get-Content (Join-Path $dir 'spec.md') -Raw
    $tasks = Get-Content (Join-Path $dir 'tasks.md') -Raw
    $model = Get-Content (Join-Path $dir 'data-model.md') -Raw
    $apiContract = Get-Content (Join-Path $dir 'contracts/api.md') -Raw

    if ($requirements -notmatch "^# SPEC-$($item.id): $([regex]::Escape($item.title))") {
        Add-Failure "SPEC-$($item.id) title does not match the manifest."
    }
    $depLine = [regex]::Match($requirements, '(?m)^\*\*Dependencies:\*\*\s*([^<\r\n]+)').Groups[1].Value
    $declaredDeps = @([regex]::Matches($depLine, 'SPEC-(\d{3})') | ForEach-Object { $_.Groups[1].Value })
    if ((Compare-Object @($item.dependencies | Sort-Object) @($declaredDeps | Sort-Object))) {
        Add-Failure "SPEC-$($item.id) requirements dependencies do not match the manifest."
    }

    $frs = Get-Ids $requirements '(?m)^- (FR-\d+):'
    $nfrs = Get-Ids $requirements '(?m)^- (NFR(?:-[A-Z]+)?-?\d+):'
    $acs = Get-Ids $requirements '(?m)^### (AC-\d+):'
    $ecs = Get-Ids $requirements '(?m)^- (EC-\d+):'
    $oss = Get-Ids $requirements '(?m)^- (OS-\d+):'
    Assert-Sequential $item.id 'FR' $frs
    Assert-Sequential $item.id 'NFR' $nfrs
    Assert-Sequential $item.id 'AC' $acs
    Assert-Sequential $item.id 'EC' $ecs
    Assert-Sequential $item.id 'OS' $oss

    $acceptanceSection = [regex]::Match($requirements, '(?ms)^## Acceptance Criteria\s*(.*?)(?=^## |\z)').Groups[1].Value
    $normativeSections = ([regex]::Match($requirements, '(?ms)^## Functional Requirements\s*(.*?)(?=^## |\z)').Groups[1].Value + "`n" +
        [regex]::Match($requirements, '(?ms)^## Non-Functional Requirements\s*(.*?)(?=^## |\z)').Groups[1].Value + "`n" + $acceptanceSection)
    if ($normativeSections -match '(?i)\b(user-friendly|appropriate|meaningful|useful|quickly|properly|as needed|and so on)\b') {
        Add-Failure "SPEC-$($item.id) contains subjective or underspecified normative wording."
    }
    foreach ($reqMatch in [regex]::Matches($requirements, '(?ms)^- ((?:FR|NFR(?:-[A-Z]+)?)-?\d+):\s*(.*?)(?=^- (?:FR|NFR(?:-[A-Z]+)?)-?\d+:|^## |\z)')) {
        if ($reqMatch.Groups[2].Value -notmatch '\b(MUST|MUST NOT|SHOULD|SHOULD NOT|MAY)\b') {
            Add-Failure "SPEC-$($item.id) $($reqMatch.Groups[1].Value) lacks an RFC 2119 obligation."
        }
    }
    $definedRequirements = @($frs + $nfrs)
    $acceptanceRefs = @([regex]::Matches($acceptanceSection, '\b(?:FR-\d+|NFR(?:-[A-Z]+)?-?\d+)\b') | ForEach-Object Value | Select-Object -Unique)
    foreach ($reqId in $definedRequirements) {
        if ($acceptanceRefs -notcontains $reqId) { Add-Failure "SPEC-$($item.id) $reqId has no acceptance criterion." }
        if ($spec -notmatch "\b$([regex]::Escape($reqId))\b") { Add-Failure "SPEC-$($item.id) $reqId missing from spec.md." }
    }
    foreach ($ref in $acceptanceRefs) {
        if ($definedRequirements -notcontains $ref) { Add-Failure "SPEC-$($item.id) acceptance references undefined $ref." }
    }
    foreach ($acMatch in [regex]::Matches($acceptanceSection, '(?ms)^### (AC-\d+):([^\r\n]*)\r?\n(.*?)(?=^### |\z)')) {
        $acId = $acMatch.Groups[1].Value
        $title = $acMatch.Groups[2].Value
        $body = $acMatch.Groups[3].Value
        if ($title -notmatch '\b(?:FR-\d+|NFR(?:-[A-Z]+)?-?\d+)\b') {
            Add-Failure "SPEC-$($item.id) $acId title has no requirement reference."
        }
        if ($body -notmatch '(?m)^Given ' -or $body -notmatch '(?m)^When ' -or $body -notmatch '(?m)^Then ') {
            Add-Failure "SPEC-$($item.id) $acId is not complete Given/When/Then."
        }
    }

    $taskMatches = @([regex]::Matches($tasks, '(?m)^- \[ \] (T\d{3})\s+(.+)$'))
    if ($taskMatches.Count -eq 0) { Add-Failure "SPEC-$($item.id) has no actionable tasks." }
    $taskIds = @($taskMatches | ForEach-Object { $_.Groups[1].Value })
    Assert-Sequential $item.id 'task' $taskIds
    if ($tasks -match '(?mi)^- \[[xX]\] T') { Add-Failure "SPEC-$($item.id) marks implementation tasks complete." }
    foreach ($task in $taskMatches) {
        $taskId = $task.Groups[1].Value
        $body = $task.Groups[2].Value
        if ($body -match 'Implement AC-|using plan\.md|Run unit, integration|Complete .* gates|under tests/|Deliver FR-\d+ at\s*:|Spec\d+(?:Feature|Frontend)\.cs|satisfies its positive, negative, authorization, persistence, and boundary cases') {
            Add-Failure "SPEC-$($item.id) $taskId is generic or directory-only."
        }
        if (-not (Test-ExactFuturePath $body)) {
            Add-Failure "SPEC-$($item.id) $taskId does not name an exact future file."
        }
    }
    $taskLines = ($taskMatches | ForEach-Object { $_.Value }) -join "`n"

    $specWorkstreamProperty = $workstreamManifest.specs.PSObject.Properties | Where-Object Name -eq $item.id
    $specWorkstreams = if ($specWorkstreamProperty) { @($specWorkstreamProperty.Value) } else { @() }
    foreach ($fr in $frs) {
        $mappedStreams = @($specWorkstreams | Where-Object { @($_.requirements) -contains $fr })
        if ($mappedStreams.Count -ne 1) { Add-Failure "SPEC-$($item.id) $fr must map to exactly one workstream; found $($mappedStreams.Count)." }
    }
    foreach ($stream in $specWorkstreams) {
        $streamName = [string]$stream.name
        $streamRequirements = @($stream.requirements | ForEach-Object { [string]$_ })
        foreach ($fr in $streamRequirements) {
            if ($frs -notcontains $fr) { Add-Failure "SPEC-$($item.id) workstream $streamName references undefined $fr." }
        }
        $testPath = [string]$stream.testPath
        $deliveryPath = [string]$stream.deliveryPath
        $testFocus = One-Line ([string]$stream.testFocus)
        if ($testPath -eq $deliveryPath) { Add-Failure "SPEC-$($item.id) workstream $streamName uses the same test and delivery path." }
        foreach ($streamFr in $streamRequirements) {
            $frTag = "\[$([regex]::Escape($streamFr))\]"
            $testCandidates = @($taskMatches | Where-Object {
                $body = $_.Groups[2].Value
                $body -match $frTag -and
                    $body -match [regex]::Escape($testPath) -and
                    $body -match [regex]::Escape($testFocus)
            })
            $deliveryCandidates = @($taskMatches | Where-Object {
                $body = $_.Groups[2].Value
                $body -match $frTag -and
                    $body -match [regex]::Escape($deliveryPath) -and
                    $body -match "(?i)\bDeliver $([regex]::Escape($streamFr)) through the bounded\b" -and
                    $body -notmatch '^\[P\]'
            })
            if ($testCandidates.Count -ne 1) {
                Add-Failure "SPEC-$($item.id) workstream $streamName/$streamFr must have exactly one focused test task for $testPath; found $($testCandidates.Count)."
            }
            if ($deliveryCandidates.Count -ne 1) {
                Add-Failure "SPEC-$($item.id) workstream $streamName/$streamFr must have exactly one delivery task for $deliveryPath; found $($deliveryCandidates.Count)."
            }
            if ($testCandidates.Count -eq 1 -and $deliveryCandidates.Count -eq 1 -and
                (Get-TaskNumber $testCandidates[0]) -ge (Get-TaskNumber $deliveryCandidates[0])) {
                Add-Failure "SPEC-$($item.id) workstream $streamName/$streamFr delivery is not preceded by its focused test."
            }
        }
    }

    foreach ($fr in $frs) {
        if ([regex]::Matches($taskLines, "\[$([regex]::Escape($fr))\]").Count -lt 2) {
            Add-Failure "SPEC-$($item.id) $fr lacks both delivery and verification tasks."
        }
    }
    foreach ($id in @($nfrs + $acs + $ecs + $oss)) {
        if ($taskLines -notmatch "\[$([regex]::Escape($id))\]") { Add-Failure "SPEC-$($item.id) $id lacks an actionable task." }
    }
    foreach ($dep in $item.dependencies) {
        if ($taskLines -notmatch "\[DEP-SPEC-$dep\]") { Add-Failure "SPEC-$($item.id) lacks a dependency task for SPEC-$dep." }
    }
    $taskDependencyIds = @([regex]::Matches($taskLines, '\[DEP-SPEC-(\d{3})\]') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique)
    $expectedTaskDeps = @($item.dependencies | Sort-Object)
    $actualTaskDeps = @($taskDependencyIds | Sort-Object)
    if (($expectedTaskDeps -join ',') -ne ($actualTaskDeps -join ',')) {
        Add-Failure "SPEC-$($item.id) actionable dependency tasks do not exactly match the manifest."
    }

    foreach ($entity in $item.entities) {
        if ($model -notmatch [regex]::Escape($entity)) { Add-Failure "SPEC-$($item.id) entity $entity missing from data-model.md." }
        if ($taskLines -notmatch "\[ENTITY-$([regex]::Escape($entity))\]") { Add-Failure "SPEC-$($item.id) entity $entity lacks model/test task." }
        $ownerProperty = $entityOwnership.canonicalOwners.PSObject.Properties | Where-Object Name -eq $entity
        $canonicalOwner = if ($ownerProperty) { [string]$ownerProperty.Value } else { [string]$item.id }
        $canonicalPath = Get-EntityArtifactPath $canonicalOwner $entity
        $entityTagPattern = "\[ENTITY-$([regex]::Escape($entity))\]"
        $globalEntityTasks = @($taskRegistry | Where-Object { $_.body -match $entityTagPattern })
        $ownerWriters = @($globalEntityTasks | Where-Object {
            $_.body -match [regex]::Escape($canonicalPath) -and
            $_.body -match '(?i)\bdeliver the canonical\b' -and
            $_.body -notmatch '^\[P\]'
        })
        if ($item.id -eq $canonicalOwner) {
            if ($ownerWriters.Count -ne 1) { Add-Failure "Canonical entity $entity must have exactly one owner delivery writer; found $($ownerWriters.Count)." }
            elseif ($ownerWriters[0].spec -ne $canonicalOwner) { Add-Failure "Canonical entity $entity is delivered by SPEC-$($ownerWriters[0].spec) instead of SPEC-$canonicalOwner." }
            $ownerTests = @($taskMatches | Where-Object {
                $_.Groups[2].Value -match $entityTagPattern -and
                $_.Groups[2].Value -match 'tests/[^\s,;]+\.[A-Za-z0-9]+' -and
                $_.Groups[2].Value -match '(?i)\b(failing|test|verify|assert|check)\b'
            })
            if ($ownerTests.Count -lt 1) { Add-Failure "Canonical entity $entity has no owner test task." }
            elseif ($ownerWriters.Count -eq 1 -and (Get-TaskNumber $ownerTests[0]) -ge $ownerWriters[0].number) { Add-Failure "Canonical entity $entity delivery is not preceded by its owner test." }
        } elseif ($item.id -eq '005') {
            $mappingPath = Get-EntityArtifactPath $item.id $entity
            $mappingTasks = @($taskMatches | Where-Object { $_.Groups[2].Value -match $entityTagPattern -and $_.Groups[2].Value -match '\[PERSISTENCE-MAPPING\]' })
            if ($mappingTasks.Count -lt 2 -or $taskLines -notmatch [regex]::Escape($mappingPath)) { Add-Failure "SPEC-005 entity $entity lacks test-first persistence mapping at $mappingPath." }
        } else {
            $consumerTag = "\[CONSUMER-SPEC-$canonicalOwner\]"
            $consumerTasks = @($taskMatches | Where-Object {
                $_.Groups[2].Value -match $entityTagPattern -and
                $_.Groups[2].Value -match $consumerTag -and
                $_.Groups[2].Value -match [regex]::Escape($canonicalPath)
            })
            if ($consumerTasks.Count -ne 1) { Add-Failure "SPEC-$($item.id) must consume canonical $entity from SPEC-$canonicalOwner exactly once; found $($consumerTasks.Count)." }
            if (@($consumerTasks | Where-Object { $_.Groups[2].Value -match '(?i)\b(deliver|implement|define|publish)\b' }).Count -gt 0) { Add-Failure "SPEC-$($item.id) attempts to redefine canonical entity $entity." }
        }
    }

    $ownedEndpoints = @($endpointManifest.endpoints | Where-Object owner -eq $item.id)
    $endpointIndex = 0
    foreach ($endpointItem in $ownedEndpoints) {
        $endpointIndex++
        $endpoint = "$(([string]$endpointItem.method).ToUpperInvariant()) $($endpointItem.path)"
        $endpointCode = 'Endpoint{0:d2}' -f $endpointIndex
        $contractPath = "specs/$name/contracts/api.md"
        $testPath = "tests/StudentRegistration.ContractTests/Specs/Spec$($item.id)/$($endpointCode)ContractTests.cs"
        $moduleSafe = ([string]$item.module) -replace '[^A-Za-z0-9]', ''
        $handlerPath = "src/StudentRegistration.Server/Modules/$moduleSafe/Endpoints/Spec$($item.id)Endpoints.cs"
        $endpointPattern = [regex]::Escape($endpoint) + '(?![A-Za-z0-9_{}?=&/\-])'
        $contractTasks = @($taskMatches | Where-Object { $_.Groups[2].Value -match $endpointPattern -and $_.Groups[2].Value -match [regex]::Escape($contractPath) })
        $testTasksForEndpoint = @($taskMatches | Where-Object { $_.Groups[2].Value -match $endpointPattern -and $_.Groups[2].Value -match [regex]::Escape($testPath) })
        $handlerTasks = @($taskMatches | Where-Object { $_.Groups[2].Value -match $endpointPattern -and $_.Groups[2].Value -match [regex]::Escape($handlerPath) -and $_.Groups[2].Value -match '(?i)\b(deliver|handler|implement)\b' })
        if ($contractTasks.Count -ne 1) { Add-Failure "SPEC-$($item.id) endpoint $endpoint must have exactly one canonical contract task; found $($contractTasks.Count)." }
        if ($testTasksForEndpoint.Count -ne 1) { Add-Failure "SPEC-$($item.id) endpoint $endpoint must have exactly one contract-test task at $testPath; found $($testTasksForEndpoint.Count)." }
        if ($handlerTasks.Count -ne 1) { Add-Failure "SPEC-$($item.id) endpoint $endpoint must have exactly one handler task at $handlerPath; found $($handlerTasks.Count)." }
        if ($testTasksForEndpoint.Count -eq 1 -and $handlerTasks.Count -eq 1 -and (Get-TaskNumber $testTasksForEndpoint[0]) -ge (Get-TaskNumber $handlerTasks[0])) {
            Add-Failure "SPEC-$($item.id) endpoint $endpoint handler is not preceded by its contract test."
        }
        $foreignHandlerClaims = @($taskRegistry | Where-Object {
            $_.spec -ne ([string]$item.id) -and
            $_.body -match $endpointPattern -and
            $_.body -match '(?i)\b(handler|deliver|implement)\b' -and
            $_.body -match 'src/[^\s,;]+/Endpoints/[^\s,;]+\.cs'
        })
        if ($foreignHandlerClaims.Count -gt 0) {
            $foreignSpecs = (@($foreignHandlerClaims.spec) | Sort-Object -Unique) -join ', '
            Add-Failure "Endpoint $endpoint has non-owner handler claims in $foreignSpecs."
        }
    }

    $ownedRoutes = @($routeManifest.routes | Where-Object { $_.owners -contains $item.id })
    foreach ($route in $ownedRoutes) {
        $routeTag = "\[$([regex]::Escape($route.id))\]"
        if ($spec -notmatch [regex]::Escape($route.id)) { Add-Failure "SPEC-$($item.id) spec.md omits owned route $($route.id)." }
        foreach ($link in @($route.links | Where-Object spec -eq $item.id)) {
            $linkTags = @(@($link.requirements) + @($link.criteria) | ForEach-Object { [string]$_ })
            $linkedTasks = @($taskMatches | Where-Object { $_.Groups[2].Value -match $routeTag -and (Test-TaskTags $_.Groups[2].Value $linkTags) })
            if ($linkedTasks.Count -lt 1) { Add-Failure "SPEC-$($item.id) route $($route.id) has no task carrying all linked FR/AC references." }
        }
    }

    if ($item.id -eq '003') {
        if (-not (Test-Path (Join-Path $dir 'page-matrix.md'))) { Add-Failure 'SPEC-003 missing page-matrix.md.' }
        $pageMatrix = if (Test-Path (Join-Path $dir 'page-matrix.md')) { Get-Content (Join-Path $dir 'page-matrix.md') -Raw } else { '' }
        foreach ($route in $routeManifest.routes) {
            $routeId = [string]$route.id
            $page = [string]$route.page
            if ($requirements -notmatch [regex]::Escape($routeId) -and $pageMatrix -notmatch [regex]::Escape($routeId)) {
                Add-Failure "SPEC-003 page artifacts omit $routeId."
            }
            $expectedRouteArtifacts = @(
                "specs/003-ux-storyboard-accessibility/design/pages/$routeId.md",
                "tests/StudentRegistration.Client.ContractTests/Routes/$($page)ContractTests.cs",
                "tests/StudentRegistration.Client.UnitTests/Pages/$($page)ComponentTests.cs",
                "tests/StudentRegistration.AccessibilityTests/Routes/$($page)AccessibilityTests.cs",
                "tests/StudentRegistration.VisualTests/Routes/$($page)VisualTests.cs"
            )
            foreach ($expectedPath in $expectedRouteArtifacts) {
                $matches = @($taskMatches | Where-Object { $_.Groups[2].Value -match "\[$([regex]::Escape($routeId))\]" -and $_.Groups[2].Value -match [regex]::Escape($expectedPath) })
                if ($matches.Count -ne 1) { Add-Failure "SPEC-003 route $routeId must have exactly one task for $expectedPath; found $($matches.Count)." }
            }
        }
        foreach ($component in $componentManifest.components) {
            $componentTag = "\[COMPONENT-$([regex]::Escape([string]$component.name))\]"
            $componentTestPath = "tests/StudentRegistration.Client.UnitTests/Components/$($component.name)Tests.cs"
            $componentTests = @($taskMatches | Where-Object { $_.Groups[2].Value -match $componentTag -and $_.Groups[2].Value -match [regex]::Escape($componentTestPath) })
            $componentDeliveries = @($taskMatches | Where-Object { $_.Groups[2].Value -match $componentTag -and $_.Groups[2].Value -match [regex]::Escape([string]$component.path) -and $_.Groups[2].Value -match '(?i)\b(deliver|implement)\b' })
            if ($componentTests.Count -ne 1) { Add-Failure "Component $($component.name) must have exactly one failing test task; found $($componentTests.Count)." }
            if ($componentDeliveries.Count -ne 1) { Add-Failure "Component $($component.name) must have exactly one delivery task; found $($componentDeliveries.Count)." }
            if ($componentTests.Count -eq 1 -and $componentDeliveries.Count -eq 1 -and (Get-TaskNumber $componentTests[0]) -ge (Get-TaskNumber $componentDeliveries[0])) {
                Add-Failure "Component $($component.name) delivery is not preceded by its test."
            }
            $foreignComponentWriters = @($taskRegistry | Where-Object { $_.spec -ne '003' -and $_.body -match [regex]::Escape([string]$component.path) -and $_.body -match '(?i)\b(deliver|implement)\b' })
            if ($foreignComponentWriters.Count -gt 0) { Add-Failure "Component $($component.name) has a non-SPEC-003 delivery writer." }
        }
        $frontendGovernancePairs = @(
            @('tests/StudentRegistration.SpecificationTests/Frontend/BrowserMatrixContractTests.cs','tests/StudentRegistration.E2ETests/browser-matrix.json'),
            @('tests/StudentRegistration.SpecificationTests/Frontend/VisualBaselineManifestTests.cs','tests/StudentRegistration.VisualTests/Baselines/baseline-manifest.json'),
            @('tests/StudentRegistration.SpecificationTests/Frontend/FlakePolicyContractTests.cs','tests/StudentRegistration.E2ETests/flake-policy.json')
        )
        foreach ($pair in $frontendGovernancePairs) {
            $governanceTest = @($taskMatches | Where-Object { $_.Groups[2].Value -match [regex]::Escape($pair[0]) })
            $governanceDelivery = @($taskMatches | Where-Object { $_.Groups[2].Value -match [regex]::Escape($pair[1]) })
            if ($governanceTest.Count -ne 1) { Add-Failure "SPEC-003 frontend governance requires exactly one test task for $($pair[0]); found $($governanceTest.Count)." }
            if ($governanceDelivery.Count -ne 1) { Add-Failure "SPEC-003 frontend governance requires exactly one delivery task for $($pair[1]); found $($governanceDelivery.Count)." }
            if ($governanceTest.Count -eq 1 -and $governanceDelivery.Count -eq 1 -and (Get-TaskNumber $governanceTest[0]) -ge (Get-TaskNumber $governanceDelivery[0])) {
                Add-Failure "SPEC-003 frontend governance delivery $($pair[1]) is not preceded by its contract test."
            }
        }
        if ($taskLines -notmatch [regex]::Escape('docs/release-evidence/frontend/safari-macos-evidence.md')) {
            Add-Failure 'SPEC-003 lacks the signed actual-Safari-on-macOS evidence task.'
        }
    }

    foreach ($route in $routeManifest.routes | Where-Object implementationOwner -eq $item.id) {
        $routeId = [string]$route.id
        $page = [string]$route.page
        $pagePath = "src/StudentRegistration.Client/Pages/$page.razor"
        $writers = @($taskRegistry | Where-Object {
            $_.body -match "\[$([regex]::Escape($routeId))\]" -and
            $_.body -match [regex]::Escape($pagePath) -and
            $_.body -match '(?i)\b(deliver|implement)\b' -and
            $_.body -notmatch '(?i)\b(test|verify|assert|failing)\b'
        })
        if ($writers.Count -ne 1) { Add-Failure "Route $routeId must have exactly one canonical Razor delivery writer; found $($writers.Count)." }
        elseif ($writers[0].spec -ne ([string]$route.implementationOwner)) { Add-Failure "Route $routeId is delivered by SPEC-$($writers[0].spec) instead of SPEC-$($route.implementationOwner)." }
        $e2ePath = if ($item.id -eq '003') {
            "tests/StudentRegistration.E2ETests/Routes/$($page)JourneyTests.cs"
        } else {
            "tests/StudentRegistration.E2ETests/Specs/Spec$($item.id)/$($page)FeatureTests.cs"
        }
        $e2eTasks = @($taskMatches | Where-Object { $_.Groups[2].Value -match "\[$([regex]::Escape($routeId))\]" -and $_.Groups[2].Value -match [regex]::Escape($e2ePath) })
        if ($e2eTasks.Count -ne 1) { Add-Failure "Route $routeId implementation owner SPEC-$($item.id) must have exactly one E2E task at $e2ePath; found $($e2eTasks.Count)." }
        if ($e2eTasks.Count -eq 1 -and $writers.Count -eq 1 -and (Get-TaskNumber $e2eTasks[0]) -ge $writers[0].number) { Add-Failure "Route $routeId page delivery is not preceded by its owner E2E test." }
    }
    if ($item.id -eq '014') {
        $matrixPath = Join-Path $dir 'concurrency-matrix.md'
        if (-not (Test-Path $matrixPath)) { Add-Failure 'SPEC-014 missing concurrency-matrix.md.' }
        $matrixText = if (Test-Path $matrixPath) { Get-Content $matrixPath -Raw } else { '' }
        $concurrencyEvidence = "$requirements`n$tasks`n$matrixText"
        foreach ($term in @(
            'StudentTermRegistrationGuard','IDEMPOTENCY_KEY_REUSED','payload hash',
            'same student','two replicas','emergency closure','conditional atomic SQL',
            'allocation savepoint','RegistrationInProgressResponse',
            'no submissionId','500 ms','REQUEST_NOT_FOUND','deadlock','reconciliation'
        )) {
            if ($concurrencyEvidence -notmatch [regex]::Escape($term)) { Add-Failure "SPEC-014 concurrency design omits $term." }
        }
        $raceRows = @($matrixText -split "`r?`n" | Where-Object {
            $_ -match '^\|.+\|$' -and $_ -notmatch '^\|\s*Race\s*\|' -and $_ -notmatch '^\|[-:\s|]+$'
        })
        $raceTaskMatches = @($taskMatches | Where-Object { $_.Groups[2].Value -match '\[RACE-R\d{2}\]' })
        $raceIds = @($raceTaskMatches | ForEach-Object { [regex]::Match($_.Groups[2].Value, '\[(RACE-R\d{2})\]').Groups[1].Value })
        Assert-Sequential $item.id 'RACE' $raceIds
        if ($raceRows.Count -ne $raceTaskMatches.Count) { Add-Failure "SPEC-014 concurrency matrix has $($raceRows.Count) rows but $($raceTaskMatches.Count) RACE tasks." }
        for ($raceIndex = 0; $raceIndex -lt $raceRows.Count; $raceIndex++) {
            $raceId = 'RACE-R{0:d2}' -f ($raceIndex + 1)
            $raceTestPath = "tests/StudentRegistration.ConcurrencyTests/Specs/Spec014/ConcurrencyMatrix/RaceR$('{0:d2}' -f ($raceIndex + 1))Tests.cs"
            $raceTasks = @($raceTaskMatches | Where-Object { $_.Groups[2].Value -match "\[$raceId\]" -and $_.Groups[2].Value -match [regex]::Escape($raceTestPath) })
            if ($raceTasks.Count -ne 1) { Add-Failure "SPEC-014 $raceId must have exactly one exact real-SQL test task at $raceTestPath; found $($raceTasks.Count)."; continue }
            $cells = @($raceRows[$raceIndex].Trim('|').Split('|') | ForEach-Object { One-Line $_ })
            foreach ($cell in $cells) {
                if (-not [string]::IsNullOrWhiteSpace($cell) -and $raceTasks[0].Groups[2].Value -notmatch [regex]::Escape($cell)) {
                    Add-Failure "SPEC-014 $raceId task omits concurrency-matrix oracle '$cell'."
                }
            }
        }
        if ($requirements -match 'Drops MUST|Drops MUST transition') { Add-Failure 'SPEC-014 still requires the out-of-scope drop workflow.' }
    }

    foreach ($deliveryTask in @($taskMatches | Where-Object {
        $_.Groups[2].Value -match '(?i)\b(deliver|implement|map the canonical)\b' -and
        $_.Groups[2].Value -notmatch '^\[P\]'
    })) {
        $deliveryBody = $deliveryTask.Groups[2].Value
        $semanticTags = @([regex]::Matches($deliveryBody, '\[((?:FR-\d+|API-[A-Za-z0-9]+|COMPONENT-[A-Za-z0-9]+|ENTITY-[A-Za-z0-9]+|RACE-R\d{2}|(?:AUTH|STU|ADM|STF|SYS)-\d{2}))\]') | ForEach-Object { $_.Groups[1].Value })
        if ($semanticTags.Count -eq 0) { Add-Failure "SPEC-$($item.id) $($deliveryTask.Groups[1].Value) delivery has no semantic traceability tag."; continue }
        $priorTests = @($taskMatches | Where-Object {
            $candidateBody = $_.Groups[2].Value
            (Get-TaskNumber $_) -lt (Get-TaskNumber $deliveryTask) -and
            $candidateBody -match 'tests/[^\s,;]+\.[A-Za-z0-9]+' -and
            $candidateBody -match '(?i)\b(failing|test|verify|assert|check)\b' -and
            @($semanticTags | Where-Object { $candidateBody -match "\[$([regex]::Escape($_))\]" }).Count -gt 0
        })
        if ($priorTests.Count -eq 0) { Add-Failure "SPEC-$($item.id) $($deliveryTask.Groups[1].Value) delivery is not preceded by a test sharing its semantic tag." }
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
        artifacts = (Get-ChildItem $dir -Recurse -File).Count
        requirements = $definedRequirements.Count
        acceptanceCriteria = $acs.Count
        edgeCases = $ecs.Count
        tasks = $taskMatches.Count
        requirementsScore = $score
        automatedGates = if ($failures.Count -eq $before) { 'PASS' } else { 'FAIL' }
        humanApproval = 'PENDING'
    })
}

foreach ($taskRecord in $taskRegistry) {
    if ($taskRecord.body -notmatch '(?i)\b(handler|deliver|implement)\b' -or $taskRecord.body -notmatch 'src/[^\s,;]+/Endpoints/[^\s,;]+\.cs') { continue }
    foreach ($endpointMatch in [regex]::Matches($taskRecord.body, '\b(GET|POST|PUT|PATCH|DELETE)\s+(/api/[A-Za-z0-9_{}?=&/\-]+)')) {
        $key = "$($endpointMatch.Groups[1].Value.ToUpperInvariant()) $($endpointMatch.Groups[2].Value)"
        $canonicalEndpoint = $endpointManifest.endpoints | Where-Object { "$(([string]$_.method).ToUpperInvariant()) $($_.path)" -eq $key }
        if ($null -eq $canonicalEndpoint) {
            Add-Failure "SPEC-$($taskRecord.spec) $($taskRecord.id) claims unregistered endpoint handler $key."
        } elseif (([string]$canonicalEndpoint.owner) -ne ([string]$taskRecord.spec)) {
            Add-Failure "SPEC-$($taskRecord.spec) $($taskRecord.id) claims $key handler owned by SPEC-$($canonicalEndpoint.owner)."
        }
    }
}
Remove-Item Env:SPECIFY_FEATURE -ErrorAction SilentlyContinue
Remove-Item Env:SPECIFY_FEATURE_DIRECTORY -ErrorAction SilentlyContinue

$sourceFiles = @(Get-ChildItem $root -Recurse -File -Include *.cs,*.razor,*.csproj,*.sln,*.sql |
    Where-Object { $_.FullName -notmatch '\\.agents\\|\\.specify\\' })
if ($sourceFiles.Count -gt 0) { Add-Failure "Planning-only boundary violated by $($sourceFiles.Count) implementation files." }

$erdPath = Join-Path $root 'docs/diagrams/ERD.md'
$erd = if (Test-Path $erdPath) { Get-Content $erdPath -Raw } else { '' }
foreach ($term in @(
    'StudentTermRegistrationGuard', 'PayloadHash', 'ProcessingState',
    'ReceivedAtUtc', 'CompletedAtUtc', 'RegistrationPaused',
    'RegistrationWindow', 'StaffTermAvailability', 'rowversion'
)) {
    if ($erd -notmatch [regex]::Escape($term)) { Add-Failure "Shared ERD omits concurrency field/entity $term." }
}

$contractTermChecks = @{
    '009-catalog-prerequisites-policy-admin' = @('expectedDraftRowVersion','previewToken','clientRequestId','STALE_PREVIEW','IDEMPOTENCY_KEY_REUSED')
    '010-offerings-groups-resources' = @('registrationPaused','expectedGroupRowVersions','previewToken','clientRequestId','GROUP_CHANGED')
    '013-schedule-recommendations' = @('expectedPlanRowVersion','requestCorrelationId','catalogueVersion','policyVersion','optimizerConfigurationVersion','recommended-option')
    '014-registration-capacity-concurrency' = @('expectedPlanRowVersion','clientRequestId','receivedAtUtc','completedAtUtc','RegistrationFinalResult','RegistrationInProgressResponse','retryAfterSeconds','no submissionId','REQUEST_NOT_FOUND','201','200','202','409','by-request')
    '016-lecturer-ta-workspace' = @('rowVersion','expectedStaffTermRowVersion','STALE_VERSION','AVAILABILITY_DEADLINE_PASSED')
    '017-admin-operations-audit-reporting' = @('expectedRowVersion','clientRequestId','previewToken','STALE_PREVIEW','IDEMPOTENCY_KEY_REUSED')
}
foreach ($contractEntry in $contractTermChecks.GetEnumerator()) {
    $contractPath = Join-Path $root "specs/$($contractEntry.Key)/contracts/api.md"
    $contractText = if (Test-Path $contractPath) { Get-Content $contractPath -Raw } else { '' }
    foreach ($term in $contractEntry.Value) {
        if ($contractText -notmatch [regex]::Escape($term)) { Add-Failure "$($contractEntry.Key) API contract omits concurrency token/result $term." }
    }
}

$constitution = Get-Content (Join-Path $root '.specify/memory/constitution.md') -Raw
if ($constitution -match '\[PRINCIPLE_|\[PROJECT_NAME\]|\[SECTION_') { Add-Failure 'Constitution contains template placeholders.' }
if ($constitution -notmatch 'Ahmed ELbamby <A\.Elbamby61869@student\.aast\.edu>') { Add-Failure 'Constitution is missing the required Git identity.' }
$expectedGitIdentity = 'Ahmed ELbamby|A.Elbamby61869@student.aast.edu|Ahmed ELbamby|A.Elbamby61869@student.aast.edu'
$historyIdentities = @(git -C $root log --format='%an|%ae|%cn|%ce' 2>$null | Select-Object -Unique)
foreach ($identity in $historyIdentities) {
    if ($identity -ne $expectedGitIdentity) { Add-Failure "Git history contains a prohibited author/committer identity: $identity" }
}
$configuredName = git -C $root config --local user.name
$configuredEmail = git -C $root config --local user.email
if ($configuredName -ne 'Ahmed ELbamby' -or $configuredEmail -ne 'A.Elbamby61869@student.aast.edu') {
    Add-Failure 'Repository-local Git author/committer configuration is not Ahmed ELbamby with the required email.'
}

$reportRows = $results | ForEach-Object { "| $($_.spec) | $($_.artifacts) | $($_.requirements) | $($_.acceptanceCriteria) | $($_.edgeCases) | $($_.tasks) | $($_.requirementsScore) | $($_.automatedGates) | $($_.humanApproval) |" }
$overall = if ($failures.Count -eq 0) { 'PASS' } else { 'FAIL' }
@"
# Spec Kit Readiness Audit

**Date**: $auditDate
**Overall automated result**: $overall
**Scope**: 18 connected specifications; planning artifacts only
**Human approval**: Pending for every specification

| Spec | Artifacts | FR+NFR | AC | EC | Actionable tasks | Strict score | Automated gates | Human approval |
|---|---:|---:|---:|---:|---:|---:|---|---|
$($reportRows -join "`r`n")

## Enforced Cross-Spec Results

- Exactly 18 specs and 27 frontend routes; every route has SPEC-003 governance and feature ownership.
- Dependencies exist, match requirements metadata, contain no cycle, and every spec reaches SPEC-001.
- Every FR/NFR has acceptance coverage; every FR has delivery and verification tasks.
- Every NFR, AC, EC, out-of-scope guard, entity, endpoint, dependency, and owned route has actionable exact-file tasks.
- SPEC-003 has per-route design, Blazor, component, E2E, accessibility, browser, and visual tasks.
- SPEC-014 has explicit cross-aggregate serialization, idempotency, cutoff, admin-versus-submit, two-replica, failure, and reconciliation design.
- Official Spec Kit prerequisite and strict workflow validators run for every package.
- No application source, migration, executable test, or deployment implementation exists.

## Failures

$(if ($failures.Count) { ($failures | ForEach-Object { "- $_" }) -join "`r`n" } else { 'None.' })
"@ | Set-Content (Join-Path $root 'docs/SPECKIT_AUDIT.md')

[pscustomobject]@{ overall = $overall; specifications = $results; failures = $failures } | ConvertTo-Json -Depth 6
if ($failures.Count -gt 0) { exit 1 }
