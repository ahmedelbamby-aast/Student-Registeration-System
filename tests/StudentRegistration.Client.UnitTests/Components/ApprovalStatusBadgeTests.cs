using Bunit;
using StudentRegistration.Client.Components.Feedback;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class ApprovalStatusBadgeTests
{
    [Theory]
    [InlineData(ApprovalStatusBadge.ApprovalState.NotRequested, "neutral")]
    [InlineData(ApprovalStatusBadge.ApprovalState.PendingApproval, "pending")]
    [InlineData(ApprovalStatusBadge.ApprovalState.Approved, "success")]
    [InlineData(ApprovalStatusBadge.ApprovalState.Rejected, "danger")]
    [InlineData(ApprovalStatusBadge.ApprovalState.ExpiredReleased, "warning")]
    [InlineData(ApprovalStatusBadge.ApprovalState.Registered, "information")]
    public void Maps_approval_state_to_semantic_tone_without_inventing_display_text(
        ApprovalStatusBadge.ApprovalState state,
        string expectedTone)
    {
        using var context = new BunitContext();

        var cut = context.Render<ApprovalStatusBadge>(parameters => parameters
            .Add(component => component.State, state)
            .Add(component => component.Text, "Caller supplied label"));

        Assert.Equal(ToKebabCase(state), cut.Find("[data-approval-state]").GetAttribute("data-approval-state"));
        Assert.Contains($"srs-status-badge--{expectedTone}", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Caller supplied label", cut.Markup, StringComparison.Ordinal);
    }

    private static string ToKebabCase(ApprovalStatusBadge.ApprovalState state) => state switch
    {
        ApprovalStatusBadge.ApprovalState.NotRequested => "not-requested",
        ApprovalStatusBadge.ApprovalState.PendingApproval => "pending-approval",
        ApprovalStatusBadge.ApprovalState.Approved => "approved",
        ApprovalStatusBadge.ApprovalState.Rejected => "rejected",
        ApprovalStatusBadge.ApprovalState.ExpiredReleased => "expired-released",
        ApprovalStatusBadge.ApprovalState.Registered => "registered",
        _ => throw new ArgumentOutOfRangeException(nameof(state))
    };
}
