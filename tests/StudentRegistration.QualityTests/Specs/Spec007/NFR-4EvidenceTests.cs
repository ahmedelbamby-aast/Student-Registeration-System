using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec007;

public sealed class NFR_4EvidenceTests
{
    private const string EvidencePath = "docs/release-evidence/SPEC-007-NFR-4.md";

    private static readonly EndpointExpectation[] Endpoints =
    [
        new("Endpoint01", "POST", "/api/auth/student/login", "Anonymous lifecycle"),
        new("Endpoint02", "POST", "/api/auth/student/activate", "Anonymous lifecycle"),
        new("Endpoint03", "POST", "/api/auth/staff/login", "Anonymous lifecycle"),
        new("Endpoint04", "POST", "/api/auth/logout", "Protected: Authenticated"),
        new("Endpoint05", "POST", "/api/auth/recovery/request", "Anonymous lifecycle"),
        new("Endpoint06", "POST", "/api/auth/recovery/complete", "Anonymous lifecycle"),
        new("Endpoint07", "POST", "/api/auth/password/change", "Protected: Authenticated"),
        new("Endpoint08", "POST", "/api/auth/sessions/revoke-all", "Protected: Authenticated"),
        new("Endpoint09", "GET", "/api/auth/session", "Protected: Authenticated"),
        new("Endpoint10", "PUT", "/api/auth/session/context", "Protected: StaffContext"),
        new("Endpoint11", "GET", "/api/admin/users", "Protected: IdentityManagement"),
        new("Endpoint12", "POST", "/api/admin/users/imports", "Protected: IdentityManagement"),
        new("Endpoint13", "GET", "/api/admin/users/imports/{importId}", "Protected: IdentityManagement"),
        new("Endpoint14", "POST", "/api/admin/users/imports/{importId}/publish", "Protected: IdentityManagement"),
        new("Endpoint15", "PATCH", "/api/admin/users/{userId}/status", "Protected: IdentityManagement"),
        new("Endpoint16", "PUT", "/api/admin/users/{userId}/roles", "Protected: IdentityManagement")
    ];

    private static readonly Regex EvidenceLink = new(
        @"\[(?<class>[A-Za-z_][A-Za-z0-9_]*)\]\((?<path>\.\./\.\./tests/[^)]+\.cs)\)",
        RegexOptions.CultureInvariant);

    [Fact]
    public void Matrix_has_the_exact_route_inventory_and_source_backed_policy_pairs()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        var rows = ParseRows(evidence);

