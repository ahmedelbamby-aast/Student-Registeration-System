using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec017;

public sealed class StudentAdministrationPageContributorTests
{
    [Fact]
    public void Adm_04_contribution_is_scoped_reasoned_versioned_and_audited()
    {
        var (contract, page) = ContributorContractAssertions.Load(
            "ADM-04",
            "SPEC-008",
            "StudentAdministrationPage.razor",
            "/admin/students",
            "spec017-adm04/1.0");

        RepositoryFiles.ContainsAll(
            contract,
            "GET /api/admin/students",
            "GET /api/admin/students/{studentId}/academic-context",
            "PATCH /api/admin/students/{studentId}/academic-profile",
            "AcademicProfiles.Manage",
            "explicit StudentId and TermId scope",
            "expected Student and StudentTermState rowversions",
            "10-to-500-character",
            "VALIDATION_ERROR",
            "STALE_VERSION",
            "INVALID_SUPERSESSION",
            "PROFILE_NOT_READY",
            "audit failure rolls back");
        RepositoryFiles.ContainsAll(
            page,
            "AcademicApi.SearchAdminStudentsAsync",
            "AcademicApi.GetAdminStudentAcademicContextAsync",
            "AcademicApi.CorrectAdminStudentAcademicProfileAsync",
            "ExpectedStudentRowVersion",
            "ExpectedStudentTermStateRowVersion",
            "Reason must not exceed 500 characters");
    }

    [Fact]
    public void Adm_04_adds_no_unscoped_or_registration_correction()
    {
        var contract = ContributorContractAssertions.Contract("ADM-04");
        RepositoryFiles.ContainsAll(
            contract,
            "No unrestricted student search",
            "registration/enrollment correction",
            "generic confirmation service",
            "second academic writer",
            "protected data on a denied route");
    }
}
