[CmdletBinding()]
param(
    [ValidateSet('Auto', 'Planning', 'Implementation')]
    [string]$Phase = 'Auto'
)

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
$auditDate = Get-Date -Format 'yyyy-MM-dd'
$failures = New-Object System.Collections.Generic.List[string]
$results = New-Object System.Collections.Generic.List[object]
$required = @('spec.md','requirements.md','plan.md','research.md','data-model.md','quickstart.md','clarifications.md','contracts/api.md','checklists/requirements.md','checklists/gates.md','tasks.md')
$implementationFiles = @(Get-ChildItem $root -Recurse -File -Include *.cs,*.razor,*.csproj,*.sln,*.slnx,*.sql |
    Where-Object { $_.FullName -notmatch '\\.agents\\|\\.specify\\' })
$implementationMode = switch ($Phase) {
    'Planning' { $false }
    'Implementation' { $true }
    default { $implementationFiles.Count -gt 0 }
}
$strictValidatorCandidates = @(
    $env:SPEC_VALIDATOR_PATH,
    (Join-Path $HOME '.codex/skills/claude-spec-driven-workflow/scripts/spec_validator.py'),
    (Join-Path $HOME '.agents/skills/claude-spec-driven-workflow/scripts/spec_validator.py')
) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) -and (Test-Path $_ -PathType Leaf) }
$strictValidatorPath = $strictValidatorCandidates | Select-Object -First 1
$persistenceMappingEvidence = @{}
foreach ($property in $persistenceManifest.contributions.PSObject.Properties) {
    $persistenceMappingEvidence[[string]$property.Name] = @(
        [string]$property.Value.testPath,
        [string]$property.Value.path
    )
}

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
function Get-ReferencedTaskIds([string]$Body) {
    $references = New-Object System.Collections.Generic.List[string]
    foreach ($range in [regex]::Matches($Body, '\bT(\d{3})-T(\d{3})\b')) {
        $first = [int]$range.Groups[1].Value
        $last = [int]$range.Groups[2].Value
        if ($last -lt $first -or ($last - $first) -gt 1000) { continue }
        for ($number = $first; $number -le $last; $number++) {
            $references.Add(('T{0:d3}' -f $number))
        }
    }
    foreach ($single in [regex]::Matches($Body, '\bT\d{3}\b')) {
        $references.Add($single.Value)
    }
    return @($references | Select-Object -Unique)
}
function Get-TaskPaths([string]$Body) {
    $paths = @([regex]::Matches($Body, '(?:src|tests|specs|docs|\.github)/[^\s,;()]+\.[A-Za-z0-9]+') | ForEach-Object {
        $_.Value.TrimEnd('.', ':')
    })
    if ($Body -match '(?<![A-Za-z0-9_./-])global\.json(?![A-Za-z0-9_./-])') { $paths += 'global.json' }
    return @($paths | Select-Object -Unique)
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
    $ownerProject = if ([string]$ownerSpec.module -eq 'Operations') { 'Api' } else { [string]$ownerSpec.module }
    $path = "src/StudentRegistration.$ownerProject/Domain/$Entity.cs"
    $overrideKey = "$SpecId`:$Entity"
    $overrideProperty = $entityOwnership.artifactOverrides.PSObject.Properties | Where-Object Name -eq $overrideKey
    if ($overrideProperty) { return [string]$overrideProperty.Value }
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
    $contributors = @()
    if (Has-Property $endpoint 'contributors') {
        if ($endpoint.contributors -isnot [System.Array]) {
            Add-Failure "Endpoint $key composite contributors must be an array."
        } else {
            $contributors = @($endpoint.contributors)
        }
    }
    if (($contributors | Select-Object -Unique).Count -ne $contributors.Count) {
        Add-Failure "Endpoint $key contains duplicate composite contributors."
    }
    foreach ($contributor in $contributors) {
        if ($ids -notcontains [string]$contributor) {
            Add-Failure "Endpoint $key has missing composite contributor SPEC-$contributor."
        }
        if ([string]$contributor -eq [string]$endpoint.owner) {
            Add-Failure "Endpoint $key repeats its owner as a composite contributor."
        }
    }
}
if (($endpointKeys | Select-Object -Unique).Count -ne $endpointKeys.Count) { Add-Failure 'Endpoint manifest contains duplicate method/path keys.' }

