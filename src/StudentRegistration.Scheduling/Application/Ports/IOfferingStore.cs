namespace StudentRegistration.Scheduling.Application.Ports;

public interface IOfferingStore
{
    Task<OfferingSnapshot?> LoadAsync(
        Guid offeringId,
        CancellationToken cancellationToken);

    Task<OfferingGroupSnapshot?> LoadGroupAsync(
        Guid groupId,
        CancellationToken cancellationToken);

    Task<OfferingSnapshot> CreateAsync(
        CreateOfferingStoreCommand command,
        CancellationToken cancellationToken);

    Task<OfferingGroupSnapshot> UpdateGroupAsync(
        UpdateGroupStoreCommand command,
        CancellationToken cancellationToken);

    Task<AdminOfferingPage> ListAsync(
        AdminOfferingQuery query,
        CancellationToken cancellationToken);
}

public enum OfferingStoreFailure
{
    NotFound,
    Conflict,
}

public sealed class OfferingStoreException(
    OfferingStoreFailure failure,
    string code,
    string message) : Exception(message)
{
    public OfferingStoreFailure Failure { get; } = failure;

    public string Code { get; } = code;
}
