using StudentRegistration.TestSupport;
using StudentRegistration.Contracts.Scheduling;

namespace StudentRegistration.AcceptanceTests.Specs.Spec016;

public sealed class AC_4Tests
{
    [Fact]
    public void Assigned_detail_exists_without_staff_admin_mutation_routes()
    {
        var queries = RepositoryFiles.Read(
            "src/StudentRegistration.StaffAdministration/Domain/StaffAssignment.cs");
        RepositoryFiles.ContainsAll(queries, "SubjectCode", "SubjectTitle", "Group", "RosterCount");
        var groupFields = typeof(GroupDto).GetProperties().Select(property => property.Name).ToArray();
        foreach (var field in new[] { "GroupCode", "Staff", "Meetings", "Capacity" })
        {
            Assert.Contains(field, groupFields);
        }

        if (RepositoryFiles.Exists(
                "src/StudentRegistration.StaffAdministration/Endpoints/Spec016Endpoints.cs"))
        {
            var endpoints = RepositoryFiles.Read(
                "src/StudentRegistration.StaffAdministration/Endpoints/Spec016Endpoints.cs");
            Assert.DoesNotContain("/api/admin", endpoints, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("capacity", endpoints, StringComparison.OrdinalIgnoreCase);
        }
    }
}
