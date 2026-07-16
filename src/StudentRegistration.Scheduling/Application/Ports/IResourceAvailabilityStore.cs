namespace StudentRegistration.Scheduling.Application.Ports;

public interface IResourceAvailabilityStore
{
    Task<StaffAvailabilitySnapshot?> LoadStaffAvailabilityByIdAsync(
        Guid availabilityId,
        CancellationToken cancellationToken);

    Task<StaffAvailabilitySnapshot?> LoadStaffAvailabilityAsync(
        Guid staffId,
        Guid termId,
        CancellationToken cancellationToken);

    Task<RoomResourceSnapshot?> LoadRoomAsync(
        Guid roomId,
        CancellationToken cancellationToken);

    Task CommitAvailabilityAsync(
        ReplaceStaffAvailabilityWrite write,
        CancellationToken cancellationToken);

    Task CommitRoomAsync(
        UpdateRoomResourceWrite write,
        CancellationToken cancellationToken);
}
