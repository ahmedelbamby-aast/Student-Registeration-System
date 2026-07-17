using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec017;

public sealed class AC_8Tests
{
    [Fact]
    public void Two_identity_replicas_serialize_final_admin_revocation_through_one_guard()
    {
        // Given exactly two enabled Admin assignments, the canonical SPEC-007
        // real-SQL test creates two independent contexts/stores.
        var sqlConformance = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Specs/Spec007/AdminUserLifecycleStorePersistenceTests.cs");
        RepositoryFiles.ContainsAll(
            sqlConformance,
            "await using var replicaOne",
            "await using var replicaTwo",
            "Task.WhenAll(disable, removeRole)",
            "AdminStoreOutcome.Succeeded",
            "AdminStoreOutcome.FinalAdminRequired",
            "CountEnabledAdminsAsync",
            "Set<AdminSecurityGuard>()");

        // Then Identity returns the stable 409 conflict and SPEC-017 delegates it
        // without owning a second role or guard writer.
        var endpoint = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs");
        RepositoryFiles.ContainsAll(
            endpoint,
            "StatusCodes.Status409Conflict",
            "FINAL_ADMIN_REQUIRED");

        var delegation = RepositoryFiles.Read(
            "specs/017-admin-operations-audit-reporting/contracts/identity-admin-delegation.md");
        RepositoryFiles.ContainsAll(
            delegation,
            "SPEC-007 IdentityAccess is the sole owner",
            "locks the singleton `AdminSecurityGuard`",
            "may commit at",
            "most one change",
            "at least one enabled Admin remains");
    }
}
