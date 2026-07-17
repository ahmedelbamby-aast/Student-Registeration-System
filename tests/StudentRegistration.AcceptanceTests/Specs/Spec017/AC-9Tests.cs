using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec017;

public sealed class AC_9Tests
{
    [Fact]
    public void Admin_quality_gate_requires_measured_metrics_audit_export_and_action_evidence()
    {
        // Given the target metric, retention-volume audit, large-export, and
        // positive/negative/concurrent command fixtures.
        var requiredDeliveries = new[]
        {
            "src/StudentRegistration.StaffAdministration/Application/AdminMetricsQuery.cs",
            "src/StudentRegistration.StaffAdministration/Application/AuditEventQueries.cs",
            "src/StudentRegistration.StaffAdministration/Application/AuditExportService.cs",
            "src/StudentRegistration.StaffAdministration/Endpoints/Spec017Endpoints.cs",
            "docs/release-evidence/SPEC-017-NFR-1.md",
            "docs/release-evidence/SPEC-017-NFR-2.md",
            "docs/release-evidence/SPEC-017-NFR-3.md",
            "docs/release-evidence/SPEC-017-NFR-4.md"
        };

        // When the admin quality gate executes, it remains red until the real
        // services and measurable evidence replace the planned contracts.
        var missing = requiredDeliveries
            .Where(path => !RepositoryFiles.Exists(path))
            .ToArray();

        Assert.True(
            missing.Length == 0,
            "Expected-red: AC-9 awaits T052/T056/T070/T091 and T092-T095. Missing: "
            + string.Join(", ", missing));

        // Then evidence must prove freshness, p95 audit latency, one leased
        // artifact with authorized expiry, and the full admin action matrix.
        var tasks = RepositoryFiles.Read(
            "specs/017-admin-operations-audit-reporting/tasks.md");
        RepositoryFiles.ContainsAll(
            tasks,
            "T092 [NFR-1]",
            "T093 [NFR-2]",
            "T094 [NFR-3]",
            "T095 [NFR-4]");
    }
}
