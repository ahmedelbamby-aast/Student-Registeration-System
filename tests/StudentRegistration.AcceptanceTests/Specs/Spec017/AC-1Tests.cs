using StudentRegistration.AcceptanceTests.Specs.Spec010;
using StudentRegistration.Scheduling.Application;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec017;

public sealed class AC_1Tests
{
    [Fact]
    public async Task Reasoned_owner_mutation_preserves_invariants_and_is_visible_as_a_complete_append_only_event()
    {
        // Given an Admin has the owner permission and confirms a valid offering
        // publication with a stable reason and request identity.
        var store = new PublicationStoreFake();
        var transaction = new PublicationTransactionFake(store);
        var command = Spec010Scenario.PublishCommand(
            store.Snapshot,
            requestId: "spec017-ac1-publication");
        var service = new OfferingPublicationService(
            new OfferingPublicationValidator(),
            transaction);

        // When the real SPEC-010 owner command validates and commits.
        var result = await service.PublishAsync(command);

        // Then the publication invariant and owner audit contribution commit once.
        Assert.Equal(OfferingPublicationOutcome.Published, result.Outcome);
        Assert.Equal("published", transaction.OfferingState);
        Assert.All(transaction.GroupStates, state => Assert.Equal("published", state));
        Assert.Equal(1, transaction.CommitCount);
        Assert.Equal(1, store.ValidationCallsInsideTransaction);
        var ownerAudit = Assert.Single(transaction.Audits);
        Assert.Equal(command.OfferingId, ownerAudit.OfferingId);
        Assert.Equal(command.ClientRequestId, ownerAudit.ClientRequestId);
        Assert.Equal(command.Reason, ownerAudit.Reason);

        // And the deferred SPEC-017 projection must expose the complete shared
        // append-only event rather than inventing a second audit writer.
        const string delivery =
            "src/StudentRegistration.StaffAdministration/Application/AuditEventQueries.cs";
        Assert.True(
            RepositoryFiles.Exists(delivery),
            "Expected-red for AC-1/T032: the complete merged append-only event projection is intentionally deferred to T070.");
        var projection = RepositoryFiles.Read(delivery);
        RepositoryFiles.ContainsAll(
            projection,
            "AuditEvent",
            "SecurityEvent",
            "Actor",
            "Reason",
            "OccurredAtUtc",
            "BeforeSummary",
            "AfterSummary",
            "CorrelationId");
        Assert.DoesNotContain("Add(", projection, StringComparison.Ordinal);
        Assert.DoesNotContain("Update(", projection, StringComparison.Ordinal);
        Assert.DoesNotContain("Remove(", projection, StringComparison.Ordinal);
    }
}
