using System.Text.Json;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec013;

public sealed class OptimizationDiagnosticModelTests
{
    [Fact]
    public void Diagnostic_preserves_inclusion_minimal_members_intervals_and_actions()
    {
        var firstCourseId =
            Guid.Parse("10000000-0000-0000-0000-000000000001");
        var secondCourseId =
            Guid.Parse("10000000-0000-0000-0000-000000000002");
        var members = new List<OptimizationDiagnosticMember>
        {
            Member(
                secondCourseId,
                Guid.Parse("30000000-0000-0000-0000-000000000002"),
                DayOfWeek.Monday,
                new TimeOnly(11, 0),
                new TimeOnly(12, 0),
                "change-group"),
            Member(
                firstCourseId,
                Guid.Parse("30000000-0000-0000-0000-000000000001"),
                DayOfWeek.Monday,
                new TimeOnly(10, 0),
                new TimeOnly(11, 30),
                "remove-course")
        };
        var overlap = new OptimizationMeetingInterval(
            DayOfWeek.Monday,
            new TimeOnly(11, 0),
            new TimeOnly(11, 30));

        var diagnostic = new OptimizationDiagnostic(
            "diag-overlap-1",
            "MEETING_OVERLAP",
            members,
            overlap);
        members.Clear();

        Assert.Equal("diag-overlap-1", diagnostic.DiagnosticId);
        Assert.Equal("MEETING_OVERLAP", diagnostic.ReasonCode);
        Assert.Equal("inclusion-minimal", diagnostic.Minimality);
        Assert.Equal(2, diagnostic.Members.Count);
        Assert.Equal(firstCourseId, diagnostic.Members[0].CourseId);
        Assert.Equal("remove-course", diagnostic.Members[0].Action);
        Assert.Equal(secondCourseId, diagnostic.Members[1].CourseId);
        Assert.Equal("change-group", diagnostic.Members[1].Action);
        Assert.Equal(overlap, diagnostic.ConflictingInterval);
    }

    [Fact]
    public void Diagnostic_rejects_invalid_identity_reason_members_actions_or_intervals()
    {
        var courseId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var member = Member(
            courseId,
            groupId,
            DayOfWeek.Tuesday,
            new TimeOnly(9, 0),
            new TimeOnly(10, 0),
            "change-group");
        var overlap = new OptimizationMeetingInterval(
            DayOfWeek.Tuesday,
            new TimeOnly(9, 30),
            new TimeOnly(10, 0));

        Assert.Throws<ArgumentException>(() =>
            new OptimizationDiagnostic(" ", "GROUP_FULL", [member]));
        Assert.Throws<ArgumentException>(() =>
            new OptimizationDiagnostic("diag-1", "invalid reason", [member]));
        Assert.Throws<ArgumentException>(() =>
            new OptimizationDiagnostic("diag-1", "GROUP_FULL", []));
        Assert.Throws<ArgumentException>(() =>
            new OptimizationDiagnostic(
                "diag-1",
                "GROUP_FULL",
                [member, member]));
        Assert.Throws<ArgumentException>(() =>
            new OptimizationDiagnostic(
                "diag-1",
                "MEETING_OVERLAP",
                [member],
                overlap));
        Assert.Throws<ArgumentException>(() =>
            new OptimizationDiagnosticMember(
                courseId,
                null,
                null,
                "change-group"));
        Assert.Throws<ArgumentException>(() =>
            new OptimizationDiagnosticMember(
                courseId,
                groupId,
                member.Interval,
                "ignore"));
        Assert.Throws<ArgumentException>(() =>
            new OptimizationMeetingInterval(
                (DayOfWeek)99,
                new TimeOnly(9, 0),
                new TimeOnly(10, 0)));
        Assert.Throws<ArgumentException>(() =>
            new OptimizationMeetingInterval(
                DayOfWeek.Tuesday,
                new TimeOnly(10, 0),
                new TimeOnly(10, 0)));
    }

    [Fact]
    public void Json_contract_is_deterministic_and_round_trips_as_a_transient_value()
    {
        var diagnostic = new OptimizationDiagnostic(
            "diag-zero-group",
            "NO_VIABLE_GROUP",
            [
                new OptimizationDiagnosticMember(
                    Guid.Parse("10000000-0000-0000-0000-000000000001"),
                    null,
                    null,
                    "remove-course")
            ]);

        var json = JsonSerializer.Serialize(
            diagnostic,
            JsonSerializerOptions.Web);
        using var document = JsonDocument.Parse(json);

        Assert.Equal(
            [
                "conflictingInterval",
                "diagnosticId",
                "members",
                "minimality",
                "reasonCode"
            ],
            document.RootElement
                .EnumerateObject()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.Equal(
            "inclusion-minimal",
            document.RootElement.GetProperty("minimality").GetString());
        Assert.Equal(
            "remove-course",
            document.RootElement
                .GetProperty("members")[0]
                .GetProperty("action")
                .GetString());

        var roundTrip = JsonSerializer.Deserialize<OptimizationDiagnostic>(
            json,
            JsonSerializerOptions.Web);

        Assert.NotNull(roundTrip);
        Assert.Equal(diagnostic.DiagnosticId, roundTrip.DiagnosticId);
        Assert.Equal(diagnostic.ReasonCode, roundTrip.ReasonCode);
        Assert.Equal(diagnostic.Members, roundTrip.Members);
        Assert.Equal(diagnostic.Minimality, roundTrip.Minimality);
        Assert.Equal(
            "StudentRegistration.Registration.Domain",
            typeof(OptimizationDiagnostic).Namespace);
        Assert.DoesNotContain(
            typeof(OptimizationDiagnostic).GetCustomAttributes(inherit: false),
            attribute => attribute.GetType().Namespace?.Contains(
                "EntityFrameworkCore",
                StringComparison.Ordinal) is true);
    }

    private static OptimizationDiagnosticMember Member(
        Guid courseId,
        Guid groupId,
        DayOfWeek day,
        TimeOnly start,
        TimeOnly end,
        string action) =>
        new(
            courseId,
            groupId,
            new OptimizationMeetingInterval(day, start, end),
            action);
}
