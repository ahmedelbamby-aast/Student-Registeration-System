namespace StudentRegistration.Scheduling.Application.Ports;

public interface IOfferingPublicationStore
{
    Task<PublicationDependencies> GetDependenciesAsync(
        Guid offeringId,
        CancellationToken cancellationToken);

    Task<OfferingPublicationSnapshot> LockAndLoadAsync(
        PublicationLockRequest request,
        CancellationToken cancellationToken);
}
