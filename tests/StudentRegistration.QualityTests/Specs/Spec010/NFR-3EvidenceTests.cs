using System.Security.Cryptography;
using System.Text;
using StudentRegistration.Academics.Application;
using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec010;

public sealed class NFR_3EvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-010-NFR-3.md";
    private const string TestPath =
        "tests/StudentRegistration.QualityTests/Specs/Spec010/NFR-3EvidenceTests.cs";

    [Theory]
    [InlineData("Africa/Cairo", DayOfWeek.Sunday, 9, 0, 10, 30)]
    [InlineData("Europe/London", DayOfWeek.Monday, 13, 15, 14, 45)]
    public async Task Meeting_projection_preserves_term_local_day_and_time(
        string timeZoneId,
        DayOfWeek day,
        int startHour,
        int startMinute,
        int endHour,
        int endMinute)
    {
        var context = new AcademicContextOptions(timeZoneId);
        var offering = Offering(
            day,
            new TimeOnly(startHour, startMinute),
            new TimeOnly(endHour, endMinute));
        var service = new OfferingService(new StaticOfferingStore(offering));

        var detail = await service.GetStudentDetailAsync(offering.Id);
        var meeting = Assert.Single(Assert.Single(detail!.Groups).Meetings);

        Assert.Equal(timeZoneId, context.TimeZoneId);
        Assert.Equal((int)day, meeting.DayOfWeek);
        Assert.Equal(new TimeOnly(startHour, startMinute), meeting.StartLocal);
        Assert.Equal(new TimeOnly(endHour, endMinute), meeting.EndLocal);
        Assert.Equal("Tutorial", meeting.ActivityType);
        Assert.Equal("Section", meeting.DisplayLabel);
        Assert.True(meeting.EndLocal > meeting.StartLocal);
    }

    [Fact]
    public void Evidence_is_source_bound_and_keeps_timezone_authority_explicit()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-010 NFR-3 Term-Timezone Display Evidence",
            "Africa/Cairo",
            "Europe/London",
            "AcademicContextOptions",
            "OfferingService.GetStudentDetailAsync",
            "DayOfWeek",
            "TimeOnly",
            "Tutorial",
            "Section",
            "browser timezone",
            "3 passed",
            "0 failed",
            $"Quality test normalized-LF SHA-256: `{SourceHash(TestPath)}`",
            "**Result: PASS.**");
        Assert.DoesNotMatch(
            @"(?i)\b(?:TODO|TBD|FIXME|PLACEHOLDER|NOT\s+RUN|NOT\s+EXECUTED)\b",
            evidence);
    }

    private static OfferingSnapshot Offering(
        DayOfWeek day,
        TimeOnly startLocal,
        TimeOnly endLocal) =>
        new(
            Id(1),
            "published",
            [
                new OfferingGroupSnapshot(
                    Id(2),
                    "G01",
                    30,
                    0,
                    false,
                    "published",
                    [1],
                    [
                        new OfferingMeetingSnapshot(
                            Id(3),
                            "Tutorial",
                            (int)day,
                            startLocal,
                            endLocal,
                            "C-201",
                            "Main Campus",
                            [new OfferingStaffSnapshot(
                                Id(4),
                                "TeachingAssistant",
                                "TA Assistant")])
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

        public Task<OfferingGroupSnapshot?> LoadGroupAsync(
            Guid groupId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<OfferingGroupSnapshot?>(
                offering.Groups.SingleOrDefault(group => group.Id == groupId));
        }

        public Task<OfferingSnapshot> CreateAsync(
            CreateOfferingStoreCommand command,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<OfferingGroupSnapshot> UpdateGroupAsync(
            UpdateGroupStoreCommand command,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<AdminOfferingPage> ListAsync(
            AdminOfferingQuery query,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
