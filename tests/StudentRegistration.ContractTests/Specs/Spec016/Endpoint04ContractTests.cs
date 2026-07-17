using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec016;

public sealed class Endpoint04ContractTests
{
    [Fact]
    public void Get_own_availability_contract_declares_safe_complete_outcomes()
    {
        var contract = RepositoryFiles.Read("specs/016-lecturer-ta-workspace/contracts/api.md");
        RepositoryFiles.ContainsAll(
            contract,
            "### GET /api/staff/availability",
            "Returns 200 `StaffTermAvailabilityDto`",
            "| Availability GET | own aggregate |",
            "unauthenticated",
            "inactive/wrong staff context or missing `Context.Read`",
            "no applicable term/aggregate",
            "canonical rate limit",
            "canonical privacy-safe error");
    }
}
