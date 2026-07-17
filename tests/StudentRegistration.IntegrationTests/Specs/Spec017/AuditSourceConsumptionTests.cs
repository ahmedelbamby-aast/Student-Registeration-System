using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.Infrastructure.SqlServer.Audit;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec017;

public sealed class AuditSourceConsumptionTests
{
    [Fact]
    public void Spec017_consumes_the_canonical_audit_identity_event_and_admin_guard_types()
    {
        Assert.Equal(
            "StudentRegistration.Infrastructure.SqlServer",
            typeof(AuditEvent).Assembly.GetName().Name);
        Assert.Equal(
            "StudentRegistration.IdentityAccess",
            typeof(SecurityEvent).Assembly.GetName().Name);
        Assert.Equal(
            "StudentRegistration.IdentityAccess",
            typeof(AdminSecurityGuard).Assembly.GetName().Name);

        Assert.Equal(
            "src/StudentRegistration.Infrastructure.SqlServer/Audit/AuditEvent.cs",
            CanonicalArtifact("004:AuditEvent"));
        Assert.True(RepositoryFiles.Exists(
            "src/StudentRegistration.IdentityAccess/Domain/SecurityEvent.cs"));
        Assert.True(RepositoryFiles.Exists(
            "src/StudentRegistration.IdentityAccess/Domain/AdminSecurityGuard.cs"));

        var staffAssembly = typeof(StudentRegistration.StaffAdministration.Domain.RosterRow)
            .Assembly;
        Assert.Null(staffAssembly.GetType(
            "StudentRegistration.StaffAdministration.Domain.AuditEvent"));
        Assert.Null(staffAssembly.GetType(
            "StudentRegistration.StaffAdministration.Domain.SecurityEvent"));
        Assert.Null(staffAssembly.GetType(
            "StudentRegistration.StaffAdministration.Domain.AdminSecurityGuard"));
    }

    [Fact]
    public void Staff_administration_exposes_no_writer_or_mutation_for_consumed_sources()
    {
        var source = ReadStaffAdministrationSources();

        Assert.DoesNotContain("class AuditEvent", source, StringComparison.Ordinal);
        Assert.DoesNotContain("class SecurityEvent", source, StringComparison.Ordinal);
        Assert.DoesNotContain("class AdminSecurityGuard", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DbSet<AuditEvent>", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DbSet<SecurityEvent>", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DbSet<AdminSecurityGuard>", source, StringComparison.Ordinal);
        Assert.DoesNotContain("new AdminSecurityGuard", source, StringComparison.Ordinal);
    }

    private static string CanonicalArtifact(string key)
    {
        using var document = System.Text.Json.JsonDocument.Parse(
            RepositoryFiles.Read(".specify/entity-ownership.json"));
        return document.RootElement
            .GetProperty("artifactOverrides")
            .GetProperty(key)
            .GetString()!;
    }

    private static string ReadStaffAdministrationSources() => string.Join(
        "\n",
        Directory.EnumerateFiles(
                RepositoryFiles.PathTo("src/StudentRegistration.StaffAdministration"),
                "*.cs",
                SearchOption.AllDirectories)
            .Where(path => !path.Contains(
                $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                StringComparison.OrdinalIgnoreCase))
            .Select(File.ReadAllText));
}
