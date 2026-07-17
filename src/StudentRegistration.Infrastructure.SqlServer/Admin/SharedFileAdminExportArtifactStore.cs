using StudentRegistration.StaffAdministration.Application;
using StudentRegistration.StaffAdministration.Application.Ports;

namespace StudentRegistration.Infrastructure.SqlServer.Admin;

/// <summary>
/// Demo adapter for two local replicas configured with the same durable root.
/// Artifact identifiers, never caller-provided paths, address files.
/// </summary>
public sealed class SharedFileAdminExportArtifactStore : IAdminExportArtifactStore
{
    private readonly string _rootPath;

    public SharedFileAdminExportArtifactStore(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentException("A shared export root is required.", nameof(rootPath));
        }

        _rootPath = Path.GetFullPath(rootPath);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<Guid> WriteAsync(
        ReadOnlyMemory<byte> content,
        CancellationToken cancellationToken = default)
    {
        if (content.IsEmpty || content.Length > AuditExportService.MaximumArtifactBytes)
        {
            throw new ArgumentOutOfRangeException(nameof(content));
        }

        var artifactId = Guid.NewGuid();
        var finalPath = ArtifactPath(artifactId);
        var temporaryPath = Path.Combine(_rootPath, $".{artifactId:N}.{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var stream = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 64 * 1024,
                FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await stream.WriteAsync(content, cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            File.Move(temporaryPath, finalPath, overwrite: false);
            return artifactId;
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    public async Task<ReadOnlyMemory<byte>?> ReadAsync(
        Guid artifactId,
        CancellationToken cancellationToken = default)
    {
        EnsureIdentifier(artifactId);
        var path = ArtifactPath(artifactId);
        if (!File.Exists(path))
        {
            return null;
        }

        var info = new FileInfo(path);
        if (info.Length is <= 0 or > AuditExportService.MaximumArtifactBytes)
        {
            throw new IOException("The export artifact violates its configured bound.");
        }

        return await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
    }

    public Task DeleteAsync(
        Guid artifactId,
        CancellationToken cancellationToken = default)
    {
        EnsureIdentifier(artifactId);
        cancellationToken.ThrowIfCancellationRequested();
        var path = ArtifactPath(artifactId);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private string ArtifactPath(Guid artifactId) =>
        Path.Combine(_rootPath, $"{artifactId:N}.export");

    private static void EnsureIdentifier(Guid artifactId)
    {
        if (artifactId == Guid.Empty)
        {
            throw new ArgumentException("A non-empty artifact identifier is required.", nameof(artifactId));
        }
    }
}
