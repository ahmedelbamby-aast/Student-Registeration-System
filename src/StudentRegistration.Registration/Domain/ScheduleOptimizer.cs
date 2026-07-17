namespace StudentRegistration.Registration.Domain;

public sealed record ScheduleOptimizationResult(
    IReadOnlyList<ScheduleOption> Options,
    IReadOnlyList<Guid> OrderedCourseIds,
    int PrunedPartialSchedules,
    int VisitedNodes,
    bool SearchLimitReached);

public sealed class ScheduleOptimizer
{
    private const int MaximumVisitedNodes = 100_000;
    private readonly ScheduleScorer _scorer;

    public ScheduleOptimizer(ScheduleScorer scorer)
    {
        ArgumentNullException.ThrowIfNull(scorer);
        _scorer = scorer;
    }

    public ScheduleOptimizationResult Optimize(
        IReadOnlyList<ScheduleCandidateGroup> candidates,
        SchedulePreferences preferences,
        decimal requiredCredits,
        CancellationToken cancellationToken = default) =>
        Optimize(
            candidates,
            preferences,
            requiredCredits,
            requiredCourseIds: null,
            cancellationToken);

    public ScheduleOptimizationResult Optimize(
        IReadOnlyList<ScheduleCandidateGroup> candidates,
        SchedulePreferences preferences,
        decimal requiredCredits,
        IReadOnlyCollection<Guid>? requiredCourseIds,
        CancellationToken cancellationToken = default,
        Func<bool>? budgetExpired = null)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(preferences);
        if (requiredCredits <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(requiredCredits));
        }

        var allCourses = (requiredCourseIds ??
                candidates.Select(candidate => candidate.CourseId).ToArray())
            .Distinct()
            .Order()
            .ToArray();
        if (allCourses.Any(courseId => courseId == Guid.Empty) ||
            (requiredCourseIds is not null &&
             allCourses.Length != requiredCourseIds.Count))
        {
            throw new ArgumentException(
                "Required course identifiers must be non-empty and unique.",
                nameof(requiredCourseIds));
        }

        var normalizedCandidates = candidates
            .GroupBy(candidate =>
                (candidate.CourseId, candidate.OfferingId, candidate.GroupId))
            .Select(group =>
            {
                var first = group.First();
                if (group.Any(candidate =>
                        candidate.Credits != first.Credits ||
                        candidate.State != first.State ||
                        candidate.Viable != first.Viable ||
                        !candidate.Meetings.SequenceEqual(first.Meetings)))
                {
                    throw new ArgumentException(
                        "Duplicate candidate identities must have identical values.",
                        nameof(candidates));
                }

                return first;
            })
            .ToArray();
        var byCourse = normalizedCandidates
            .Where(candidate =>
                allCourses.Contains(candidate.CourseId) &&
                candidate.Viable &&
                string.Equals(
                    candidate.State,
                    "published",
                    StringComparison.Ordinal))
            .GroupBy(candidate => candidate.CourseId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(candidate => candidate.GroupId)
                    .ToArray());
        var orderedCourses = allCourses
            .OrderBy(courseId =>
                byCourse.TryGetValue(courseId, out var groups)
                    ? groups.Length
                    : 0)
            .ThenBy(courseId => courseId)
            .ToArray();

        if (orderedCourses.Length == 0 ||
            orderedCourses.Any(courseId =>
                !byCourse.TryGetValue(courseId, out var groups) ||
                groups.Length == 0))
        {
            return new([], orderedCourses, 0, 0, false);
        }

        var best = new List<ScheduleCandidateGroup[]>();
        var selected = new List<ScheduleCandidateGroup>(orderedCourses.Length);
        var pruned = 0;
        var visited = 0;
        var limitReached = false;

        Search(0, 0m);

        var options = best
            .OrderBy(
                schedule => schedule,
                Comparer<ScheduleCandidateGroup[]>.Create(
                    (first, second) =>
                        _scorer.Compare(first, second, preferences)))
            .Take(3)
            .Select((schedule, index) =>
                new ScheduleOption(
                    index + 1,
                    schedule.Select(group =>
                        new ScheduleOptionSelection(
                            group.CourseId,
                            group.OfferingId,
                            group.GroupId))
                        .ToArray(),
                    _scorer.Score(schedule, preferences)))
            .ToArray();

        return new(
            options,
            Array.AsReadOnly(orderedCourses),
            pruned,
            visited,
            limitReached);

        void Search(int courseIndex, decimal credits)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (budgetExpired?.Invoke() == true)
            {
                limitReached = true;
                return;
            }
            if (visited >= MaximumVisitedNodes)
            {
                limitReached = true;
                return;
            }

            visited++;
            if (courseIndex == orderedCourses.Length)
            {
                if (credits == requiredCredits)
                {
                    AddToBest(selected.ToArray());
                }
                else
                {
                    pruned++;
                }

                return;
            }

            foreach (var candidate in byCourse[orderedCourses[courseIndex]])
            {
                if (credits + candidate.Credits > requiredCredits ||
                    OverlapsSelected(candidate, selected))
                {
                    pruned++;
                    continue;
                }

                selected.Add(candidate);
                Search(courseIndex + 1, credits + candidate.Credits);
                selected.RemoveAt(selected.Count - 1);
                if (limitReached)
                {
                    return;
                }
            }
        }

        void AddToBest(ScheduleCandidateGroup[] schedule)
        {
            best.Add(schedule);
            best.Sort((first, second) =>
                _scorer.Compare(first, second, preferences));
            if (best.Count > 3)
            {
                best.RemoveAt(best.Count - 1);
            }
        }
    }

    private static bool OverlapsSelected(
        ScheduleCandidateGroup candidate,
        IReadOnlyList<ScheduleCandidateGroup> selected) =>
        selected.Any(existing =>
            existing.Meetings.Any(first =>
                candidate.Meetings.Any(second =>
                    first.DayOfWeek == second.DayOfWeek &&
                    first.StartLocal < second.EndLocal &&
                    second.StartLocal < first.EndLocal)));
}
