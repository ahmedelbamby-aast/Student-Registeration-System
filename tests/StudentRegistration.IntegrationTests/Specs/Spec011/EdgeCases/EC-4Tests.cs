using StudentRegistration.Registration.Application;
using StudentRegistration.TestSupport;
using StudentRegistration.TestSupport.Spec011;

namespace StudentRegistration.IntegrationTests.Specs.Spec011.EdgeCases;

public sealed class EC_4Tests
{
    [Fact]
    public async Task No_eligible_page_keeps_policy_reason_window_support_and_reset_state()
    {
        var fixture = new Spec011ScenarioBuilder
        {
            WindowOpen = false
        };

        var result = await Spec011ServiceFactory.Search(fixture).SearchAsync(
            fixture.ApplicationUserId,
            fixture.TermId,
            new(null, "eligible", null, null, "all", null, 1, 20));

        Assert.Equal(OfferingSearchOutcome.Found, result.Outcome);
        Assert.Empty(result.Page!.Items);
        Assert.Equal("DEMO-POC-2026.1", result.Metadata!.PolicyVersion);
        Assert.Contains(
            "REGISTRATION_WINDOW_CLOSED",
            result.Metadata.ReasonCodes);
        Assert.Equal("closed", result.Metadata.RegistrationWindowState);
        Assert.Equal(
            EligibilityService.SupportReferencePath,
            result.Metadata.SupportReferencePath);

        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/SubjectDiscoveryPage.razor");
        RepositoryFiles.ContainsAll(
            page,
            "Reset filters",
            "ResetFiltersAsync",
            "Policy version",
            "Registration window",
            "Open Registrar support");
    }
}
