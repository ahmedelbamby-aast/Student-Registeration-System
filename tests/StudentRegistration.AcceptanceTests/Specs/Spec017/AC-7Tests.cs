using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec017;

public sealed class AC_7Tests
{
    [Fact]
    public void Identity_owner_rolls_back_a_validated_mutation_when_audit_persistence_fails()
    {
        // Given SPEC-007 has validated the sensitive status change, its real-SQL
        // conformance test injects the shared SPEC-004 writer failure.
        var sqlConformance = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Specs/Spec007/AdminUserLifecycleStorePersistenceTests.cs");
        RepositoryFiles.ContainsAll(
            sqlConformance,
            "var importedBeforeFault",
            "var factsBeforeFault",
            "new ThrowingAuditWriter()",
            "AdminStoreOutcome.StorageFailure",
            "var importedAfterFault",
            "importedBeforeFault.Version.SequenceEqual(importedAfterFault.Version)",
            "factsBeforeFault.Security",
            "factsBeforeFault.Audit");

        // Then the canonical Identity endpoint maps storage failure to one safe,
        // correlated generic failure and never reports mutation success.
        var endpoint = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs");
        RepositoryFiles.ContainsAll(
            endpoint,
            "StatusCodes.Status500InternalServerError",
            "ADMIN_OPERATION_FAILED",
            "context.TraceIdentifier");
        Assert.DoesNotContain("Injected audit failure", endpoint, StringComparison.Ordinal);
    }
}
