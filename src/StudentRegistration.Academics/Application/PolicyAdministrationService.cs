using System.Globalization;
using StudentRegistration.Academics.Domain;

namespace StudentRegistration.Academics.Application;

public sealed record PolicyDraft(
    PolicySet PolicySet,
    IReadOnlyList<PolicyRule> Rules);

public sealed record PolicyValidationResult(
    bool IsValid,
    IReadOnlyList<string> ErrorCodes);

public sealed record PolicySimulationInput(
    decimal Gpa,
    decimal EarnedCredits,
    string Standing,
    bool RegistrationWindowOpen,
    bool HasBlockingHold,
    int RequestedCredits,
    IReadOnlyList<string> RequestedCourseCodes,
    IReadOnlyList<string> CompletedCourseCodes,
    bool AllGroupsHaveCapacity,
    bool HasTimetableConflict);

public sealed record PolicyRuleSimulationResult(
    string RuleCode,
    bool Passed,
    string? RequiredValue,
    string? CurrentValue,
    string SourceReference,
    CatalogueSourceKind SourceKind,
    string Explanation);

public sealed record PolicySimulationResult(
    bool Eligible,
    string PolicyVersion,
    IReadOnlyList<PolicyRuleSimulationResult> RuleResults);

public enum PolicyPublishOutcome
{
    Published,
    Unauthorized,
    InvalidLifecycle,
}

public sealed class PolicyAdministrationService
{
    private const string GeneralSource = "SRC-GENERAL-2016";
    private const string CurriculumSource = "SRC-DATA-SCIENCE";
    private const string DemoSource = "DEMO-APPROVAL-2026.1";

    private static readonly string[] RequiredRuleCodes =
    [
        "ACADEMIC_STANDING_ALLOWED",
        "CAPACITY_REQUIRED",
        "CONFLICT_BLOCKED",
        "DS413_MIN_EARNED_CREDITS",
        "DS413_MIN_GPA",
        "NORMAL_MAX_CREDITS",
        "NORMAL_MIN_CREDITS",
        "NORMAL_RECOMMENDED_CREDITS",
        "OVERLOAD_MAX_CREDITS",
        "OVERLOAD_MIN_GPA",
        "PREREQUISITES_REQUIRED",
        "PROBATION_MAX_CREDITS",
        "REGISTRATION_WINDOW_OPEN",
    ];

    public static PolicyDraft CreateDemoPolicySet(Guid termId)
    {
        var policySet = new PolicySet(
            Guid.NewGuid(),
            "DEMO-POC-2026.1",
            termId,
            programId: null,
            "AI-DS",
            new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc),
            effectiveToUtc: null,
            PolicySetState.Draft);
        var rules = new[]
        {
            Rule(policySet, "REGISTRATION_WINDOW_OPEN", PolicyValueType.Boolean, "true", DemoSource, CatalogueSourceKind.SyntheticDemo),
            Rule(policySet, "PREREQUISITES_REQUIRED", PolicyValueType.Boolean, "true", GeneralSource, CatalogueSourceKind.OfficialSource),
            Rule(policySet, "DS413_MIN_GPA", PolicyValueType.Number, "2.0", CurriculumSource, CatalogueSourceKind.OfficialSource),
            Rule(policySet, "DS413_MIN_EARNED_CREDITS", PolicyValueType.Number, "96", CurriculumSource, CatalogueSourceKind.OfficialSource),
            Rule(policySet, "ACADEMIC_STANDING_ALLOWED", PolicyValueType.StringList, "[\"Active\"]", GeneralSource, CatalogueSourceKind.OfficialSource),
            Rule(policySet, "NORMAL_MIN_CREDITS", PolicyValueType.Number, "9", GeneralSource, CatalogueSourceKind.OfficialSource),
            Rule(policySet, "NORMAL_RECOMMENDED_CREDITS", PolicyValueType.Number, "18", DemoSource, CatalogueSourceKind.SyntheticDemo),
            Rule(policySet, "NORMAL_MAX_CREDITS", PolicyValueType.Number, "18", GeneralSource, CatalogueSourceKind.OfficialSource),
            Rule(policySet, "OVERLOAD_MAX_CREDITS", PolicyValueType.Number, "21", DemoSource, CatalogueSourceKind.SyntheticDemo),
            Rule(policySet, "OVERLOAD_MIN_GPA", PolicyValueType.Number, "3.0", DemoSource, CatalogueSourceKind.SyntheticDemo),
            Rule(policySet, "PROBATION_MAX_CREDITS", PolicyValueType.Number, "12", GeneralSource, CatalogueSourceKind.OfficialSource),
            Rule(policySet, "CAPACITY_REQUIRED", PolicyValueType.Boolean, "true", DemoSource, CatalogueSourceKind.SyntheticDemo),
            Rule(policySet, "CONFLICT_BLOCKED", PolicyValueType.Boolean, "true", DemoSource, CatalogueSourceKind.SyntheticDemo),
        };

