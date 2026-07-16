namespace StudentRegistration.Registration.Application.Ports;

public interface IRegistrationPlanOwnerReader
{
    Task<Guid?> ResolveStudentIdAsync(
        Guid applicationUserId,
        Guid termId,
        CancellationToken cancellationToken = default);
}
