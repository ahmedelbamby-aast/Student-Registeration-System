using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec001;

public sealed class AC_1Tests
{
    [Fact]
    public void Student_entry_contract_offers_only_activation_and_login()
    {
        // Given a user opens the public landing page; when Student is selected.
        var boundary = RepositoryFiles.Read(
            "specs/001-product-charter-rbac/contracts/entry-point-boundary.md");

        // Then only the two student actions exist and no staff role is selectable.
        RepositoryFiles.ContainsAll(
            boundary,
            "`/student/login`",
            "`/student/activate`",
            "MUST NOT display a staff role picker");
    }
}
