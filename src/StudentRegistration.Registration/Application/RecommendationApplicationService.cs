using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using StudentRegistration.Contracts.Registration;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.Registration.Application;

public enum RecommendationApplyOutcome
{
    Updated,
    InvalidOptionToken,
    ContextNotFound,
    OptionExpired,
    PlanChanged,
    StaleInput,
    Unavailable
}

public sealed record RecommendationApplyResult(
    RecommendationApplyOutcome Outcome,
    RegistrationPlanView? Plan = null,
    string? SafeCode = null);

public sealed record RecommendationOptionDescriptor(
    Guid StudentId,
    Guid TermId,
    Guid PlanId,
    string PlanRowVersion,
    IReadOnlyList<ScheduleOptionSelection> Selections,
    string AcademicContextVersion,
    string CatalogueVersion,
    Guid PolicySetId,
    string PolicyVersion,
    IReadOnlyDictionary<string, string> OfferingVersions,
    IReadOnlyDictionary<string, string> GroupVersions,
    string OptimizerConfigurationVersion,
    string RequestCorrelationId);

public sealed class RecommendationApplicationService
{
    private const string ProtectionPurpose = "Registration.ScheduleOption.v1";
    private const int MaximumOptionSelections = 100;
    private const int MaximumOptionTokenLength = 64 * 1024;
    private static readonly TimeSpan OptionLifetime = TimeSpan.FromMinutes(10);
    private readonly IDataProtector _protector;
    private readonly IRecommendationPlanWriter _writer;
    private readonly TimeProvider _timeProvider;

    public RecommendationApplicationService(
        IDataProtectionProvider dataProtectionProvider,
        IRecommendationPlanWriter writer,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(dataProtectionProvider);
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(timeProvider);
        _protector = dataProtectionProvider.CreateProtector(ProtectionPurpose);
        _writer = writer;
        _timeProvider = timeProvider;
    }

    public string IssueOptionToken(RecommendationOptionDescriptor descriptor)
    {
        Validate(descriptor);
        var issuedAtUtc = _timeProvider.GetUtcNow();
        var payload = new ProtectedOptionPayload(
            descriptor.StudentId,
            descriptor.TermId,
            descriptor.PlanId,
            descriptor.PlanRowVersion.Trim(),
            descriptor.Selections
                .OrderBy(selection => selection.CourseId)
                .ThenBy(selection => selection.GroupId)
                .ToArray(),
            descriptor.AcademicContextVersion.Trim(),
            descriptor.CatalogueVersion.Trim(),
            descriptor.PolicySetId,
            descriptor.PolicyVersion.Trim(),
            Ordered(descriptor.OfferingVersions),
            Ordered(descriptor.GroupVersions),
            descriptor.OptimizerConfigurationVersion.Trim(),
            descriptor.RequestCorrelationId.Trim(),
            issuedAtUtc,
            issuedAtUtc.Add(OptionLifetime));
        return _protector.Protect(JsonSerializer.Serialize(payload));
    }

    public Task<RecommendationApplyResult> ApplyAsync(
        Guid authenticatedStudentId,
        Guid termId,
        ApplyScheduleOptionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ApplyAsync(
            authenticatedStudentId,
            termId,
            request.OptionToken,
            request.ExpectedPlanRowVersion,
            request.RequestCorrelationId,
            cancellationToken);
    }

