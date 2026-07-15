using System.Reflection;

namespace StudentRegistration.AcceptanceTests.Specs.Spec008;

public sealed class SC_1OutcomeTests
{
    [Fact]
    public void Registration_availability_uses_only_server_time_and_approved_windows()
    {
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("specs/008-academic-term-student-profile/spec.md"),
            "**SC-1**: Registration availability is determined only by authoritative institutional time and approved windows.");
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("specs/008-academic-term-student-profile/contracts/api.md"),
            "All instants are normalized to UTC",
            "OpensAtUtc <= serverNowUtc < ClosesAtUtc",
            "No client time/term input",
            "server time");

        var academics = Assembly.Load("StudentRegistration.Academics");
        var resolver = academics.GetType(
            "StudentRegistration.Academics.Application.AcademicContextResolver");
        Assert.True(
            resolver is not null,
            "SC-1 requires the server-owned AcademicContextResolver.");
        Assert.NotNull(resolver!.GetMethod("ResolveAsync", BindingFlags.Instance | BindingFlags.Public));

        var source = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/Application/AcademicContextResolver.cs");
        RepositoryFiles.ContainsAll(
            source,
            "TimeProvider",
            "GetUtcNow",
            "OpensAtUtc",
            "ClosesAtUtc",
            "RegistrationWindowState");
        Assert.DoesNotContain("DateTime.Now", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.UtcNow", source, StringComparison.Ordinal);
        Assert.DoesNotContain("browser", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("clientSelectedTerm", source, StringComparison.OrdinalIgnoreCase);
    }
}
