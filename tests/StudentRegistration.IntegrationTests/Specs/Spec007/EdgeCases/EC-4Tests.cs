using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec007.EdgeCases;

public sealed class EC_4Tests
{
    [Fact]
    public void Expired_session_requires_reauthentication_before_plan_revalidation()
    {
        var mapper = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Features/Identity/IdentityRouteStateMapper.cs");
        var requirements = RepositoryFiles.Read(
            "specs/007-identity-account-lifecycle/requirements.md");
        RepositoryFiles.ContainsAll(mapper, "expired", "reauthenticate");
        RepositoryFiles.ContainsAll(requirements, "Session expires during plan edit", "revalidate plan");
    }
}
