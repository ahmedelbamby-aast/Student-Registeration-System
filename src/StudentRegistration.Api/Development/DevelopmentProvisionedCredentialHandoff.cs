using System.Text.Json;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Application.Ports;

namespace StudentRegistration.Api.Development;

/// <summary>
/// Crash-safe, Development-only credential handoff for Admin imports. A pending
/// artifact is not discoverable as a completed handoff until the SQL publisher
/// calls <see cref="CompleteAsync"/> after its transaction commits.
/// </summary>
public sealed class DevelopmentProvisionedCredentialHandoff : IProvisionedCredentialHandoff
{
    private const int MaximumCredentialCount = 25_004;
    private const int MaximumLoginIdentifierLength = 128;
    private const int MinimumSecretLength = 15;
    private const int MaximumSecretLength = 128;
    private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(7);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IHostEnvironment _environment;
    private readonly TimeProvider _timeProvider;
    private readonly string _pendingDirectory;
    private readonly string _completedDirectory;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public DevelopmentProvisionedCredentialHandoff(
        IHostEnvironment environment,
        TimeProvider timeProvider)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        EnsureDevelopment();

        var repositoryRoot = Path.GetFullPath(environment.ContentRootPath);
        var credentialRoot = ResolveContainedPath(repositoryRoot, ".local", "credentials");
        _pendingDirectory = ResolveContainedPath(credentialRoot, ".pending");
        _completedDirectory = ResolveContainedPath(credentialRoot, "imports");
    }

    public async Task PrepareAsync(
        Guid importId,
        IReadOnlyCollection<DemoCredential> credentials,
        CancellationToken cancellationToken)
    {
        EnsureDevelopment();
        ValidateImportId(importId);
        ValidateCredentials(credentials);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            DeleteExpiredCore(cancellationToken);

            Directory.CreateDirectory(_pendingDirectory);
            Directory.CreateDirectory(_completedDirectory);
            var completedPath = CompletedPath(importId);
            if (File.Exists(completedPath))
            {
                return;
            }

            var generatedAtUtc = _timeProvider.GetUtcNow();
            var payload = new CredentialHandoff(
                "demo-identity-import-handoff/1.0",
                importId,
                generatedAtUtc,
                generatedAtUtc.Add(RetentionPeriod),
                credentials
                    .Select(credential => new CredentialHandoffEntry(
                        credential.LoginIdentifier,
                        credential.Secret,
                        new DateTimeOffset(
                            DateTime.SpecifyKind(credential.GeneratedAtUtc, DateTimeKind.Utc))))
                    .ToArray());

            var pendingPath = PendingPath(importId);
            if (File.Exists(pendingPath))
            {
                await RequireMatchingPendingAsync(pendingPath, payload, cancellationToken);
                return;
            }

            var temporaryPath = ResolveContainedPath(
                _pendingDirectory,
                $"{importId:N}.{Guid.NewGuid():N}.tmp");
            try
            {
                await using (var stream = new FileStream(
                                 temporaryPath,
                                 FileMode.CreateNew,
                                 FileAccess.Write,
                                 FileShare.None,
                                 bufferSize: 16 * 1024,
                                 FileOptions.Asynchronous | FileOptions.WriteThrough))
                {
                    RestrictOwnerAccess(temporaryPath);
                    await JsonSerializer.SerializeAsync(
                        stream,
                        payload,
                        JsonOptions,
                        cancellationToken);
                    await stream.FlushAsync(cancellationToken);
                }

                try
                {
                    File.Move(temporaryPath, pendingPath);
                    RestrictOwnerAccess(pendingPath);
                }
                catch (IOException) when (File.Exists(completedPath) || File.Exists(pendingPath))
                {
                    if (!File.Exists(completedPath))
                    {
                        await RequireMatchingPendingAsync(
                            pendingPath,
                            payload,
                            cancellationToken);
                    }
                }
            }
            finally
            {
                DeleteIfPresent(temporaryPath);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task CompleteAsync(Guid importId, CancellationToken cancellationToken)
    {
        EnsureDevelopment();
        ValidateImportId(importId);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(_completedDirectory);
            var completedPath = CompletedPath(importId);
            var pendingPath = PendingPath(importId);
            if (File.Exists(completedPath))
            {
                DeleteIfPresent(pendingPath);
                return;
            }

            if (!File.Exists(pendingPath))
            {
                throw new InvalidOperationException(
                    "IDENTITY_HANDOFF_NOT_PREPARED: The import credential handoff is unavailable.");
            }

            File.Move(pendingPath, completedPath);
            RestrictOwnerAccess(completedPath);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task AbortAsync(Guid importId, CancellationToken cancellationToken)
    {
        EnsureDevelopment();
        ValidateImportId(importId);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            DeleteIfPresent(PendingPath(importId));
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task DeleteExpiredAsync(CancellationToken cancellationToken = default)
    {
        EnsureDevelopment();
        await _gate.WaitAsync(cancellationToken);
        try
        {
            DeleteExpiredCore(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    private void DeleteExpiredCore(CancellationToken cancellationToken)
    {
        var cutoff = _timeProvider.GetUtcNow().Subtract(RetentionPeriod).UtcDateTime;
        DeleteExpiredFrom(_pendingDirectory, cutoff, cancellationToken);
        DeleteExpiredFrom(_completedDirectory, cutoff, cancellationToken);
    }

    private static async Task RequireMatchingPendingAsync(
        string pendingPath,
        CredentialHandoff expected,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            pendingPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 16 * 1024,
            FileOptions.Asynchronous);
        var existing = await JsonSerializer.DeserializeAsync<CredentialHandoff>(
            stream,
            JsonOptions,
            cancellationToken);
        if (existing is null
            || existing.ImportId != expected.ImportId
            || !existing.Accounts.SequenceEqual(expected.Accounts))
        {
            throw new InvalidOperationException(
                "IDENTITY_HANDOFF_PREPARE_MISMATCH: The import already has different pending credentials.");
        }
    }

    private void DeleteExpiredFrom(
        string directory,
        DateTime cutoffUtc,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var path in Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var containedPath = ResolveContainedPath(directory, Path.GetFileName(path));
            if (File.GetLastWriteTimeUtc(containedPath) <= cutoffUtc)
            {
                DeleteIfPresent(containedPath);
            }
        }
    }

    private string PendingPath(Guid importId) =>
        ResolveContainedPath(_pendingDirectory, $"import-{importId:N}.pending.json");

    private string CompletedPath(Guid importId) =>
        ResolveContainedPath(_completedDirectory, $"import-{importId:N}.json");

    private void EnsureDevelopment()
    {
        if (!_environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "Development import credential handoffs are unavailable outside Development.");
        }
    }

    private static void ValidateImportId(Guid importId)
    {
        if (importId == Guid.Empty)
        {
            throw new ArgumentException("An import ID is required.", nameof(importId));
        }
    }

    private static void ValidateCredentials(IReadOnlyCollection<DemoCredential> credentials)
    {
        ArgumentNullException.ThrowIfNull(credentials);
        if (credentials.Count is < 1 or > MaximumCredentialCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(credentials),
                $"Credential count must be between 1 and {MaximumCredentialCount}.");
        }

        foreach (var credential in credentials)
        {
            if (credential is null
                || string.IsNullOrWhiteSpace(credential.LoginIdentifier)
                || credential.LoginIdentifier.Length > MaximumLoginIdentifierLength
                || string.IsNullOrEmpty(credential.Secret)
                || credential.Secret.Length is < MinimumSecretLength or > MaximumSecretLength)
            {
                throw new ArgumentException(
                    "Every transient credential must satisfy the approved demo bounds.",
                    nameof(credentials));
            }
        }
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
                "Development credential artifacts must remain inside the repository-local boundary.");
        }

        return path;
    }

    private static void RestrictOwnerAccess(string path)
    {
        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }

    private static void DeleteIfPresent(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (DirectoryNotFoundException)
        {
            // Idempotent cleanup.
        }
    }

    private sealed record CredentialHandoff(
        string ContractVersion,
        Guid ImportId,
        DateTimeOffset GeneratedAtUtc,
        DateTimeOffset ExpiresAtUtc,
        IReadOnlyList<CredentialHandoffEntry> Accounts);

    private sealed record CredentialHandoffEntry(
        string LoginIdentifier,
        string Secret,
        DateTimeOffset GeneratedAtUtc);
}
