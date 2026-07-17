namespace StudentRegistration.StaffAdministration.Application.Ports;

public interface IStaffIdentityResolver
{
    Task<Guid?> ResolveActiveStaffIdAsync(
        Guid actorApplicationUserId,
        CancellationToken cancellationToken = default);
}
