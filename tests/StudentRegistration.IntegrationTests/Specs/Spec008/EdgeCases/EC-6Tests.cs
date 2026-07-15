using System.Reflection;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec008.EdgeCases;

public sealed class EC_6Tests
{
    [Fact]
    public void Multiple_registration_open_or_teaching_terms_fail_context_composition()
    {
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("specs/008-academic-term-student-profile/requirements.md"),
            "EC-6: Multiple RegistrationOpen or Teaching terms",
            "CONTEXT_UNAVAILABLE",
            "do not choose by browser input or unordered query");
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("specs/008-academic-term-student-profile/contracts/api.md"),
            "more than one is invalid",
            "configuration and returns",
            "503 CONTEXT_UNAVAILABLE",
            "A second Teaching term is rejected",
            "without a partial body");

        var resolver = Assembly.Load("StudentRegistration.Academics").GetType(
            "StudentRegistration.Academics.Application.AcademicContextResolver");
        Assert.True(
            resolver is not null,
            "EC-6 requires AcademicContextResolver ambiguity handling.");

        var source = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/Application/AcademicContextResolver.cs");
        RepositoryFiles.ContainsAll(
            source,
            "RegistrationOpen",
            "Teaching",
            "ContextUnavailable",
            "CONTEXT_UNAVAILABLE");
        Assert.DoesNotContain("browser", source, StringComparison.OrdinalIgnoreCase);
    }
}
