namespace StudentRegistration.IdentityAccess.Application.Ports;

/// <summary>
/// Environment adapter for reveal-once credentials created by an Admin import.
/// Prepare keeps a batch non-visible, Complete publishes it after the SQL commit,
/// and Abort removes it when SQL cannot commit. Operations are idempotent by import ID.
/// Development and Testing provide bounded adapters; Production has no adapter and
/// therefore fails closed until institutional credential delivery is approved.
/// </summary>
public interface IProvisionedCredentialHandoff
{
    Task PrepareAsync(
        Guid importId,
        IReadOnlyCollection<DemoCredential> credentials,
        CancellationToken cancellationToken);

    Task CompleteAsync(Guid importId, CancellationToken cancellationToken);

    Task AbortAsync(Guid importId, CancellationToken cancellationToken);
}
