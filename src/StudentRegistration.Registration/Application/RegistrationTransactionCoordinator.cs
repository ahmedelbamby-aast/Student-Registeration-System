using StudentRegistration.Academics.Application.Ports;

namespace StudentRegistration.Registration.Application;

public enum RegistrationPublicationScope
{
    Catalogue,
    Policy
}

public enum RegistrationMutableInput
{
    RegistrationWindow,
    StudentProfile,
    StudentHolds,
    CatalogueVersion,
    PolicySetVersion,
    Eligibility,
    CreditLoad,
    DuplicateCourses,
    SectionGroupStateAndMeetingVersions,
    Timetable,
    CommitBoundaryVersions
}

public sealed record RegistrationTransactionPlan(
    Guid StudentId,
    Guid TermId,
    string ExpectedStudentTermStateRowVersion,
    Guid PlanId,
    string ExpectedPlanRowVersion,
    string ExpectedRegistrationContextVersion,
    DateTime ReceivedAtUtc,
    string CatalogueScopeCode,
    string PolicyScopeCode,
    IReadOnlyList<Guid> GroupIds,
    IReadOnlyList<Guid>? BoundaryGroupIds = null);

public sealed record RegistrationFinalValidation(
    Guid StudentId,
    Guid TermId,
    Guid PlanId,
    string ExpectedPlanRowVersion,
    string ExpectedRegistrationContextVersion,
    DateTime ReceivedAtUtc,
    string CatalogueScopeCode,
    string PolicyScopeCode,
    IReadOnlyList<Guid> GroupIds,
    IReadOnlyList<RegistrationMutableInput> MutableInputs,
    string ExpectedStudentTermStateRowVersion = "");

/// <summary>
/// The SQL-local registration operations that run inside the canonical
/// SPEC-008 student/term transaction callback. Implementations must not make
/// remote calls or open a second transaction.
/// </summary>
public interface IRegistrationLocalTransactionStore
{
    Task LockRegistrationContextAsync(
        Guid studentId,
        Guid termId,
        CancellationToken cancellationToken);

    Task LockSerializablePublicationScopeRangeAsync(
        RegistrationPublicationScope scope,
        string normalizedScopeCode,
        CancellationToken cancellationToken);

    Task LockSectionGroupVersionsAsync(
        IReadOnlyList<Guid> sortedGroupIds,
        CancellationToken cancellationToken);

    Task ReReadAndValidateAsync(
        RegistrationFinalValidation validation,
        CancellationToken cancellationToken);
}

/// <summary>
/// Enters the canonical SPEC-008 student/term transaction boundary before
/// later registration work is allowed to run.
/// </summary>
public sealed class RegistrationTransactionCoordinator
{
    public const int MaximumRetryAttempts = 3;

    public static IReadOnlyList<RegistrationMutableInput> RequiredMutableInputs { get; } =
        Array.AsReadOnly<RegistrationMutableInput>(
        [
            RegistrationMutableInput.RegistrationWindow,
            RegistrationMutableInput.StudentProfile,
            RegistrationMutableInput.StudentHolds,
            RegistrationMutableInput.CatalogueVersion,
            RegistrationMutableInput.PolicySetVersion,
            RegistrationMutableInput.Eligibility,
            RegistrationMutableInput.CreditLoad,
            RegistrationMutableInput.DuplicateCourses,
            RegistrationMutableInput.SectionGroupStateAndMeetingVersions,
            RegistrationMutableInput.Timetable,
            RegistrationMutableInput.CommitBoundaryVersions
        ]);

    private readonly IRegistrationBoundary _academicBoundary;
    private readonly IRegistrationLocalTransactionStore? _localStore;

    public RegistrationTransactionCoordinator(
        IRegistrationBoundary academicBoundary)
        : this(academicBoundary, localStore: null)
    {
    }

    public RegistrationTransactionCoordinator(
        IRegistrationBoundary academicBoundary,
        IRegistrationLocalTransactionStore? localStore)
    {
        _academicBoundary = academicBoundary ??
            throw new ArgumentNullException(nameof(academicBoundary));
        _localStore = localStore;
    }

    public Task<RegistrationBoundaryResult> ExecuteAsync(
        Guid studentId,
        Guid termId,
        string expectedStudentTermStateRowVersion,
        Func<CancellationToken, Task> transactionWork,
        CancellationToken cancellationToken = default) =>
        _academicBoundary.ExecuteRegistrationBoundaryAsync(
            studentId,
            termId,
            expectedStudentTermStateRowVersion,
            transactionWork,
            cancellationToken);

