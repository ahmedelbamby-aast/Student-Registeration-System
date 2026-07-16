namespace StudentRegistration.Scheduling.Application.Ports;

public interface IOfferingStore
{
    Task<OfferingSnapshot?> LoadAsync(
        Guid offeringId,
        CancellationToken cancellationToken);

    Task<OfferingSnapshot> CreateAsync(
        CreateOfferingStoreCommand command,
        CancellationToken cancellationToken);

    Task<byte[]> UpdateGroupAsync(
        UpdateGroupStoreCommand command,
        CancellationToken cancellationToken);

    Task<AdminOfferingPage> ListAsync(
        AdminOfferingQuery query,
        CancellationToken cancellationToken);
}
