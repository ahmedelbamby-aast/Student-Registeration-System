using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Shared;

public sealed class ApiVersionPolicyTests
{
    private const string PolicyPath = "docs/API_VERSIONING.md";

    [Fact]
    public void First_API_version_and_breaking_change_rules_are_explicit()
    {
        var policy = RepositoryFiles.Read(PolicyPath);

        RepositoryFiles.ContainsAll(
            policy,
            "Current public API version: v1",
            "Non-breaking changes",
            "Breaking changes",
            "explicit approval",
            "generated OpenAPI",
            "semantic diff",
            "Deprecation");
    }

    [Fact]
    public void A_new_major_version_is_not_created_without_a_real_breaking_change()
    {
        var policy = RepositoryFiles.Read(PolicyPath);

        RepositoryFiles.ContainsAll(
            policy,
            "Do not introduce v2",
            "operation",
            "schema",
            "status code",
            "security requirement",
            "Formatting or ordering");
    }
}
