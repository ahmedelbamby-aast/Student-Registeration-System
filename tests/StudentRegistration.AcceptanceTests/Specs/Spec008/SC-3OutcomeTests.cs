using System.Reflection;

namespace StudentRegistration.AcceptanceTests.Specs.Spec008;

public sealed class SC_3OutcomeTests
{
    [Fact]
    public void Published_windows_cannot_create_overlapping_active_contexts_in_one_term()
    {
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("specs/008-academic-term-student-profile/spec.md"),
            "**SC-3**: No two Published registration windows overlap anywhere in the same",
            "no overlapping active registration context can apply to a student");
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("specs/008-academic-term-student-profile/contracts/api.md"),
            "Publication forbids any overlap between Published windows in the same term",
            "regardless of scope",
            "409 STALE_VERSION/WINDOW_OVERLAP");

        var academics = Assembly.Load("StudentRegistration.Academics");
        var service = academics.GetType(
            "StudentRegistration.Academics.Application.RegistrationWindowService");
        Assert.True(
            service is not null,
            "SC-3 requires the owner RegistrationWindowService.");
        Assert.Contains(
            service!.GetMethods(BindingFlags.Instance | BindingFlags.Public),
            method => method.Name.Contains("Publish", StringComparison.Ordinal));

        var source = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/Application/RegistrationWindowService.cs");
        RepositoryFiles.ContainsAll(
            source,
            "AcademicTerm",
            "RegistrationWindow",
            "OrderBy",
            "Id",
            "OpensAtUtc",
            "ClosesAtUtc",
            "Published",
            "WindowOverlap",
            "ExpectedTermRowVersion",
            "ExpectedWindowRowVersion",
            "Audit");
    }
}
