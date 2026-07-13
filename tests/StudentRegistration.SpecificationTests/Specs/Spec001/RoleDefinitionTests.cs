using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec001;

public sealed class RoleDefinitionTests
{
    private const string RolesPath =
        "specs/001-product-charter-rbac/contracts/roles.md";

    [Fact]
    public void Canonical_vocabulary_defines_exactly_four_case_sensitive_role_tokens()
    {
        var roles = RepositoryFiles.Read(RolesPath);
        var table = RepositoryFiles.Section(roles, "Canonical role definitions");

        RepositoryFiles.ContainsAll(
            table,
            "`Student`",
            "`Admin`",
            "`Lecturer`",
            "`TeachingAssistant`");

        Assert.Equal(4, table.Split('\n').Count(line => line.StartsWith("| `", StringComparison.Ordinal)));
        Assert.DoesNotContain("| `TA`", table, StringComparison.Ordinal);
    }

    [Fact]
    public void Vocabulary_keeps_runtime_identity_ownership_in_spec_007()
    {
        var roles = RepositoryFiles.Read(RolesPath);

        RepositoryFiles.ContainsAll(
            roles,
            "Runtime owner: SPEC-007",
            "MUST NOT create a runtime role assignment",
            "case-sensitive");
    }
}
