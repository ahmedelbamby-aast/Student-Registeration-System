using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec007;

public sealed class TraceabilityEvidenceTests
{
    private static readonly string[] RequiredIds =
    [
        .. Enumerable.Range(1, 14).Select(number => $"FR-{number}"),
        .. Enumerable.Range(1, 4).Select(number => $"NFR-{number}"),
        .. Enumerable.Range(1, 10).Select(number => $"AC-{number}"),
        .. Enumerable.Range(1, 6).Select(number => $"EC-{number}"),
        .. Enumerable.Range(1, 3).Select(number => $"SC-{number}"),
        "AUTH-02", "AUTH-03", "AUTH-04", "AUTH-05", "STU-08", "ADM-03", "SYS-01",
        "ApplicationUser", "Staff", "StudentActivation", "AccountRecoveryChallenge",
        "RoleAssignment", "AuthenticationAbuseState", "IdentityImportBatch",
        "SecurityEvent", "AdminSecurityGuard",
        .. Enumerable.Range(1, 16).Select(number => $"Endpoint{number:00}")
    ];

    [Fact]
    public void Every_spec007_requirement_route_entity_and_endpoint_has_one_passing_row()
    {
        var evidence = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-007-traceability.md");

        foreach (var id in RequiredIds)
        {
            var rows = Regex.Matches(
                evidence,
                $@"(?m)^\|\s*{Regex.Escape(id)}\s*\|(?<body>[^\r\n]+)\|\s*PASS\s*\|\s*$");
            Assert.True(rows.Count == 1, $"{id} must have exactly one PASS row; found {rows.Count}.");
            Assert.DoesNotContain("pending", rows[0].Value, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("TBD", rows[0].Value, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Scope_and_release_decision_keep_production_fail_closed()
    {
        var scope = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-007-scope-review.md");
        var approval = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-007-release-approval.md");

        RepositoryFiles.ContainsAll(
            scope,
            "OS-1",
            "OS-2",
            "OS-3",
            "OS-4",
            "Development",
            "Testing",
            "Production",
            "fail closed");
        RepositoryFiles.ContainsAll(
            approval,
            "Ahmed ELbamby",
            "Product",
            "Security",
            "Identity owner",
            "QA",
            "Accessibility",
            "Data / concurrency",
            "Operations",
            "APPROVED",
            "Production NOT APPROVED");
    }
}
