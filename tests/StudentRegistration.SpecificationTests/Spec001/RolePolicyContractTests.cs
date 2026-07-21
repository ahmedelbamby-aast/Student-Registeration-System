using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Spec001;

public sealed class RolePolicyContractTests
{
    [Fact]
    public void Role_boundary_separates_student_and_staff_entry_and_denies_client_escalation()
    {
        var boundary = RepositoryFiles.Read(
            "specs/001-product-charter-rbac/contracts/role-boundary.md");

        RepositoryFiles.ContainsAll(
            boundary,
            "Student entry",
            "Shared staff entry",
            "server-derived",
            "Direct-route denial",
            "Invalid multi-role outcome",
            "No-role outcome");
    }
}
