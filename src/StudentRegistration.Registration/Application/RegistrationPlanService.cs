using System.Globalization;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.Registration.Application;

public enum RegistrationPlanOperationOutcome
{
    Found,
    Updated,
    Validated,
    NotFound,
    StaleVersion,
    InvalidSelection,
    DependencyUnavailable
}

public sealed record RegistrationPlanReplacement(
    string ExpectedRowVersion,
    IReadOnlyList<Guid> SelectedGroupIds);

public sealed record RegistrationPlanResolutionAction(
    string Action,
    Guid TargetGroupId,
    string Label,
    string Route);

public sealed record RegistrationPlanSelectionIssue(
    string Code,
    Guid OfferingId,
    Guid GroupId,
    string GroupCode,
    string Message,
    IReadOnlyList<RegistrationPlanResolutionAction> Actions);

public sealed record RegistrationPlanLoadReason(
    string Code,
    bool Blocking,
    string Message,
    string RequiredValue,
    string CurrentValue,
    Guid PolicySetId,
    string PolicyVersion,
    string SourceReference);

public sealed record RegistrationPlanSelectedGroup(
    Guid OfferingId,
    Guid GroupId,
    string GroupCode,
    string CourseCode,
    string SubjectTitle,
    decimal Credits,
    int Capacity,
    int EnrolledCount,
    string OfferingVersion,
    string GroupVersion,
    IReadOnlyList<RegistrationPlanMeetingSnapshot> Meetings);

public sealed record RegistrationPlanView(
    Guid Id,
    Guid TermId,
    string RowVersion,
    IReadOnlyList<RegistrationPlanSelectedGroup> SelectedGroups,
    decimal TotalCredits,
    decimal DefaultTargetCredits,
    decimal MaximumAllowedCredits,
    IReadOnlyList<RegistrationPlanLoadReason> LoadReasons,
    IReadOnlyList<ScheduleConflict> Conflicts,
    IReadOnlyList<RegistrationPlanSelectionIssue> SelectionIssues,
    ValidationSnapshot Validation,
    bool ReviewBlocked);

public sealed record RegistrationPlanOperationResult(
    RegistrationPlanOperationOutcome Outcome,
    RegistrationPlanView? Plan = null,
    string? ErrorCode = null);

