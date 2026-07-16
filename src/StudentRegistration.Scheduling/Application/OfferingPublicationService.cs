using StudentRegistration.Scheduling.Application.Ports;

namespace StudentRegistration.Scheduling.Application;

public enum OfferingPublicationOutcome
{
    Published,
    ValidationFailed,
    StorageFailure,
}

public sealed record PublishOfferingCommand(
    Guid OfferingId,
    byte[] ExpectedOfferingVersion,
    IReadOnlyDictionary<Guid, byte[]> ExpectedGroupVersions,
    IReadOnlyDictionary<Guid, byte[]> ExpectedRoomVersions,
    IReadOnlyDictionary<Guid, byte[]> ExpectedStaffTermAvailabilityVersions,
    string PreviewToken,
    string ClientRequestId,
    string Reason);

public sealed record OfferingPublicationAudit(
    Guid OfferingId,
    string ClientRequestId,
    string Reason);

public sealed class OfferingPublicationState(
    string offeringState,
    IReadOnlyList<string> groupStates,
    IReadOnlyList<OfferingPublicationAudit> audits)
{
    public string OfferingState { get; set; } = offeringState;

    public IList<string> GroupStates { get; } = groupStates.ToList();

    public IList<OfferingPublicationAudit> Audits { get; } = audits.ToList();
}

public sealed record OfferingPublicationResult(
    OfferingPublicationOutcome Outcome,
    string? ErrorCode = null);

public interface IOfferingPublicationTransaction
{
    Task<OfferingPublicationResult> ExecuteAsync(
        PublishOfferingCommand command,
        Func<
            IOfferingPublicationStore,
            OfferingPublicationState,
            CancellationToken,
            Task<OfferingPublicationResult>> operation,
        CancellationToken cancellationToken);
}

public sealed class SchedulingDeadlockException : Exception;

public sealed class OfferingPublicationService(
    OfferingPublicationValidator validator,
    IOfferingPublicationTransaction transaction)
{
    public async Task<OfferingPublicationResult> PublishAsync(
        PublishOfferingCommand command,
        CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return await transaction.ExecuteAsync(
                    command,
                    async (scopedStore, state, scopedCancellationToken) =>
                    {
                        var validation = await validator.ValidateAsync(
                            new(
                                command.OfferingId,
                                command.ExpectedOfferingVersion,
                                command.ExpectedGroupVersions,
                                command.ExpectedRoomVersions,
                                command.ExpectedStaffTermAvailabilityVersions),
                            scopedStore,
                            scopedCancellationToken);
                        if (!validation.Valid)
                        {
                            return new(
                                OfferingPublicationOutcome.ValidationFailed,
                                validation.ReasonCodes.FirstOrDefault()
                                    ?? validation.ErrorCode);
                        }

                        state.OfferingState = "published";
                        for (var index = 0; index < state.GroupStates.Count; index++)
                        {
                            state.GroupStates[index] = "published";
                        }
                        state.Audits.Add(
                            new(
                                command.OfferingId,
                                command.ClientRequestId,
                                command.Reason));
                        return new(OfferingPublicationOutcome.Published);
                    },
                    cancellationToken);
            }
            catch (SchedulingDeadlockException) when (attempt == 0)
            {
            }
        }
    }
}
