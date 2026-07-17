using StudentRegistration.Contracts;
using Microsoft.AspNetCore.Http;

namespace StudentRegistration.Registration.Application;

public enum RegistrationConflictReason
{
    GroupFull = 1,
    PlanChanged = 2,
    PolicyChanged = 3,
    ScheduleConflict = 4,
    GroupChanged = 5,
    WindowChanged = 6,
    IdempotencyKeyReused = 7,
}

public sealed record RegistrationConflictResult(
    int StatusCode,
    ApiError Error,
    bool Mutated)
{
    public string? CurrentVersion => Error.CurrentVersion;
}

/// <summary>
/// Converts expected registration conflicts into the stable public 409 shape.
/// It performs no mutation and exposes a current version only when supplied.
/// </summary>
public sealed class RegistrationConflictMapper
{
    public RegistrationConflictResult Map(
        RegistrationConflictReason reason,
        string correlationId,
        string? currentVersion = null)
    {
        var (code, message) = reason switch
        {
            RegistrationConflictReason.GroupFull =>
                ("GROUP_FULL", "A selected group no longer has an available seat."),
            RegistrationConflictReason.PlanChanged =>
                ("PLAN_CHANGED", "The registration plan changed."),
            RegistrationConflictReason.PolicyChanged =>
                ("POLICY_CHANGED", "The governing registration policy changed."),
            RegistrationConflictReason.ScheduleConflict =>
                ("SCHEDULE_CONFLICT", "The final schedule contains a conflict."),
            RegistrationConflictReason.GroupChanged =>
                ("GROUP_CHANGED", "A selected group changed."),
            RegistrationConflictReason.WindowChanged =>
                ("WINDOW_CHANGED", "The registration window changed."),
            RegistrationConflictReason.IdempotencyKeyReused =>
                ("IDEMPOTENCY_KEY_REUSED", "The request key was reused with a different payload."),
            _ => throw new ArgumentOutOfRangeException(nameof(reason))
        };

        return new(
            StatusCode: StatusCodes.Status409Conflict,
            Error: new ApiError(
                code,
                message,
                correlationId,
                currentVersion: currentVersion),
            Mutated: false);
    }
}
