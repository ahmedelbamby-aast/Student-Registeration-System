namespace StudentRegistration.Scheduling.Application.Ports;

public interface ISectionGroupCapacityStore
{
    Task<GroupCapacitySnapshot?> ReadAsync(
        Guid groupId,
        CancellationToken cancellationToken);

    Task<GroupCapacityOutcome> ExecuteAsync(
        Guid groupId,
        byte[] expectedVersion,
        Func<GroupCapacityState, GroupCapacityOutcome> operation,
        CancellationToken cancellationToken);
}
