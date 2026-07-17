using StudentRegistration.Registration.Domain;

namespace StudentRegistration.Registration.Application.Ports;

public enum RecommendationPlanWriteOutcome
{
    Updated,
    PlanChanged,
    StaleInput,
    InvalidSelection,
    Unavailable
}

public sealed record RecommendationPlanReplacement(
    Guid StudentId,
    Guid TermId,
    Guid PlanId,
    string ExpectedPlanRowVersion,
    IReadOnlyList<ScheduleOptionSelection> Selections,
    string AcademicContextVersion,
    string CatalogueVersion,
    Guid PolicySetId,
    string PolicyVersion,
    IReadOnlyDictionary<string, string> OfferingVersions,
    IReadOnlyDictionary<string, string> GroupVersions,
    string OptimizerConfigurationVersion);

public sealed record RecommendationPlanWriteResult(
    RecommendationPlanWriteOutcome Outcome,
    RegistrationPlanView? Plan = null);

public interface IRecommendationPlanWriter
{
    Task<RecommendationPlanWriteResult> ReplaceAsync(
        RecommendationPlanReplacement replacement,
        CancellationToken cancellationToken = default);
}