    /// <summary>
    /// Enters SPEC-008's database-backed student/term boundary before any
    /// registration lock, final re-read, or atomic commit work is performed.
    /// </summary>
    public Task<RegistrationBoundaryResult> ExecuteRegistrationAsync(
        RegistrationTransactionPlan plan,
        Func<CancellationToken, Task> atomicCommit,
        CancellationToken cancellationToken = default)
    {
        ValidatePlan(plan);
        ArgumentNullException.ThrowIfNull(atomicCommit);

        return _academicBoundary.ExecuteRegistrationBoundaryAsync(
            plan.StudentId,
            plan.TermId,
            plan.ExpectedStudentTermStateRowVersion,
            token => ExecuteRegistrationPlanAsync(plan, atomicCommit, token),
            cancellationToken);
    }

    /// <summary>
    /// Performs the complete SQL-local lock, final-validation, and commit plan.
    /// This method is intended to run only from the successful SPEC-008 callback.
    /// The local store must revalidate PlanRowVersion, effective Policy Version,
    /// eligibility, Credit load, timetable Conflict, and the RegistrationContext
    /// version. Validation failures are mapped by RegistrationConflictMapper to
    /// PLAN_CHANGED, POLICY_CHANGED, SCHEDULE_CONFLICT, WINDOW_CLOSED, or
    /// WINDOW_CHANGED and must not invoke the Enrollment commit callback.
    ///
    /// Transaction ownership remains deliberately singular: SPEC-008's store
    /// performs BeginTransaction and Commit around this callback. The atomic
    /// allocation callback owns CreateSavepoint/Rollback behavior and persists
    /// Enrollment plus DecisionSnapshot state; this coordinator never starts a
    /// nested transaction or makes a remote call.
    /// </summary>
    public async Task ExecuteRegistrationPlanAsync(
        RegistrationTransactionPlan plan,
        Func<CancellationToken, Task> atomicCommit,
        CancellationToken cancellationToken = default)
    {
        ValidatePlan(plan);
        ArgumentNullException.ThrowIfNull(atomicCommit);
        cancellationToken.ThrowIfCancellationRequested();

        await LockRegistrationContextAsync(
            plan.StudentId,
            plan.TermId,
            cancellationToken).ConfigureAwait(false);
        await LockPolicyPublicationScopeAsync(
            plan.CatalogueScopeCode,
            plan.PolicyScopeCode,
            cancellationToken).ConfigureAwait(false);
        var selectedGroupIds = NormalizeGroupIds(plan.GroupIds);
        var boundaryGroupIds = NormalizeGroupIds(
            plan.BoundaryGroupIds is { Count: > 0 }
                ? plan.BoundaryGroupIds
                : plan.GroupIds);
        await LockGroupVersionsAsync(boundaryGroupIds, cancellationToken).ConfigureAwait(false);

        var catalogueScope = NormalizeScopeCode(
            plan.CatalogueScopeCode,
            nameof(plan.CatalogueScopeCode));
        var policyScope = NormalizeScopeCode(
            plan.PolicyScopeCode,
            nameof(plan.PolicyScopeCode));
        await RevalidateAsync(
            new RegistrationFinalValidation(
                plan.StudentId,
                plan.TermId,
                plan.PlanId,
                plan.ExpectedPlanRowVersion.Trim(),
                plan.ExpectedRegistrationContextVersion.Trim(),
                plan.ReceivedAtUtc,
                catalogueScope,
                policyScope,
                selectedGroupIds,
                RequiredMutableInputs,
                plan.ExpectedStudentTermStateRowVersion),
            cancellationToken).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();
        await atomicCommit(cancellationToken).ConfigureAwait(false);
    }

    public Task LockRegistrationContextAsync(
        Guid studentId,
        Guid termId,
        CancellationToken cancellationToken = default)
    {
        RequireId(studentId, nameof(studentId));
        RequireId(termId, nameof(termId));
        cancellationToken.ThrowIfCancellationRequested();
        return Store.LockRegistrationContextAsync(studentId, termId, cancellationToken);
    }

    /// <summary>
    /// Locks normalized catalogue then policy scope-code key ranges. The local
    /// store owns the SQL Server serializable range-lock implementation.
    /// </summary>
    public async Task LockPolicyPublicationScopeAsync(
        string catalogueScopeCode,
        string policyScopeCode,
        CancellationToken cancellationToken = default)
    {
        var catalogue = NormalizeScopeCode(
            catalogueScopeCode,
            nameof(catalogueScopeCode));
        var policy = NormalizeScopeCode(policyScopeCode, nameof(policyScopeCode));
        cancellationToken.ThrowIfCancellationRequested();

        await Store.LockSerializablePublicationScopeRangeAsync(
            RegistrationPublicationScope.Catalogue,
            catalogue,
            cancellationToken).ConfigureAwait(false);
        await Store.LockSerializablePublicationScopeRangeAsync(
            RegistrationPublicationScope.Policy,
            policy,
            cancellationToken).ConfigureAwait(false);
    }

