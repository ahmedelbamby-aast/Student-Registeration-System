using System.Text.Json;
using StudentRegistration.Client.Features.Frontend.Models;

namespace StudentRegistration.IntegrationTests.Specs.Spec003;

public sealed class UiStatusModelTests
{
    [Fact]
    public void Serializes_stable_status_content_actions_and_reference()
    {
        var model = new UiStatus(
            "SERVICE_UNAVAILABLE",
            "Registration is temporarily unavailable",
            "Try again after the service recovers.",
            UiStatusSeverity.Warning,
            [new UiStatusAction("retry", "Try again", "/status/retry")],
            "REF-20260713-001");

        using var document = JsonDocument.Parse(
            JsonSerializer.Serialize(model, JsonSerializerOptions.Web));
        var root = document.RootElement;

        Assert.Equal("SERVICE_UNAVAILABLE", root.GetProperty("code").GetString());
        Assert.Equal(JsonValueKind.String, root.GetProperty("severity").ValueKind);
        Assert.Equal("REF-20260713-001", root.GetProperty("referenceId").GetString());
        Assert.Equal("retry", root.GetProperty("nextActions")[0].GetProperty("actionId").GetString());
    }

    [Fact]
    public void Rejects_a_blank_stable_code()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new UiStatus(
                " ",
                "Heading",
                "Message",
                UiStatusSeverity.Error,
                [],
                null));

        Assert.Equal("code", exception.ParamName);
    }
}