        return new(policySet, rules);
    }

    public PolicyValidationResult Validate(PolicyDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        var codes = draft.Rules.Select(rule => rule.Code).ToArray();
        var errors = new List<string>();

        if (codes.Distinct(StringComparer.Ordinal).Count() != codes.Length)
        {
            errors.Add("DUPLICATE_RULE_CODE");
        }

        if (!RequiredRuleCodes.Order().SequenceEqual(codes.Order()))
        {
            errors.Add("POLICY_RULE_SET_INCOMPLETE");
        }

        if (draft.Rules.Any(rule => rule.PolicySetId != draft.PolicySet.Id))
        {
            errors.Add("POLICY_RULE_SCOPE_INVALID");
        }

        if (draft.PolicySet.State is not PolicySetState.Draft)
        {
            errors.Add("POLICY_NOT_EDITABLE");
        }

        if (errors.Count == 0)
        {
            draft.PolicySet.MarkValidated();
        }

        return new(errors.Count == 0, errors);
    }

    public PolicySimulationResult Simulate(
        PolicyDraft draft,
        PolicySimulationInput input)
    {
        ArgumentNullException.ThrowIfNull(draft);
        ArgumentNullException.ThrowIfNull(input);
        if (input.Gpa is < 0m or > 4m)
        {
            throw new ArgumentOutOfRangeException(nameof(input.Gpa));
        }

        if (input.EarnedCredits < 0m || input.RequestedCredits < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(input));
        }

        var requested = input.RequestedCourseCodes
            .Select(NormalizeCode)
            .ToHashSet(StringComparer.Ordinal);
        var completed = input.CompletedCourseCodes
            .Select(NormalizeCode)
            .ToHashSet(StringComparer.Ordinal);
        var curriculum = CataloguePublicationService.CreateDemoCurriculum().Courses
            .ToDictionary(course => course.Code, StringComparer.Ordinal);
        var rules = draft.Rules.ToDictionary(rule => rule.Code, StringComparer.Ordinal);
        var results = new List<PolicyRuleSimulationResult>
        {
            Result(rules["REGISTRATION_WINDOW_OPEN"], input.RegistrationWindowOpen, "open", input.RegistrationWindowOpen ? "open" : "closed"),
            Result(rules["ACADEMIC_STANDING_ALLOWED"], string.Equals(input.Standing?.Trim(), "Active", StringComparison.OrdinalIgnoreCase), "Active", input.Standing),
            Result(rules["NORMAL_MIN_CREDITS"], input.RequestedCredits >= 9, "9", input.RequestedCredits.ToString(CultureInfo.InvariantCulture)),
            Result(rules["NORMAL_RECOMMENDED_CREDITS"], true, "18", input.RequestedCredits.ToString(CultureInfo.InvariantCulture)),
            Result(rules["NORMAL_MAX_CREDITS"], input.RequestedCredits <= 18 || input.Gpa >= 3m && input.RequestedCredits <= 21, "18 or qualified overload", input.RequestedCredits.ToString(CultureInfo.InvariantCulture)),
            Result(rules["OVERLOAD_MAX_CREDITS"], input.RequestedCredits <= 21, "21", input.RequestedCredits.ToString(CultureInfo.InvariantCulture)),
            Result(rules["OVERLOAD_MIN_GPA"], input.RequestedCredits <= 18 || input.Gpa >= 3m, "3.0", input.Gpa.ToString("0.00", CultureInfo.InvariantCulture)),
            Result(rules["PROBATION_MAX_CREDITS"], input.Gpa >= 2m || input.RequestedCredits <= 12, "12", input.RequestedCredits.ToString(CultureInfo.InvariantCulture)),
            Result(rules["CAPACITY_REQUIRED"], input.AllGroupsHaveCapacity, "available", input.AllGroupsHaveCapacity ? "available" : "full"),
            Result(rules["CONFLICT_BLOCKED"], !input.HasTimetableConflict, "no-conflict", input.HasTimetableConflict ? "conflict" : "no-conflict"),
        };

        var prerequisitesPassed = requested.All(code =>
            !curriculum.TryGetValue(code, out var course)
            || course.PrerequisiteCodes.All(completed.Contains));
        results.Add(Result(
            rules["PREREQUISITES_REQUIRED"],
            prerequisitesPassed,
            "all-completed",
            prerequisitesPassed ? "all-completed" : "missing"));

        var requestsProject = requested.Contains("DS413");
        results.Add(Result(
            rules["DS413_MIN_GPA"],
            !requestsProject || input.Gpa >= 2m,
            "2.0",
            input.Gpa.ToString("0.00", CultureInfo.InvariantCulture)));
        results.Add(Result(
            rules["DS413_MIN_EARNED_CREDITS"],
            !requestsProject || input.EarnedCredits >= 96m,
            "96",
            input.EarnedCredits.ToString("0.##", CultureInfo.InvariantCulture)));

        var ordered = results.OrderBy(result => result.RuleCode, StringComparer.Ordinal).ToArray();
        return new(
            ordered.All(result => result.Passed),
            draft.PolicySet.VersionCode,
            ordered);
    }

    public PolicyPublishOutcome Publish(PolicyDraft draft, bool canPublish)
    {
        ArgumentNullException.ThrowIfNull(draft);
        if (!canPublish)
        {
            return PolicyPublishOutcome.Unauthorized;
        }

        if (draft.PolicySet.State is not PolicySetState.Validated)
        {
            return PolicyPublishOutcome.InvalidLifecycle;
        }

        draft.PolicySet.Publish();
        return PolicyPublishOutcome.Published;
    }

    private static PolicyRule Rule(
        PolicySet policySet,
        string code,
        PolicyValueType valueType,
        string value,
        string sourceReference,
        CatalogueSourceKind sourceKind) =>
        new(
            Guid.NewGuid(),
            policySet.Id,
            code,
            $"{code}_FAILED",
            valueType,
            value,
            sourceReference,
            sourceKind);

    private static PolicyRuleSimulationResult Result(
        PolicyRule rule,
        bool passed,
        string? required,
        string? current) =>
        new(
            rule.Code,
            passed,
            required,
            current,
            rule.SourceReference,
            rule.SourceKind,
            passed
                ? $"{rule.Code} passed."
                : $"{rule.Code} failed: required {required}, current {current}.");

    private static string NormalizeCode(string code) =>
        string.IsNullOrWhiteSpace(code)
            ? throw new ArgumentException("A course code is required.", nameof(code))
            : code.Trim().Normalize().ToUpperInvariant();
}
