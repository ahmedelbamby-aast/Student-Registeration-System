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

    [Theory]
    [InlineData(18, null, "normal", "18-credit normal registration limit", "Current total: 18 credits")]
    [InlineData(19, 3.00, "overload", "19 credits of maximum 21", "Current CGPA: 3.00; required CGPA: 3.00")]
    [InlineData(21, 3.25, "overload", "21 credits of maximum 21", "Approval is required and is not implied")]
    public void Presents_normal_and_overload_limits_with_exact_non_implying_values(
        int credits,
        double? cgpa,
        string expectedKind,
        string firstExpectedText,
        string secondExpectedText)
    {
        using var context = new BunitContext();
        var cut = context.Render<ApprovalStatusBadge>(parameters =>
        {
            parameters.Add(component => component.State, ApprovalStatusBadge.ApprovalState.PendingApproval)
                .Add(component => component.Text, "Pending approval")
                .Add(component => component.CurrentCredits, credits);
            if (cgpa is not null)
            {
                parameters.Add(component => component.CurrentCgpa, (decimal)cgpa.Value);
            }
        });

        var load = cut.Find("[data-credit-load]");
        Assert.Equal(expectedKind, load.GetAttribute("data-credit-load"));
        Assert.Contains(firstExpectedText, load.TextContent, StringComparison.Ordinal);
        Assert.Contains(secondExpectedText, load.TextContent, StringComparison.Ordinal);
    }
}
