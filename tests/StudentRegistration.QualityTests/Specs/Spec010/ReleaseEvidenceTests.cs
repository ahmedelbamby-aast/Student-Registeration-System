using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec010;

public sealed class ReleaseEvidenceTests
{
    [Fact]
    public void Scope_review_verifies_all_four_exclusions_against_the_delivered_boundary()
    {
        var evidence = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-010-scope-review.md");
        var schedulingFiles = Directory
            .EnumerateFiles(
                RepositoryFiles.PathTo("src/StudentRegistration.Scheduling"),
                "*.cs",
                SearchOption.AllDirectories)
            .Select(File.ReadAllText)
            .ToArray();
        var combinedSource = string.Join("\n", schedulingFiles);

        RepositoryFiles.ContainsAll(
            evidence,
            "OS-1",
            "OS-2",
            "OS-3",
            "OS-4",
            "No institution-wide timetable generation",
            "No automatic reassignment",
            "No waitlist or seat reservation",
            "No force-over-capacity",
            "SPEC-017",
            "**Result: PASS.**");
        Assert.DoesNotContain(
            "Waitlist",
            combinedSource,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "SeatReservation",
            combinedSource,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "ForceCapacity",
            combinedSource,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "InstitutionTimetableGenerator",
            combinedSource,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Traceability_contains_every_approved_inventory_row()
    {
        var trace = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-010-traceability.md");

        AssertInventory(trace, "FR", 10);
        AssertInventory(trace, "NFR", 4);
        AssertInventory(trace, "AC", 9);
        AssertInventory(trace, "EC", 5);
        AssertInventory(trace, "SC", 3);

        foreach (var route in new[] { "STU-03", "ADM-06", "ADM-07" })
        {
            Assert.Contains($"| {route} |", trace, StringComparison.Ordinal);
        }

        foreach (var entity in new[]
                 {
                     "CourseOffering",
                     "SectionGroup",
                     "MeetingSlot",
                     "Room",
                     "GroupStaffAssignment",
                     "StaffTermAvailability",
                     "StaffAvailability",
                     "ScheduleImpactAlert"
                 })
        {
            Assert.Contains($"`{entity}`", trace, StringComparison.Ordinal);
        }

        for (var endpoint = 1; endpoint <= 14; endpoint++)
        {
            Assert.Contains($"| {endpoint:00} |", trace, StringComparison.Ordinal);
        }

        RepositoryFiles.ContainsAll(
            trace,
            "SPEC-011 owns the STU-03 Razor page",
            "SPEC-017 contributor work is downstream",
            "not claimed as completed by SPEC-010",
            "SchedulingModelConfiguration.cs",
            "S2CatalogueScheduling",
            "**Result: PASS.**");
    }

    private static void AssertInventory(
        string trace,
        string prefix,
        int count)
    {
        for (var number = 1; number <= count; number++)
        {
            Assert.Contains(
                $"| {prefix}-{number} |",
                trace,
                StringComparison.Ordinal);
        }
    }
}
