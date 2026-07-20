using StudentRegistration.TestSupport;

namespace StudentRegistration.MigrationTests;

public sealed class Spec014ApprovalSeatHoldsMigrationTests
{
    [Fact]
    public void Migration_adds_the_approved_registration_entities_and_safe_upgrade_defaults()
    {
        var migration = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Migrations/" +
            "20260720185102_Spec014ApprovalSeatHoldsAndFirstTermAutomation.cs");

        RepositoryFiles.ContainsAll(
            migration,
            "RegistrationSubmissionLines",
            "RegistrationSeatHolds",
            "RegistrationApprovalDecisions",
            "FirstTermAutoEnrollmentBatches",
            "FirstTermAutoEnrollmentItems",
            "ProgramTermOrdinal",
            "HeldSeatCount",
            "defaultValue: 2",
            "defaultValue: \"student-self-service\"",
            "[EnrolledCount] + [HeldSeatCount] <= [Capacity]");
    }
}
