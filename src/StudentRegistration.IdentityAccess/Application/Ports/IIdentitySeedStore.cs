namespace StudentRegistration.IdentityAccess.Application.Ports;

public interface IIdentitySeedStore
{
    Task<IReadOnlySet<Guid>> ReconcileAsync(
        IReadOnlyList<DemoSeedIdentity> identities,
        string clientRequestId,
        CancellationToken cancellationToken);
}

public sealed record DemoSeedIdentity(
    Guid UserId,
    string UserName,
    string NormalizedUserName,
    string? UniversityId,
    string PasswordHash,
    string SecurityStamp,
    string DisplayName,
    string? StaffNumber,
    IReadOnlyList<string> Roles,
    DateTime ProvisionedAtUtc);
