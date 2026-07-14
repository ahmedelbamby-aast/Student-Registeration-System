namespace StudentRegistration.AcceptanceTests.Specs.Spec007;

public sealed class AC_10Tests
{
    [Fact]
    public void Governed_admin_lifecycle_is_bounded_versioned_idempotent_and_audited()
    {
        Spec007AcceptanceAssertions.Source(
            "src/StudentRegistration.IdentityAccess/Application/AdminUserLifecycleService.cs",
            "MaximumImportRows = 500",
            "ComputeCanonicalContentHash",
            "ExpectedRowVersion",
            "CreateSecurityStamp",
            "FinalAdminRequired",
            "StaleVersion");
        Spec007AcceptanceAssertions.Source(
            "src/StudentRegistration.IdentityAccess/Application/Ports/IAdminUserLifecycleStore.cs",
            "AdminSecurityGuard",
            "SecurityEvent and AuditEvent",
            "all-or-nothing",
            "FINAL_ADMIN_REQUIRED");
        Spec007AcceptanceAssertions.Source(
            "src/StudentRegistration.IdentityAccess/Domain/IdentityImportCandidateRow.cs",
            "IdentityImportBatchId",
            "Ordinal",
            "ExternalReference",
            "Roles");
        Spec007AcceptanceAssertions.Source(
            "src/StudentRegistration.IdentityAccess/Application/Ports/IProvisionedCredentialHandoff.cs",
            "PrepareAsync",
            "CompleteAsync",
            "AbortAsync",
            "Production");
        Spec007AcceptanceAssertions.Source(
            "src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs",
            "/api/admin/users",
            "/api/admin/users/imports",
            "/api/admin/users/{userId}/status",
            "/api/admin/users/{userId}/roles",
            "IdentityManagement");
    }
}
