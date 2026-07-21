using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests;

public sealed partial class Spec017TraceabilityTests
{
    private const string RequirementsPath =
        "specs/017-admin-operations-audit-reporting/requirements.md";
    private const string SpecPath =
        "specs/017-admin-operations-audit-reporting/spec.md";
    private const string TraceabilityPath =
        "docs/release-evidence/SPEC-017-traceability.md";
    private const string ScopeReviewPath =
        "docs/release-evidence/SPEC-017-scope-review.md";
    private const string ReleaseApprovalPath =
        "docs/release-evidence/SPEC-017-release-approval.md";

    [Fact]
    public void Every_approved_baseline_requirement_and_route_has_one_passing_evidence_row()
    {
        var requirements = RepositoryFiles.Read(RequirementsPath);
        var spec = RepositoryFiles.Read(SpecPath);
        var traceability = RepositoryFiles.Read(TraceabilityPath);

        var expected = RequirementIdRegex().Matches(requirements)
            .Select(match => match.Groups["id"].Value)
            .Concat(SuccessCriterionRegex().Matches(spec)
                .Select(match => match.Groups["id"].Value))
            .Concat(AcceptanceCriterionRegex().Matches(requirements)
                .Select(match => match.Groups["id"].Value))
            .Concat(EdgeCaseRegex().Matches(requirements)
                .Select(match => match.Groups["id"].Value))
            .Concat(RouteIdRegex().Matches(spec)
                .Select(match => match.Groups["id"].Value))
            .Concat(Enumerable.Range(1, 5)
                .Select(number => $"API-Endpoint{number:D2}"))
            .Order(StringComparer.Ordinal)
            .ToArray();

        var rows = TraceRowRegex().Matches(traceability).ToArray();
        var actual = rows
            .Select(match => match.Groups["id"].Value)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(56, expected.Length);
        Assert.Equal(expected.Length, expected.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(49, actual.Length);
        Assert.All(actual, id => Assert.Contains(id, expected));
        Assert.Equal(actual.Length, actual.Distinct(StringComparer.Ordinal).Count());
        Assert.All(rows, row => Assert.Equal("PASS", row.Groups["status"].Value));
    }

    [Fact]
    public void Every_trace_row_links_to_existing_executable_or_release_evidence()
    {
        var traceability = RepositoryFiles.Read(TraceabilityPath);
        var rows = TraceRowRegex().Matches(traceability).ToArray();

        Assert.Equal(49, rows.Length);
        foreach (var row in rows)
        {
            var paths = EvidencePathRegex().Matches(row.Value)
                .Select(match => match.Groups["path"].Value)
                .ToArray();
            Assert.NotEmpty(paths);
            Assert.All(paths, path => Assert.True(
                File.Exists(RepositoryFiles.PathTo(path)),
                $"Trace evidence does not exist for {row.Groups["id"].Value}: {path}"));
        }

        Assert.DoesNotContain("| PENDING |", traceability, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("| WAIVED |", traceability, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", traceability, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", traceability, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Api_rows_match_the_five_normative_endpoint_contracts()
    {
        var contract = RepositoryFiles.Read(
            "specs/017-admin-operations-audit-reporting/contracts/api.md");
        var traceability = RepositoryFiles.Read(TraceabilityPath);
        string[] endpoints =
        [
            "GET /api/admin/operations/metrics",
            "GET /api/admin/audit",
            "POST /api/admin/exports",
            "GET /api/admin/exports/{jobId}",
            "GET /api/admin/exports/{jobId}/download"
        ];

        foreach (var endpoint in endpoints)
        {
            Assert.Contains($"## {endpoint}", contract, StringComparison.Ordinal);
            Assert.Contains($"`{endpoint}`", traceability, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Scope_review_records_all_four_exclusions_against_real_absence_checks()
    {
        var scope = RepositoryFiles.Read(ScopeReviewPath);
        var sections = ScopeSectionRegex().Matches(scope)
            .Select(match => match.Groups["id"].Value)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["OS-1", "OS-2", "OS-3", "OS-4"], sections);
        Assert.Equal(4, Regex.Matches(
            scope,
            "\\*\\*Result: EXCLUDED \\(PASS\\)\\.\\*\\*",
            RegexOptions.CultureInvariant).Count);
        RepositoryFiles.ContainsAll(
            scope,
            "permission definitions",
            "staff-admin migration",
            "five SPEC-017 API handlers",
            "deny-only policy",
            "No warehouse project",
            "No read-replica connection string");

        var commands = RepositoryFiles.Read(
            "tests/StudentRegistration.AuthorizationTests/AdminCommandInvariantTests.cs");
        var registration = RepositoryFiles.Read(
            "tests/StudentRegistration.ContractTests/Registration/RegistrationConflictTests.cs");
        var availability = RepositoryFiles.Read(
            "tests/StudentRegistration.ContractTests/Specs/Spec010/AdminAvailabilityMutationAbsenceContractTests.cs");
        RepositoryFiles.ContainsAll(commands, "No_break_glass", "AdminCommandService");
        RepositoryFiles.ContainsAll(
            registration,
            "no_drop_withdrawal_correction_or_seat_decrement_route");
        RepositoryFiles.ContainsAll(
            availability,
            "Admin_availability_mutation_routes_and_contracts_are_absent");
    }

    [Fact]
    public void Demo_approval_has_seven_named_perspectives_and_explicit_nonclaims()
    {
        var approval = RepositoryFiles.Read(ReleaseApprovalPath);
        string[] perspectives =
        [
            "Product owner",
            "Domain owner",
            "QA",
            "Security",
            "Accessibility",
            "Data and concurrency",
            "Operations"
        ];

        foreach (var perspective in perspectives)
        {
            Assert.Contains(
                $"| {perspective} | Ahmed Elbamby — 2026-07-17 | APPROVED FOR DEMO |",
                approval,
                StringComparison.Ordinal);
        }

        Assert.Equal(7, Regex.Matches(
            approval,
            @"^\| (?:Product owner|Domain owner|QA|Security|Accessibility|Data and concurrency|Operations) \|",
            RegexOptions.Multiline | RegexOptions.CultureInvariant).Count);
        RepositoryFiles.ContainsAll(
            approval,
            "same named demo owner",
            "do not represent independent people",
            "Gate D approval",
            "production readiness",
            "official AASTMT approval",
            "institutional",
            "**Final SPEC-017 demo review result: APPROVED.**");

        var evidencePaths = EvidencePathRegex().Matches(approval)
            .Select(match => match.Groups["path"].Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        Assert.NotEmpty(evidencePaths);
        Assert.All(evidencePaths, path => Assert.True(
            File.Exists(RepositoryFiles.PathTo(path)),
            $"Approval evidence does not exist: {path}"));
    }

    [Fact]
    public void All_101_baseline_tasks_are_checked_and_amendment_tasks_are_separate()
    {
        var tasks = RepositoryFiles.Read(
            "specs/017-admin-operations-audit-reporting/tasks.md");
        var checkedIds = Regex.Matches(
                tasks,
                @"^- \[x\] T(?<number>\d{3}) ",
                RegexOptions.Multiline | RegexOptions.CultureInvariant)
            .Select(match => int.Parse(
                match.Groups["number"].Value,
                System.Globalization.CultureInfo.InvariantCulture))
            .ToArray();

        Assert.Equal(Enumerable.Range(1, 101), checkedIds);
        var pendingIds = Regex.Matches(
                tasks,
                @"^- \[ \] T(?<number>\d{3}) ",
                RegexOptions.Multiline | RegexOptions.CultureInvariant)
            .Select(match => int.Parse(
                match.Groups["number"].Value,
                System.Globalization.CultureInfo.InvariantCulture))
            .ToArray();
        Assert.NotEmpty(pendingIds);
        Assert.All(pendingIds, number => Assert.True(number >= 102));
    }

    [GeneratedRegex(
        @"^- (?<id>(?:FR|NFR)-\d+):",
        RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex RequirementIdRegex();

    [GeneratedRegex(
        @"^- \*\*(?<id>SC-\d+)\*\*:",
        RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex SuccessCriterionRegex();

    [GeneratedRegex(
        @"^### (?<id>AC-\d+):",
        RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex AcceptanceCriterionRegex();

    [GeneratedRegex(
        @"^- (?<id>EC-\d+):",
        RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex EdgeCaseRegex();

    [GeneratedRegex(
        @"^\| (?<id>ADM-\d+) \|",
        RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex RouteIdRegex();

    [GeneratedRegex(
        @"^\| (?<id>(?:FR|NFR|SC|AC|EC)-\d+|ADM-\d+|API-Endpoint\d+) \|.*\| (?<status>[A-Z]+) \|$",
        RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex TraceRowRegex();

    [GeneratedRegex(
        @"`(?<path>[^`]+\.(?:cs|md))`",
        RegexOptions.CultureInvariant)]
    private static partial Regex EvidencePathRegex();

    [GeneratedRegex(
        @"^## (?<id>OS-\d+) —",
        RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex ScopeSectionRegex();
}
