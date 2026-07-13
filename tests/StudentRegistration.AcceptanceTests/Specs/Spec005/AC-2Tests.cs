namespace StudentRegistration.AcceptanceTests.Specs.Spec005;

public sealed class AC_2Tests
{
    private const string DeferredReason =
        "Deferred real-SQL/application proof: activate SPEC-010 SectionGroup and SectionGroupCapacityService, SchedulingModelConfiguration in S2CatalogueScheduling, and the SPEC-006 stable error contract at entity-ownership 2.0.0 and persistence-manifest 2.1.0.";

    [Fact(Skip = DeferredReason)]
    public void Capacity_reduction_below_twenty_active_enrollments_is_rejected_without_changing_the_group()
    {
        // Given a real SectionGroup with EnrolledCount 20 and 20 active Enrollment rows.
        // When an authorized command attempts to reduce Capacity to 19.
        // Then the application/database rejects the invalid reduction and preserves the current capacity/version.
        throw new NotImplementedException(
            "Run through the real SPEC-010 capacity command and SQL check constraint; do not prove this with local arithmetic.");
    }
}