    public async Task<RecommendationApplyResult> ApplyAsync(
        Guid authenticatedStudentId,
        Guid termId,
        string optionToken,
        string expectedPlanRowVersion,
        string requestCorrelationId,
        CancellationToken cancellationToken = default)
    {
        if (authenticatedStudentId == Guid.Empty ||
            termId == Guid.Empty ||
            string.IsNullOrWhiteSpace(optionToken) ||
            optionToken.Length > MaximumOptionTokenLength ||
            string.IsNullOrWhiteSpace(expectedPlanRowVersion) ||
            string.IsNullOrWhiteSpace(requestCorrelationId))
        {
            return Failure(
                RecommendationApplyOutcome.InvalidOptionToken,
                "INVALID_OPTION_TOKEN");
        }

        ProtectedOptionPayload payload;
        try
        {
            payload = JsonSerializer.Deserialize<ProtectedOptionPayload>(
                    _protector.Unprotect(optionToken))
                ?? throw new JsonException("The protected option is empty.");
            Validate(payload);
        }
        catch (Exception exception)
            when (exception is CryptographicException or JsonException or
                ArgumentException)
        {
            return Failure(
                RecommendationApplyOutcome.InvalidOptionToken,
                "INVALID_OPTION_TOKEN");
        }

        var now = _timeProvider.GetUtcNow();
        if (payload.IssuedAtUtc > now.AddMinutes(1) ||
            payload.ExpiresAtUtc - payload.IssuedAtUtc > OptionLifetime)
        {
            return Failure(
                RecommendationApplyOutcome.InvalidOptionToken,
                "INVALID_OPTION_TOKEN");
        }

        if (payload.ExpiresAtUtc <= now)
        {
            return Failure(
                RecommendationApplyOutcome.OptionExpired,
                "OPTION_EXPIRED");
        }

        if (payload.StudentId != authenticatedStudentId ||
            payload.TermId != termId ||
            payload.PlanId == Guid.Empty)
        {
            return Failure(
                RecommendationApplyOutcome.ContextNotFound,
                "REGISTRATION_CONTEXT_NOT_FOUND");
        }

        if (!string.Equals(
                payload.RequestCorrelationId,
                requestCorrelationId.Trim(),
                StringComparison.Ordinal))
        {
            return Failure(
                RecommendationApplyOutcome.InvalidOptionToken,
                "INVALID_OPTION_TOKEN");
        }

        if (!string.Equals(
                payload.PlanRowVersion,
                expectedPlanRowVersion.Trim(),
                StringComparison.Ordinal))
        {
            return Failure(
                RecommendationApplyOutcome.PlanChanged,
                "PLAN_CHANGED");
        }

        var stored = await _writer.ReplaceAsync(
            new(
                payload.StudentId,
                payload.TermId,
                payload.PlanId,
                payload.PlanRowVersion,
                payload.Selections,
                payload.AcademicContextVersion,
                payload.CatalogueVersion,
                payload.PolicySetId,
                payload.PolicyVersion,
                payload.OfferingVersions,
                payload.GroupVersions,
                payload.OptimizerConfigurationVersion),
            cancellationToken);
        return stored.Outcome switch
        {
            RecommendationPlanWriteOutcome.Updated when stored.Plan is not null =>
                new(RecommendationApplyOutcome.Updated, stored.Plan),
            RecommendationPlanWriteOutcome.Updated =>
                Failure(
                    RecommendationApplyOutcome.Unavailable,
                    "RECOMMENDATIONS_UNAVAILABLE"),
            RecommendationPlanWriteOutcome.PlanChanged =>
                Failure(RecommendationApplyOutcome.PlanChanged, "PLAN_CHANGED"),
            RecommendationPlanWriteOutcome.StaleInput or
            RecommendationPlanWriteOutcome.InvalidSelection =>
                Failure(RecommendationApplyOutcome.StaleInput, "STALE_INPUT"),
            _ => Failure(
                RecommendationApplyOutcome.Unavailable,
                "RECOMMENDATIONS_UNAVAILABLE")
        };
    }

    private static RecommendationApplyResult Failure(
        RecommendationApplyOutcome outcome,
        string safeCode) =>
        new(outcome, SafeCode: safeCode);

    private static SortedDictionary<string, string> Ordered(
        IReadOnlyDictionary<string, string> versions)
    {
        ArgumentNullException.ThrowIfNull(versions);
        if (versions.Any(pair =>
                string.IsNullOrWhiteSpace(pair.Key) ||
                string.IsNullOrWhiteSpace(pair.Value)))
        {
            throw new ArgumentException(
                "Every dependency version must have a non-empty key and value.",
                nameof(versions));
        }

        var ordered = new SortedDictionary<string, string>(
            StringComparer.Ordinal);
        foreach (var pair in versions)
        {
            ordered.Add(pair.Key, pair.Value);
        }

        return ordered;
    }

