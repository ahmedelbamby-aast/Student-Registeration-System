using Microsoft.AspNetCore.DataProtection;
using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Registration;

public sealed class RecommendationVersionTests
{
    [Fact]
    public async Task Shared_provider_allows_another_replica_to_apply_without_option_storage()
    {
        var provider = new EphemeralDataProtectionProvider();
        var writer = new FakeWriter();
        var issuer = Service(provider, writer, Utc(12, 0));
        var replica = Service(provider, writer, Utc(12, 5));
        var descriptor = Descriptor();
        var token = issuer.IssueOptionToken(descriptor);

        var result = await replica.ApplyAsync(
            descriptor.StudentId,
            descriptor.TermId,
            token,
            descriptor.PlanRowVersion,
            descriptor.RequestCorrelationId);

        Assert.Equal(RecommendationApplyOutcome.Updated, result.Outcome);
        Assert.Equal(descriptor.Selections, writer.LastCommand!.Selections);
        Assert.Equal(1, writer.CallCount);
    }

    [Fact]
    public async Task Tamper_cross_owner_and_expiry_fail_without_mutation()
    {
        var provider = new EphemeralDataProtectionProvider();
        var writer = new FakeWriter();
        var issuer = Service(provider, writer, Utc(12, 0));
        var descriptor = Descriptor();
        var token = issuer.IssueOptionToken(descriptor);

        var tampered = await issuer.ApplyAsync(
            descriptor.StudentId,
            descriptor.TermId,
            token[..^1] + (token[^1] == 'A' ? "B" : "A"),
            descriptor.PlanRowVersion,
            descriptor.RequestCorrelationId);
        var wrongOwner = await issuer.ApplyAsync(
            Id(99),
            descriptor.TermId,
            token,
            descriptor.PlanRowVersion,
            descriptor.RequestCorrelationId);
        var expired = await Service(provider, writer, Utc(12, 11)).ApplyAsync(
            descriptor.StudentId,
            descriptor.TermId,
            token,
            descriptor.PlanRowVersion,
            descriptor.RequestCorrelationId);

        Assert.Equal("INVALID_OPTION_TOKEN", tampered.SafeCode);
        Assert.Equal("REGISTRATION_CONTEXT_NOT_FOUND", wrongOwner.SafeCode);
        Assert.Equal("OPTION_EXPIRED", expired.SafeCode);
        Assert.Equal(0, writer.CallCount);
    }

    [Fact]
    public async Task Stale_plan_and_dependency_versions_fail_without_mutation()
    {
        var provider = new EphemeralDataProtectionProvider();
        var descriptor = Descriptor();
        var token = Service(provider, new FakeWriter(), Utc(12, 0))
            .IssueOptionToken(descriptor);
        var planWriter = new FakeWriter(
            RecommendationPlanWriteOutcome.PlanChanged);
        var dependencyWriter = new FakeWriter(
            RecommendationPlanWriteOutcome.StaleInput);

        var stalePlan = await Service(provider, planWriter, Utc(12, 1)).ApplyAsync(
            descriptor.StudentId,
            descriptor.TermId,
            token,
            descriptor.PlanRowVersion,
            descriptor.RequestCorrelationId);
        var staleInput = await Service(provider, dependencyWriter, Utc(12, 1))
            .ApplyAsync(
                descriptor.StudentId,
                descriptor.TermId,
                token,
                descriptor.PlanRowVersion,
                descriptor.RequestCorrelationId);

        Assert.Equal("PLAN_CHANGED", stalePlan.SafeCode);
        Assert.Equal("STALE_INPUT", staleInput.SafeCode);
        Assert.Null(stalePlan.Plan);
        Assert.Null(staleInput.Plan);
    }

    [Fact]
    public async Task Apply_passes_every_bound_version_to_one_atomic_writer_call()
    {
        var provider = new EphemeralDataProtectionProvider();
        var descriptor = Descriptor();
        var writer = new FakeWriter();
        var service = Service(provider, writer, Utc(12, 0));
        var token = service.IssueOptionToken(descriptor);

        await service.ApplyAsync(
            descriptor.StudentId,
            descriptor.TermId,
            token,
            descriptor.PlanRowVersion,
            descriptor.RequestCorrelationId);

        var command = Assert.IsType<RecommendationPlanReplacement>(
            writer.LastCommand);
        Assert.Equal(descriptor.PlanId, command.PlanId);
        Assert.Equal(descriptor.PlanRowVersion, command.ExpectedPlanRowVersion);
        Assert.Equal(descriptor.AcademicContextVersion, command.AcademicContextVersion);
        Assert.Equal(descriptor.CatalogueVersion, command.CatalogueVersion);
        Assert.Equal(descriptor.PolicySetId, command.PolicySetId);
        Assert.Equal(descriptor.PolicyVersion, command.PolicyVersion);
        Assert.Equal(descriptor.OfferingVersions, command.OfferingVersions);
        Assert.Equal(descriptor.GroupVersions, command.GroupVersions);
        Assert.Equal(
            descriptor.OptimizerConfigurationVersion,
            command.OptimizerConfigurationVersion);
        Assert.Equal(1, writer.CallCount);
    }

    private static RecommendationApplicationService Service(
        IDataProtectionProvider provider,
        IRecommendationPlanWriter writer,
        DateTimeOffset utcNow) =>
        new(provider, writer, new FixedTimeProvider(utcNow));

    private static RecommendationOptionDescriptor Descriptor() =>
        new(
            Id(1),
            Id(2),
            Id(3),
            "plan-version-5",
            [
                new ScheduleOptionSelection(Id(11), Id(12), Id(13)),
                new ScheduleOptionSelection(Id(21), Id(22), Id(23))
            ],
            "academic-v1",
            "catalogue-v1",
            Id(4),
            "policy-v1",
            new Dictionary<string, string>
            {
                [Id(12).ToString("N")] = "offering-v1",
                [Id(22).ToString("N")] = "offering-v2"
            },
            new Dictionary<string, string>
            {
                [Id(13).ToString("N")] = "group-v1",
                [Id(23).ToString("N")] = "group-v2"
            },
            "1.0.0",
            "request-0001");

    private static DateTimeOffset Utc(int hour, int minute) =>
        new(2026, 7, 17, hour, minute, 0, TimeSpan.Zero);

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:D12}");

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class FakeWriter(
        RecommendationPlanWriteOutcome outcome =
            RecommendationPlanWriteOutcome.Updated)
        : IRecommendationPlanWriter
    {
        public int CallCount { get; private set; }

        public RecommendationPlanReplacement? LastCommand { get; private set; }

        public Task<RecommendationPlanWriteResult> ReplaceAsync(
            RecommendationPlanReplacement replacement,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastCommand = replacement;
            return Task.FromResult(new RecommendationPlanWriteResult(
                outcome,
                outcome == RecommendationPlanWriteOutcome.Updated
                    ? Plan()
                    : null));
        }

        private static RegistrationPlanView Plan() =>
            new(
                Id(3),
                Id(2),
                "plan-version-6",
                [],
                18m,
                18m,
                18m,
                [],
                [],
                [],
                new ValidationSnapshot(
                    Utc(12, 0).UtcDateTime,
                    "academic-v1",
                    "policy-v1",
                    "catalogue-v1",
                    new Dictionary<Guid, string>(),
                    new Dictionary<Guid, string>()),
                false);
    }
}