        Assert.Equal(16, rows.Length);
        Assert.Equal(16, rows.Select(row => row.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(11, rows.Count(row => row.Surface.StartsWith("Protected:", StringComparison.Ordinal)));
        Assert.Equal(5, rows.Count(row => row.Surface == "Anonymous lifecycle"));

        foreach (var expected in Endpoints)
        {
            var row = Assert.Single(rows, candidate => candidate.Id == expected.Id);
            Assert.Equal(expected.Method, row.Method);
            Assert.Equal(expected.Route, row.Route.Trim('`'));
            Assert.Equal(expected.Surface, row.Surface);
            Assert.False(string.IsNullOrWhiteSpace(row.Positive));
            Assert.False(string.IsNullOrWhiteSpace(row.Negative));

            var contractClass = $"{expected.Id}ContractTests";
            var contractPath =
                $"tests/StudentRegistration.ContractTests/Specs/Spec007/{contractClass}.cs";
            Assert.Contains($"[{contractClass}](../../{contractPath})", row.Evidence, StringComparison.Ordinal);
            Assert.True(RepositoryFiles.Exists(contractPath), $"Missing {contractPath}");
            var contractSource = RepositoryFiles.Read(contractPath);
            Assert.Contains($"public sealed class {contractClass}", contractSource, StringComparison.Ordinal);
            Assert.Contains(
                expected.IsProtected
                    ? "Spec007ContractAssertions.IsProtected(endpoint)"
                    : "Spec007ContractAssertions.IsAnonymous(endpoint)",
                contractSource,
                StringComparison.Ordinal);

            var links = EvidenceLink.Matches(row.Evidence);
            Assert.NotEmpty(links);
            foreach (Match link in links)
            {
                AssertLinkedTestClassExists(link, row.Id);
            }

            if (expected.IsProtected)
            {
                Assert.Contains(
                    "[IdentityEndpointAuthorizationMatrixTests]",
                    row.Evidence,
                    StringComparison.Ordinal);
            }
            else
            {
                Assert.StartsWith("Not applicable:", row.Positive, StringComparison.Ordinal);
            }
        }

        var normalizedEvidence = Regex.Replace(evidence, @"\s+", " ");
        RepositoryFiles.ContainsAll(
            normalizedEvidence,
            "complete at the compiled endpoint-metadata plus executed policy layer",
            "Direct TestServer handler-level positive/negative authorization is evidenced for Endpoint10 only",
            "does not treat this document's wording as proof");
        Assert.DoesNotContain("**Result: PASS.**", evidence, StringComparison.Ordinal);
    }

    [Fact]
    public void Executable_matrix_discovers_all_protected_routes_and_runs_both_principals()
    {
        var source = RepositoryFiles.Read(
            "tests/StudentRegistration.AuthorizationTests/IdentityEndpointAuthorizationMatrixTests.cs");

        RepositoryFiles.ContainsAll(
            source,
            "MapSpec007Endpoints",
            "AuthorizationPolicy.CombineAsync",
            "Assert.True",
            "Assert.False",
            "Assert.Equal(11, ProtectedEndpoints.Length)",
            "RolePolicies.StaffContext",
            "RolePolicies.IdentityManagement");

        foreach (var endpoint in Endpoints.Where(endpoint => endpoint.IsProtected))
        {
            Assert.Contains(
                $"(\"{endpoint.Method}\", \"{endpoint.Route}\")",
                source,
                StringComparison.Ordinal);
        }

        var httpEvidence = RepositoryFiles.Read(
            "tests/StudentRegistration.SecurityTests/IdentityRuntimeCompositionTests.cs");
        RepositoryFiles.ContainsAll(
            httpEvidence,
            "Endpoint10_rejects_students_and_antiforgery_precedes_the_handler",
            "Endpoint10_issues_only_the_selected_role_as_authorizing_claims",
            "HttpStatusCode.Forbidden",
            "HttpStatusCode.OK");
    }

    private static MatrixRow[] ParseRows(string markdown)
    {
        var section = RepositoryFiles.Section(markdown, "Endpoint inventory and authorization matrix");
        return section
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.StartsWith("| ", StringComparison.Ordinal))
            .Select(line => line.Trim().Trim('|').Split('|', StringSplitOptions.TrimEntries))
            .Where(cells => cells.Length == 7
                && !string.Equals(cells[0], "ID", StringComparison.Ordinal)
                && !cells[0].StartsWith("---", StringComparison.Ordinal))
            .Select(cells => new MatrixRow(
                cells[0], cells[1], cells[2], cells[3], cells[4], cells[5], cells[6]))
            .ToArray();
    }

    private static void AssertLinkedTestClassExists(Match link, string rowId)
    {
        var className = link.Groups["class"].Value;
        var relativePath = link.Groups["path"].Value[6..];
        Assert.DoesNotContain(className, nameof(NFR_4EvidenceTests), StringComparison.Ordinal);
        Assert.True(
            RepositoryFiles.Exists(relativePath),
            $"{rowId} references a missing test source: {relativePath}");
        Assert.Matches(
            $@"\bpublic\s+(?:sealed\s+)?class\s+{Regex.Escape(className)}\b",
            RepositoryFiles.Read(relativePath));
    }

    private sealed record EndpointExpectation(
        string Id,
        string Method,
        string Route,
        string Surface)
    {
        public bool IsProtected => Surface.StartsWith("Protected:", StringComparison.Ordinal);
    }

    private sealed record MatrixRow(
        string Id,
        string Method,
        string Route,
        string Surface,
        string Positive,
        string Negative,
        string Evidence);
}
