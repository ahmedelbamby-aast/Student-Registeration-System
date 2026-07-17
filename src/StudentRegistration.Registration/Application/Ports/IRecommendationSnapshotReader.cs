using StudentRegistration.Registration.Domain;

namespace StudentRegistration.Registration.Application.Ports;

public enum RecommendationSnapshotOutcome
{
    Captured,
    NotFound,
    PlanChanged,
    StaleInput,
    Unavailable
}

public sealed record RecommendationSnapshot(
    Guid StudentId,
    Guid TermId,
    Guid PlanId,
    string PlanRowVersion,
    string AcademicContextVersion,
    string CatalogueVersion,
    Guid PolicySetId,
    string PolicyVersion,
    IReadOnlyDictionary<string, string> OfferingVersions,
    IReadOnlyDictionary<string, string> GroupVersions,
    decimal RequiredCredits,
    IReadOnlyList<Guid> SelectedCourseIds,
    IReadOnlyList<Guid> SelectedOfferingIds,
    IReadOnlyList<ScheduleCandidateGroup> Candidates,
    IReadOnlyList<RegistrationPlanGroupSnapshot> Groups);

public sealed record RecommendationSnapshotResult(
    RecommendationSnapshotOutcome Outcome,
    RecommendationSnapshot? Snapshot = null);

public interface IRecommendationSnapshotReader
{
    Task<RecommendationSnapshotResult> ReadAsync(
        Guid studentId,
        Guid termId,
        string expectedPlanRowVersion,
        DateTime evaluatedAtUtc,
        CancellationToken cancellationToken = default);
}