public sealed class RegistrationPlanService(
    IRegistrationPlanOwnerReader ownerReader,
    IRegistrationPlanStore store,
    IRegistrationPlanContextReader contextReader,
    ScheduleConflictDetector conflictDetector,
    TimeProvider timeProvider)
{
    public const string InitialEmptyVersion = "initial-empty/1";
    public const decimal DefaultTargetCredits = 18m;
    public const decimal MaximumAllowedCredits = 18m;

    public async Task<RegistrationPlanOperationResult> GetAsync(
        Guid applicationUserId,
        Guid termId,
        CancellationToken cancellationToken = default)
    {
        var studentId = await ResolveOwnerAsync(
            applicationUserId,
            termId,
            cancellationToken);
        if (!studentId.HasValue)
        {
            return new(RegistrationPlanOperationOutcome.NotFound);
        }

        var current = await store.ReadAsync(
            studentId.Value,
            termId,
            cancellationToken);
        var prepared = await PrepareAsync(
            studentId.Value,
            termId,
            current?.Items.Select(item => item.SelectedGroupId).ToArray() ?? [],
            current,
            cancellationToken);
        return prepared is null
            ? new(RegistrationPlanOperationOutcome.NotFound)
            : new(RegistrationPlanOperationOutcome.Found, prepared.View);
    }

    public async Task<RegistrationPlanOperationResult> ReplaceAsync(
        Guid applicationUserId,
        Guid termId,
        RegistrationPlanReplacement replacement,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(replacement);
        if (string.IsNullOrWhiteSpace(replacement.ExpectedRowVersion) ||
            replacement.SelectedGroupIds is null)
        {
            return new(
                RegistrationPlanOperationOutcome.InvalidSelection,
                ErrorCode: "PLAN_REPLACEMENT_INVALID");
        }

        var studentId = await ResolveOwnerAsync(
            applicationUserId,
            termId,
            cancellationToken);
        if (!studentId.HasValue)
        {
            return new(RegistrationPlanOperationOutcome.NotFound);
        }

        if (replacement.SelectedGroupIds.Any(id => id == Guid.Empty) ||
            replacement.SelectedGroupIds.Distinct().Count() !=
                replacement.SelectedGroupIds.Count)
        {
            return new(
                RegistrationPlanOperationOutcome.InvalidSelection,
                ErrorCode: "DUPLICATE_GROUP_SELECTION");
        }

        var prepared = await PrepareAsync(
            studentId.Value,
            termId,
            replacement.SelectedGroupIds,
            current: null,
            cancellationToken);
        if (prepared is null)
        {
            return new(RegistrationPlanOperationOutcome.NotFound);
        }

        var hardError = HardSelectionError(prepared);
        if (hardError is not null)
        {
            return new(
                RegistrationPlanOperationOutcome.InvalidSelection,
                hardError == "GROUP_UNAVAILABLE" ? null : prepared.View,
                hardError);
        }

        var stored = await store.ReplaceAsync(
            new(
                studentId.Value,
                termId,
                replacement.ExpectedRowVersion.Trim(),
                prepared.Selections,
                prepared.View.TotalCredits,
                prepared.View.ReviewBlocked
                    ? RegistrationPlanState.ReviewBlocked
                    : RegistrationPlanState.Draft,
                prepared.View.Conflicts,
                prepared.View.Validation),
            cancellationToken);
        if (stored.Outcome is RegistrationPlanStoreOutcome.StorageUnavailable)
        {
            return new(RegistrationPlanOperationOutcome.DependencyUnavailable);
        }

        if (stored.Plan is null)
        {
            return new(RegistrationPlanOperationOutcome.DependencyUnavailable);
        }

        var projected = await PrepareAsync(
            studentId.Value,
            termId,
            stored.Plan.Items.Select(item => item.SelectedGroupId).ToArray(),
            stored.Plan,
            cancellationToken);
        if (projected is null)
        {
            return new(RegistrationPlanOperationOutcome.DependencyUnavailable);
        }

        return stored.Outcome is RegistrationPlanStoreOutcome.StaleVersion
            ? new(
                RegistrationPlanOperationOutcome.StaleVersion,
                projected.View,
                "STALE_VERSION")
            : new(RegistrationPlanOperationOutcome.Updated, projected.View);
    }

    public async Task<RegistrationPlanOperationResult> ValidateAsync(
        Guid applicationUserId,
        Guid termId,
        CancellationToken cancellationToken = default)
    {
        var studentId = await ResolveOwnerAsync(
            applicationUserId,
            termId,
            cancellationToken);
        if (!studentId.HasValue)
        {
            return new(RegistrationPlanOperationOutcome.NotFound);
        }

        var current = await store.ReadAsync(
            studentId.Value,
            termId,
            cancellationToken);
        var prepared = await PrepareAsync(
            studentId.Value,
            termId,
            current?.Items.Select(item => item.SelectedGroupId).ToArray() ?? [],
            current,
            cancellationToken);
        return prepared is null
            ? new(RegistrationPlanOperationOutcome.NotFound)
            : new(RegistrationPlanOperationOutcome.Validated, prepared.View);
    }

    private async Task<Guid?> ResolveOwnerAsync(
        Guid applicationUserId,
        Guid termId,
        CancellationToken cancellationToken)
    {
        if (applicationUserId == Guid.Empty || termId == Guid.Empty)
        {
            return null;
        }

        return await ownerReader.ResolveStudentIdAsync(
            applicationUserId,
            termId,
            cancellationToken);
    }

    private async Task<PreparedPlan?> PrepareAsync(
        Guid studentId,
        Guid termId,
        IReadOnlyList<Guid> selectedGroupIds,
        RegistrationPlan? current,
        CancellationToken cancellationToken)
    {
        var evaluatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var context = await contextReader.ReadAsync(
            studentId,
            termId,
            selectedGroupIds,
            evaluatedAtUtc,
            cancellationToken);
        if (context is null)
        {
            return null;
        }

        var groupsById = context.Groups
            .GroupBy(group => group.GroupId)
            .ToDictionary(group => group.Key, group => group.Single());
        var selected = selectedGroupIds
            .Where(groupsById.ContainsKey)
            .Select(groupId => groupsById[groupId])
            .ToArray();
        var missingRequestedGroup = selected.Length != selectedGroupIds.Count;
        var issues = SelectionIssues(selected, current);
        var duplicateOffering = selected
            .GroupBy(group => group.OfferingId)
            .Any(group => group.Count() > 1);
        if (duplicateOffering)
        {
            issues.Insert(0, new(
                "DUPLICATE_OFFERING_SELECTION",
                selected[0].OfferingId,
                selected[0].GroupId,
                selected[0].GroupCode,
                "Select at most one group for each course offering.",
                []));
        }

        var totalCredits = selected.Sum(group => group.Credits);
        var loadBlocked = totalCredits > MaximumAllowedCredits;
        var loadReason = new RegistrationPlanLoadReason(
            loadBlocked ? "LOAD_ABOVE_MAXIMUM" : "LOAD_WITHIN_MAXIMUM",
            loadBlocked,
            loadBlocked
                ? "The selected load exceeds the approved 18-credit maximum."
                : "The selected load is within the approved 18-credit maximum.",
            MaximumAllowedCredits.ToString(CultureInfo.InvariantCulture),
            totalCredits.ToString(CultureInfo.InvariantCulture),
            context.PolicySetId,
            context.PolicyVersion,
            context.PolicySourceReference);
        var conflicts = conflictDetector.Detect(selected.Select(ToScheduleGroup).ToArray());
        var validation = new ValidationSnapshot(
            evaluatedAtUtc,
            context.AcademicContextVersion,
            context.PolicyVersion,
            context.CatalogueVersion,
            selected
                .GroupBy(group => group.OfferingId)
                .ToDictionary(group => group.Key, group => group.First().OfferingVersion),
            selected.ToDictionary(group => group.GroupId, group => group.GroupVersion));
        var reviewBlocked = loadBlocked || issues.Count > 0 || conflicts.Count > 0;
        var selections = selected.Select(group => new RegistrationPlanSelection(
            Guid.NewGuid(),
            group.OfferingId,
            group.GroupId,
            group.OfferingVersion,
            group.GroupVersion)).ToArray();
        var view = new RegistrationPlanView(
            current?.Id ?? Guid.Empty,
            termId,
            current is null || current.Version.Length == 0
                ? InitialEmptyVersion
                : Convert.ToBase64String(current.Version),
            selected.Select(ToView).ToArray(),
            totalCredits,
            DefaultTargetCredits,
            MaximumAllowedCredits,
            [loadReason],
            conflicts,
            issues,
            validation,
            reviewBlocked);
        return new(view, selections, missingRequestedGroup);
    }

    private static string? HardSelectionError(PreparedPlan prepared)
    {
        if (prepared.MissingRequestedGroup)
        {
            return "GROUP_UNAVAILABLE";
        }

        if (prepared.View.SelectionIssues.Any(issue =>
            issue.Code == "DUPLICATE_OFFERING_SELECTION"))
        {
            return "DUPLICATE_OFFERING_SELECTION";
        }

        return prepared.View.LoadReasons.Any(reason => reason.Blocking)
            ? "LOAD_ABOVE_MAXIMUM"
            : null;
    }

    private static List<RegistrationPlanSelectionIssue> SelectionIssues(
        IReadOnlyList<RegistrationPlanGroupSnapshot> groups,
        RegistrationPlan? current)
    {
        var issues = new List<RegistrationPlanSelectionIssue>();
        var captured = current?.Items.ToDictionary(item => item.SelectedGroupId);
        foreach (var group in groups)
        {
            if (captured is not null &&
                captured.TryGetValue(group.GroupId, out var item) &&
                (!string.Equals(item.CapturedOfferingVersion, group.OfferingVersion,
                    StringComparison.Ordinal) ||
                 !string.Equals(item.CapturedGroupVersion, group.GroupVersion,
                    StringComparison.Ordinal)))
            {
                issues.Add(new(
                    "GROUP_CHANGED",
                    group.OfferingId,
                    group.GroupId,
                    group.GroupCode,
                    "The selected group changed and must be reviewed.",
                    Actions(group, $"{group.CourseCode} {group.GroupCode}")));
            }

            var lifecycleCode = group.State.ToLowerInvariant() switch
            {
                "draft" => "GROUP_UNPUBLISHED",
                "closed" => "GROUP_CLOSED",
                "cancelled" => "GROUP_CANCELLED",
                "published" => null,
                _ => "GROUP_UNAVAILABLE"
            };
            AddIssue(issues, group, lifecycleCode);
            AddIssue(
                issues,
                group,
                group.RegistrationPaused ? "REGISTRATION_PAUSED" : null);
            AddIssue(
                issues,
                group,
                group.EnrolledCount >= group.Capacity ? "GROUP_FULL" : null);
        }

        return issues;
    }

    private static void AddIssue(
        ICollection<RegistrationPlanSelectionIssue> issues,
        RegistrationPlanGroupSnapshot group,
        string? code)
    {
        if (code is null)
        {
            return;
        }

        var message = code switch
        {
            "GROUP_UNPUBLISHED" => "The selected group is not published.",
            "GROUP_CLOSED" => "The selected group is closed.",
            "GROUP_CANCELLED" => "The selected group is cancelled.",
            "REGISTRATION_PAUSED" => "Registration is paused for the selected group.",
            "GROUP_FULL" =>
                "The selected group is full; capacity remains advisory until submission.",
            _ => "The selected group is not currently available."
        };
        issues.Add(new(
            code,
            group.OfferingId,
            group.GroupId,
            group.GroupCode,
            message,
            Actions(group, $"{group.CourseCode} {group.GroupCode}")));
    }

    private static IReadOnlyList<RegistrationPlanResolutionAction> Actions(
        RegistrationPlanGroupSnapshot group,
        string label) =>
        [
            new(
                "change-group",
                group.GroupId,
                $"Change {label}",
                $"/student/subjects/{group.OfferingId:D}"),
            new(
                "remove-group",
                group.GroupId,
                $"Remove {label}",
                "/student/schedule")
        ];

    private static SelectedScheduleGroup ToScheduleGroup(
        RegistrationPlanGroupSnapshot group) =>
        new(
            group.GroupId,
            group.GroupCode,
            group.CourseCode,
            group.SubjectTitle,
            group.Meetings.Select(meeting => new SelectedScheduleMeeting(
                meeting.MeetingId,
                meeting.DayOfWeek,
                meeting.StartLocal,
                meeting.EndLocal)).ToArray());

    private static RegistrationPlanSelectedGroup ToView(
        RegistrationPlanGroupSnapshot group) =>
        new(
            group.OfferingId,
            group.GroupId,
            group.GroupCode,
            group.CourseCode,
            group.SubjectTitle,
            group.Credits,
            group.Capacity,
            group.EnrolledCount,
            group.OfferingVersion,
            group.GroupVersion,
            group.Meetings);

    private sealed record PreparedPlan(
        RegistrationPlanView View,
        IReadOnlyList<RegistrationPlanSelection> Selections,
        bool MissingRequestedGroup);
}
