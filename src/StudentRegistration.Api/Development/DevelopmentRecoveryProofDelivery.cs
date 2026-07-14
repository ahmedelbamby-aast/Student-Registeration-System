using System.Text.Json;
using Microsoft.Extensions.Hosting;
using StudentRegistration.IdentityAccess.Application.Ports;

namespace StudentRegistration.Api.Development;

/// <summary>
/// Development-only recovery proof handoff. It exists solely to demonstrate
/// the provider boundary without claiming an institutional email/SMS service.
/// </summary>
public sealed class DevelopmentRecoveryProofDelivery : IAccountRecoveryProofDelivery
{
    private const int MaximumSubjectReferenceLength = 128;
    private const int MaximumProofLength = 1_024;
    private const string ArtifactPrefix = "recovery-";
    private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(7);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IHostEnvironment _environment;
    private readonly TimeProvider _timeProvider;
    private readonly string _storageDirectory;

    public DevelopmentRecoveryProofDelivery(
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
            "recovery");
    }

    public async Task<RecoveryProofDeliveryResult> DeliverAsync(
        RecoveryProofDeliveryRequest request,
        CancellationToken cancellationToken)
    {
        EnsureDevelopment();
        ArgumentNullException.ThrowIfNull(request);
        Validate(request);

        await DeleteExpiredAsync(cancellationToken);

        var deliveredAtUtc = _timeProvider.GetUtcNow();
        var proofExpiresAtUtc = new DateTimeOffset(
            DateTime.SpecifyKind(request.ExpiresAtUtc, DateTimeKind.Utc));
        if (proofExpiresAtUtc <= deliveredAtUtc)
        {
            return new RecoveryProofDeliveryResult(false, "development-expired");
        }

        var retentionLimitUtc = deliveredAtUtc.Add(RetentionPeriod);
        var artifactExpiresAtUtc = proofExpiresAtUtc <= retentionLimitUtc
            ? proofExpiresAtUtc
            : retentionLimitUtc;
        var deliveryReference = Guid.NewGuid().ToString("N");
        var artifactName =
            $"{ArtifactPrefix}{artifactExpiresAtUtc.UtcTicks:D19}-{deliveryReference}.json";

        Directory.CreateDirectory(_storageDirectory);
        var artifactPath = ResolveContainedPath(_storageDirectory, artifactName);
        var payload = new RecoveryProofArtifact(
            "demo-identity-provisioning/1.0",
            request.ApplicationUserId,
            request.SubjectReference,
            request.Proof,
            deliveredAtUtc,
            proofExpiresAtUtc,
            artifactExpiresAtUtc);

        try
        {
            await using var stream = new FileStream(
                artifactPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4 * 1024,
                FileOptions.Asynchronous | FileOptions.WriteThrough);

            RestrictOwnerAccess(artifactPath);
            await JsonSerializer.SerializeAsync(
                stream,
                payload,
                JsonOptions,
                cancellationToken);
            await stream.FlushAsync(cancellationToken);

            // Only an opaque reference crosses back into IdentityAccess; the
            // local path and proof never enter the HTTP response or logs.
            return new RecoveryProofDeliveryResult(
                true,
                $"development:{deliveryReference}");
        }
        catch
        {
            DeletePartialArtifact(artifactPath);
            throw;
        }
    }

    /// <summary>
    /// Deletes only artifacts created by this adapter, without recursion.
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
                "The local recovery proof adapter is unavailable outside Development.");
        }
    }

    private static void Validate(RecoveryProofDeliveryRequest request)
    {
        if (request.ApplicationUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "A recovery delivery requires an application user reference.",
                nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.SubjectReference)
            || request.SubjectReference.Length > MaximumSubjectReferenceLength)
        {
            throw new ArgumentException(
                "The recovery subject reference is invalid or unbounded.",
                nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Proof)
            || request.Proof.Length > MaximumProofLength)
        {
            throw new ArgumentException(
                "The recovery proof is invalid or unbounded.",
                nameof(request));
        }

        if (request.ExpiresAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "The recovery proof expiry must be expressed in UTC.",
                nameof(request));
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

    private sealed record RecoveryProofArtifact(
        string ContractVersion,
        Guid ApplicationUserId,
        string SubjectReference,
        string Proof,
        DateTimeOffset DeliveredAtUtc,
        DateTimeOffset ProofExpiresAtUtc,
        DateTimeOffset ArtifactExpiresAtUtc);
}
