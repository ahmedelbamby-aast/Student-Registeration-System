using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Spec005;

public sealed class RegistrationGuardSchemaContractTests
{
    private const string ContractPath = "docs/data/registration-transaction-schema.md";

    [Fact]
    public void Fr9_contract_reuses_academic_boundary_and_defines_payload_bound_submission()
    {
        var contract = Spec005ContractTestSupport.ReadBoundedContract(ContractPath);
        Spec005ContractTestSupport.AssertWorkstream(
            "Registration serialization and idempotency persistence",
            ContractPath,
            "tests/StudentRegistration.SpecificationTests/Spec005/RegistrationGuardSchemaContractTests.cs",
            "FR-9");

        using var ownership = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/entity-ownership.json"));
        var owners = ownership.RootElement.GetProperty("canonicalOwners");
        Assert.Equal("014", owners.GetProperty("RegistrationSubmission").GetString());
        Assert.Equal("008", owners.GetProperty("StudentTermAcademicState").GetString());
        Assert.False(owners.TryGetProperty("StudentTermRegistrationGuard", out _));
        Assert.DoesNotContain("IdempotencyRecord", ownership.RootElement.ToString(), StringComparison.Ordinal);

        using var persistence = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/persistence-manifest.json"));
        var contribution = persistence.RootElement.GetProperty("contributions").GetProperty("014");
        var academicContribution = persistence.RootElement.GetProperty("contributions").GetProperty("008");
        Assert.Equal("writable-transactional", contribution.GetProperty("mode").GetString());
        Assert.DoesNotContain("IdempotencyRecord", persistence.RootElement.ToString(), StringComparison.Ordinal);
        var entities = contribution.GetProperty("entities")
            .EnumerateArray()
            .Select(entity => entity.GetString())
            .ToArray();
        Assert.Contains("RegistrationSubmission", entities);
        Assert.DoesNotContain("StudentTermRegistrationGuard", entities);
        Assert.Contains(
            "StudentTermAcademicState",
            academicContribution.GetProperty("entities")
                .EnumerateArray()
                .Select(entity => entity.GetString()));

        Spec005ContractTestSupport.AssertContainsNormalized(
            contract,
            "registration-transaction-schema/1.1",
            "FR-9",
            "Canonical owner: SPEC-014",
            contribution.GetProperty("path").GetString()!,
            "StudentTermAcademicState is canonically owned and mapped by SPEC-008",
            "Unique StudentTermAcademicState(StudentId, TermId)",
            "ExecuteRegistrationBoundaryAsync",
            "A duplicate Registration-owned row or in-memory lock is not a substitute",
            "StudentId: owner",
            "TermId: scope",
            "ClientRequestId: key",
            "PayloadHash", "ProcessingState", "ResultCode", "Reference",
            "ReceiptSnapshotJson", "DecisionSnapshotJson",
            "ReceivedAtUtc", "UpdatedAtUtc", "CompletedAtUtc",
            "Same key and same payload replays the stored deterministic result",
            "A different payload returns 409 IDEMPOTENCY_KEY_REUSED",
            "Reference and ReceiptSnapshotJson exist only for an accepted final result",
            "DecisionSnapshotJson preserves the exact decision",
            "A deterministic rejection rolls allocation changes back to the savepoint before the final Rejected result commits",
            "An infrastructure failure rolls back the entire transaction and idempotency claim",
            "No standalone Processing row may commit",
            "A post-commit process failure replays the stored result",
            "The bounded 500 ms HTTP 202 response is transport-only, contains no SubmissionId, and persists no result row",
            "RegistrationSubmission is the sole idempotency claim/final-result record",
            "A separate IdempotencyRecord entity, mapping, or table is prohibited");
        Spec005ContractTestSupport.AssertMirrorsErd(
            contract,
            "Unique RegistrationSubmission(StudentId, TermId, ClientRequestId)",
            "RegistrationSubmission.ReceivedAtUtc is its immutable creation instant",
            "CompletedAtUtc is nullable until a final outcome",
            "Unique StudentTermAcademicState(StudentId, TermId)",
            "RegistrationSubmission idempotency claim, final result, decision snapshot, audit event, and any successful counters/enrollments commit in one SQL transaction",
            "a Processing claim cannot be committed on its own");
    }
}