if (-not (Has-Property $persistenceManifest 'version') -or
    -not (Has-Property $persistenceManifest 'dbContext') -or
    -not (Has-Property $persistenceManifest 'contributions') -or
    -not (Has-Property $persistenceManifest 'nonProductionDataProfiles') -or
    -not (Has-Property $persistenceManifest 'migrations')) {
    Add-Failure 'Persistence manifest must contain version, dbContext, contributions, nonProductionDataProfiles, and migrations.'
} else {
    Assert-RequiredProperties 'Persistence DbContext' $persistenceManifest.dbContext @('owner','path','testPath')
    if ([string]$persistenceManifest.dbContext.owner -ne '004') { Add-Failure 'SPEC-004 must be the sole StudentRegistrationDbContext owner.' }
    if ([string]$persistenceManifest.dbContext.path -ne 'src/StudentRegistration.Infrastructure.SqlServer/Persistence/StudentRegistrationDbContext.cs') {
        Add-Failure 'Persistence manifest has a non-canonical StudentRegistrationDbContext path.'
    }
    foreach ($property in $persistenceManifest.contributions.PSObject.Properties) {
        $specId = [string]$property.Name
        $contribution = $property.Value
        Assert-RequiredProperties "Persistence contribution SPEC-$specId" $contribution @('mode','entities','path','testPath')
        if ($ids -notcontains $specId) { Add-Failure "Persistence contribution references missing SPEC-$specId." }
        if ([string]$contribution.path -notmatch '^src/StudentRegistration\.Infrastructure\.SqlServer/Persistence/Configurations/.+\.cs$') {
            Add-Failure "SPEC-$specId has invalid persistence configuration path $($contribution.path)."
        }
        if ([string]$contribution.testPath -notmatch '^tests/.+Tests\.cs$') {
            Add-Failure "SPEC-$specId has invalid persistence mapping test path $($contribution.testPath)."
        }
        $declaredSpec = $manifest.specs | Where-Object id -eq $specId
        foreach ($entity in @($contribution.entities)) {
            if (@($declaredSpec.entities) -notcontains [string]$entity) {
                Add-Failure "SPEC-$specId persistence contribution lists undeclared entity $entity."
            }
        }
    }
    $profileIds = @($persistenceManifest.nonProductionDataProfiles.id)
    if ((Compare-Object @('Development','Testing') @($profileIds | Sort-Object))) {
        Add-Failure 'Persistence non-production profiles must be exactly Development and Testing.'
    }
    foreach ($profile in @($persistenceManifest.nonProductionDataProfiles)) {
        Assert-RequiredProperties "Non-production data profile $($profile.id)" $profile @(
            'id','databasePattern','allowedEnvironments','orchestratorPath',
            'identityContributorPath','academicContributorPath',
            'credentialHandling','resetMode','testPath'
        )
        if (@($profile.allowedEnvironments).Count -ne 1 -or
            [string]$profile.allowedEnvironments[0] -ne [string]$profile.id) {
            Add-Failure "Non-production profile $($profile.id) is not restricted to its matching environment."
        }
        if ([string]$profile.databasePattern -match '(?i)production' -or
            [string]$profile.credentialHandling -notmatch '(?i)hash') {
            Add-Failure "Non-production profile $($profile.id) has an unsafe database or credential contract."
        }
        if ([string]$profile.testPath -ne 'tests/StudentRegistration.IntegrationTests/Persistence/EnvironmentDatabaseProvisioningTests.cs') {
            Add-Failure "Non-production profile $($profile.id) does not use the canonical provisioning test."
        }
    }
    $migrationIds = @($persistenceManifest.migrations.id)
    if (($migrationIds | Select-Object -Unique).Count -ne $migrationIds.Count) { Add-Failure 'Persistence migrations contain duplicate IDs.' }
    if (@($persistenceManifest.migrations | Where-Object kind -eq 'initial').Count -ne 1 -or [string]$persistenceManifest.migrations[0].kind -ne 'initial') {
        Add-Failure 'Persistence migrations must start with exactly one initial migration.'
    }
    foreach ($migration in @($persistenceManifest.migrations)) {
        Assert-RequiredProperties "Migration $($migration.id)" $migration @('id','kind','owner','path','snapshotPath','prerequisiteSpecs')
        if ([string]$migration.id -notmatch '^[A-Za-z][A-Za-z0-9]+$') { Add-Failure "Migration ID $($migration.id) is invalid." }
        if ([string]$migration.kind -notin @('initial','incremental')) { Add-Failure "Migration $($migration.id) has invalid kind $($migration.kind)." }
        if ($ids -notcontains [string]$migration.owner) { Add-Failure "Migration $($migration.id) references missing owner SPEC-$($migration.owner)." }
        if (@($migration.prerequisiteSpecs).Count -eq 0) { Add-Failure "Migration $($migration.id) has no prerequisite mapping specifications." }
        foreach ($specId in @($migration.prerequisiteSpecs)) {
            if ($ids -notcontains [string]$specId) { Add-Failure "Migration $($migration.id) references missing prerequisite SPEC-$specId." }
            if ($null -eq $persistenceManifest.contributions.PSObject.Properties[[string]$specId]) {
                Add-Failure "Migration $($migration.id) prerequisite SPEC-$specId has no persistence contribution."
            }
        }
    }
}

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
        if ([string]$stream.deliveryPath -match '^src/StudentRegistration\.(?:Server|Domain|Application)/' -or
            [string]$stream.deliveryPath -match '^src/StudentRegistration\.Infrastructure/') {
            Add-Failure "SPEC-$specId workstream $($stream.name) uses a prohibited layer-project delivery path $($stream.deliveryPath)."
        }
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
    $expectedLinkedSpecs = @($route.owners | Where-Object {
        [string]$_ -ne [string]$route.designOwner
    } | Sort-Object -Unique)
    $actualLinkedSpecs = @($route.links.spec | ForEach-Object { [string]$_ } | Sort-Object -Unique)
    if (Compare-Object $expectedLinkedSpecs $actualLinkedSpecs) {
        Add-Failure "$($route.id) links must exactly cover every owner/contributor other than its design owner."
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

if (-not (Has-Property $pageApiManifest 'version') -or -not (Has-Property $pageApiManifest 'pages')) {
    Add-Failure 'Page-to-API manifest must contain version and pages.'
} else {
    $pageApiRouteIds = @($pageApiManifest.pages.PSObject.Properties.Name)
    if ((Compare-Object @($routeIds | Sort-Object) @($pageApiRouteIds | Sort-Object))) {
        Add-Failure 'Page-to-API manifest keys do not exactly match the 27 route IDs.'
    }
    foreach ($routeId in $pageApiRouteIds) {
        $pageEndpoints = @($pageApiManifest.pages.PSObject.Properties[$routeId].Value)
        if ($pageEndpoints.Count -eq 0) { Add-Failure "$routeId has no frontend API dependency." }
        if (($pageEndpoints | Select-Object -Unique).Count -ne $pageEndpoints.Count) {
            Add-Failure "$routeId repeats an endpoint in the page-to-API manifest."
        }
        foreach ($pageEndpoint in $pageEndpoints) {
            if ($endpointKeys -notcontains [string]$pageEndpoint) {
                Add-Failure "$routeId references unregistered page endpoint $pageEndpoint."
                continue
            }
            $canonicalEndpoint = $endpointManifest.endpoints | Where-Object {
                "$(([string]$_.method).ToUpperInvariant()) $($_.path)" -eq [string]$pageEndpoint
            }
            $route = $routeManifest.routes | Where-Object id -eq $routeId
            $endpointContributors = @()
            if ((Has-Property $canonicalEndpoint 'contributors') -and
                $canonicalEndpoint.contributors -is [System.Array]) {
                $endpointContributors = @($canonicalEndpoint.contributors)
            }
            foreach ($authority in @([string]$canonicalEndpoint.owner) + $endpointContributors) {
                if ($route.owners -notcontains [string]$authority) {
                    Add-Failure "$routeId consumes $pageEndpoint but omits authority/contributor SPEC-$authority."
                }
            }
        }
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

function Test-DependsTransitively([string]$SpecId, [string]$RequiredId, [hashtable]$Seen) {
    if ($Seen[$SpecId]) { return $false }
    $Seen[$SpecId] = $true
    $node = $manifest.specs | Where-Object id -eq $SpecId
    foreach ($dependencyId in @($node.dependencies)) {
        if ($dependencyId -eq $RequiredId) { return $true }
        if (Test-DependsTransitively $dependencyId $RequiredId $Seen) { return $true }
    }
    return $false
}
foreach ($consumerSpec in $manifest.specs) {
    foreach ($entity in @($consumerSpec.entities)) {
        $ownerProperty = $entityOwnership.canonicalOwners.PSObject.Properties[$entity]
        if ($null -eq $ownerProperty) { continue }
        $ownerId = [string]$ownerProperty.Value
        if ($ownerId -eq [string]$consumerSpec.id -or [string]$consumerSpec.id -eq '005') { continue }
        if (-not (Test-DependsTransitively ([string]$consumerSpec.id) $ownerId @{})) {
            Add-Failure "SPEC-$($consumerSpec.id) consumes canonical $entity from downstream/unrelated SPEC-$ownerId without a dependency path."
        }
    }
}

$taskRegistry = New-Object System.Collections.Generic.List[object]
foreach ($taskSpec in $manifest.specs) {
    $taskFeatureName = "$($taskSpec.id)-$($taskSpec.slug)"
    $taskFile = Join-Path $root "specs/$taskFeatureName/tasks.md"
    if (-not (Test-Path $taskFile -PathType Leaf)) { continue }
    $taskText = Get-Content $taskFile -Raw
    foreach ($taskMatch in [regex]::Matches($taskText, '(?mi)^- \[(?: |x)\] (T\d{3})\s+(.+)$')) {
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
    $plan = Get-Content (Join-Path $dir 'plan.md') -Raw
    $tasks = Get-Content (Join-Path $dir 'tasks.md') -Raw
    $model = Get-Content (Join-Path $dir 'data-model.md') -Raw
    $apiContract = Get-Content (Join-Path $dir 'contracts/api.md') -Raw
    $approvalPath = Join-Path $dir 'checklists/approval.md'
    $approvalRecord = if (Test-Path $approvalPath -PathType Leaf) { Get-Content $approvalPath -Raw } else { '' }
    $isApproved = $spec -match '(?mi)^\*\*Status\*\*:\s*APPROVED\b' -and
        $requirements -match '(?mi)^\*\*Status:\*\*\s*APPROVED\b' -and
        $approvalRecord -match '(?i)APPROVED' -and
        $approvalRecord -match 'Ahmed ELbamby'
    if (-not $isApproved) { Add-Failure "SPEC-$($item.id) lacks synchronized Gate A Approved status and Ahmed ELbamby approval record." }

    if ("$requirements`n$apiContract" -match ':\s*unknown(?:\[\])?\s*[;,}]') {
        Add-Failure "SPEC-$($item.id) requirements/API contract contains an untyped unknown field."
    }

    $canonicalTypeBlock = [regex]::Match($apiContract, '(?s)```typescript\s*(.*?)\s*```')
    if ($canonicalTypeBlock.Success) {
        $requirementsTypeBlock = [regex]::Match($requirements, '(?s)## API Contracts.*?```typescript\s*(.*?)\s*```')
        if (-not $requirementsTypeBlock.Success) {
            Add-Failure "SPEC-$($item.id) requirements omit the canonical TypeScript API contract block."
        } else {
            $normalizedCanonicalTypes = [regex]::Replace($canonicalTypeBlock.Groups[1].Value, '(?m)//.*$', '')
            $normalizedRequirementsTypes = [regex]::Replace($requirementsTypeBlock.Groups[1].Value, '(?m)//.*$', '')
            $normalizedCanonicalTypes = [regex]::Replace($normalizedCanonicalTypes, '\s+', '')
            $normalizedRequirementsTypes = [regex]::Replace($normalizedRequirementsTypes, '\s+', '')
            if ($normalizedCanonicalTypes -cne $normalizedRequirementsTypes) {
                Add-Failure "SPEC-$($item.id) requirements API declarations drift from contracts/api.md."
            }
        }
    }

    if ($model -match '(?m)^## Owned Entities\s*$' -or $model -match '(?i)Feature-owned concept') {
        Add-Failure "SPEC-$($item.id) data model uses an unqualified ownership claim instead of canonical owner/consumer responsibilities."
    }

    foreach ($literal in [regex]::Matches("$requirements`n$apiContract", '\b(GET|POST|PUT|PATCH|DELETE)\s+(/api/[A-Za-z0-9_{}?=&/\-]+)')) {
        $literalPath = $literal.Groups[2].Value.Split('?')[0]
        $literalKey = "$($literal.Groups[1].Value.ToUpperInvariant()) $literalPath"
        if ($endpointKeys -notcontains $literalKey) {
            Add-Failure "SPEC-$($item.id) documents unregistered endpoint literal $literalKey."
        }
    }

    if ($plan -notmatch '(?m)^## (?:Feature (?:Design|Design and Boundaries)|Design Decisions)' -or
        $plan -notmatch '(?m)^## (?:Delivery Sequence|Delivery Sequence and Rollback|Workstreams and Order|Execution and Gate Order|Execution Strategy)') {
        Add-Failure "SPEC-$($item.id) plan lacks feature-specific design/boundary and delivery-sequence sections."
    }

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

    $taskMatches = @([regex]::Matches($tasks, '(?m)^- \[(?: |x)\] (T\d{3})\s+(.+)$'))
    if ($taskMatches.Count -eq 0) { Add-Failure "SPEC-$($item.id) has no actionable tasks." }
    $taskIds = @($taskMatches | ForEach-Object { $_.Groups[1].Value })
    Assert-Sequential $item.id 'task' $taskIds
    $completedTaskMatches = @([regex]::Matches($tasks, '(?mi)^- \[[xX]\] (T\d{3})\s+(.+)$'))
    foreach ($completedTask in $completedTaskMatches) {
        $isApprovalTask = $completedTask.Groups[2].Value -match "(?i)Ahmed Elbamby's (?:human approval|.*Gate A demo approval)|human approval for SPEC|Gate A demo approval by Ahmed"
        $completedTaskNumber = [int]($completedTask.Groups[1].Value.Substring(1))
        $isDesignOnlySchemaContract =
            $item.id -eq '005' -and
            $completedTaskNumber -ge 6 -and $completedTaskNumber -le 30 -and
            $completedTask.Groups[2].Value -match '\[SCHEMA-CONTRACT\]' -and
            $completedTask.Groups[2].Value -match '(?i)without loading or requiring (?:downstream )?runtime source'
        if (-not $implementationMode -and -not $isApprovalTask) {
            Add-Failure "SPEC-$($item.id) marks non-approval task $($completedTask.Groups[1].Value) complete before implementation."
        }
        if ($implementationMode) {
            foreach ($completedPath in Get-TaskPaths $completedTask.Groups[2].Value) {
                # A design-only schema contract verifies a declared future source path;
                # the separately approved canonical-owner spec creates that runtime file.
                if ($isDesignOnlySchemaContract -and $completedPath -match '^src/') { continue }
                if (-not (Test-Path (Join-Path $root $completedPath) -PathType Leaf)) {
                    Add-Failure "SPEC-$($item.id) marks $($completedTask.Groups[1].Value) complete but required artifact $completedPath is missing."
                }
            }
        }
    }
    foreach ($task in $taskMatches) {
        $taskId = $task.Groups[1].Value
        $body = $task.Groups[2].Value
        if ($body -match 'Implement AC-|using plan\.md|Run unit, integration|Complete .* gates|under tests/|Deliver FR-\d+ at\s*:|Spec\d+(?:Feature|Frontend)\.cs|satisfies its positive, negative, authorization, persistence, and boundary cases') {
            Add-Failure "SPEC-$($item.id) $taskId is generic or directory-only."
        }
        if (-not (Test-ExactFuturePath $body)) {
            Add-Failure "SPEC-$($item.id) $taskId does not name an exact future file."
        }
        if ($body -match 'src/StudentRegistration\.(?:Server|Domain|Application)/' -or
            $body -match 'src/StudentRegistration\.Infrastructure/(?!SqlServer)') {
            Add-Failure "SPEC-$($item.id) $taskId uses a prohibited layer-project source path instead of the owning business-module project."
        }
    }
    $taskLines = ($taskMatches | ForEach-Object { $_.Value }) -join "`n"

    $parallelPathClaims = New-Object System.Collections.Generic.List[object]
    foreach ($parallelTask in @($taskMatches | Where-Object { $_.Groups[2].Value -match '^\[P\]' })) {
        foreach ($parallelPath in Get-TaskPaths $parallelTask.Groups[2].Value) {
            $parallelPathClaims.Add([pscustomobject]@{ path = $parallelPath; task = $parallelTask.Groups[1].Value })
        }
    }
    foreach ($parallelGroup in ($parallelPathClaims | Group-Object path | Where-Object Count -gt 1)) {
        $collidingIds = (@($parallelGroup.Group.task) | Sort-Object) -join ', '
        Add-Failure "SPEC-$($item.id) falsely marks parallel tasks $collidingIds that write the same path $($parallelGroup.Name)."
    }

    $approvalTasksForSpec = @($taskMatches | Where-Object { $_.Groups[2].Value -match "(?i)Ahmed Elbamby's (?:human approval|.*Gate A demo approval)|human approval for SPEC|Gate A demo approval by Ahmed" })
    if ($approvalTasksForSpec.Count -ne 1) {
        Add-Failure "SPEC-$($item.id) must have exactly one final pre-implementation human-approval task; found $($approvalTasksForSpec.Count)."
    } else {
        $approvalNumber = Get-TaskNumber $approvalTasksForSpec[0]
        $readinessTasks = @($taskMatches | Where-Object {
            $_.Groups[2].Value -match '\[DEP-SPEC-|implementation-readiness\.md|\[CONSISTENCY-ANALYSIS\]'
        })
        if (@($readinessTasks | Where-Object { (Get-TaskNumber $_) -ge $approvalNumber }).Count -gt 0) {
            Add-Failure "SPEC-$($item.id) requests human approval before dependency/readiness/consistency analysis is complete."
        }
        $executionTasks = @($taskMatches | Where-Object {
            $_.Groups[2].Value -match '(?:tests|src|\.github)/[^\s,;]+\.[A-Za-z0-9]+'
        })
        if (@($executionTasks | Where-Object { (Get-TaskNumber $_) -le $approvalNumber }).Count -gt 0) {
            Add-Failure "SPEC-$($item.id) schedules test/source execution before the human-approval gate."
        }
    }

    if ($persistenceMappingEvidence.ContainsKey([string]$item.id)) {
        $mappingPaths = $persistenceMappingEvidence[[string]$item.id]
        $mappingTests = @($taskMatches | Where-Object {
            $_.Groups[2].Value -match '\[PERSISTENCE-MAPPING\]' -and
            $_.Groups[2].Value -match [regex]::Escape($mappingPaths[0]) -and
            $_.Groups[2].Value -match '(?i)\b(failing|test)\b'
        })
        $mappingDeliveries = @($taskMatches | Where-Object {
            $_.Groups[2].Value -match '\[PERSISTENCE-MAPPING\]' -and
            $_.Groups[2].Value -match [regex]::Escape($mappingPaths[1]) -and
            $_.Groups[2].Value -match '(?i)\bdeliver\b' -and
            $_.Groups[2].Value -notmatch '^\[P\]'
        })
        if ($mappingTests.Count -ne 1) { Add-Failure "SPEC-$($item.id) must have exactly one real-SQL persistence mapping test at $($mappingPaths[0]); found $($mappingTests.Count)." }
        if ($mappingDeliveries.Count -ne 1) { Add-Failure "SPEC-$($item.id) must have exactly one owner mapping contribution at $($mappingPaths[1]); found $($mappingDeliveries.Count)." }
        if ($mappingTests.Count -eq 1 -and $mappingDeliveries.Count -eq 1 -and
            (Get-TaskNumber $mappingTests[0]) -ge (Get-TaskNumber $mappingDeliveries[0])) {
            Add-Failure "SPEC-$($item.id) persistence mapping delivery is not preceded by its real-SQL test."
        }
        $mapping = $persistenceManifest.contributions.PSObject.Properties[[string]$item.id].Value
        foreach ($entity in @($mapping.entities)) {
            $entityTag = "\[ENTITY-$([regex]::Escape([string]$entity))\]"
            if ($mappingTests.Count -eq 1 -and $mappingTests[0].Groups[2].Value -notmatch $entityTag) {
                Add-Failure "SPEC-$($item.id) persistence mapping test omits entity tag $entity."
            }
            if ($mappingDeliveries.Count -eq 1 -and $mappingDeliveries[0].Groups[2].Value -notmatch $entityTag) {
                Add-Failure "SPEC-$($item.id) persistence mapping delivery omits entity tag $entity."
            }
        }
    }

    $specWorkstreamProperty = $workstreamManifest.specs.PSObject.Properties | Where-Object Name -eq $item.id
    $specWorkstreams = if ($specWorkstreamProperty) { @($specWorkstreamProperty.Value) } else { @() }
    foreach ($fr in $frs) {
        $mappedStreams = @($specWorkstreams | Where-Object { @($_.requirements) -contains $fr })
        if ($mappedStreams.Count -ne 1) { Add-Failure "SPEC-$($item.id) $fr must map to exactly one workstream; found $($mappedStreams.Count)." }
    }
    foreach ($stream in $specWorkstreams) {
        $streamName = [string]$stream.name
        $workstreamTag = ($streamName -replace '[^A-Za-z0-9]+', '-').Trim('-').ToUpperInvariant()
        $workstreamTagPattern = "\[WORKSTREAM-$([regex]::Escape($workstreamTag))\]"
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
                    $body -match $workstreamTagPattern -and
                    $body -match '(?i)\b(?:test|tests|suite|verify|assert|coverage)\b'
            })
            $deliveryCandidates = @($taskMatches | Where-Object {
                $body = $_.Groups[2].Value
                $body -match $frTag -and
                    $body -match [regex]::Escape($deliveryPath) -and
                    $body -match $workstreamTagPattern -and
                    $body -match '(?i)\bDeliver\b' -and
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
    for ($successIndex = 1; $successIndex -le @($item.successCriteria).Count; $successIndex++) {
        $successId = "SC-$successIndex"
        $successTasks = @($taskMatches | Where-Object {
            $_.Groups[2].Value -match "\[$([regex]::Escape($successId))\]" -and
            $_.Groups[2].Value -match '(?:tests|docs/release-evidence)/[^\s,;]+\.[A-Za-z0-9]+'
        })
        if ($successTasks.Count -lt 1) {
            Add-Failure "SPEC-$($item.id) $successId has no explicit measurable test/release-evidence task."
        }
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
        if ($canonicalOwner -ne [string]$item.id -and
            $model -notmatch "(?is)$([regex]::Escape($entity)).{0,300}(?:SPEC-$canonicalOwner|canonical owner)" -and
            $model -notmatch "(?is)(?:SPEC-$canonicalOwner|canonical owner).{0,300}$([regex]::Escape($entity))") {
            Add-Failure "SPEC-$($item.id) data model does not identify $entity as consumed from canonical owner SPEC-$canonicalOwner."
        }
        $canonicalPath = Get-EntityArtifactPath $canonicalOwner $entity
        $entityTagPattern = "\[ENTITY-$([regex]::Escape($entity))\]"
        $globalEntityTasks = @($taskRegistry | Where-Object { $_.body -match $entityTagPattern })
        $ownerWriters = @($globalEntityTasks | Where-Object {
            $_.body -match [regex]::Escape($canonicalPath) -and
            $_.body -match '(?i)\b(?:deliver|publish) the canonical\b' -and
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
            $schemaTasks = @($taskMatches | Where-Object {
                $_.Groups[2].Value -match $entityTagPattern -and
                $_.Groups[2].Value -match '\[SCHEMA-CONTRACT\]' -and
                $_.Groups[2].Value -match "\[CONSUMER-SPEC-$canonicalOwner\]" -and
                $_.Groups[2].Value -match [regex]::Escape($canonicalPath) -and
                $_.Groups[2].Value -match [regex]::Escape('docs/diagrams/ERD.md')
            })
            if ($schemaTasks.Count -ne 1) { Add-Failure "SPEC-005 entity $entity must have exactly one schema-conformance task against canonical owner SPEC-$canonicalOwner; found $($schemaTasks.Count)." }
            if (@($schemaTasks | Where-Object { $_.Groups[2].Value -match '(?i)\b(deliver|implement|map the canonical)\b' }).Count -gt 0) {
                Add-Failure "SPEC-005 attempts to deliver downstream runtime model/mapping $entity."
            }
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
        $moduleProject = if ([string]$item.module -eq 'Operations') { 'Api' } else { [string]$item.module }
        $handlerPath = "src/StudentRegistration.$moduleProject/Endpoints/Spec$($item.id)Endpoints.cs"
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
        $behaviorTests = @($taskMatches | Where-Object {
            $_.Groups[2].Value -match 'tests/[^\s,;]+\.[A-Za-z0-9]+' -and
            ($_.Groups[2].Value -match '\[WORKSTREAM-[A-Za-z0-9-]+\]' -or
             ($_.Groups[2].Value -match '\[AC-\d+\]' -and $_.Groups[2].Value -match 'tests/StudentRegistration\.AcceptanceTests/'))
        })
        if ($handlerTasks.Count -eq 1 -and $testTasksForEndpoint.Count -eq 1) {
            $handlerBody = $handlerTasks[0].Groups[2].Value
            $referencedTaskIds = @(Get-ReferencedTaskIds $handlerBody)
            $contractTaskId = $testTasksForEndpoint[0].Groups[1].Value
            if ($referencedTaskIds -notcontains $contractTaskId) {
                Add-Failure "SPEC-$($item.id) endpoint $endpoint handler does not explicitly depend on contract test $contractTaskId."
            }
            $referencedBehaviorTests = @($behaviorTests | Where-Object { $referencedTaskIds -contains $_.Groups[1].Value })
            if ($referencedBehaviorTests.Count -lt 1) {
                Add-Failure "SPEC-$($item.id) endpoint $endpoint handler has no explicit acceptance/workstream test dependency."
            }
            foreach ($referencedTest in @($testTasksForEndpoint[0]) + $referencedBehaviorTests) {
                if ((Get-TaskNumber $referencedTest) -ge (Get-TaskNumber $handlerTasks[0])) {
                    Add-Failure "SPEC-$($item.id) endpoint $endpoint handler precedes referenced test $($referencedTest.Groups[1].Value)."
                }
            }
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

    $declaredRoutes = @($routeManifest.routes | Where-Object {
        [string]$_.designOwner -eq [string]$item.id -or
        [string]$_.implementationOwner -eq [string]$item.id
    })
    foreach ($route in $declaredRoutes) {
        if ($spec -notmatch [regex]::Escape($route.id)) {
            Add-Failure "SPEC-$($item.id) spec.md omits canonical route responsibility $($route.id)."
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
        if ($taskLines -notmatch [regex]::Escape('docs/release-evidence/SPEC-003-NFR-7.md') -or
            $taskLines -notmatch '(?i)WebKit.*(?:not|rather than).*Safari|label WebKit only as WebKit') {
            Add-Failure 'SPEC-003 lacks the approved POC browser-matrix evidence and WebKit-not-Safari rule.'
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

    $score = $null
    if ([string]::IsNullOrWhiteSpace($strictValidatorPath)) {
        Add-Failure 'Strict requirements validator was not found. Set SPEC_VALIDATOR_PATH or install the documented development skill.'
    } else {
        $validationJson = & python $strictValidatorPath --file (Join-Path $dir 'requirements.md') --strict --json 2>&1
        if ($LASTEXITCODE -ne 0) { Add-Failure "SPEC-$($item.id) strict requirements validation failed: $validationJson" }
        $score = try { ($validationJson | ConvertFrom-Json).score } catch { $null }
    }

    $results.Add([pscustomobject]@{
        spec = "SPEC-$($item.id)"
        artifacts = (Get-ChildItem $dir -Recurse -File).Count
        requirements = $definedRequirements.Count
        acceptanceCriteria = $acs.Count
        edgeCases = $ecs.Count
        tasks = $taskMatches.Count
        requirementsScore = $score
        automatedGates = if ($failures.Count -eq $before) { 'PASS' } else { 'FAIL' }
        humanApproval = if ($isApproved) { 'APPROVED' } else { 'PENDING' }
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

$sourceWriterClaims = New-Object System.Collections.Generic.List[object]
foreach ($taskRecord in $taskRegistry) {
    if ($taskRecord.body -notmatch '(?i)\b(deliver|implement|map the canonical)\b' -or
        $taskRecord.body -match '(?i)\bwithout editing\b') { continue }
    foreach ($path in Get-TaskPaths $taskRecord.body) {
        if ($path -match '^src/') {
            $sourceWriterClaims.Add([pscustomobject]@{ path = $path; spec = $taskRecord.spec; task = $taskRecord.id })
        }
    }
}
foreach ($writerGroup in ($sourceWriterClaims | Group-Object path)) {
    $writerSpecs = @($writerGroup.Group.spec | Select-Object -Unique)
    $declaredSnapshotPaths = @($persistenceManifest.migrations.snapshotPath | Select-Object -Unique)
    if ($declaredSnapshotPaths -contains $writerGroup.Name) {
        $allowedSnapshotWriters = @($persistenceManifest.migrations.owner | Select-Object -Unique)
        $foreignSnapshotWriters = @($writerSpecs | Where-Object { $allowedSnapshotWriters -notcontains $_ })
        if ($foreignSnapshotWriters.Count -gt 0) {
            Add-Failure "EF model snapshot has undeclared migration writers in SPEC-$($foreignSnapshotWriters -join ', SPEC-')."
        }
    } elseif ($writerSpecs.Count -gt 1) {
        $claims = @($writerGroup.Group | ForEach-Object { "SPEC-$($_.spec)/$($_.task)" }) -join ', '
        Add-Failure "Canonical source path $($writerGroup.Name) has delivery writers in multiple specs: $claims."
    }
}
$dbContextPath = 'src/StudentRegistration.Infrastructure.SqlServer/Persistence/StudentRegistrationDbContext.cs'
$dbContextWriters = @($taskRegistry | Where-Object {
    $_.body -match [regex]::Escape($dbContextPath) -and
    $_.body -match '(?i)\b(create|deliver|implement)\b' -and
    $_.body -notmatch '(?i)\b(test|verify|assert|failing)\b'
})
if ($dbContextWriters.Count -ne 1 -or ($dbContextWriters.Count -eq 1 -and $dbContextWriters[0].spec -ne '004')) {
    Add-Failure "StudentRegistrationDbContext must have exactly one canonical SPEC-004 writer; found $($dbContextWriters.Count)."
}
for ($migrationIndex = 0; $migrationIndex -lt @($persistenceManifest.migrations).Count; $migrationIndex++) {
    $migration = $persistenceManifest.migrations[$migrationIndex]
    $tag = "\[MIGRATION-$([regex]::Escape([string]$migration.id))\]"
    $testTasks = @($taskRegistry | Where-Object {
        $_.spec -eq [string]$migration.owner -and $_.body -match $tag -and
        $_.body -match 'tests/[^\s,;]+Tests\.cs' -and $_.body -match '(?i)\b(?:test|tests|failing|verify)\b'
    })
    $deliveryTasks = @($taskRegistry | Where-Object {
        $_.spec -eq [string]$migration.owner -and $_.body -match $tag -and
        $_.body -match [regex]::Escape([string]$migration.path) -and
        $_.body -match [regex]::Escape([string]$migration.snapshotPath) -and
        $_.body -match '(?i)\b(?:generate|deliver)\b'
    })
    if ($testTasks.Count -ne 1) { Add-Failure "Migration $($migration.id) must have exactly one owner test task; found $($testTasks.Count)." }
    if ($deliveryTasks.Count -ne 1) { Add-Failure "Migration $($migration.id) must have exactly one owner delivery task; found $($deliveryTasks.Count)." }
    if ($testTasks.Count -eq 1 -and $deliveryTasks.Count -eq 1 -and $testTasks[0].number -ge $deliveryTasks[0].number) {
        Add-Failure "Migration $($migration.id) delivery is not preceded by its test."
    }
    foreach ($prerequisiteId in @($migration.prerequisiteSpecs)) {
        if ([string]$migration.owner -ne [string]$prerequisiteId -and
            -not (Test-DependsTransitively ([string]$migration.owner) ([string]$prerequisiteId) @{})) {
            Add-Failure "Migration $($migration.id) owner SPEC-$($migration.owner) does not depend on prerequisite SPEC-$prerequisiteId."
        }
    }
    if ($migrationIndex -gt 0) {
        $previousOwner = [string]$persistenceManifest.migrations[$migrationIndex - 1].owner
        if ([string]$migration.owner -ne $previousOwner -and -not (Test-DependsTransitively ([string]$migration.owner) $previousOwner @{})) {
            Add-Failure "Migration $($migration.id) owner SPEC-$($migration.owner) does not follow prior migration owner SPEC-$previousOwner."
        }
    }
}
Remove-Item Env:SPECIFY_FEATURE -ErrorAction SilentlyContinue
Remove-Item Env:SPECIFY_FEATURE_DIRECTORY -ErrorAction SilentlyContinue

if (-not $implementationMode -and $implementationFiles.Count -gt 0) {
    Add-Failure "Planning-only boundary violated by $($implementationFiles.Count) implementation files."
}

$erdPath = Join-Path $root 'docs/diagrams/ERD.md'
$erd = if (Test-Path $erdPath) { Get-Content $erdPath -Raw } else { '' }
foreach ($term in @(
    'StudentTermRegistrationGuard', 'PayloadHash', 'ProcessingState',
    'ReceivedAtUtc', 'CompletedAtUtc', 'RegistrationPaused',
    'RegistrationWindow', 'StaffTermAvailability', 'RegistrationReceipt',
    'CatalogueVersion', 'AccountRecoveryChallenge', 'RoleAssignment',
    'ScheduleImpactAlert', 'ExportJob', 'AdminSecurityGuard',
    'BeforeSummaryJson', 'AfterSummaryJson', 'CorrelationId',
    'LeaseOwnerId', 'LeaseExpiresAtUtc', 'ApplicationUserId', 'ProgramCode',
    'CourseCode', 'ActorReference', 'SubjectReference', 'rowversion'
)) {
    if ($erd -notmatch [regex]::Escape($term)) { Add-Failure "Shared ERD omits concurrency field/entity $term." }
}

$classDiagramPath = Join-Path $root 'docs/diagrams/CLASS_DIAGRAM.md'
$classDiagram = if (Test-Path $classDiagramPath) { Get-Content $classDiagramPath -Raw } else { '' }
foreach ($term in @(
    'RegistrationCommandFactory', 'RegistrationTransactionCoordinator',
    'SqlSeatAllocator', 'RegistrationSubmissionStore',
    'StudentRegistrationDbContext', 'OptimizationCoordinator', 'ScheduleOptimizer',
    'ScheduleScorer', 'RecommendationApplicationService', 'AcademicContextResolver',
    'IRegistrationTransactionCoordinator', 'CancellationToken'
)) {
    if ($classDiagram -notmatch [regex]::Escape($term)) { Add-Failure "Shared class diagram omits current design term $term." }
}
foreach ($staleTerm in @('RegistrationService', 'IRegistrationCommitter', 'SqlRegistrationCommitter', 'RegistrationDbContext')) {
    if ($classDiagram -match "\b$([regex]::Escape($staleTerm))\b") { Add-Failure "Shared class diagram still contains stale type $staleTerm." }
}

$identityPlanningText = @(
    (Get-Content (Join-Path $root 'specs/007-identity-account-lifecycle/spec.md') -Raw),
    (Get-Content (Join-Path $root 'specs/007-identity-account-lifecycle/requirements.md') -Raw),
    (Get-Content (Join-Path $root 'specs/007-identity-account-lifecycle/data-model.md') -Raw),
    (Get-Content (Join-Path $root 'specs/007-identity-account-lifecycle/contracts/api.md') -Raw),
    (Get-Content (Join-Path $root 'specs/007-identity-account-lifecycle/tasks.md') -Raw),
    (Get-Content (Join-Path $root 'docs/diagrams/ERD.md') -Raw)
) -join "`n"
foreach ($staleIdentityTerm in @(
    'StaffMfaChallenge','MfaChallengeDto','MfaVerifyRequest',
    '/api/auth/staff/mfa/verify','StaffMfaTests',
    'WORKSTREAM-STAFF-AUTHENTICATION-AND-MFA'
)) {
    if ($identityPlanningText -match [regex]::Escape($staleIdentityTerm)) {
        Add-Failure "Password-only demo identity design still contains stale MFA artifact $staleIdentityTerm."
    }
}

$contractTermChecks = @{
    '006-domain-class-api-contracts' = @('TermSummaryDto','serviceState','role-selection-required','supportReferencePath','PAGE_SIZE_INVALID','expectedRowVersion','STALE_VERSION','If-Match','OpenAPI','semantic diff')
    '007-identity-account-lifecycle' = @('StaffLoginRequest','IdentityImportBatchDto','SessionDto','sessionState','activeRole','recovery/complete','revoke-all','expectedRoleSetVersion','role-selection-required','AdminSecurityGuard','FINAL_ADMIN_REQUIRED')
    '008-academic-term-student-profile' = @('TermSummaryDto','PublicContextDto','AcademicProfileCorrectionOperation','set-gpa','upsert-transcript-attempt','transcript','blocksRegistration','supportReferencePath','expectedStudentRowVersion','WINDOW_OVERLAP')
    '009-catalog-prerequisites-policy-admin' = @('CatalogueDraftDto','CatalogueDraftOperation','PolicyRuleAdminDto','PolicySetMutationRequest','PolicyPublishRequest','PolicySimulationResult','CatalogueVersionSummaryDto','ImportBatchDto','expectedDraftRowVersion','previewToken','clientRequestId','STALE_PREVIEW','IDEMPOTENCY_KEY_REUSED')
    '010-offerings-groups-resources' = @('registrationPaused','expectedGroupRowVersions','expectedRoomRowVersions','expectedStaffTermAvailabilityRowVersions','StaffTermAvailabilityDto','ScheduleImpactAlert','previewToken','clientRequestId','GROUP_CHANGED')
    '011-eligibility-subject-discovery' = @('requiredValue','currentValue','sourceReference','supportReferencePath','Page<OfferingEligibilityDto>','PAGE_SIZE_INVALID')
    '012-schedule-builder-conflicts' = @('ScheduleConflictDto','courseCode','subjectTitle','actions','expectedPlanRowVersion','registration-plan/validate','STALE_VERSION')
    '013-schedule-recommendations' = @('MeetingIntervalDto','SchedulePreferencesDto','OptimizerConfiguration','preference-violations','idle-minutes','optionToken','OptimizationDiagnosticDto','inclusion-minimal','expectedPlanRowVersion','requestCorrelationId','catalogueVersion','policyVersion','optimizerConfigurationVersion','Registration.ScheduleOption.v1','10 minutes','recommended-option')
    '014-registration-capacity-concurrency' = @('RegistrationGroupSnapshotDto','RegistrationReceiptSnapshotDto','expectedPlanRowVersion','clientRequestId','receivedAtUtc','completedAtUtc','RegistrationFinalResult','RegistrationInProgressResponse','retryAfterSeconds','no submissionId','REQUEST_NOT_FOUND','201','200','202','409','route TermId','by-request','ReceiptSnapshot')
    '015-student-registration-records' = @('RegistrationReceiptDto','RegistrationDetailDto','RegistrationRejectedResultDto','noPartialRegistration','Page<RegistrationHistoryRowDto>','reference?:','Reference','ReceiptSnapshot','RegistrationRecords.Read','PAGE_SIZE_INVALID')
    '016-lecturer-ta-workspace' = @('RosterRowDto','Page<RosterRowDto>','rowVersion','expectedStaffTermRowVersion','ScheduleImpactAlert','STALE_VERSION','AVAILABILITY_DEADLINE_PASSED')
    '017-admin-operations-audit-reporting' = @('RedactedChangeSummaryDto','sourceStream','RegistrationReconciliationAlertDto','supportReferencePath','beforeSummary','afterSummary','correlationId','ExportJobDto','LeaseOwnerId','LeaseExpiresAtUtc','AdminSecurityGuard','FINAL_ADMIN_REQUIRED','expectedRowVersion','clientRequestId','previewToken','STALE_PREVIEW','IDEMPOTENCY_KEY_REUSED')
    '018-quality-security-scalability-operations' = @('HealthSummary','OperationalMetric','observedAtUtc')
}
foreach ($contractEntry in $contractTermChecks.GetEnumerator()) {
    $contractPath = Join-Path $root "specs/$($contractEntry.Key)/contracts/api.md"
    $contractText = if (Test-Path $contractPath) { Get-Content $contractPath -Raw } else { '' }
    foreach ($term in $contractEntry.Value) {
        if ($contractText -notmatch [regex]::Escape($term)) { Add-Failure "$($contractEntry.Key) API contract omits concurrency token/result $term." }
    }
}

$requirementsApiTermChecks = @{
    '007-identity-account-lifecycle' = @('activeRole','sessionState','expiring','role-selection-required')
    '008-academic-term-student-profile' = @('PublicContextDto','AcademicProfileCorrectionOperation','upsert-transcript-attempt')
    '009-catalog-prerequisites-policy-admin' = @('CatalogueDraftOperation','PolicyRuleAdminDto','PolicyPublishRequest','PolicySimulationResult')
    '010-offerings-groups-resources' = @('ScheduleImpactAlertDto')
    '015-student-registration-records' = @('RegistrationDetailDto','RegistrationRejectedResultDto','reference?:','noPartialRegistration')
    '017-admin-operations-audit-reporting' = @('RedactedChangeSummaryDto','AdminOperationsMetricsDto','RegistrationReconciliationAlertDto','ExportJobDto')
}
foreach ($requirementsEntry in $requirementsApiTermChecks.GetEnumerator()) {
    $requirementsPath = Join-Path $root "specs/$($requirementsEntry.Key)/requirements.md"
    $requirementsText = if (Test-Path $requirementsPath) { Get-Content $requirementsPath -Raw } else { '' }
    foreach ($term in $requirementsEntry.Value) {
        if ($requirementsText -notmatch [regex]::Escape($term)) {
            Add-Failure "$($requirementsEntry.Key) requirements API section omits canonical term $term."
        }
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
**Scope**: 18 connected specifications and their implementation gates
**Human approval**: Gate A APPROVED by Ahmed ELbamby for demo implementation

| Spec | Artifacts | FR+NFR | AC | EC | Actionable tasks | Strict score | Automated gates | Human approval |
|---|---:|---:|---:|---:|---:|---:|---|---|
$($reportRows -join "`r`n")

## Enforced Cross-Spec Results

- Exactly 18 specs and 27 frontend routes; every route has SPEC-003 governance and feature ownership.
- Dependencies exist, match requirements metadata, contain no cycle, and every spec reaches SPEC-001.
- Every FR/NFR has acceptance coverage; every FR has delivery and verification tasks.
- Every NFR, AC, EC, out-of-scope guard, entity, endpoint, dependency, and owned route has actionable exact-file tasks.
- Every requirements API declaration is structurally synchronized with its canonical `contracts/api.md` declaration block.
- SPEC-003 has per-route design, Blazor, component, E2E, accessibility, browser, and visual tasks.
- SPEC-014 has explicit cross-aggregate serialization, idempotency, cutoff, admin-versus-submit, two-replica, failure, and reconciliation design.
- Official Spec Kit prerequisite and strict workflow validators run for every package.
- No prohibited premature downstream runtime source, migration, or production deployment artifact was detected.

## Failures

$(if ($failures.Count) { ($failures | ForEach-Object { "- $_" }) -join "`r`n" } else { 'None.' })
"@ | Set-Content (Join-Path $root 'docs/SPECKIT_AUDIT.md')

[pscustomobject]@{ overall = $overall; specifications = $results; failures = $failures } | ConvertTo-Json -Depth 6
if ($failures.Count -gt 0) { exit 1 }
