[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter()]
    [string] $RepositoryRoot = (Join-Path $PSScriptRoot '..\..'),

    [Parameter()]
    [string[]] $ArtifactDirectories = @('.local', 'credentials', 'logs', 'exports'),

    [Parameter()]
    [ValidateRange(1, 30)]
    [int] $RetentionDays = 7,

    [Parameter()]
    [datetimeoffset] $NowUtc = (Get-Date).ToUniversalTime()
)

$ErrorActionPreference = 'Stop'
$resolvedRepositoryRoot = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$cutoffUtc = $NowUtc.AddDays(-$RetentionDays)
$approvedDirectoryNames = @('.local', 'credentials', 'logs', 'exports')
$pathComparison = if ($IsWindows) {
    [StringComparison]::OrdinalIgnoreCase
}
else {
    [StringComparison]::Ordinal
}

function Test-PathWithinRoot {
    param(
        [Parameter(Mandatory)]
        [string] $Path,

        [Parameter(Mandatory)]
        [string] $Root
    )

    $trimmedRoot = [IO.Path]::TrimEndingDirectorySeparator($Root)
    $rootPrefix = $trimmedRoot + [IO.Path]::DirectorySeparatorChar
    return $Path.Equals($trimmedRoot, $pathComparison) -or
        $Path.StartsWith($rootPrefix, $pathComparison)
}

$approvedRoots = @{}
foreach ($directoryName in $approvedDirectoryNames) {
    $approvedRoots[$directoryName] = [IO.Path]::GetFullPath(
        (Join-Path $resolvedRepositoryRoot $directoryName))
}

foreach ($artifactDirectory in $ArtifactDirectories) {
    if ([string]::IsNullOrWhiteSpace($artifactDirectory) -or
        [IO.Path]::IsPathFullyQualified($artifactDirectory)) {
        throw "Artifact directory must be a repository-relative approved path."
    }

    $candidatePath = [IO.Path]::GetFullPath(
        (Join-Path $resolvedRepositoryRoot $artifactDirectory))
    if (-not (Test-PathWithinRoot -Path $candidatePath -Root $resolvedRepositoryRoot)) {
        throw "Artifact directory resolves outside the repository."
    }

    $approvedRoot = $null
    foreach ($root in $approvedRoots.Values) {
        if (Test-PathWithinRoot -Path $candidatePath -Root $root) {
            $approvedRoot = $root
            break
        }
    }

    if ($null -eq $approvedRoot) {
        throw "Artifact directory is outside the approved cleanup roots."
    }

    if (-not (Test-Path -LiteralPath $candidatePath -PathType Container)) {
        continue
    }

    $resolvedArtifactRoot = (Resolve-Path -LiteralPath $candidatePath).Path
    if (-not (Test-PathWithinRoot -Path $resolvedArtifactRoot -Root $approvedRoot) -or
        -not (Test-PathWithinRoot -Path $resolvedArtifactRoot -Root $resolvedRepositoryRoot)) {
        throw "Resolved artifact directory escaped an approved cleanup root."
    }

    $pendingDirectories = [Collections.Generic.Stack[IO.DirectoryInfo]]::new()
    $pendingDirectories.Push([IO.DirectoryInfo]::new($resolvedArtifactRoot))

    while ($pendingDirectories.Count -gt 0) {
        $currentDirectory = $pendingDirectories.Pop()

        foreach ($file in $currentDirectory.EnumerateFiles()) {
            if (($file.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
                continue
            }

            if ($file.LastWriteTimeUtc -lt $cutoffUtc.UtcDateTime -and
                $PSCmdlet.ShouldProcess($file.FullName, 'Remove expired local artifact')) {
                Remove-Item -LiteralPath $file.FullName -Force
            }
        }

        foreach ($directory in $currentDirectory.EnumerateDirectories()) {
            if (($directory.Attributes -band [IO.FileAttributes]::ReparsePoint) -eq 0) {
                $pendingDirectories.Push($directory)
            }
        }
    }
}
