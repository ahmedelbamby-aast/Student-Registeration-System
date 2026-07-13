namespace StudentRegistration.SpecificationTests.Specs.Spec005.Entities;

public sealed class AuditEventErdContractTests
{
    [Fact]
    public void Reference_declares_owner_source_fields_and_invariants() =>
        ErdReferenceContractAssertions.AssertReference(
            "specs/005-erd-data-lifecycle/contracts/entities/AuditEvent.md",
            new(
                "AuditEvent",
                "AUDIT_EVENT",
                "SPEC-004",
                "src/StudentRegistration.Infrastructure.SqlServer/Audit/AuditEvent.cs",
                [
                    "uniqueidentifier Id PK", "string ActorReference",
                    "string SubjectReference", "string Action", "string EntityType",
                    "string EntityId", "string Reason", "string BeforeSummaryJson",
                    "string AfterSummaryJson", "string CorrelationId",
                    "datetime2 OccurredAtUtc"
                ],
                [
                    "audit foundation: AuditEvent is owned upstream by SPEC-004 infrastructure",
                    "AuditEvent has no relational foreign keys",
                    "Audit events are append-only for sensitive administrative actions"
                ]));
}
