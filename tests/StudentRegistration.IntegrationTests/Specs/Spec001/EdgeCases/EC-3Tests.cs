using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec001.EdgeCases;

public sealed class EC_3Tests
{
    [Fact]
    public void Out_of_scope_enhancement_requires_a_new_approved_spec()
    {
        var requirements = RepositoryFiles.Read(
            "specs/001-product-charter-rbac/requirements.md");
        var projectPlan = RepositoryFiles.Read("docs/PROJECT_PLAN.md");

        RepositoryFiles.ContainsAll(
            requirements,
            "EC-3",
            "create/review a new spec",
            "Out of Scope");
        RepositoryFiles.ContainsAll(
            projectPlan,
            "Behavior changes begin with a spec pull request",
            "Approved --> InDevelopment");
    }
}
