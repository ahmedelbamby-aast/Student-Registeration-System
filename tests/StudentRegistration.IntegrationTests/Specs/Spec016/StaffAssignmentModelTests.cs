using System.Reflection;
using StudentRegistration.Contracts.Scheduling;
using StudentRegistration.Contracts.Staff;
using StudentRegistration.StaffAdministration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec016;

public sealed class StaffAssignmentModelTests
{
    [Fact]
    public void Workspace_assignment_projects_the_canonical_scheduling_group_without_persistence_ownership()
    {
        var group = Group();
        var assignment = new StaffAssignment(
            "CS401",
            "Distributed Systems",
            group,
            StaffWorkspaceContract.LecturerRole,
            17);

        var dto = assignment.ToDto();

        Assert.Equal("CS401", dto.SubjectCode);
        Assert.Equal("Distributed Systems", dto.SubjectTitle);
        Assert.Same(group, dto.Group);
        Assert.Equal("Lecturer", dto.StaffRole);
        Assert.Equal(17, dto.RosterCount);
        Assert.DoesNotContain(
            typeof(StaffAssignment).GetProperties(),
            property => property.Name is "Version" or "RowVersion");
        Assert.DoesNotContain(
            typeof(StaffAssignment).GetCustomAttributes(inherit: true),
            attribute => attribute.GetType().Namespace?.StartsWith(
                "System.ComponentModel.DataAnnotations.Schema",
                StringComparison.Ordinal) == true);
        Assert.Null(typeof(StaffAssignment).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            Type.EmptyTypes,
            modifiers: null));
    }

    [Fact]
    public void Workspace_assignment_rejects_invalid_or_mismatched_projection_values()
    {
        Assert.Throws<ArgumentException>(() => new StaffAssignment(
            " ", "Subject", Group(), "Lecturer", 1));
        Assert.Throws<ArgumentException>(() => new StaffAssignment(
            "CS401", "Subject", Group(), "Admin", 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new StaffAssignment(
            "CS401", "Subject", Group(), "Lecturer", -1));
    }

    private static GroupDto Group() => new(
        Guid.NewGuid(),
        Guid.NewGuid(),
        "L01",
        30,
        17,
        false,
        "published",
        true,
        [],
        [],
        [],
        "group-v1");
}
