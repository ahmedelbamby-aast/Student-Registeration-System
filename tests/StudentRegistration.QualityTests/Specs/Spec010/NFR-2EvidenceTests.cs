using System.Security.Cryptography;
using System.Text;
using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec010;

public sealed class NFR_2EvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-010-NFR-2.md";
    private const string TestPath =
        "tests/StudentRegistration.QualityTests/Specs/Spec010/NFR-2EvidenceTests.cs";

    private static readonly string[] ExpectedResourceCodes =
    [
        "STALE_DEPENDENCY",
        "ROOM_CONFLICT",
        "ROOM_UNAVAILABLE",
        "ROOM_CAPACITY_TOO_SMALL",
        "STAFF_CONFLICT",
        "STAFF_UNAVAILABLE",
        "INVALID_SLOT",
        "MISSING_TUTORIAL_OR_LABORATORY"
    ];

    [Fact]
    public async Task Publication_validator_returns_the_same_ordered_actionable_codes()
    {
        var snapshot = InvalidPublicationSnapshot();
        var store = new StaticPublicationStore(snapshot);
        var command = new ValidateOfferingCommand(
            snapshot.OfferingId,
            ExpectedOfferingVersion: [9],
            new Dictionary<Guid, byte[]> { [snapshot.GroupId] = snapshot.GroupVersion },
            new Dictionary<Guid, byte[]> { [snapshot.RoomId] = snapshot.CurrentRoomVersion },
            new Dictionary<Guid, byte[]>
            {
                [snapshot.StaffTermAvailabilityId] = snapshot.StaffVersion
            });
        var validator = new OfferingPublicationValidator();

        for (var iteration = 0; iteration < 100; iteration++)
        {
            var result = await validator.ValidateAsync(command, store);
            Assert.False(result.Valid);
            Assert.Null(result.ErrorCode);
            Assert.Equal(ExpectedResourceCodes, result.ReasonCodes);
        }

        Assert.Equal(100, store.DependencyReadCount);
        Assert.Equal(100, store.LockedReadCount);
    }

    [Fact]
    public async Task Bundle_validator_uses_only_the_approved_staffing_codes()
    {
        var service = new OfferingService(
            new StaticOfferingStore(IncompleteBundleOffering()));
        var result = await service.ValidateForPublicationAsync(Id(1));

        Assert.False(result.Valid);
        Assert.Equal(
            [
                "MISSING_LECTURER",
                "MISSING_TUTORIAL_OR_LABORATORY"
            ],
            result.ReasonCodes);
        Assert.All(
            result.ReasonCodes.Concat(ExpectedResourceCodes),
            code => Assert.Contains(code, ApprovedPublicationCodes));
    }

    [Fact]
    public void Evidence_is_source_bound_and_maps_each_exercised_code_to_recovery()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-010 NFR-2 Stable Publication-Code Evidence",
            "100 repeated validations",
            "ordinal order",
            "STALE_DEPENDENCY",
            "ROOM_CONFLICT",
            "ROOM_CAPACITY_TOO_SMALL",
            "STAFF_UNAVAILABLE",
            "INVALID_SLOT",
            "MISSING_LECTURER",
            "MISSING_TUTORIAL_OR_LABORATORY",
            "Recovery action",
            "3 passed",
            "0 failed",
            $"Quality test normalized-LF SHA-256: `{SourceHash(TestPath)}`",
            "**Result: PASS.**");
        Assert.DoesNotMatch(
            @"(?i)\b(?:TODO|TBD|FIXME|PLACEHOLDER|NOT\s+RUN|NOT\s+EXECUTED)\b",
            evidence);
    }

    private static readonly HashSet<string> ApprovedPublicationCodes =
        new(
            [
                "MISSING_LECTURE",
                "MISSING_LECTURER",
                "MISSING_TUTORIAL_OR_LABORATORY",
                "MISSING_TEACHING_ASSISTANT",
                "INVALID_ACTIVITY_ROLE",
                "INVALID_SLOT",
                "OVERNIGHT_SLOT_NOT_SUPPORTED",
                "ROOM_CONFLICT",
                "ROOM_UNAVAILABLE",
                "ROOM_CAPACITY_TOO_SMALL",
                "STAFF_CONFLICT",
                "STAFF_UNAVAILABLE",
                "DUPLICATE_OFFERING",
                "DUPLICATE_GROUP_CODE",
                "STALE_DEPENDENCY"
            ],
            StringComparer.Ordinal);

    private static OfferingPublicationSnapshot InvalidPublicationSnapshot() =>
        new(
            Id(1),
            OfferingVersion: [1],
            Id(2),
            GroupVersion: [2],
            GroupCapacity: 30,
            Id(3),
            RoomCapacity: 20,
            RoomAvailable: false,
            RoomOverlap: true,
            CurrentRoomVersion: [3],
            Id(4),
            StaffAvailable: false,
            StaffOverlap: true,
            StaffVersion: [4],
            SlotValid: false,
            BundleComplete: false);

    private static OfferingSnapshot IncompleteBundleOffering() =>
        new(
            Id(1),
            "draft",
            [
                new OfferingGroupSnapshot(
                    Id(2),
                    "G01",
                    30,
                    0,
                    false,
                    "draft",
                    [1],
                    [
                        new OfferingMeetingSnapshot(
                            Id(3),
                            "Lecture",
                            (int)DayOfWeek.Sunday,
                            new TimeOnly(9, 0),
                            new TimeOnly(10, 0),
                            "C-101",
                            "Main Campus",
                            [])
                    ])
            ]);

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:000000000000}");

    private static string SourceHash(string relativePath)
    {
        var source = RepositoryFiles.Read(relativePath)
            .Replace("\r\n", "\n", StringComparison.Ordinal);
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(source)));
    }

    private sealed class StaticPublicationStore(OfferingPublicationSnapshot snapshot)
        : IOfferingPublicationStore
    {
        public int DependencyReadCount { get; private set; }

        public int LockedReadCount { get; private set; }

        public Task<PublicationDependencies> GetDependenciesAsync(
            Guid offeringId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DependencyReadCount++;
            return Task.FromResult(new PublicationDependencies(
                snapshot.OfferingId,
                [snapshot.GroupId],
                [snapshot.RoomId],
                [snapshot.StaffTermAvailabilityId]));
        }

        public Task<OfferingPublicationSnapshot> LockAndLoadAsync(
            PublicationLockRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LockedReadCount++;
            return Task.FromResult(snapshot);
        }
    }

    private sealed class StaticOfferingStore(OfferingSnapshot offering)
        : IOfferingStore
    {
        public Task<OfferingSnapshot?> LoadAsync(
            Guid offeringId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<OfferingSnapshot?>(
                offeringId == offering.Id ? offering : null);
        }

        public Task<OfferingSnapshot> CreateAsync(
            CreateOfferingStoreCommand command,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<byte[]> UpdateGroupAsync(
            UpdateGroupStoreCommand command,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<AdminOfferingPage> ListAsync(
            AdminOfferingQuery query,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
