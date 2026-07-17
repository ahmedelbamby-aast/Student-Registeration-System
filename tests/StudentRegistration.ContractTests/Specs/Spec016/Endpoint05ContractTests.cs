using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec016;

public sealed class Endpoint05ContractTests
{
    [Fact]
    public void Put_availability_contract_declares_atomic_versioned_outcomes()
    {
        var contract = RepositoryFiles.Read("specs/016-lecturer-ta-workspace/contracts/api.md");
        RepositoryFiles.ContainsAll(
            contract,
            "### PUT /api/staff/availability",
            "Success is 200",
            "`AvailabilityUpdateResult`",
            "`AVAILABILITY_RANGE_INVALID`",
            "`STALE_VERSION` or `AVAILABILITY_DEADLINE_PASSED`",
            "atomic rollback and canonical privacy-safe error",
            "impactAlertIds");
    }
}
