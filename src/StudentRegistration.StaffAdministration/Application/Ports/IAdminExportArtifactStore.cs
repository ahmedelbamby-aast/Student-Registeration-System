namespace StudentRegistration.StaffAdministration.Application.Ports;

public interface IAdminExportArtifactStore
{
    Task<Guid> WriteAsync(
        ReadOnlyMemory<byte> content,
        CancellationToken cancellationToken = default);

    Task<ReadOnlyMemory<byte>?> ReadAsync(
        Guid artifactId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid artifactId,
        CancellationToken cancellationToken = default);
}
