using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec001;

public sealed class NFR_3EvidenceTests
{
    [Fact]
    public void Every_generated_protected_action_has_api_security_metadata()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(
            "specs/006-domain-class-api-contracts/contracts/openapi/student-registration-v1.json"));
        var operations = document.RootElement.GetProperty("paths")
            .EnumerateObject()
            .SelectMany(path => path.Value.EnumerateObject()
                .Select(operation => (
                    Path: path.Name,
                    Method: operation.Name,
                    Value: operation.Value)))
            .ToArray();
        var protectedOperations = operations
            .Where(operation => operation.Value.TryGetProperty("security", out _))
            .ToArray();
        var anonymousOperations = operations
            .Where(operation => !operation.Value.TryGetProperty("security", out _))
            .Select(operation => $"{operation.Method.ToUpperInvariant()} {operation.Path}")
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(20, protectedOperations.Length);
        Assert.Equal(
            new[]
            {
                "GET /api/health",
                "GET /api/public/context",
                "POST /api/auth/recovery/complete",
                "POST /api/auth/recovery/request",
                "POST /api/auth/staff/login",
                "POST /api/auth/student/activate",
                "POST /api/auth/student/login"
            },
            anonymousOperations);
        Assert.All(
            protectedOperations,
            operation => Assert.Contains(
                "StudentRegistration.Identity",
                operation.Value.GetProperty("security").GetRawText(),
                StringComparison.Ordinal));
    }

    [Fact]
    public void Authorization_suites_execute_positive_and_negative_server_policies()
    {
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "tests/StudentRegistration.AuthorizationTests/IdentityEndpointAuthorizationMatrixTests.cs"),
            "Every_protected_endpoint_executes_its_positive_and_negative_policy_pair",
            "Assert.True",
            "Assert.False");
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "tests/StudentRegistration.AuthorizationTests/AcademicPermissionPolicyTests.cs"),
            "RolePolicies.AcademicTermsManage",
            "RolePolicies.AcademicProfilesManage",
            "AuthorizeAsync");
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("docs/release-evidence/SPEC-001-NFR-3.md"),
            "**Release result:** PASS",
            "21 protected operations",
            "explicitly anonymous operations");
    }

    [Fact]
    public void Scope_review_records_all_four_exclusions()
    {
        var scope = string.Join(
            " ",
            RepositoryFiles.Read("docs/release-evidence/SPEC-001-scope-review.md")
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        RepositoryFiles.ContainsAll(
            scope,
            "Payment, grade entry, attendance, waitlist, advisor workflow, and notifications",
            "Public staff registration",
            "Client-side-only authorization",
            "Multi-tenancy and native mobile applications",
            "**Result: PASS.**");
    }
}
