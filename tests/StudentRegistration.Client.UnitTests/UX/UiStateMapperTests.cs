using StudentRegistration.Client.Features.Frontend.Models;
using StudentRegistration.Client.UX;

namespace StudentRegistration.Client.UnitTests.UX;

public sealed class UiStateMapperTests
{
    private readonly UiStateMapper _mapper = new(new TestUiTextProvider());

    public static TheoryData<RouteUiState, bool> ExplicitStates => new()
    {
        { RouteUiState.Loading, false },
        { RouteUiState.Empty, false },
        { RouteUiState.Success, true },
        { RouteUiState.ValidationError, false },
        { RouteUiState.ServiceError, false },
        { RouteUiState.Unauthorized, false },
        { RouteUiState.SessionExpired, false },
        { RouteUiState.Stale, false },
        { RouteUiState.Offline, false }
    };

    [Theory]
    [MemberData(nameof(ExplicitStates))]
    public void Maps_every_explicit_route_state(RouteUiState state, bool serverAccepted)
    {
        var result = _mapper.Map(new UiStateInput(
            state,
            serverAccepted,
            reasonCode: null,
            referenceId: "REF-001",
            nextActions: []));

        Assert.Equal(state, result.State);
        Assert.Equal("REF-001", result.Status.ReferenceId);
        Assert.NotEmpty(result.Status.Heading);
        Assert.NotEmpty(result.Status.Message);
    }

    [Fact]
    public void Unknown_server_reason_uses_safe_fallback_but_preserves_code_reference_and_action()
    {
        var result = _mapper.Map(new UiStateInput(
            RouteUiState.Success,
            serverAccepted: false,
            "FUTURE_SERVER_REASON",
            "REF-UNKNOWN",
            [new UiStatusAction("support", "Support", "/status/support") ]));

        Assert.Equal(RouteUiState.ServiceError, result.State);
        Assert.Equal("FUTURE_SERVER_REASON", result.Status.Code);
        Assert.Equal("REF-UNKNOWN", result.Status.ReferenceId);
        Assert.Equal("[Ui.State.UnknownReason.Heading]", result.Status.Heading);
        Assert.Equal("[Ui.State.UnknownReason.Message]", result.Status.Message);
        Assert.Single(result.Status.NextActions);
    }

    [Theory]
    [InlineData("PREREQUISITE_NOT_COMPLETED", RouteUiState.ValidationError)]
    [InlineData("MEETING_CONFLICT", RouteUiState.ValidationError)]
    [InlineData("UNAUTHORIZED", RouteUiState.Unauthorized)]
    [InlineData("SESSION_EXPIRED", RouteUiState.SessionExpired)]
    [InlineData("STALE_VERSION", RouteUiState.Stale)]
    [InlineData("PLAN_CHANGED", RouteUiState.Stale)]
    [InlineData("WINDOW_CLOSED", RouteUiState.Stale)]
    [InlineData("GROUP_FULL", RouteUiState.Stale)]
    [InlineData("SERVICE_UNAVAILABLE", RouteUiState.ServiceError)]
    public void Preserves_stable_server_reasons_and_maps_their_safe_state(
        string reasonCode,
        RouteUiState expectedState)
    {
        var result = _mapper.Map(new UiStateInput(
            RouteUiState.Success,
            serverAccepted: false,
            reasonCode,
            "REF-AUTHORITY",
            []));

        Assert.Equal(expectedState, result.State);
        Assert.NotEqual(RouteUiState.Success, result.State);
        Assert.Equal(reasonCode, result.Status.Code);
        Assert.Equal("REF-AUTHORITY", result.Status.ReferenceId);
    }

    [Fact]
    public void Rejected_server_result_can_never_be_converted_to_client_success()
    {
        var result = _mapper.Map(new UiStateInput(
            RouteUiState.Success,
            serverAccepted: false,
            "GROUP_FULL",
            "REF-RACE-LOSER",
            []));

        Assert.False(result.ServerAccepted);
        Assert.Equal(RouteUiState.Stale, result.State);
        Assert.Equal(UiStatusSeverity.Warning, result.Status.Severity);
    }

    private sealed class TestUiTextProvider : IUiTextProvider
    {
        public string Get(string key) => $"[{key}]";
    }
}
