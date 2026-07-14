using System.Text.Json;
using Microsoft.Extensions.Hosting;
using StudentRegistration.IdentityAccess.Application;

namespace StudentRegistration.Api.Development;

/// <summary>
/// Writes the one-time credential handoff used by the local Development demo.
/// The generated artifact is intentionally outside the web root and beneath
/// the repository-local, Git-ignored .local/credentials directory.
/// </summary>
public sealed class DemoCredentialSheetWriter
{
    private const int MaximumCredentialCount = 25_004;
    private const int MaximumLoginIdentifierLength = 128;
    private const int MinimumSecretLength = 15;
    private const int MaximumSecretLength = 128;
    private const string ArtifactPrefix = "credentials-";
    private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(7);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IHostEnvironment _environment;
    private readonly TimeProvider _timeProvider;
    private readonly string _storageDirectory;

    public DemoCredentialSheetWriter(
        IHostEnvironment environment,
        TimeProvider timeProvider)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

        EnsureDevelopment();
        var repositoryRoot = ResolveRepositoryRoot(environment.ContentRootPath);
        _storageDirectory = ResolveContainedPath(
            repositoryRoot,
            ".local",
            "credentials");
    }

    /// <summary>
    /// Creates one new credential sheet. Existing sheets are never appended to
    /// or overwritten, so a caller must explicitly consume the returned path.
    /// </summary>
    public async Task<string> WriteAsync(
        IReadOnlyCollection<DemoCredential> credentials,
        CancellationToken cancellationToken = default)
    {
        EnsureDevelopment();
        ArgumentNullException.ThrowIfNull(credentials);
        ValidateCredentials(credentials);

        await DeleteExpiredAsync(cancellationToken);

        var generatedAtUtc = _timeProvider.GetUtcNow();
        var expiresAtUtc = generatedAtUtc.Add(RetentionPeriod);
        Directory.CreateDirectory(_storageDirectory);

        var artifactName =
            $"{ArtifactPrefix}{expiresAtUtc.UtcTicks:D19}-{Guid.NewGuid():N}.json";
        var artifactPath = ResolveContainedPath(_storageDirectory, artifactName);
        var payload = new CredentialSheet(
            "demo-identity-provisioning/1.0",
            generatedAtUtc,
            expiresAtUtc,
            credentials
                .Select(credential => new CredentialSheetEntry(
                    credential.LoginIdentifier,
                    credential.Secret,
                    new DateTimeOffset(
                        DateTime.SpecifyKind(credential.GeneratedAtUtc, DateTimeKind.Utc))))
                .ToArray());

        try
        {
            await using var stream = new FileStream(
                artifactPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 16 * 1024,
                FileOptions.Asynchronous | FileOptions.WriteThrough);

            RestrictOwnerAccess(artifactPath);
            await JsonSerializer.SerializeAsync(
                stream,
                payload,
                JsonOptions,
                cancellationToken);
            await stream.FlushAsync(cancellationToken);
            return artifactPath;
        }
        catch
        {
            DeletePartialArtifact(artifactPath);
            throw;
        }
    }

    /// <summary>
    /// Deletes only credential sheets owned by this adapter and never traverses
    /// outside its repository-local directory.
    /// </summary>
    public Task DeleteExpiredAsync(CancellationToken cancellationToken = default)
    {
        EnsureDevelopment();
        if (!Directory.Exists(_storageDirectory))
        {
            return Task.CompletedTask;
        }

        var utcNow = _timeProvider.GetUtcNow();
        foreach (var path in Directory.EnumerateFiles(
                     _storageDirectory,
                     $"{ArtifactPrefix}*.json",
                     SearchOption.TopDirectoryOnly))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var containedPath = ResolveContainedPath(
                _storageDirectory,
                Path.GetFileName(path));
            if (GetExpiry(containedPath) <= utcNow)
            {
                File.Delete(containedPath);
            }
        }

        return Task.CompletedTask;
    }

    private void EnsureDevelopment()
    {
        if (!_environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "Development credential sheets are unavailable outside Development.");
        }
    }

    private static void ValidateCredentials(IReadOnlyCollection<DemoCredential> credentials)
    {
        if (credentials.Count is < 1 or > MaximumCredentialCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(credentials),
                $"Credential count must be between 1 and {MaximumCredentialCount}.");
        }

        foreach (var credential in credentials)
        {
            if (credential is null)
            {
                throw new ArgumentException(
                    "A credential sheet cannot contain a null entry.",
                    nameof(credentials));
            }

            if (string.IsNullOrWhiteSpace(credential.LoginIdentifier)
                || credential.LoginIdentifier.Length > MaximumLoginIdentifierLength)
            {
                throw new ArgumentException(
                    "Every credential requires a bounded login identifier.",
                    nameof(credentials));
            }

            if (string.IsNullOrEmpty(credential.Secret)
                || credential.Secret.Length is < MinimumSecretLength or > MaximumSecretLength)
            {
                throw new ArgumentException(
                    "Every transient credential must satisfy the approved demo length bounds.",
                    nameof(credentials));
            }
        }
    }

    private DateTimeOffset GetExpiry(string path)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        var ticksStart = ArtifactPrefix.Length;
        var ticksEnd = fileName.IndexOf('-', ticksStart);
        if (ticksEnd > ticksStart
            && long.TryParse(
                fileName.AsSpan(ticksStart, ticksEnd - ticksStart),
                out var ticks)
            && ticks >= DateTimeOffset.MinValue.UtcTicks
            && ticks <= DateTimeOffset.MaxValue.UtcTicks)
        {
            return new DateTimeOffset(ticks, TimeSpan.Zero);
        }

        // This fallback bounds any earlier adapter artifact that used metadata
        // instead of an expiry-bearing filename.
        var lastWriteUtc = File.GetLastWriteTimeUtc(path);
        return new DateTimeOffset(lastWriteUtc, TimeSpan.Zero).Add(RetentionPeriod);
    }

    private static string ResolveRepositoryRoot(string contentRootPath)
    {
        if (string.IsNullOrWhiteSpace(contentRootPath))
        {
            throw new InvalidOperationException(
                "A repository content root is required for Development artifacts.");
        }

        return Path.GetFullPath(contentRootPath);
    }

    private static string ResolveContainedPath(string root, params string[] segments)
    {
        var path = Path.GetFullPath(Path.Combine([root, .. segments]));
        var relative = Path.GetRelativePath(root, path);
        if (Path.IsPathRooted(relative)
            || relative.Equals("..", StringComparison.Ordinal)
            || relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
            || relative.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Development artifact path must remain inside the repository-local directory.");
        }

        return path;
    }

    private static void RestrictOwnerAccess(string artifactPath)
    {
        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(
                artifactPath,
                UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }

    private static void DeletePartialArtifact(string artifactPath)
    {
        try
        {
            File.Delete(artifactPath);
        }
        catch (IOException)
        {
            // Preserve the original write exception without exposing secrets.
        }
        catch (UnauthorizedAccessException)
        {
            // Preserve the original write exception without exposing secrets.
        }
    }

    private sealed record CredentialSheet(
        string ContractVersion,
        DateTimeOffset GeneratedAtUtc,
        DateTimeOffset ExpiresAtUtc,
        IReadOnlyList<CredentialSheetEntry> Accounts);

    private sealed record CredentialSheetEntry(
        string LoginIdentifier,
        string Secret,
        DateTimeOffset GeneratedAtUtc);
}
