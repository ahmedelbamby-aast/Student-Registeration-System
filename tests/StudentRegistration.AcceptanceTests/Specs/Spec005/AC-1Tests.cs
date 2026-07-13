namespace StudentRegistration.AcceptanceTests.Specs.Spec005;

public sealed class AC_1Tests
{
    private const string DeferredReason =
        "Deferred real-SQL/API proof: activate SPEC-014 Enrollment and RegistrationModelConfiguration, the SPEC-015 S6Registration migration, and the SPEC-006/SPEC-014 stable 409 conflict mapping at entity-ownership 2.0.0 and persistence-manifest 2.1.0.";

    [Fact(Skip = DeferredReason)]
    public void Existing_student_offering_enrollment_rejects_a_concurrent_duplicate_with_a_stable_409_and_no_second_row()
    {
        // Given a committed Enrollment for one student and offering in SQL Server.
        // When a second transaction concurrently inserts the same student/offering.
        // Then the unique guard rejects it and the real API returns its owner-defined stable 409 without mutation.
        throw new NotImplementedException(
            "Run against the composed S6Registration schema and the real SPEC-014 endpoint; do not replace this with an in-memory duplicate check.");
    }
}
