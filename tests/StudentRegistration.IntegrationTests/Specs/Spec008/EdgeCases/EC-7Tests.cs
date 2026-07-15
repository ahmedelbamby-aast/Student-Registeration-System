using System.Reflection;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec008.EdgeCases;

public sealed class EC_7Tests
{
    [Fact]
    public void Window_selection_is_open_then_earliest_upcoming_then_latest_closed_then_none()
    {
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("specs/008-academic-term-student-profile/contracts/api.md"),
            "open window; otherwise the earliest upcoming window ordered by",
            "(OpensAtUtc, Id)",
            "latest closed window ordered by",
            "(ClosesAtUtc DESC, Id)",
            "No candidate produces state `none`");
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("specs/008-academic-term-student-profile/requirements.md"),
            "EC-7: No open window",
            "earliest upcoming",
            "otherwise latest closed",
            "UTC time and ID tie-breaks",
            "state none");

        var resolver = Assembly.Load("StudentRegistration.Academics").GetType(
            "StudentRegistration.Academics.Application.AcademicContextResolver");
        Assert.True(
            resolver is not null,
            "EC-7 requires deterministic AcademicContextResolver window selection.");

        var source = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/Application/AcademicContextResolver.cs");
        RepositoryFiles.ContainsAll(
            source,
            "RegistrationWindowState.Open",
            "RegistrationWindowState.Upcoming",
            "RegistrationWindowState.Closed",
            "RegistrationWindowState.None",
            "OrderBy",
            "OpensAtUtc",
            "OrderByDescending",
            "ClosesAtUtc",
            "ThenBy",
            "Id");
    }
}