    public Task LockGroupVersionsAsync(
        IReadOnlyList<Guid> groupIds,
        CancellationToken cancellationToken = default)
    {
        var sortedGroupIds = NormalizeGroupIds(groupIds);
        cancellationToken.ThrowIfCancellationRequested();
        return Store.LockSectionGroupVersionsAsync(sortedGroupIds, cancellationToken);
    }

    /// <summary>
    /// Retries only the supplied complete transaction operation, at most three
    /// times, and never retries cancellation or a deterministic failure.
    /// </summary>
    public async Task<TResult> ExecuteWithRetryAsync<TResult>(
        Func<CancellationToken, Task<TResult>> completeTransaction,
        Func<Exception, bool> isTransient,
        int maximumAttempts = MaximumRetryAttempts,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(completeTransaction);
        ArgumentNullException.ThrowIfNull(isTransient);
        if (maximumAttempts is < 1 or > MaximumRetryAttempts)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumAttempts),
                $"Retry attempts must be between one and {MaximumRetryAttempts}.");
        }

        for (var attempt = 1; ; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                return await completeTransaction(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (
                exception is not OperationCanceledException &&
                attempt < maximumAttempts &&
                isTransient(exception))
            {
                // The next loop invokes the complete transaction delegate again.
            }
        }
    }

    public Task<RegistrationBoundaryResult> ExecuteWithCompleteRetryAsync(
        RegistrationTransactionPlan plan,
        Func<CancellationToken, Task> atomicCommit,
        Func<Exception, bool> isTransient,
        int maximumAttempts = MaximumRetryAttempts,
        CancellationToken cancellationToken = default)
    {
        ValidatePlan(plan);
        ArgumentNullException.ThrowIfNull(atomicCommit);

        return ExecuteWithRetryAsync(
            token => ExecuteRegistrationAsync(plan, atomicCommit, token),
            isTransient,
            maximumAttempts,
            cancellationToken);
    }

    private IRegistrationLocalTransactionStore Store =>
        _localStore ?? throw new InvalidOperationException(
            "A SQL-local registration transaction store is required for the full plan.");

    private static void ValidatePlan(RegistrationTransactionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        RequireId(plan.StudentId, nameof(plan.StudentId));
        RequireId(plan.TermId, nameof(plan.TermId));
        RequireId(plan.PlanId, nameof(plan.PlanId));
        if (string.IsNullOrWhiteSpace(plan.ExpectedStudentTermStateRowVersion))
        {
            throw new ArgumentException(
                "The expected student-term state row version is required.",
                nameof(plan));
        }

        if (string.IsNullOrWhiteSpace(plan.ExpectedPlanRowVersion) ||
            string.IsNullOrWhiteSpace(plan.ExpectedRegistrationContextVersion))
        {
            throw new ArgumentException(
                "Expected plan and registration-context versions are required.",
                nameof(plan));
        }

        if (plan.ReceivedAtUtc.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "ReceivedAtUtc must be an authoritative UTC instant.",
                nameof(plan));
        }

        _ = NormalizeScopeCode(plan.CatalogueScopeCode, nameof(plan.CatalogueScopeCode));
        _ = NormalizeScopeCode(plan.PolicyScopeCode, nameof(plan.PolicyScopeCode));
        _ = NormalizeGroupIds(plan.GroupIds);
        if (plan.BoundaryGroupIds is { Count: > 0 })
        {
            _ = NormalizeGroupIds(plan.BoundaryGroupIds);
        }
    }

    private static IReadOnlyList<Guid> NormalizeGroupIds(IReadOnlyList<Guid> groupIds)
    {
        ArgumentNullException.ThrowIfNull(groupIds);
        if (groupIds.Count == 0 || groupIds.Any(item => item == Guid.Empty))
        {
            throw new ArgumentException(
                "At least one non-empty group identifier is required.",
                nameof(groupIds));
        }

        return groupIds.Distinct().OrderBy(item => item).ToArray();
    }

    private Task RevalidateAsync(
        RegistrationFinalValidation validation,
        CancellationToken cancellationToken) =>
        Store.ReReadAndValidateAsync(validation, cancellationToken);

    private static string NormalizeScopeCode(string scopeCode, string parameterName)
    {
        var normalized = scopeCode?.Trim().Normalize().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException(
                "A publication scope code is required.",
                parameterName);
        }

        return normalized;
    }

    private static void RequireId(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A non-empty identifier is required.", parameterName);
        }
    }
}
