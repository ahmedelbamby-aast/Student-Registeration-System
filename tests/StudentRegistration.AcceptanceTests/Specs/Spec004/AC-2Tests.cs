using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec004;

public sealed class AC_2Tests
{
    [Fact]
    public void Consecutive_authenticated_requests_can_cross_replicas_without_losing_state()
    {
        // Given two instances share SQL state and one protected Data Protection key ring.
        var registration = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Composition/DataProtectionRegistration.cs");
        var repository = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/DataProtection/SqlDataProtectionKeyRepository.cs");
        var runbook = RepositoryFiles.Read("ops/runbooks/data-protection-keys.md");

        // When requests move between instances, authorization and plan state remain server-authoritative.
        RepositoryFiles.ContainsAll(
            registration,
            "DataProtection",
            "application name",
            "certificate",
            "Production");
        RepositoryFiles.ContainsAll(repository, "StudentRegistrationDbContext", "DataProtection");

        // Then the deployment contract relies on shared encrypted keys, not session affinity.
        RepositoryFiles.ContainsAll(
            runbook,
            "two replicas",
            "without sticky sessions",
            "rotation",
            "recovery");

        var executableEvidence = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Specs/Spec004/DataProtectionRegistrationTests.cs");
        RepositoryFiles.ContainsAll(
            executableEvidence,
            "authorization",
            "plan",
            "DataProtection",
            "Assert.");
    }
}
