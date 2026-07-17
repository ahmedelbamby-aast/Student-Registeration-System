using StudentRegistration.Registration.Application;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Registration;

public sealed class RegistrationConflictTests
{
    [Theory]
    [InlineData(RegistrationConflictReason.GroupFull, "GROUP_FULL", false)]
    [InlineData(RegistrationConflictReason.PlanChanged, "PLAN_CHANGED", true)]
    [InlineData(RegistrationConflictReason.PolicyChanged, "POLICY_CHANGED", true)]
    [InlineData(RegistrationConflictReason.ScheduleConflict, "SCHEDULE_CONFLICT", false)]
    [InlineData(RegistrationConflictReason.GroupChanged, "GROUP_CHANGED", true)]
    [InlineData(RegistrationConflictReason.WindowChanged, "WINDOW_CHANGED", true)]
    [InlineData(RegistrationConflictReason.IdempotencyKeyReused, "IDEMPOTENCY_KEY_REUSED", false)]
    public void Expected_business_conflicts_are_stable_409_results(
        RegistrationConflictReason reason,
        string expectedCode,
        bool carriesCurrentVersion)
    {
        var mapper = new RegistrationConflictMapper();

        var result = mapper.Map(
            reason,
            correlationId: "correlation-014",
            currentVersion: carriesCurrentVersion ? "version-2" : null);

        Assert.Equal(409, result.StatusCode);
        Assert.Equal(expectedCode, result.Error.Code);
        Assert.Equal("correlation-014", result.Error.CorrelationId);
        Assert.Equal(
            carriesCurrentVersion ? "version-2" : null,
            result.Error.CurrentVersion);
        Assert.False(result.Mutated);
    }

    [Fact]
    public void Mvp_exposes_no_drop_withdrawal_correction_or_seat_decrement_route()
    {
        var endpointFiles = Directory.Exists(RepositoryFiles.PathTo("src"))
            ? Directory.GetFiles(
                RepositoryFiles.PathTo("src"),
                "*Endpoints.cs",
                SearchOption.AllDirectories)
            : [];
        var endpointSource = string.Join(
            "\n",
            endpointFiles.Select(File.ReadAllText));

        Assert.DoesNotContain("/drop", endpointSource, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/withdraw", endpointSource, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/correction", endpointSource, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("seat-decrement", endpointSource, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DecrementSeat", endpointSource, StringComparison.Ordinal);
    }
}
