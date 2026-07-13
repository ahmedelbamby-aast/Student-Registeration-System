using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec001;

public sealed class EntryPointBoundaryTests
{
    [Fact]
    public void Student_and_staff_entry_points_are_distinct_and_have_no_role_claim_control()
    {
        var boundary = RepositoryFiles.Read(
            "specs/001-product-charter-rbac/contracts/entry-point-boundary.md");

        RepositoryFiles.ContainsAll(
            boundary,
            "`/student/login`",
            "`/student/activate`",
            "`/staff/login`",
            "one shared staff login",
            "no public staff registration",
            "No MFA or 2FA");
    }
}
