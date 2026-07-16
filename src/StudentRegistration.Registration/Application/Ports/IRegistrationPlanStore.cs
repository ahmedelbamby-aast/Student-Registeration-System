using StudentRegistration.Registration.Domain;

namespace StudentRegistration.Registration.Application.Ports;

public enum RegistrationPlanStoreOutcome
{
    Updated,
    StaleVersion,
    StorageUnavailable
}

public sealed record RegistrationPlanStoreCommand(
    Guid StudentId,
    Guid TermId,
    string ExpectedRowVersion,
    IReadOnlyList<RegistrationPlanSelection> Selections,
    decimal TotalCredits,
    RegistrationPlanState State,
    IReadOnlyList<ScheduleConflict> Conflicts,
    ValidationSnapshot Validation);

public sealed record RegistrationPlanStoreResult(
    RegistrationPlanStoreOutcome Outcome,
    RegistrationPlan? Plan = null);

public interface IRegistrationPlanStore
{
    Task<RegistrationPlan?> ReadAsync(
        Guid studentId,
        Guid termId,
        CancellationToken cancellationToken = default);

    Task<RegistrationPlanStoreResult> ReplaceAsync(
        RegistrationPlanStoreCommand command,
        CancellationToken cancellationToken = default);
}
