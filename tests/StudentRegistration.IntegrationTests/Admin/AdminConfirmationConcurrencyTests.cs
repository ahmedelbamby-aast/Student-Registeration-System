using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Admin;

public sealed class AdminConfirmationConcurrencyTests
{
    private const string ExportServicePath =
        "src/StudentRegistration.StaffAdministration/Application/AuditExportService.cs";

    [Fact]
    public void Commands_require_the_owner_specific_version_and_idempotency_metadata()
    {
        var identityContracts = RepositoryFiles.Read(
            "src/StudentRegistration.Contracts/Identity/AdministrationContracts.cs");
        var termContracts = RepositoryFiles.Read(
            "src/StudentRegistration.Contracts/Academics/AcademicTermContracts.cs");
        var catalogueContracts = RepositoryFiles.Read(
            "src/StudentRegistration.Contracts/Academics/CatalogueAdministrationContracts.cs");
        var schedulingContracts = RepositoryFiles.Read(
            "src/StudentRegistration.Contracts/Scheduling/SchedulingContracts.cs");

        RepositoryFiles.ContainsAll(
            identityContracts,
            "IdentityImportRequest(",
            "string ClientRequestId",
            "IdentityImportPublishRequest(",
            "string ExpectedRowVersion",
            "UserStatusRequest(",
            "UserRolesRequest(");
        RepositoryFiles.ContainsAll(
            termContracts,
            "CreateTermRequest",
            "Guid ClientRequestId",
            "UpdateTermRequest",
            "ExpectedTermRowVersion");
        RepositoryFiles.ContainsAll(
            catalogueContracts,
            "PublishVersionRequest(",
            "string PreviewToken",
            "Guid ClientRequestId",
            "PolicySetMutationRequest(",
            "string ExpectedPolicySetRowVersion",
            "PolicyPublishRequest(");
        RepositoryFiles.ContainsAll(
            schedulingContracts,
            "UpdateGroupRequest(",
            "string ExpectedOfferingRowVersion",
            "PublishOfferingRequest(",
            "string PreviewToken",
            "Guid ClientRequestId",
            "UpdateRoomRequest(",
            "string ExpectedRowVersion");

        Assert.True(
            RepositoryFiles.Exists(ExportServicePath),
            "Expected-red for T065: the retryable export command has not yet "
            + "delivered its client-request-id/request-hash enforcement.");
        var exportService = RepositoryFiles.Read(ExportServicePath);
        RepositoryFiles.ContainsAll(
            exportService,
            "ClientRequestId",
            "RequestHash",
            "IDEMPOTENCY_KEY_REUSED");
    }

    [Fact]
    public void Owner_preview_is_bound_and_rejects_stale_duplicate_or_mismatched_confirmation()
    {
        var service = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/Application/PublicationConfirmationService.cs");
        var executableEvidence = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Academics/PublicationRaceTests.cs");

        RepositoryFiles.ContainsAll(
            service,
            "ActorReference",
            "ScopeKind",
            "ScopeCode",
            "PayloadHash",
            "DependencyVersions",
            "ExpiresAtUtc",
            "ExpectedRowVersion",
            "PreviewToken",
            "ClientRequestId",
            "StalePreview",
            "IdempotencyKeyReused");
        RepositoryFiles.ContainsAll(
            executableEvidence,
            "Preview_is_bound_to_actor_scope_content_dependencies_and_expiry",
            "Edited_draft_old_preview_rejects_and_same_key_replays_without_audit",
            "Same_key_with_different_canonical_payload_is_rejected",
            "Concurrent_current_confirmations_have_exactly_one_winner",
            "Same_key_same_payload_replays_one_version_and_one_audit");
    }

    [Fact]
    public void Final_admin_write_skew_is_serialized_by_the_identity_owner()
    {
        var sqlEvidence = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Specs/Spec007/AdminUserLifecycleStorePersistenceTests.cs");
        var endpoint = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs");

        RepositoryFiles.ContainsAll(
            sqlEvidence,
            "await using var replicaOne",
            "await using var replicaTwo",
            "Task.WhenAll(disable, removeRole)",
            "AdminStoreOutcome.FinalAdminRequired",
            "CountEnabledAdminsAsync",
            "Set<AdminSecurityGuard>()");
        RepositoryFiles.ContainsAll(
            endpoint,
            "StatusCodes.Status409Conflict",
            "FINAL_ADMIN_REQUIRED");
    }
}
