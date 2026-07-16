using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Domain;

namespace StudentRegistration.AcceptanceTests.Specs.Spec009;

public sealed class AC_3Tests
{
    [Theory]
    [InlineData(2.00, 95, 9, true, false)]
    [InlineData(2.00, 96, 18, true, true)]
    [InlineData(3.00, 120, 19, false, false)]
    [InlineData(1.99, 120, 12, false, true)]
    [InlineData(1.99, 120, 13, false, false)]
    public void Policy_simulation_proves_project_and_load_boundaries(
        decimal gpa,
        decimal earnedCredits,
        int requestedCredits,
        bool requestProject,
        bool eligible)
    {
        var draft = PolicyAdministrationService.CreateDemoPolicySet(Guid.NewGuid());
        var result = new PolicyAdministrationService().Simulate(
            draft,
            new(
                gpa,
                earnedCredits,
                "Active",
                true,
                false,
                requestedCredits,
                requestProject ? ["DS413"] : ["BA101", "BA113", "GN111"],
                ["BA101", "GN111", "GN112"],
                true,
                false));

        Assert.Equal(eligible, result.Eligible);
        Assert.Equal("DEMO-POC-2026.1", result.PolicyVersion);
        Assert.All(result.RuleResults, item =>
        {
            Assert.False(string.IsNullOrWhiteSpace(item.SourceReference));
            Assert.True(Enum.IsDefined<CatalogueSourceKind>(item.SourceKind));
        });
    }
}
