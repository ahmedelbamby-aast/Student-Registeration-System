using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec010;

public sealed class Endpoint14ContractTests
{
    [Fact]
    public void Alert_resolution_requires_current_version_reason_and_passing_revalidation()
    {
        var request = Spec010ContractAssertions.ContractType(
            "ResolveScheduleImpactAlertRequest");
        Spec010ContractAssertions.HasExactProperties(
            request,
            "ExpectedAlertRowVersion", "Reason");

        var endpoint = Spec010ContractAssertions.Endpoint(
            "POST",
            "/api/admin/schedule-impact-alerts/{alertId}/resolve");
        Spec010ContractAssertions.RequiresOfferingsManage(endpoint);
        Spec010ContractAssertions.RequiresAntiforgery(endpoint);
        Spec010ContractAssertions.HandlerAccepts(endpoint, request.Name);
        Spec010ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.Scheduling.ScheduleImpactAlertDto");
        Spec010ContractAssertions.DeclaresStandardErrors(endpoint, true, true);
        Spec010ContractAssertions.ContractContains(
            "requires a non-empty reason",
            "dependency identities stored by its latest passing revalidation",
            "requires their current versions to match that snapshot",
            "REVALIDATION_REQUIRED",
            "REVALIDATION_FAILED",
            "resolving and auditing atomically",
            "No silent, stale, unaudited, or partial resolution is committed");
    }
}
