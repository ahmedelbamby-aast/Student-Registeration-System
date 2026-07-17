using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Infrastructure.SqlServer.Audit;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Domain;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.Registration;

/// <summary>
/// Thin SQL adapter for the SPEC-014 HTTP boundary. It composes the existing
/// transaction coordinator, scoped submission store, and seat allocator; it
/// does not introduce a second transaction or a second idempotency record.
/// </summary>
public sealed class SqlRegistrationEndpointStore(
    StudentRegistrationDbContext dbContext,
    IRegistrationPlanContextReader planContextReader,
    RegistrationSubmissionStore submissionStore,
    SqlSeatAllocator seatAllocator,
    EligibilityService eligibilityService,
    TimeProvider timeProvider) :
    IRegistrationEndpointStore,
    IRegistrationLocalTransactionStore
{
    private string? _pendingRejectionCode;
    private string? _pendingCurrentVersion;
    private bool _claimContended;
    private Guid _preflightPolicySetId;
    private RegistrationPlanContextSnapshot? _currentSnapshot;

    public async Task<RegistrationCommandContext?> ResolveCommandContextAsync(
        Guid applicationUserId,
        Guid termId,
        CancellationToken cancellationToken = default)
    {
        if (applicationUserId == Guid.Empty || termId == Guid.Empty)
        {
            return null;
        }

        var student = await dbContext.Set<Student>()
            .AsNoTracking()
            .Where(candidate =>
                candidate.ApplicationUserId == applicationUserId &&
                candidate.IsActive)
            .Select(candidate => new
            {
                candidate.Id,
                candidate.ProgramCode,
                candidate.Cohort
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (student is null)
        {
            return null;
        }

        var windows = await dbContext.Set<RegistrationWindow>()
            .AsNoTracking()
            .Where(window => window.TermId == termId &&
                (window.State == RegistrationWindowLifecycleState.Published ||
                 window.State == RegistrationWindowLifecycleState.EmergencyClosed))
            .ToArrayAsync(cancellationToken);
        var window = windows
            .OrderByDescending(candidate => candidate.ScopeType == RegistrationWindowScopeType.Cohort)
            .ThenByDescending(candidate => candidate.ScopeType == RegistrationWindowScopeType.Program)
            .ThenBy(candidate => candidate.Id)
            .FirstOrDefault(candidate =>
                candidate.ScopeType == RegistrationWindowScopeType.AllStudents ||
                candidate.ScopeType == RegistrationWindowScopeType.Program &&
                    string.Equals(candidate.ScopeValue, student.ProgramCode,
                        StringComparison.OrdinalIgnoreCase) ||
                candidate.ScopeType == RegistrationWindowScopeType.Cohort &&
                    string.Equals(candidate.ScopeValue, student.Cohort,
                        StringComparison.OrdinalIgnoreCase));
        if (window is null || window.Version.Length == 0)
        {
            return null;
        }

        var resolved = new ResolvedRegistrationContext(
            applicationUserId,
            student.Id,
            termId,
            Convert.ToBase64String(window.Version),
            window.OpensAtUtc,
            window.ClosesAtUtc,
            window.State == RegistrationWindowLifecycleState.EmergencyClosed);
        return new(resolved, "DEFERRED_TO_PLAN_VALIDATION");
    }

    public async Task<RegistrationEndpointResult> SubmitAsync(
        RegistrationCommand command,
        RegistrationTransactionCoordinator coordinator,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(coordinator);
        var payloadHash = RegistrationSubmissionStore.ComputeCanonicalPayloadHash(
            $"{command.PlanId:D}|{command.ExpectedPlanRowVersion}|{command.ClientRequestId:D}");
        var scope = new RegistrationRequestScope(
            command.StudentId,
            command.TermId,
            command.ClientRequestId);

        var visible = await submissionStore.ObserveScopedAsync(
            scope,
            payloadHash,
            RegistrationSubmissionStore.MaximumObservationWindow,
            cancellationToken);
        var visibleResult = ToVisibleResult(visible, command.TermId, command.ClientRequestId);
        if (visibleResult is not null)
        {
            return visibleResult;
        }

        var plan = await dbContext.Set<RegistrationPlan>()
            .AsNoTracking()
            .Include(candidate => candidate.Items)
            .SingleOrDefaultAsync(candidate =>
                candidate.Id == command.PlanId &&
                candidate.StudentId == command.StudentId &&
                candidate.TermId == command.TermId,
                cancellationToken);
        if (plan is null || plan.Items.Count == 0)
        {
            return Conflict("PLAN_CHANGED");
        }

        var currentPlanVersion = Convert.ToBase64String(plan.Version);
        if (plan.Validation is null)
        {
            return Conflict("PLAN_CHANGED", currentPlanVersion);
        }

        _currentSnapshot = await planContextReader.ReadAsync(
            command.StudentId,
            command.TermId,
            plan.Items.Select(item => item.SelectedGroupId).ToArray(),
            command.ReceivedAtUtc,
            cancellationToken);
        if (_currentSnapshot is null)
        {
            return Conflict(
                "POLICY_CHANGED",
                _currentSnapshot?.PolicyVersion ?? plan.Validation.PolicyVersion);
        }

        _preflightPolicySetId = _currentSnapshot.PolicySetId;

        var selectedGroupIds = plan.Items
            .Select(item => item.SelectedGroupId)
            .Distinct()
            .ToArray();
        var existingGroupIds = await (
                from enrollment in dbContext.Set<Enrollment>().AsNoTracking()
                join offering in dbContext.Set<CourseOffering>().AsNoTracking()
                    on enrollment.OfferingId equals offering.Id
                where enrollment.StudentId == command.StudentId &&
                    enrollment.State == EnrollmentState.Active &&
                    offering.TermId == command.TermId
                select enrollment.GroupId)
            .Distinct()
            .ToArrayAsync(cancellationToken);
        var boundaryGroupIds = selectedGroupIds
            .Concat(existingGroupIds)
            .Distinct()
            .OrderBy(id => id)
            .ToArray();
        var transactionPlan = new RegistrationTransactionPlan(
            command.StudentId,
            command.TermId,
            plan.Validation.AcademicContextVersion,
            command.PlanId,
            command.ExpectedPlanRowVersion,
            command.ExpectedRegistrationContextVersion,
            command.ReceivedAtUtc,
            _currentSnapshot.CatalogueScopeCode,
            _currentSnapshot.PolicyScopeCode,
            selectedGroupIds,
            boundaryGroupIds);

        RegistrationEndpointResult? transactionResult = null;
        _pendingRejectionCode = null;
        _pendingCurrentVersion = null;
        _claimContended = false;
        var boundary = await coordinator.ExecuteRegistrationAsync(
            transactionPlan,
            async token =>
            {
                SubmissionClaimResult claim;
                try
                {
                    claim = await submissionStore.ClaimInsideTransactionAsync(
                        scope, payloadHash, command.ReceivedAtUtc, token);
                }
                catch (RegistrationClaimContendedException)
                {
                    _claimContended = true;
                    throw;
                }
                var existing = ToVisibleResult(claim, command.TermId, command.ClientRequestId);
                if (existing is not null)
                {
                    transactionResult = existing;
                    return;
                }

                var submission = claim.Submission!;
                if (_pendingRejectionCode is not null)
                {
                    await FinalizeRejectedAsync(
                        submission, _pendingRejectionCode, command, token);
                    transactionResult = Conflict(
                        _pendingRejectionCode,
                        _pendingCurrentVersion);
                    return;
                }

                var allocation = await seatAllocator.AllocateAllOrRejectAsync(
                    transactionPlan.GroupIds, token);
                if (!allocation.IsAccepted)
                {
                    var reason = allocation.ReasonCode ?? "GROUP_FULL";
                    await FinalizeRejectedAsync(submission, reason, command, token);
                    transactionResult = Conflict(reason);
                    return;
                }

                try
                {
                    foreach (var group in _currentSnapshot.Groups)
                    {
                        dbContext.Add(new Enrollment(
                            Guid.NewGuid(),
                            command.StudentId,
                            group.OfferingId,
                            group.GroupId,
                            submission.Id,
                            EnrollmentState.Active,
                            command.ReceivedAtUtc));
                    }

                    var groups = _currentSnapshot.Groups.Select(ToGroup).ToArray();
                    var term = await dbContext.Set<AcademicTerm>()
                        .AsNoTracking()
                        .SingleAsync(item => item.Id == command.TermId, token);
                    var receipt = new RegistrationReceiptSnapshotDto(
                        new(term.Id, term.Code, term.DisplayName, term.TimeZoneId),
                        groups,
                        _currentSnapshot.Groups.Sum(group => group.Credits),
                        _currentSnapshot.PolicySetId,
                        _currentSnapshot.PolicyVersion,
                        command.ReceivedAtUtc);
                    var decision = Decision("REGISTERED", command);
                    var completedAt = timeProvider.GetUtcNow().UtcDateTime;
                    AddAudit(submission, command, "RegistrationAccepted", "REGISTERED", completedAt);
                    await submissionStore.FinalizeAcceptedAsync(
                        submission,
                        "REGISTERED",
                        NewReference(),
                        JsonSerializer.Serialize(receipt),
                        JsonSerializer.Serialize(decision),
                        completedAt,
                        token);
                    transactionResult = new(
                        RegistrationEndpointOutcome.Created,
                        ToFinal(submission, receipt, decision, groups));
                }
                catch (DbUpdateException exception)
                    when (IsDuplicateEnrollment(exception))
                {
                    await seatAllocator.RollbackAllocationAsync(token);
                    dbContext.ChangeTracker.Clear();
                    var retainedClaim = await dbContext.Set<RegistrationSubmission>()
                        .SingleAsync(candidate => candidate.Id == submission.Id, token);
                    await FinalizeRejectedAsync(
                        retainedClaim,
                        "POLICY_CHANGED",
                        command,
                        token);
                    transactionResult = Conflict(
                        "POLICY_CHANGED",
                        _currentSnapshot.AcademicContextVersion);
                }
            },
            cancellationToken);

        if (_claimContended)
        {
            var observed = await submissionStore.ReplayCommittedAsync(
                scope,
                payloadHash,
                cancellationToken);
            return ToVisibleResult(observed, command.TermId, command.ClientRequestId) ??
                Processing(command.TermId, command.ClientRequestId);
        }

        if (boundary.Outcome is RegistrationBoundaryOutcome.Committed &&
            transactionResult is not null)
        {
            return transactionResult;
        }

        return boundary.Outcome switch
        {
            RegistrationBoundaryOutcome.HoldBlocked => Conflict("HOLD_BLOCKED"),
            RegistrationBoundaryOutcome.StaleVersion => Conflict(
                "PLAN_CHANGED",
                await CurrentAcademicVersionAsync(
                    command.StudentId,
                    command.TermId,
                    cancellationToken)),
            RegistrationBoundaryOutcome.NotFound => new(
                RegistrationEndpointOutcome.NotFound,
                ErrorCode: "REGISTRATION_CONTEXT_NOT_FOUND"),
            _ => new(RegistrationEndpointOutcome.Unavailable,
                ErrorCode: "REGISTRATION_UNAVAILABLE"),
        };
    }

    public async Task<RegistrationEndpointResult> LookupAsync(
        Guid applicationUserId,
        Guid termId,
        Guid clientRequestId,
        CancellationToken cancellationToken = default)
    {
        var studentId = await dbContext.Set<Student>()
            .AsNoTracking()
            .Where(student =>
                student.ApplicationUserId == applicationUserId &&
                student.IsActive)
            .Select(student => (Guid?)student.Id)
            .SingleOrDefaultAsync(cancellationToken);
        if (studentId is null || clientRequestId == Guid.Empty)
        {
            return new(RegistrationEndpointOutcome.NotFound, ErrorCode: "REQUEST_NOT_FOUND");
        }

        var observation = await submissionStore.ReadFinalByRequestBoundedAsync(
            new RegistrationRequestScope(
                studentId.Value,
                termId,
                clientRequestId),
            RegistrationSubmissionStore.MaximumObservationWindow,
            cancellationToken);
        return observation.Submission is not null
            ? new(
                RegistrationEndpointOutcome.Replayed,
                ToStoredFinal(observation.Submission))
            : observation.IsInProgress
                ? Processing(termId, clientRequestId)
                : new(
                    RegistrationEndpointOutcome.NotFound,
                    ErrorCode: "REQUEST_NOT_FOUND");
    }

    public async Task LockRegistrationContextAsync(
        Guid studentId,
        Guid termId,
        CancellationToken cancellationToken)
    {
        _ = await dbContext.Set<RegistrationWindow>()
            .FromSqlInterpolated($"""
                SELECT * FROM [academics].[RegistrationWindows] WITH (UPDLOCK, HOLDLOCK)
                WHERE [TermId] = {termId}
                """)
            .ToArrayAsync(cancellationToken);
    }

    public async Task LockSerializablePublicationScopeRangeAsync(
        RegistrationPublicationScope scope,
        string normalizedScopeCode,
        CancellationToken cancellationToken)
    {
        if (scope is RegistrationPublicationScope.Catalogue)
        {
            _ = await dbContext.Set<CatalogueVersion>()
                .FromSqlInterpolated($"""
                    SELECT * FROM [academics].[CatalogueVersions] WITH (UPDLOCK, HOLDLOCK)
                    WHERE [ScopeCode] = {normalizedScopeCode}
                    """)
                .ToArrayAsync(cancellationToken);
            return;
        }

        _ = await dbContext.Set<PolicySet>()
            .FromSqlInterpolated($"""
                SELECT * FROM [academics].[PolicySets] WITH (UPDLOCK, HOLDLOCK)
                WHERE [ScopeCode] = {normalizedScopeCode}
                """)
            .ToArrayAsync(cancellationToken);
    }

    public async Task LockSectionGroupVersionsAsync(
        IReadOnlyList<Guid> sortedGroupIds,
        CancellationToken cancellationToken)
    {
        foreach (var groupId in sortedGroupIds)
        {
            _ = await dbContext.Set<SectionGroup>()
                .FromSqlInterpolated($"""
                    SELECT * FROM [scheduling].[SectionGroups] WITH (UPDLOCK, HOLDLOCK)
                    WHERE [Id] = {groupId}
                    """)
                .SingleOrDefaultAsync(cancellationToken);
        }
    }

    public async Task ReReadAndValidateAsync(
        RegistrationFinalValidation validation,
        CancellationToken cancellationToken)
    {
        _pendingRejectionCode = null;
        _pendingCurrentVersion = null;
        var plan = await dbContext.Set<RegistrationPlan>()
            .AsNoTracking()
            .Include(candidate => candidate.Items)
            .SingleOrDefaultAsync(candidate =>
                candidate.Id == validation.PlanId &&
                candidate.StudentId == validation.StudentId &&
                candidate.TermId == validation.TermId,
                cancellationToken);
        if (plan is null)
        {
            Reject("PLAN_CHANGED");
            return;
        }

        var planVersion = Convert.ToBase64String(plan.Version);
        if (!string.Equals(
                planVersion,
                validation.ExpectedPlanRowVersion,
                StringComparison.Ordinal) ||
            plan.Validation is null)
        {
            Reject("PLAN_CHANGED", planVersion);
            return;
        }

        if (plan.ReviewBlocked || plan.Conflicts.Count > 0)
        {
            Reject("SCHEDULE_CONFLICT", planVersion);
            return;
        }

        var window = await ResolveWindowForStudentAsync(
            validation.StudentId,
            validation.TermId,
            cancellationToken);
        if (window is null)
        {
            Reject("WINDOW_CHANGED");
            return;
        }

        var windowVersion = Convert.ToBase64String(window.Version);
        if (!string.Equals(
                windowVersion,
                validation.ExpectedRegistrationContextVersion,
                StringComparison.Ordinal) ||
            window.State == RegistrationWindowLifecycleState.EmergencyClosed)
        {
            Reject("WINDOW_CHANGED", windowVersion);
            return;
        }

        if (validation.ReceivedAtUtc < window.OpensAtUtc ||
            validation.ReceivedAtUtc >= window.ClosesAtUtc)
        {
            Reject("WINDOW_CLOSED", windowVersion);
            return;
        }

        var snapshot = await planContextReader.ReadAsync(
            validation.StudentId,
            validation.TermId,
            validation.GroupIds,
            validation.ReceivedAtUtc,
            cancellationToken);
        if (snapshot is null)
        {
            Reject("POLICY_CHANGED", plan.Validation.PolicyVersion);
            return;
        }

        _currentSnapshot = snapshot;
        if (!string.Equals(
                snapshot.AcademicContextVersion,
                validation.ExpectedStudentTermStateRowVersion,
                StringComparison.Ordinal))
        {
            Reject("PLAN_CHANGED", snapshot.AcademicContextVersion);
            return;
        }

        if (!string.Equals(
                snapshot.PolicyVersion,
                plan.Validation.PolicyVersion,
                StringComparison.Ordinal) ||
            snapshot.PolicySetId != _preflightPolicySetId)
        {
            Reject("POLICY_CHANGED", snapshot.PolicyVersion);
            return;
        }

        if (!string.Equals(
                snapshot.CatalogueVersion,
                plan.Validation.CatalogueVersion,
                StringComparison.Ordinal))
        {
            Reject("POLICY_CHANGED", snapshot.CatalogueVersion);
            return;
        }

        var planItems = plan.Items.ToDictionary(item => item.SelectedGroupId);
        if (snapshot.Groups.Count != validation.GroupIds.Count ||
            snapshot.Groups.Any(group =>
                !planItems.TryGetValue(group.GroupId, out var item) ||
                !string.Equals(
                    item.CapturedOfferingVersion,
                    group.OfferingVersion,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    item.CapturedGroupVersion,
                    group.GroupVersion,
                    StringComparison.Ordinal) ||
                !string.Equals(group.State, "published", StringComparison.OrdinalIgnoreCase) ||
                group.RegistrationPaused))
        {
            Reject(
                "GROUP_CHANGED",
                snapshot.Groups.FirstOrDefault()?.GroupVersion);
            return;
        }

        var applicationUserId = await dbContext.Set<Student>()
            .Where(student => student.Id == validation.StudentId)
            .Select(student => student.ApplicationUserId)
            .SingleAsync(cancellationToken);
        var eligibility = await eligibilityService.EvaluateTermForCommitAsync(
            applicationUserId,
            validation.TermId,
            validation.ReceivedAtUtc,
            cancellationToken);
        if (eligibility.Outcome is not EligibilityEvaluationOutcome.Found)
        {
            Reject("POLICY_CHANGED", snapshot.PolicyVersion);
            return;
        }

        foreach (var group in snapshot.Groups)
        {
            var decision = eligibility.Items.SingleOrDefault(candidate =>
                candidate.OfferingId == group.OfferingId);
            var selected = decision?.Groups.SingleOrDefault(candidate =>
                candidate.GroupId == group.GroupId);
            if (decision is null ||
                selected is null ||
                !decision.Eligible ||
                !selected.Selectable)
            {
                var codes = decision?.Reasons
                    .Where(reason => reason.Blocking && !reason.Passed)
                    .Select(reason => reason.Code)
                    .Concat(selected?.NonSelectableReasons.Select(reason => reason.Code) ?? [])
                    .ToHashSet(StringComparer.Ordinal) ?? [];
                Reject(
                    codes.Contains("GROUP_FULL") ? "GROUP_FULL" :
                    codes.Contains("MEETING_CONFLICT") ? "SCHEDULE_CONFLICT" :
                    codes.Contains("REGISTRATION_WINDOW_CLOSED") ? "WINDOW_CLOSED" :
                    "POLICY_CHANGED",
                    decision?.PolicyVersion ?? snapshot.PolicyVersion);
                return;
            }
        }

        var existing = await (
                from enrollment in dbContext.Set<Enrollment>().AsNoTracking()
                join offering in dbContext.Set<CourseOffering>().AsNoTracking()
                    on enrollment.OfferingId equals offering.Id
                join course in dbContext.Set<Course>().AsNoTracking()
                    on offering.CourseId equals course.Id
                where enrollment.StudentId == validation.StudentId &&
                    enrollment.State == EnrollmentState.Active &&
                    offering.TermId == validation.TermId
                select new
                {
                    enrollment.OfferingId,
                    enrollment.GroupId,
                    course.Credits
                })
            .ToArrayAsync(cancellationToken);
        if (existing.Any(item => snapshot.Groups.Any(group =>
                group.OfferingId == item.OfferingId)))
        {
            Reject("POLICY_CHANGED", snapshot.AcademicContextVersion);
            return;
        }

        var maximumCredits = await dbContext.Set<StudentTermAcademicState>()
            .Where(state =>
                state.StudentId == validation.StudentId &&
                state.TermId == validation.TermId)
            .Select(state => state.GpaAtStart < 2m ? 12m : 18m)
            .SingleAsync(cancellationToken);
        if (plan.TotalCredits < 9m ||
            plan.TotalCredits + existing.Sum(item => item.Credits) > maximumCredits)
        {
            Reject("POLICY_CHANGED", snapshot.PolicyVersion);
            return;
        }

        var existingGroupIds = existing.Select(item => item.GroupId).Distinct().ToArray();
        if (existingGroupIds.Length > 0 &&
            await HasTimetableConflictAsync(
                validation.GroupIds,
                existingGroupIds,
                cancellationToken))
        {
            Reject("SCHEDULE_CONFLICT", snapshot.AcademicContextVersion);
        }
    }

    private async Task FinalizeRejectedAsync(
        RegistrationSubmission submission,
        string reason,
        RegistrationCommand command,
        CancellationToken cancellationToken)
    {
        var decision = Decision(reason, command);
        var completedAt = timeProvider.GetUtcNow().UtcDateTime;
        AddAudit(
            submission,
            command,
            "RegistrationSubmissionRejected",
            reason,
            completedAt);
        await submissionStore.FinalizeRejectedAsync(
            submission,
            reason,
            JsonSerializer.Serialize(decision),
            completedAt,
            cancellationToken);
    }

    private void AddAudit(
        RegistrationSubmission submission,
        RegistrationCommand command,
        string action,
        string reason,
        DateTime occurredAtUtc) =>
        dbContext.AuditEvents.Add(new AuditEvent(
            Guid.NewGuid(),
            $"application-user:{command.ApplicationUserId:D}",
            $"student:{command.StudentId:D}",
            action,
            nameof(RegistrationSubmission),
            submission.Id.ToString("N"),
            reason,
            null,
            JsonSerializer.Serialize(new
            {
                termId = command.TermId,
                resultCode = reason,
                groupCount = _currentSnapshot?.Groups.Count ?? 0
            }),
            command.ClientRequestId.ToString("N"),
            occurredAtUtc));

    private void Reject(string code, string? currentVersion = null)
    {
        _pendingRejectionCode = code;
        _pendingCurrentVersion = currentVersion;
    }

    private async Task<RegistrationWindow?> ResolveWindowForStudentAsync(
        Guid studentId,
        Guid termId,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Set<Student>()
            .AsNoTracking()
            .Where(candidate => candidate.Id == studentId && candidate.IsActive)
            .Select(candidate => new { candidate.ProgramCode, candidate.Cohort })
            .SingleOrDefaultAsync(cancellationToken);
        if (student is null)
        {
            return null;
        }

        var windows = await dbContext.Set<RegistrationWindow>()
            .AsNoTracking()
            .Where(window => window.TermId == termId &&
                (window.State == RegistrationWindowLifecycleState.Published ||
                 window.State == RegistrationWindowLifecycleState.EmergencyClosed))
            .ToArrayAsync(cancellationToken);
        return windows
            .OrderByDescending(candidate =>
                candidate.ScopeType == RegistrationWindowScopeType.Cohort)
            .ThenByDescending(candidate =>
                candidate.ScopeType == RegistrationWindowScopeType.Program)
            .ThenBy(candidate => candidate.Id)
            .FirstOrDefault(candidate =>
                candidate.ScopeType == RegistrationWindowScopeType.AllStudents ||
                candidate.ScopeType == RegistrationWindowScopeType.Program &&
                    string.Equals(candidate.ScopeValue, student.ProgramCode,
                        StringComparison.OrdinalIgnoreCase) ||
                candidate.ScopeType == RegistrationWindowScopeType.Cohort &&
                    string.Equals(candidate.ScopeValue, student.Cohort,
                        StringComparison.OrdinalIgnoreCase));
    }

    private async Task<bool> HasTimetableConflictAsync(
        IReadOnlyList<Guid> selectedGroupIds,
        IReadOnlyList<Guid> existingGroupIds,
        CancellationToken cancellationToken)
    {
        var allGroupIds = selectedGroupIds
            .Concat(existingGroupIds)
            .Distinct()
            .ToArray();
        var meetings = await dbContext.Set<MeetingSlot>()
            .AsNoTracking()
            .Where(meeting => allGroupIds.Contains(meeting.GroupId))
            .Select(meeting => new
            {
                meeting.GroupId,
                meeting.DayOfWeek,
                meeting.StartLocal,
                meeting.EndLocal
            })
            .ToArrayAsync(cancellationToken);
        var selected = meetings
            .Where(meeting => selectedGroupIds.Contains(meeting.GroupId))
            .ToArray();
        var existing = meetings
            .Where(meeting => existingGroupIds.Contains(meeting.GroupId))
            .ToArray();
        return selected.Any(left => existing.Any(right =>
            left.DayOfWeek == right.DayOfWeek &&
            left.StartLocal < right.EndLocal &&
            right.StartLocal < left.EndLocal));
    }

    private async Task<string?> CurrentAcademicVersionAsync(
        Guid studentId,
        Guid termId,
        CancellationToken cancellationToken)
    {
        var version = await dbContext.Set<StudentTermAcademicState>()
            .AsNoTracking()
            .Where(state => state.StudentId == studentId && state.TermId == termId)
            .Select(state => state.Version)
            .SingleOrDefaultAsync(cancellationToken);
        return version is { Length: > 0 }
            ? Convert.ToBase64String(version)
            : null;
    }

    private static bool IsDuplicateEnrollment(DbUpdateException exception) =>
        exception.GetBaseException() is SqlException sql &&
        sql.Number is 2601 or 2627 &&
        sql.Message.Contains(
            "IX_Enrollments_StudentId_OfferingId",
            StringComparison.OrdinalIgnoreCase);

    private DecisionSnapshot Decision(string resultCode, RegistrationCommand command) =>
        new(
            _currentSnapshot!.PolicyVersion,
            _currentSnapshot.AcademicContextVersion,
            command.ExpectedPlanRowVersion,
            _currentSnapshot.Groups.ToDictionary(
                group => group.GroupId,
                group => group.GroupVersion),
            resultCode);

    private static RegistrationEndpointResult? ToVisibleResult(
        SubmissionClaimResult claim,
        Guid termId,
        Guid clientRequestId) => claim.Status switch
        {
            SubmissionClaimStatus.Replayed when claim.Submission is not null =>
                new(RegistrationEndpointOutcome.Replayed,
                    ToStoredFinal(claim.Submission)),
            SubmissionClaimStatus.InProgress => Processing(termId, clientRequestId),
            SubmissionClaimStatus.PayloadMismatch => Conflict("IDEMPOTENCY_KEY_REUSED"),
            _ => null,
        };

    private static RegistrationEndpointResult Processing(
        Guid termId,
        Guid clientRequestId) => new(
            RegistrationEndpointOutcome.Processing,
            InProgress: new(
                clientRequestId,
                "processing",
                1,
                $"/api/student/terms/{termId:D}/registrations/by-request/{clientRequestId:D}"));

    private static RegistrationEndpointResult Conflict(
        string code,
        string? currentVersion = null) =>
        new(
            RegistrationEndpointOutcome.Conflict,
            ErrorCode: code,
            CurrentVersion: currentVersion);

    private static RegistrationFinalResult ToStoredFinal(
        RegistrationSubmission submission)
    {
        var receipt = string.IsNullOrWhiteSpace(submission.ReceiptSnapshotJson)
            ? null
            : JsonSerializer.Deserialize<RegistrationReceiptSnapshotDto>(
                submission.ReceiptSnapshotJson);
        var decision = JsonSerializer.Deserialize<DecisionSnapshot>(
            submission.DecisionSnapshotJson!);
        return ToFinal(
            submission,
            receipt,
            decision!,
            receipt?.Groups ?? []);
    }

    private static RegistrationFinalResult ToFinal(
        RegistrationSubmission submission,
        RegistrationReceiptSnapshotDto? receipt,
        DecisionSnapshot decision,
        IReadOnlyList<RegistrationGroupSnapshotDto> groups) => new(
            submission.Id,
            submission.ProcessingState == RegistrationSubmissionState.Accepted
                ? "accepted"
                : "rejected",
            submission.ResultCode!,
            groups,
            submission.ReceivedAtUtc,
            submission.CompletedAtUtc!.Value,
            receipt?.PolicySetId ?? Guid.Empty,
            receipt?.PolicyVersion ?? decision.PolicyVersion,
            decision.PlanVersion,
            submission.Reference,
            receipt);

    private static RegistrationGroupSnapshotDto ToGroup(
        RegistrationPlanGroupSnapshot group) => new(
            group.OfferingId,
            group.CourseCode,
            group.SubjectTitle,
            group.GroupId,
            group.GroupCode,
            group.Credits,
            group.Meetings.Select(meeting => new RegistrationMeetingSnapshotDto(
                meeting.MeetingId,
                meeting.ActivityKind,
                (int)meeting.DayOfWeek,
                meeting.StartLocal.ToString("HH:mm"),
                meeting.EndLocal.ToString("HH:mm"),
                meeting.RoomCode,
                meeting.Location,
                Staff(meeting))).ToArray());

    private static IReadOnlyList<RegistrationMeetingStaffSnapshotDto> Staff(
        RegistrationPlanMeetingSnapshot meeting)
    {
        var staff = new List<RegistrationMeetingStaffSnapshotDto>();
        if (!string.IsNullOrWhiteSpace(meeting.LecturerName))
        {
            staff.Add(new("Lecturer", meeting.LecturerName));
        }

        staff.AddRange(meeting.TeachingAssistantNames.Select(name =>
            new RegistrationMeetingStaffSnapshotDto("TeachingAssistant", name)));
        return staff;
    }

    private static string NewReference() =>
        $"REG-{Guid.NewGuid():N}"[..16].ToUpperInvariant();
}
