using StudentRegistration.Scheduling.Application.Ports;

namespace StudentRegistration.Scheduling.Application;

public sealed record ValidateOfferingCommand(
    Guid OfferingId,
    byte[] ExpectedOfferingVersion,
    IReadOnlyDictionary<Guid, byte[]> ExpectedGroupVersions,
    IReadOnlyDictionary<Guid, byte[]> ExpectedRoomVersions,
    IReadOnlyDictionary<Guid, byte[]> ExpectedStaffTermAvailabilityVersions);

public sealed record PublicationDependencies(
    Guid OfferingId,
    IReadOnlyList<Guid> GroupIds,
    IReadOnlyList<Guid> RoomIds,
    IReadOnlyList<Guid> StaffTermAvailabilityIds);

public sealed record PublicationLockRequest(
    Guid OfferingId,
    IReadOnlyList<Guid> OrderedResourceIds);

public sealed record OfferingPublicationSnapshot(
    Guid OfferingId,
    byte[] OfferingVersion,
    Guid GroupId,
    byte[] GroupVersion,
    int GroupCapacity,
    Guid RoomId,
    int RoomCapacity,
    bool RoomAvailable,
    bool RoomOverlap,
    byte[] CurrentRoomVersion,
    Guid StaffTermAvailabilityId,
    bool StaffAvailable,
    bool StaffOverlap,
    byte[] StaffVersion,
    bool SlotValid,
    bool BundleComplete);

public sealed record OfferingPublicationValidationResult(
    bool Valid,
    IReadOnlyList<string> ReasonCodes,
    string? ErrorCode = null);

public sealed class OfferingPublicationValidator
{
    public async Task<OfferingPublicationValidationResult> ValidateAsync(
        ValidateOfferingCommand command,
        IOfferingPublicationStore store,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(store);
        if (command.ExpectedOfferingVersion is null
            || command.ExpectedGroupVersions is null
            || command.ExpectedRoomVersions is null
            || command.ExpectedStaffTermAvailabilityVersions is null)
        {
            return new(false, [], "VALIDATION_ERROR");
        }

        var dependencies = await store.GetDependenciesAsync(
            command.OfferingId,
            cancellationToken);
        if (dependencies is null)
        {
            return new(false, [], "OFFERING_NOT_FOUND");
        }
        var orderedIds = new List<Guid> { dependencies.OfferingId };
        orderedIds.AddRange(dependencies.GroupIds.Order());
        orderedIds.AddRange(dependencies.RoomIds.Order());
        orderedIds.AddRange(dependencies.StaffTermAvailabilityIds.Order());

        var snapshot = await store.LockAndLoadAsync(
            new(command.OfferingId, orderedIds),
            cancellationToken);
        if (snapshot is null)
        {
            return new(false, [], "OFFERING_NOT_FOUND");
        }
        var reasons = new List<string>();

        AddWhen(
            reasons,
            HasStaleDependency(command, dependencies, snapshot),
            "STALE_DEPENDENCY");
        AddWhen(reasons, snapshot.RoomOverlap, "ROOM_CONFLICT");
        AddWhen(reasons, !snapshot.RoomAvailable, "ROOM_UNAVAILABLE");
        AddWhen(
            reasons,
            snapshot.RoomCapacity < snapshot.GroupCapacity,
            "ROOM_CAPACITY_TOO_SMALL");
        AddWhen(reasons, snapshot.StaffOverlap, "STAFF_CONFLICT");
        AddWhen(reasons, !snapshot.StaffAvailable, "STAFF_UNAVAILABLE");
        AddWhen(reasons, !snapshot.SlotValid, "INVALID_SLOT");
        AddWhen(
            reasons,
            !snapshot.BundleComplete,
            "MISSING_TUTORIAL_OR_LABORATORY");

        return new(reasons.Count == 0, reasons);
    }

    private static bool HasStaleDependency(
        ValidateOfferingCommand command,
        PublicationDependencies dependencies,
        OfferingPublicationSnapshot snapshot) =>
        dependencies.OfferingId != command.OfferingId
        || snapshot.OfferingId != command.OfferingId
        || dependencies.GroupIds.Any(
            id => !command.ExpectedGroupVersions.ContainsKey(id))
        || dependencies.RoomIds.Any(
            id => !command.ExpectedRoomVersions.ContainsKey(id))
        || dependencies.StaffTermAvailabilityIds.Any(
            id => !command.ExpectedStaffTermAvailabilityVersions.ContainsKey(id))
        || !command.ExpectedOfferingVersion.SequenceEqual(snapshot.OfferingVersion)
        || !VersionMatches(
            command.ExpectedGroupVersions,
            snapshot.GroupId,
            snapshot.GroupVersion)
        || !VersionMatches(
            command.ExpectedRoomVersions,
            snapshot.RoomId,
            snapshot.CurrentRoomVersion)
        || !VersionMatches(
            command.ExpectedStaffTermAvailabilityVersions,
            snapshot.StaffTermAvailabilityId,
            snapshot.StaffVersion);

    private static bool VersionMatches(
        IReadOnlyDictionary<Guid, byte[]> expectedVersions,
        Guid id,
        byte[] currentVersion) =>
        expectedVersions.TryGetValue(id, out var expected)
        && expected.SequenceEqual(currentVersion);

    private static void AddWhen(
        List<string> reasons,
        bool condition,
        string reason)
    {
        if (condition)
        {
            reasons.Add(reason);
        }
    }
}
