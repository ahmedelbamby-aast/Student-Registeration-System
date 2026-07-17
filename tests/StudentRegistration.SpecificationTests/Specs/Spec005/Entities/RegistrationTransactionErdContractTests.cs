using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec005.Entities;

public sealed class RegistrationTransactionErdContractTests
{
    [Fact]
    public void Reference_declares_submission_owner_source_fields_and_invariants() =>
        ErdReferenceContractAssertions.AssertReference(
            "specs/005-erd-data-lifecycle/contracts/entities/RegistrationTransaction.md",
            new(
                "RegistrationSubmission",
                "REGISTRATION_SUBMISSION",
                "SPEC-014",
                "src/StudentRegistration.Registration/Domain/RegistrationSubmission.cs",
                [
                    "uniqueidentifier Id PK", "uniqueidentifier StudentId FK",
                    "uniqueidentifier TermId FK", "uniqueidentifier ClientRequestId",
                    "string PayloadHash", "string ProcessingState", "string ResultCode",
                    "string Reference UK", "string ReceiptSnapshotJson",
                    "string DecisionSnapshotJson", "datetime2 ReceivedAtUtc",
                    "datetime2 UpdatedAtUtc", "datetime2 CompletedAtUtc nullable"
                ],
                [
                    "Unique RegistrationSubmission(StudentId, TermId, ClientRequestId)",
                    "RegistrationSubmission.ReceivedAtUtc is its immutable creation instant",
                    "CompletedAtUtc is nullable until a final outcome",
                    "Unique non-null RegistrationSubmission.Reference",
                    "RegistrationSubmission idempotency claim, final result, decision snapshot, audit event, and any successful counters/enrollments commit in one SQL transaction",
                    "A separate IdempotencyRecord table and a seventh SPEC-008 idempotency entity are prohibited"
                ]));

    [Fact]
    public void Reference_consumes_the_spec008_boundary_without_a_duplicate_guard()
    {
        var reference = RepositoryFiles.Read(
            "specs/005-erd-data-lifecycle/contracts/entities/RegistrationTransaction.md");
        var ownership = RepositoryFiles.Read(".specify/entity-ownership.json");
        var persistence = RepositoryFiles.Read(".specify/persistence-manifest.json");

        RepositoryFiles.ContainsAll(
            reference,
            "Consumed StudentTermAcademicState boundary",
            "Canonical owner: SPEC-008",
            "src/StudentRegistration.Academics/Domain/StudentTermAcademicState.cs",
            "ExecuteRegistrationBoundaryAsync",
            "SPEC-014 consumes that public boundary without a",
            "second entity or mapping");
        Assert.DoesNotContain("StudentTermRegistrationGuard", reference, StringComparison.Ordinal);
        Assert.DoesNotContain("StudentTermRegistrationGuard", ownership, StringComparison.Ordinal);
        Assert.DoesNotContain("StudentTermRegistrationGuard", persistence, StringComparison.Ordinal);
    }
}
