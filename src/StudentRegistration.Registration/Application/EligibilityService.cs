using System.Globalization;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Domain;
using StudentRegistration.Scheduling.Application.Ports;

namespace StudentRegistration.Registration.Application;

public enum EligibilityEvaluationOutcome
{
    Found,
    ContextNotFound,
    OfferingNotFound,
    DependencyUnavailable
}

public sealed record DiscoveryMetadata(
    string PolicyVersion,
    IReadOnlyList<string> ReasonCodes,
    string RegistrationWindowState,
    string SupportReferencePath);

public sealed record EligibilityEvaluationResult(
    EligibilityEvaluationOutcome Outcome,
    IReadOnlyList<OfferingEligibility> Items,
    OfferingEligibility? Offering = null,
    string? ErrorCode = null,
    DiscoveryMetadata? Metadata = null);

public sealed class EligibilityService(
    IEligibilityAcademicReader academics,
    IEligibilityOfferingReader offerings,
    ICurrentPlanReader currentPlan,
    GroupSummaryProjection groups,
    TimeProvider timeProvider)
{
    public const string SupportReferencePath = "/support/registrar";
    private const decimal DefaultTargetCredits = 18m;
    private const decimal NormalMaximumCredits = 18m;
    private const decimal ProbationMaximumCredits = 12m;
    private static readonly DateOnly UnavailableSourceDate = new(2026, 7, 13);
    private static readonly DateTime UnavailableEffectiveFromUtc =
        new(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc);

    public async Task<EligibilityEvaluationResult> EvaluateTermAsync(
        Guid applicationUserId,
        Guid termId,
        CancellationToken cancellationToken = default) =>
        await EvaluateTermAtAsync(
            applicationUserId,
            termId,
            UtcNow(),
            cancellationToken);

    public async Task<EligibilityEvaluationResult> EvaluateTermAtAsync(
        Guid applicationUserId,
        Guid termId,
        DateTime evaluatedAtUtc,
        CancellationToken cancellationToken = default)
    {
        if (applicationUserId == Guid.Empty ||
            termId == Guid.Empty ||
            evaluatedAtUtc.Kind is not DateTimeKind.Utc)
        {
            return Failure(
                EligibilityEvaluationOutcome.ContextNotFound,
                "REGISTRATION_CONTEXT_NOT_FOUND");
        }

        try
        {
            return await EvaluateTermCoreAsync(
                applicationUserId,
                termId,
                evaluatedAtUtc,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return Failure(
                EligibilityEvaluationOutcome.DependencyUnavailable,
                "DISCOVERY_UNAVAILABLE");
        }
    }

    public Task<EligibilityEvaluationResult> EvaluateTermForCommitAsync(
        Guid applicationUserId,
        Guid termId,
        DateTime evaluatedAtUtc,
        CancellationToken cancellationToken = default)
    {
        if (applicationUserId == Guid.Empty ||
            termId == Guid.Empty ||
            evaluatedAtUtc.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "A valid authenticated registration context and UTC ingress time are required.");
        }

        return EvaluateTermCoreAsync(
            applicationUserId,
            termId,
            evaluatedAtUtc,
            cancellationToken);
    }

    private async Task<EligibilityEvaluationResult> EvaluateTermCoreAsync(
        Guid applicationUserId,
        Guid termId,
        DateTime evaluatedAtUtc,
        CancellationToken cancellationToken)
    {
        var offeringSnapshots = await offerings.ListForTermAsync(
            termId,
            cancellationToken);
        var academic = await academics.ReadAsync(
            applicationUserId,
            termId,
            offeringSnapshots.Select(item => item.CourseId).Distinct().ToArray(),
            evaluatedAtUtc,
            cancellationToken);
        if (academic is null || academic.StudentId == Guid.Empty)
        {
            return Failure(
                EligibilityEvaluationOutcome.ContextNotFound,
                "REGISTRATION_CONTEXT_NOT_FOUND");
        }

        var plan = await currentPlan.ReadAsync(
            academic.StudentId,
            termId,
            cancellationToken);
        var decisions = offeringSnapshots
            .OrderBy(item => item.OfferingId)
            .Select(item => Evaluate(item, academic, plan, evaluatedAtUtc))
            .ToArray();
        return new(
            EligibilityEvaluationOutcome.Found,
            decisions,
            Metadata: Metadata(academic, decisions));
    }

    public async Task<EligibilityEvaluationResult> EvaluateOfferingAsync(
        Guid applicationUserId,
        Guid offeringId,
        CancellationToken cancellationToken = default) =>
        await EvaluateOfferingAtAsync(
            applicationUserId,
            offeringId,
            UtcNow(),
            cancellationToken);

    public async Task<EligibilityEvaluationResult> EvaluateOfferingAtAsync(
        Guid applicationUserId,
        Guid offeringId,
        DateTime evaluatedAtUtc,
        CancellationToken cancellationToken = default)
    {
        if (applicationUserId == Guid.Empty ||
            offeringId == Guid.Empty ||
            evaluatedAtUtc.Kind is not DateTimeKind.Utc)
        {
            return Failure(
                EligibilityEvaluationOutcome.OfferingNotFound,
                "OFFERING_NOT_FOUND_OR_OUTSIDE_CONTEXT");
        }

        try
        {
            var offering = await offerings.FindAsync(offeringId, cancellationToken);
            if (offering is null)
            {
                return Failure(
                    EligibilityEvaluationOutcome.OfferingNotFound,
                    "OFFERING_NOT_FOUND_OR_OUTSIDE_CONTEXT");
            }

            var academic = await academics.ReadAsync(
                applicationUserId,
                offering.TermId,
                [offering.CourseId],
                evaluatedAtUtc,
                cancellationToken);
            if (academic is null || academic.StudentId == Guid.Empty)
            {
                return Failure(
                    EligibilityEvaluationOutcome.OfferingNotFound,
                    "OFFERING_NOT_FOUND_OR_OUTSIDE_CONTEXT");
            }

            var plan = await currentPlan.ReadAsync(
                academic.StudentId,
                offering.TermId,
                cancellationToken);
            var decision = Evaluate(offering, academic, plan, evaluatedAtUtc);
            return new(
                EligibilityEvaluationOutcome.Found,
                [decision],
                decision,
                Metadata: Metadata(academic, [decision]));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return Failure(
                EligibilityEvaluationOutcome.DependencyUnavailable,
                "DISCOVERY_UNAVAILABLE");
        }
    }

    private OfferingEligibility Evaluate(
        EligibilityOfferingSnapshot offering,
        EligibilityAcademicSnapshot academic,
        CurrentPlanSnapshot plan,
        DateTime evaluatedAtUtc)
    {
        var course = academic.Catalogue?.Courses.SingleOrDefault(
            item => item.CourseId == offering.CourseId);
        var policy = academic.Policy;
        var offeringAlreadySelected = plan.Selections.Any(selection =>
            selection.OfferingId == offering.OfferingId);
        var projectedCredits = plan.Credits + (offeringAlreadySelected
            ? 0m
            : course?.Credits ?? 0m);
        var maximum = academic.Gpa < 2m
            ? ProbationMaximumCredits
            : NormalMaximumCredits;
        var otherPlanMeetings = plan.Selections
            .Where(selection => selection.OfferingId != offering.OfferingId)
            .SelectMany(selection => selection.Meetings)
            .ToArray();
        var groupModels = offering.Groups
            .OrderBy(item => item.GroupCode, StringComparer.Ordinal)
            .ThenBy(item => item.GroupId)
            .Select(item => groups.Project(item, otherPlanMeetings))
            .ToArray();
        var reasons = new List<EligibilityReason>();

        if (course is null || academic.Catalogue is null)
        {
            reasons.Add(UnavailableReason(
                "CATALOGUE_UNAVAILABLE",
                "The approved catalogue or course projection is unavailable.",
                policy));
            reasons.Add(UnavailableReason(
                "DECISION_DATA_UNAVAILABLE",
                "A complete eligibility decision cannot be constructed.",
                policy));
        }
        else if (!HasCompletePolicy(policy))
        {
            reasons.Add(UnavailableReason(
                "POLICY_UNAVAILABLE",
                "The complete approved policy set is unavailable.",
                policy));
            reasons.Add(UnavailableReason(
                "DECISION_DATA_UNAVAILABLE",
                "A complete eligibility decision cannot be constructed.",
                policy));
        }
        else
        {
            AddGovernedReasons(
                reasons,
                course,
                academic,
                plan,
                projectedCredits,
                groupModels,
                policy!);
        }

        if (groupModels.Any(group =>
            group.NonSelectableReasons.Any(reason =>
                reason.Code is "GROUP_INCOMPLETE"
                    or "ROOM_UNAVAILABLE"
                    or "GROUP_STATE_UNAVAILABLE"))
            && reasons.All(reason => reason.Code != "DECISION_DATA_UNAVAILABLE"))
        {
            reasons.Add(UnavailableReason(
                "DECISION_DATA_UNAVAILABLE",
                "A complete selectable group projection is unavailable.",
                policy));
        }

        var eligible = reasons.All(reason => reason.Passed || !reason.Blocking)
            && groupModels.Any(group => group.Selectable);
        var inputSummary = new SortedDictionary<string, string>(
            StringComparer.Ordinal)
        {
            ["blockingHoldCount"] = academic.ActiveHolds.Count(
                hold => hold.BlocksRegistration).ToString(CultureInfo.InvariantCulture),
            ["currentPlanCredits"] = plan.Credits.ToString(
                CultureInfo.InvariantCulture),
            ["earnedCredits"] = academic.EarnedCredits.ToString(
                CultureInfo.InvariantCulture),
            ["gpa"] = academic.Gpa.ToString(CultureInfo.InvariantCulture),
            ["groupCount"] = groupModels.Length.ToString(
                CultureInfo.InvariantCulture),
            ["maximumAllowedCredits"] = maximum.ToString(
                CultureInfo.InvariantCulture),
            ["projectedPlanCredits"] = projectedCredits.ToString(
                CultureInfo.InvariantCulture),
            ["standing"] = string.IsNullOrWhiteSpace(academic.Standing)
                ? "unavailable"
                : academic.Standing
        };

        return new OfferingEligibility(
            offering.OfferingId,
            course?.Code ?? offering.CourseId.ToString("D"),
            course?.Title ?? "Course data unavailable",
            course?.Credits ?? 0m,
            plan.Credits,
            projectedCredits,
            DefaultTargetCredits,
            maximum,
            eligible,
            reasons,
            groupModels,
            inputSummary,
            evaluatedAtUtc,
            Encode(academic.AcademicContextVersion),
            academic.Catalogue?.VersionCode ?? "unavailable",
            policy?.PolicySetId ?? Guid.Empty,
            policy?.Version ?? "unavailable",
            Encode(offering.RowVersion),
            string.IsNullOrWhiteSpace(plan.Version)
                ? "unavailable"
                : plan.Version);
    }

    private static void AddGovernedReasons(
        List<EligibilityReason> reasons,
        EligibilityCourseSnapshot course,
        EligibilityAcademicSnapshot academic,
        CurrentPlanSnapshot plan,
        decimal projectedCredits,
        IReadOnlyList<GroupSummary> groupModels,
        EligibilityPolicySnapshot policy)
    {
        Add(reasons, policy, "RegistrationWindow",
            academic.RegistrationWindow.IsOpen,
            "REGISTRATION_WINDOW_OPEN",
            "REGISTRATION_WINDOW_CLOSED",
            "The configured registration window is open.",
            "The configured registration window is closed.");

        var standingAllowed = string.Equals(
            academic.Standing,
            "Active",
            StringComparison.OrdinalIgnoreCase);
        Add(reasons, policy, "AcademicStanding", standingAllowed,
            "ACADEMIC_STANDING_ELIGIBLE",
            "ACADEMIC_STANDING_UNAVAILABLE",
            "The academic standing is eligible.",
            "The academic standing is unavailable or not approved.",
            "Active",
            academic.Standing);

        var blockingHolds = academic.ActiveHolds.Count(hold => hold.BlocksRegistration);
        Add(reasons, policy, "BlockingHold", blockingHolds == 0,
            "NO_BLOCKING_HOLD",
            "REGISTRATION_HOLD",
            "No active blocking hold applies.",
            "An active hold blocks registration.",
            "0",
            blockingHolds.ToString(CultureInfo.InvariantCulture));

        var passedCodes = academic.CurrentTranscriptLeaves
            .Where(item => item.IsCurrentLeaf && Is(item.Status, "passed"))
            .Select(item => item.CourseCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingPrerequisites = course.PrerequisiteCourseCodes
            .Where(code => !passedCodes.Contains(code))
            .Order(StringComparer.Ordinal)
            .ToArray();
        Add(reasons, policy, "Prerequisite", missingPrerequisites.Length == 0,
            "PREREQUISITES_MET",
            "PREREQUISITE_NOT_COMPLETED",
            "All course prerequisites are completed.",
            "One or more course prerequisites are not completed.",
            string.Join(",", course.PrerequisiteCourseCodes.Order()),
            string.Join(",", passedCodes.Order()));

        if (course.MinimumGpa is { } minimumGpa)
        {
            Add(reasons, policy, "Prerequisite", academic.Gpa >= minimumGpa,
                "MINIMUM_GPA_MET",
                "MINIMUM_GPA_NOT_MET",
                "The course GPA condition is met.",
                "The course GPA condition is not met.",
                minimumGpa.ToString(CultureInfo.InvariantCulture),
                academic.Gpa.ToString(CultureInfo.InvariantCulture));
        }

        if (course.MinimumEarnedCredits is { } minimumEarnedCredits)
        {
            Add(
                reasons,
                policy,
                "Prerequisite",
                academic.EarnedCredits >= minimumEarnedCredits,
                "MINIMUM_EARNED_CREDITS_MET",
                "MINIMUM_EARNED_CREDITS_NOT_MET",
                "The earned-credit condition is met.",
                "The earned-credit condition is not met.",
                minimumEarnedCredits.ToString(CultureInfo.InvariantCulture),
                academic.EarnedCredits.ToString(CultureInfo.InvariantCulture));
        }

        var normalAllowed = academic.Gpa < 2m
            || projectedCredits <= NormalMaximumCredits;
        Add(reasons, policy, "CreditLoad", normalAllowed,
            "LOAD_ALLOWED",
            "LOAD_ABOVE_NORMAL_MAXIMUM",
            "The projected load is within the normal maximum.",
            "The projected load exceeds the normal maximum.",
            NormalMaximumCredits.ToString(CultureInfo.InvariantCulture),
            projectedCredits.ToString(CultureInfo.InvariantCulture));

        var probationAllowed = academic.Gpa >= 2m
            || projectedCredits <= ProbationMaximumCredits;
        Add(reasons, policy, "ProbationLoad", probationAllowed,
            academic.Gpa >= 2m
                ? "PROBATION_NOT_APPLICABLE"
                : "PROBATION_LOAD_ALLOWED",
            "PROBATION_LOAD_EXCEEDED",
            academic.Gpa >= 2m
                ? "The probation maximum does not apply."
                : "The projected load is within the probation maximum.",
            "The projected load exceeds the probation maximum.",
            ProbationMaximumCredits.ToString(CultureInfo.InvariantCulture),
            projectedCredits.ToString(CultureInfo.InvariantCulture));

        var currentCoursePassed = academic.CurrentTranscriptLeaves.Any(item =>
            item.IsCurrentLeaf
            && Is(item.Status, "passed")
            && Is(item.CourseCode, course.Code));
        Add(reasons, policy, "RepeatEligibility", !currentCoursePassed,
            "REPEAT_NOT_APPLICABLE",
            "REPEAT_POLICY_UNAVAILABLE",
            "No repeat-policy interpretation is required.",
            "A passed course requires an unapproved repeat-policy interpretation.");

        var capacityAvailable = groupModels.Any(group =>
            group.State == "published"
            && group.SeatsRemaining > 0
            && group.NonSelectableReasons.All(item =>
                item.Code is not "REGISTRATION_PAUSED"));
        Add(reasons, policy, "Capacity", capacityAvailable,
            "CAPACITY_AVAILABLE",
            "GROUP_FULL",
            "At least one published group currently has a seat.",
            "No published group currently has an available seat.");

        var conflictFree = groupModels.Any(group =>
            group.State == "published"
            && group.SeatsRemaining > 0
            && group.NonSelectableReasons.All(item =>
                item.Code is not "MEETING_CONFLICT"));
        Add(reasons, policy, "MeetingConflict", conflictFree,
            "NO_MEETING_CONFLICT",
            "MEETING_CONFLICT",
            "At least one group has no exact meeting overlap.",
            "Every otherwise available group has an exact meeting overlap.");
    }

    private static void Add(
        List<EligibilityReason> reasons,
        EligibilityPolicySnapshot policy,
        string ruleType,
        bool passed,
        string passedCode,
        string failedCode,
        string passedMessage,
        string failedMessage,
        string? required = null,
        string? current = null)
    {
        var rule = policy.Rules.Single(item => Is(item.TypeKey, ruleType));
        reasons.Add(new EligibilityReason(
            passed ? passedCode : failedCode,
            passed,
            !passed,
            passed ? passedMessage : failedMessage,
            required,
            current,
            policy.PolicySetId,
            policy.Version,
            rule.SourceReference,
            rule.SourceAccessedOn,
            policy.ApprovedBy,
            policy.EffectiveFromUtc,
            policy.EffectiveToUtc,
            false,
            passed ? null : SupportReferencePath));
    }

    private static bool HasCompletePolicy(EligibilityPolicySnapshot? policy)
    {
        if (policy is null ||
            policy.PolicySetId == Guid.Empty ||
            string.IsNullOrWhiteSpace(policy.Version) ||
            string.IsNullOrWhiteSpace(policy.ApprovedBy) ||
            string.IsNullOrWhiteSpace(policy.ApprovalReference))
        {
            return false;
        }

        string[] required =
        [
            "RegistrationWindow",
            "AcademicStanding",
            "BlockingHold",
            "Prerequisite",
            "CreditLoad",
            "ProbationLoad",
            "RepeatEligibility",
            "Capacity",
            "MeetingConflict"
        ];
        return required.All(type =>
            policy.Rules.Count(rule => Is(rule.TypeKey, type)) == 1
            && policy.Rules.Where(rule => Is(rule.TypeKey, type)).All(rule =>
                !string.IsNullOrWhiteSpace(rule.SourceReference)
                && rule.SourceAccessedOn != default));
    }

    private static EligibilityReason UnavailableReason(
        string code,
        string message,
        EligibilityPolicySnapshot? policy) =>
        new(
            code,
            false,
            true,
            message,
            null,
            null,
            policy?.PolicySetId ?? Guid.Empty,
            policy?.Version ?? "unavailable",
            policy?.ApprovalReference ?? "DEMO-APPROVAL-2026.1",
            policy?.Rules.FirstOrDefault()?.SourceAccessedOn ??
                UnavailableSourceDate,
            policy?.ApprovedBy ?? "Ahmed ELbamby",
            policy?.EffectiveFromUtc ?? UnavailableEffectiveFromUtc,
            policy?.EffectiveToUtc,
            false,
            SupportReferencePath);

    private static DiscoveryMetadata Metadata(
        EligibilityAcademicSnapshot academic,
        IReadOnlyList<OfferingEligibility> decisions)
    {
        var codes = decisions
            .SelectMany(item => item.Reasons)
            .Select(item => item.Code)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return new(
            academic.Policy?.Version ?? "unavailable",
            codes,
            academic.RegistrationWindow.IsOpen ? "open" : "closed",
            SupportReferencePath);
    }

    private static EligibilityEvaluationResult Failure(
        EligibilityEvaluationOutcome outcome,
        string errorCode) =>
        new(
            outcome,
            [],
            ErrorCode: errorCode,
            Metadata: new DiscoveryMetadata(
                "unavailable",
                [],
                "unavailable",
                SupportReferencePath));

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;

    private static string Encode(byte[]? value) =>
        value is { Length: > 0 }
            ? Convert.ToBase64String(value)
            : "unavailable";

    private static bool Is(string? value, string expected) =>
        string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
}
