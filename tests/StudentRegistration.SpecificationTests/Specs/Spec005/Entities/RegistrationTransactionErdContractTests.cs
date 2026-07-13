namespace StudentRegistration.SpecificationTests.Specs.Spec005.Entities;

public sealed class RegistrationTransactionErdContractTests
{
    [Fact]
    public void Reference_declares_both_owners_sources_fields_and_invariants() =>
        ErdReferenceContractAssertions.AssertReferences(
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
                    "A separate IdempotencyRecord table is prohibited"
                ]),
            new(
                "StudentTermRegistrationGuard",
                "STUDENT_TERM_REGISTRATION_GUARD",
                "SPEC-014",
                "src/StudentRegistration.Registration/Domain/StudentTermRegistrationGuard.cs",
                [
                    "uniqueidentifier Id PK", "uniqueidentifier StudentId FK",
                    "uniqueidentifier TermId FK", "rowversion Version"
                ],
                [
                    "STUDENT ||--o{ STUDENT_TERM_REGISTRATION_GUARD : serializes",
                    "ACADEMIC_TERM ||--o{ STUDENT_TERM_REGISTRATION_GUARD : serializes",
                    "Unique StudentTermRegistrationGuard(StudentId, TermId)"
                ]));
}