    private static void Validate(RecommendationOptionDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (descriptor.StudentId == Guid.Empty ||
            descriptor.TermId == Guid.Empty ||
            descriptor.PlanId == Guid.Empty ||
            descriptor.PolicySetId == Guid.Empty ||
            descriptor.Selections is null ||
            descriptor.Selections.Count == 0 ||
            descriptor.Selections.Count > MaximumOptionSelections ||
            descriptor.Selections.Any(selection => selection is null) ||
            string.IsNullOrWhiteSpace(descriptor.PlanRowVersion) ||
            string.IsNullOrWhiteSpace(descriptor.AcademicContextVersion) ||
            string.IsNullOrWhiteSpace(descriptor.CatalogueVersion) ||
            string.IsNullOrWhiteSpace(descriptor.PolicyVersion) ||
            string.IsNullOrWhiteSpace(descriptor.OptimizerConfigurationVersion) ||
            string.IsNullOrWhiteSpace(descriptor.RequestCorrelationId))
        {
            throw new ArgumentException(
                "A complete version-bound recommendation descriptor is required.",
                nameof(descriptor));
        }

        _ = Ordered(descriptor.OfferingVersions);
        _ = Ordered(descriptor.GroupVersions);
        var uniqueSelections = descriptor.Selections
            .Select(selection =>
                (selection.CourseId, selection.OfferingId, selection.GroupId))
            .Distinct()
            .Count();
        if (uniqueSelections != descriptor.Selections.Count ||
            descriptor.Selections.Select(selection => selection.CourseId)
                .Distinct()
                .Count() != descriptor.Selections.Count ||
            descriptor.Selections.Select(selection => selection.OfferingId)
                .Distinct()
                .Count() != descriptor.Selections.Count ||
            descriptor.Selections.Select(selection => selection.GroupId)
                .Distinct()
                .Count() != descriptor.Selections.Count ||
            descriptor.Selections.Any(selection =>
                !descriptor.OfferingVersions.ContainsKey(
                    selection.OfferingId.ToString("N")) ||
                !descriptor.GroupVersions.ContainsKey(
                    selection.GroupId.ToString("N"))))
        {
            throw new ArgumentException(
                "Every complete option selection must have unique identifiers and bound offering/group versions.",
                nameof(descriptor));
        }
    }

    private static void Validate(ProtectedOptionPayload payload)
    {
        if (payload.StudentId == Guid.Empty ||
            payload.TermId == Guid.Empty ||
            payload.PlanId == Guid.Empty ||
            payload.PolicySetId == Guid.Empty ||
            payload.Selections is null ||
            payload.Selections.Count == 0 ||
            payload.OfferingVersions is null ||
            payload.GroupVersions is null ||
            string.IsNullOrWhiteSpace(payload.PlanRowVersion) ||
            string.IsNullOrWhiteSpace(payload.AcademicContextVersion) ||
            string.IsNullOrWhiteSpace(payload.CatalogueVersion) ||
            string.IsNullOrWhiteSpace(payload.PolicyVersion) ||
            string.IsNullOrWhiteSpace(payload.OptimizerConfigurationVersion) ||
            string.IsNullOrWhiteSpace(payload.RequestCorrelationId) ||
            payload.ExpiresAtUtc <= payload.IssuedAtUtc)
        {
            throw new ArgumentException("The protected option payload is invalid.");
        }

        Validate(
            new RecommendationOptionDescriptor(
                payload.StudentId,
                payload.TermId,
                payload.PlanId,
                payload.PlanRowVersion,
                payload.Selections,
                payload.AcademicContextVersion,
                payload.CatalogueVersion,
                payload.PolicySetId,
                payload.PolicyVersion,
                payload.OfferingVersions,
                payload.GroupVersions,
                payload.OptimizerConfigurationVersion,
                payload.RequestCorrelationId));
    }

    private sealed record ProtectedOptionPayload(
        Guid StudentId,
        Guid TermId,
        Guid PlanId,
        string PlanRowVersion,
        IReadOnlyList<ScheduleOptionSelection> Selections,
        string AcademicContextVersion,
        string CatalogueVersion,
        Guid PolicySetId,
        string PolicyVersion,
        IReadOnlyDictionary<string, string> OfferingVersions,
        IReadOnlyDictionary<string, string> GroupVersions,
        string OptimizerConfigurationVersion,
        string RequestCorrelationId,
        DateTimeOffset IssuedAtUtc,
        DateTimeOffset ExpiresAtUtc);
}
