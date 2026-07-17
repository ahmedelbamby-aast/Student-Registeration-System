using StudentRegistration.Registration.Domain;

namespace StudentRegistration.Registration.Application;

public enum OptimizationStatus
{
    Complete,
    NoSolution,
    TimeBudget
}

public sealed record OptimizationExecutionResult(
    OptimizationStatus Status,
    IReadOnlyList<ScheduleOption> Options,
    IReadOnlyList<OptimizationDiagnostic> Diagnostics);

public sealed class OptimizationCoordinator
{
    public const string TimeBudgetToken = "time-budget";
    public const string NoSolutionToken = "no-solution";
    public const string InclusionMinimalToken = "inclusion-minimal";
    public const string ChangeGroupAction = "change-group";
    public const string RemoveCourseAction = "remove-course";
    private readonly ScheduleOptimizer _optimizer;
    private readonly TimeProvider _timeProvider;

    public OptimizationCoordinator(
        ScheduleOptimizer optimizer,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(optimizer);
        _optimizer = optimizer;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<OptimizationExecutionResult> OptimizeAsync(
        IReadOnlyList<ScheduleCandidateGroup> candidates,
        SchedulePreferences preferences,
        decimal requiredCredits,
        TimeSpan computationBudget,
        CancellationToken cancellationToken = default) =>
        await OptimizeAsync(
            candidates,
            preferences,
            requiredCredits,
            requiredCourseIds: null,
            computationBudget,
            cancellationToken);

    internal async Task<OptimizationExecutionResult> OptimizeAsync(
        IReadOnlyList<ScheduleCandidateGroup> candidates,
        SchedulePreferences preferences,
        decimal requiredCredits,
        IReadOnlyCollection<Guid>? requiredCourseIds,
        TimeSpan computationBudget,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(preferences);
        if (computationBudget < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(computationBudget));
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (computationBudget == TimeSpan.Zero)
        {
            return new(OptimizationStatus.TimeBudget, [], []);
        }

        var deadline = _timeProvider.GetUtcNow().Add(computationBudget);
        var result = _optimizer.Optimize(
            candidates,
            preferences,
            requiredCredits,
            requiredCourseIds,
            cancellationToken,
            () => _timeProvider.GetUtcNow() >= deadline);
        await Task.CompletedTask;
        if (result.SearchLimitReached)
        {
            return new(OptimizationStatus.TimeBudget, result.Options, []);
        }

        return result.Options.Count > 0
            ? new(OptimizationStatus.Complete, result.Options, [])
            : new(
                OptimizationStatus.NoSolution,
                [],
                Diagnostics(candidates, requiredCredits, requiredCourseIds));
    }

    private static IReadOnlyList<OptimizationDiagnostic> Diagnostics(
        IReadOnlyList<ScheduleCandidateGroup> candidates,
        decimal requiredCredits,
        IReadOnlyCollection<Guid>? requiredCourseIds)
    {
        var courseIds = (requiredCourseIds ??
                candidates.Select(candidate => candidate.CourseId).ToArray())
            .Distinct()
            .Order()
            .ToArray();
        foreach (var courseId in courseIds)
        {
            if (!candidates.Any(candidate =>
                    candidate.CourseId == courseId &&
                    candidate.Viable &&
                    string.Equals(
                        candidate.State,
                        "published",
                        StringComparison.Ordinal)))
            {
                return
                [
                    new(
                        $"no-viable-group-{courseId:N}",
                        "NO_VIABLE_GROUP",
                        [
                            new(
                                courseId,
                                null,
                                null,
                                "remove-course")
                        ])
                ];
            }
        }

        var viable = candidates
            .Where(candidate =>
                candidate.Viable &&
                string.Equals(
                    candidate.State,
                    "published",
                    StringComparison.Ordinal))
            .OrderBy(candidate => candidate.CourseId)
            .ThenBy(candidate => candidate.GroupId)
            .ToArray();
        var byCourse = viable
            .GroupBy(candidate => candidate.CourseId)
            .OrderBy(group => group.Key)
            .ToArray();
        for (var firstIndex = 0; firstIndex < byCourse.Length; firstIndex++)
        {
            for (var secondIndex = firstIndex + 1;
                 secondIndex < byCourse.Length;
                 secondIndex++)
            {
                var combinations = byCourse[firstIndex]
                    .SelectMany(first => byCourse[secondIndex]
                        .Select(second => (First: first, Second: second)))
                    .ToArray();
                if (combinations.Length == 0 ||
                    combinations.Any(pair =>
                        FirstOverlap(pair.First, pair.Second) is null))
                {
                    continue;
                }

                var viableFirst = combinations[0].First;
                var viableSecond = combinations[0].Second;
                var overlap = FirstOverlap(
                    viableFirst,
                    viableSecond)!.Value;

                var (firstMeeting, secondMeeting, interval) = overlap;
                return
                [
                    new(
                        $"meeting-overlap-{viableFirst.GroupId:N}-" +
                        $"{viableSecond.GroupId:N}",
                        "MEETING_OVERLAP",
                        [
                            Member(viableFirst, firstMeeting),
                            Member(viableSecond, secondMeeting)
                        ],
                        interval)
                ];
            }
        }

        if (courseIds.Length == 0)
        {
            throw new InvalidOperationException(
                "At least one selected course is required.");
        }

        return
        [
            new(
                $"credit-load-{requiredCredits:0.##}",
                "CREDIT_LOAD_MISMATCH",
                [
                    new OptimizationDiagnosticMember(
                        courseIds[0],
                        null,
                        null,
                        "remove-course")
                ])
        ];
    }

    private static OptimizationDiagnosticMember Member(
        ScheduleCandidateGroup group,
        ScheduleCandidateMeeting meeting) =>
        new(
            group.CourseId,
            group.GroupId,
            new(
                meeting.DayOfWeek,
                meeting.StartLocal,
                meeting.EndLocal),
            "change-group");

    private static (
        ScheduleCandidateMeeting First,
        ScheduleCandidateMeeting Second,
        OptimizationMeetingInterval Overlap)?
        FirstOverlap(
            ScheduleCandidateGroup first,
            ScheduleCandidateGroup second)
    {
        foreach (var firstMeeting in first.Meetings)
        {
            foreach (var secondMeeting in second.Meetings)
            {
                if (firstMeeting.DayOfWeek != secondMeeting.DayOfWeek ||
                    firstMeeting.StartLocal >= secondMeeting.EndLocal ||
                    secondMeeting.StartLocal >= firstMeeting.EndLocal)
                {
                    continue;
                }

                return (
                    firstMeeting,
                    secondMeeting,
                    new(
                        firstMeeting.DayOfWeek,
                        Max(firstMeeting.StartLocal, secondMeeting.StartLocal),
                        Min(firstMeeting.EndLocal, secondMeeting.EndLocal)));
            }
        }

        return null;
    }

    private static TimeOnly Max(TimeOnly first, TimeOnly second) =>
        first > second ? first : second;

    private static TimeOnly Min(TimeOnly first, TimeOnly second) =>
        first < second ? first : second;
}
