using System.Text.Json;
using StudentRegistration.Client.Features.Frontend.Models;

namespace StudentRegistration.IntegrationTests.Specs.Spec003;

public sealed class FrontendAppContextViewModelTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 13, 12, 0, 0, TimeSpan.FromHours(3));

    [Fact]
    public void Serializes_the_complete_authoritative_context_contract()
    {
        var model = new FrontendAppContextView(
            Now,
            "Africa/Cairo",
            "Summer 2026",
            "Summer 2026",
            Now.AddDays(-1),
            Now.AddDays(7),
            "Ahmed Student",
            ["Student"],
            "Student",
            roleSelectionRequired: false,
            "active",
            Now.AddHours(1),
            "available",
            "/status/support-reference");

        using var document = JsonDocument.Parse(
            JsonSerializer.Serialize(model, JsonSerializerOptions.Web));
        var root = document.RootElement;

        Assert.Equal("Africa/Cairo", root.GetProperty("timeZone").GetString());
        Assert.Equal("Summer 2026", root.GetProperty("teachingTerm").GetString());
        Assert.Equal("Summer 2026", root.GetProperty("registrationTerm").GetString());
        Assert.Equal("Student", root.GetProperty("activeRole").GetString());
        Assert.Equal("/status/support-reference", root.GetProperty("supportReferencePath").GetString());
        Assert.Single(root.GetProperty("authorizedRoles").EnumerateArray());
    }

    [Fact]
    public void Rejects_a_missing_active_role_when_role_selection_is_not_required()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new FrontendAppContextView(
                Now,
                "Africa/Cairo",
                "Summer 2026",
                "Summer 2026",
                Now.AddDays(-1),
                Now.AddDays(7),
                "Ahmed Student",
                ["Student"],
                activeRole: null,
                roleSelectionRequired: false,
                "active",
                Now.AddHours(1),
                "available",
                "/status/support-reference"));

        Assert.Equal("activeRole", exception.ParamName);
    }
}
