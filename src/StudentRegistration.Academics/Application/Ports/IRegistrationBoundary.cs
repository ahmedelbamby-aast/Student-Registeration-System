namespace StudentRegistration.Academics.Application.Ports;

public enum RegistrationBoundaryOutcome
{
    Committed,
    HoldBlocked,
    StaleVersion,
    NotFound,
    ProfileNotReady,
    StorageUnavailable
}

public sealed record RegistrationBoundaryResult(RegistrationBoundaryOutcome Outcome)
{
    public string? ErrorCode => Outcome switch
    {
        RegistrationBoundaryOutcome.HoldBlocked => "HOLD_BLOCKED",
        RegistrationBoundaryOutcome.StaleVersion => "STALE_VERSION",
        RegistrationBoundaryOutcome.NotFound => "PROFILE_NOT_FOUND",
        RegistrationBoundaryOutcome.ProfileNotReady => "PROFILE_NOT_READY",
        RegistrationBoundaryOutcome.StorageUnavailable => "CONTEXT_UNAVAILABLE",
        _ => null
    };
}

/// <summary>
/// Exposes the canonical Academics-owned student/term serialization boundary
/// without coupling consuming modules to an Academics application service.
/// </summary>
public interface IRegistrationBoundary
{
    Task<RegistrationBoundaryResult> ExecuteRegistrationBoundaryAsync(
        Guid studentId,
        Guid termId,
        string expectedStudentTermStateRowVersion,
        Func<CancellationToken, Task> commitCallback,
        CancellationToken cancellationToken = default);
}
