using System.Collections.ObjectModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace StudentRegistration.TestSupport.Spec002;

/// <summary>
/// Executes the governed SPEC-002 demo contract in tests only. Production policy
/// evaluation remains owned by SPEC-009.
/// </summary>
public sealed class Spec002PolicyTestHarness
{
    public const string Version = "DEMO-POC-2026.1";
    public const string EvidenceScope = "governance-contract-reference-only";

    private static readonly DateTimeOffset EffectiveFrom =
        new(2026, 7, 13, 0, 0, 0, TimeSpan.Zero);
    private static readonly HashSet<string> KnownRuleTypes =
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

    private readonly IReadOnlyDictionary<string, PolicySourceRecord> sourcesById;

    private Spec002PolicyTestHarness(IReadOnlyList<PolicySourceRecord> sources)
    {
        Sources = sources;
        sourcesById = Sources.ToDictionary(source => source.Id, StringComparer.Ordinal);
        BoundaryCases = CreateBoundaryCases();
    }

    public IReadOnlyList<PolicyBoundaryCase> BoundaryCases { get; }

    public IReadOnlyList<PolicySourceRecord> Sources { get; }

    public static Spec002PolicyTestHarness Load()
    {
        var sources = VerifyAndLoadGovernedArtifacts();
        return new Spec002PolicyTestHarness(sources);
    }

    public PolicyDecision Evaluate(PolicyEvaluationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var results = new List<PolicyDecisionResult>();
        var selection = SelectPolicy(input);
        if (!selection.Selected)
        {
            AddResult(
                results,
                passed: false,
                selection.FailureReasonCode!,
                selection.FailureReasonCode == "POLICY_UNAVAILABLE"
                    ? "No approved, published, effective rulebook matches the authoritative context."
                    : "More than one applicable rulebook has equal highest priority.",
                "DEMO-APPROVAL-2026.1");

            var failClosedSummary = CreateInputSummary(input, selection);
            var failClosedResult = results[0];
            return new PolicyDecision(
                Eligible: false,
                PolicyVersion: "NOT_SELECTED",
                input.EvaluatedAtUtc,
                failClosedSummary,
                ApprovedBy: "Not selected",
                EffectiveFromUtc: default,
                EffectiveToUtc: null,
                new ReadOnlyCollection<PolicyDecisionResult>(results),
                failClosedResult.ReasonCode,
                failClosedResult,
                MaximumCredits: null,
                UsedFallback: false,
                AdminAlertRaised: true,
                CreateFingerprint(
                    "NOT_SELECTED",
                    input,
                    failClosedSummary,
                    results,
                    eligible: false,
                    failClosedResult.ReasonCode));
        }

        AddResult(
            results,
            passed: true,
            "POLICY_AVAILABLE",
            "Exactly one approved, published, effective rulebook matches the context.",
            "DEMO-APPROVAL-2026.1");
        AddResult(
            results,
            input.WithinRegistrationWindow,
            input.WithinRegistrationWindow ? "REGISTRATION_WINDOW_OPEN" : "REGISTRATION_WINDOW_CLOSED",
            input.WithinRegistrationWindow
                ? "The configured registration window contains server time."
                : "Server time is outside the configured registration window.",
            "DEMO-APPROVAL-2026.1");

        var standingAllowed = string.Equals(input.StandingCode, "Active", StringComparison.Ordinal);
        AddResult(
            results,
            standingAllowed,
            standingAllowed ? "ACADEMIC_STANDING_ALLOWED" : "ACADEMIC_STANDING_UNAVAILABLE",
            standingAllowed
                ? "The authoritative standing is Active."
                : "The authoritative standing is absent or not approved for registration.",
            "DEMO-APPROVAL-2026.1");

        AddResult(
            results,
            !input.HasBlockingHold,
            input.HasBlockingHold ? "REGISTRATION_HOLD" : "NO_BLOCKING_HOLD",
            input.HasBlockingHold
                ? "A current hold is configured to block registration."
                : "No current hold blocks registration.",
            "DEMO-APPROVAL-2026.1");

        AddResult(
            results,
            input.PrerequisitesMet,
            input.PrerequisitesMet ? "PREREQUISITES_MET" : "PREREQUISITE_NOT_COMPLETED",
            input.PrerequisitesMet
                ? "Every required completed-course prerequisite is satisfied."
                : "A required completed-course prerequisite is not satisfied.",
            "SRC-DATA-SCIENCE");

        var earnedCreditsMet = input.EarnedCredits >= input.RequiredEarnedCredits;
        AddResult(
            results,
            earnedCreditsMet,
            earnedCreditsMet
                ? "MINIMUM_EARNED_CREDITS_MET"
                : "MINIMUM_EARNED_CREDITS_NOT_MET",
            earnedCreditsMet
                ? "The minimum earned-credit condition is satisfied."
                : $"The selection requires {input.RequiredEarnedCredits} earned credits.",
            "SRC-DATA-SCIENCE");

        AddCreditLoadResults(input, results);

        AddResult(
            results,
            !input.RequiresRepeatInterpretation,
            input.RequiresRepeatInterpretation
                ? "REPEAT_POLICY_UNAVAILABLE"
                : "NO_REPEAT_REVIEW_REQUIRED",
            input.RequiresRepeatInterpretation
                ? "The demo has no approved repeat workflow, so repeat interpretation fails closed."
                : "No earlier attempt requires repeat interpretation.",
            "UNRESOLVED-REPEAT-WORKFLOW");

        var capacityAvailable = input.CommittedEnrollments < input.GroupCapacity;
        AddResult(
            results,
            capacityAvailable,
            capacityAvailable ? "CAPACITY_AVAILABLE" : "GROUP_FULL",
            capacityAvailable
                ? "The published group has remaining advisory capacity."
                : "The published group has no remaining capacity.",
            "DEMO-CAPACITY-FIRST-COMMIT");

        var hasConflict = HasExactMeetingOverlap(input.Meetings);
        AddResult(
            results,
            !hasConflict,
            hasConflict ? "MEETING_CONFLICT" : "NO_MEETING_CONFLICT",
            hasConflict
                ? "Selected meetings overlap and submission is blocked."
                : "Selected meetings do not overlap; the configured travel buffer is zero.",
            "DEMO-CONFLICT-EXACT");

        var firstFailure = results.FirstOrDefault(result => !result.Passed);
        var reasonCode = firstFailure?.ReasonCode ?? SelectSuccessfulReason(input);
        var primaryResult = results.Single(result => result.ReasonCode == reasonCode);
        var eligible = firstFailure is null;
        var maximumCredits = input.Gpa < 2.0m ? 12 : 18;
        var summary = CreateInputSummary(input, selection);
        var fingerprint = CreateFingerprint(
            selection.Rulebook!.Version,
            input,
            summary,
            results,
            eligible,
            reasonCode);

        return new PolicyDecision(
            eligible,
            selection.Rulebook.Version,
            input.EvaluatedAtUtc,
            summary,
            selection.Rulebook.ApprovedBy,
            selection.Rulebook.EffectiveFromUtc,
            selection.Rulebook.EffectiveToUtc,
            new ReadOnlyCollection<PolicyDecisionResult>(results),
            reasonCode,
            primaryResult,
            maximumCredits,
            UsedFallback: false,
            AdminAlertRaised: false,
            fingerprint);
    }

    public PolicyPublicationValidation ValidatePublication(PolicyPublicationDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        var rejectionCodes = new List<string>();

        if (!KnownRuleTypes.Contains(draft.RuleType))
        {
            rejectionCodes.Add("UNKNOWN_RULE_TYPE");
        }

        if (!string.IsNullOrWhiteSpace(draft.ExecutableExpression))
        {
            rejectionCodes.Add("EXECUTABLE_POLICY_CONTENT_FORBIDDEN");
        }

        if (draft.WaitlistEnabled)
        {
            rejectionCodes.Add("WAITLIST_NOT_APPROVED");
        }

        if (draft.CapacityOverrideEnabled)
        {
            rejectionCodes.Add("CAPACITY_OVERRIDE_NOT_APPROVED");
        }

        if (draft.CandidateRulebooks.Count == 0)
        {
            rejectionCodes.Add("POLICY_UNAVAILABLE");
        }
        else if (HasAmbiguousPublication(draft.CandidateRulebooks))
        {
            rejectionCodes.Add("POLICY_SCOPE_AMBIGUOUS");
        }

        if (!draft.HasCompleteProvenance)
        {
            rejectionCodes.Add("POLICY_PROVENANCE_INCOMPLETE");
        }

        var requiresAmendment = draft.WaitlistEnabled || draft.CapacityOverrideEnabled;
        return new PolicyPublicationValidation(
            rejectionCodes.Count == 0,
            new ReadOnlyCollection<string>(rejectionCodes),
            requiresAmendment,
            StoredExecutableContent: false);
    }

    public PolicyDecisionSnapshot Capture(PolicyDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);
        return new PolicyDecisionSnapshot(
            decision.PolicyVersion,
            decision.EvaluatedAtUtc,
            new ReadOnlyDictionary<string, string>(
                new SortedDictionary<string, string>(
                    decision.InputSummary.ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value,
                        StringComparer.Ordinal),
                    StringComparer.Ordinal)),
            decision.PrimaryResult.Source,
            decision.PrimaryResult.Explanation,
            decision.PrimaryResult.ReasonCode,
            decision.DeterministicFingerprint);
    }

    public PolicyHistoryView PublishSuccessor(
        PolicyDecisionSnapshot original,
        string successorVersion)
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentException.ThrowIfNullOrWhiteSpace(successorVersion);
        if (string.Equals(original.PolicyVersion, successorVersion, StringComparison.Ordinal))
        {
            throw new ArgumentException("A successor must use a new version.", nameof(successorVersion));
        }

        return new PolicyHistoryView(original, successorVersion);
    }

    public PolicySourceOutageReview MarkSourceUnavailable(PolicyDecisionSnapshot historicalDecision)
    {
        ArgumentNullException.ThrowIfNull(historicalDecision);
        var current = historicalDecision.Source with
        {
            Availability = "Unavailable",
            ReviewRequired = true
        };
        var currentRegister = Sources
            .Select(source => source.Id == current.Id ? current : source)
            .ToArray();
        return new PolicySourceOutageReview(
            current,
            currentRegister,
            historicalDecision,
            ReviewRequired: true,
            AdminAlertRequired: true,
            HistoricalDecisionChanged: false);
    }

    public PolicyCatalogueValidation ValidateCatalogueRow(PolicyCatalogueRow row)
    {
        ArgumentNullException.ThrowIfNull(row);
        var rejectionCodes = new List<string>();

        if (row.IsSyntheticGap)
        {
            var syntheticFields = new[]
            {
                row.CodeProvenance,
                row.TitleProvenance,
                row.SequenceProvenance,
                row.PrerequisiteProvenance,
                row.CreditsProvenance
            };
            if (!row.Code.StartsWith("DEMO-", StringComparison.Ordinal) ||
                syntheticFields.Any(field =>
                    !string.Equals(field.Kind, "SyntheticDemo", StringComparison.Ordinal) ||
                    string.IsNullOrWhiteSpace(field.SourceReference)))
            {
                rejectionCodes.Add("SYNTHETIC_GAP_PROVENANCE_INVALID");
            }

            if (string.IsNullOrWhiteSpace(row.SyntheticGapLabel) ||
                string.IsNullOrWhiteSpace(row.SyntheticGapRationale))
            {
                rejectionCodes.Add("SYNTHETIC_GAP_LABEL_REQUIRED");
            }

            if (string.IsNullOrWhiteSpace(row.VisibleDisclaimer) ||
                !row.VisibleDisclaimer.Contains("not AASTMT-published", StringComparison.OrdinalIgnoreCase))
            {
                rejectionCodes.Add("SYNTHETIC_GAP_DISCLAIMER_REQUIRED");
            }
        }
        else
        {
            var officialFields = new[]
            {
                row.CodeProvenance,
                row.TitleProvenance,
                row.SequenceProvenance,
                row.PrerequisiteProvenance
            };
            if (officialFields.Any(field =>
                    !string.Equals(field.Kind, "OfficialAASTMT", StringComparison.Ordinal) ||
                    !string.Equals(field.SourceReference, "SRC-DATA-SCIENCE", StringComparison.Ordinal)))
            {
                rejectionCodes.Add("OFFICIAL_CURRICULUM_PROVENANCE_REQUIRED");
            }

            if (row.Credits != 3 ||
                !string.Equals(row.CreditsProvenance.Kind, "SyntheticDemo", StringComparison.Ordinal) ||
                !string.Equals(row.CreditsProvenance.SourceReference, "DEMO-CREDITS-3", StringComparison.Ordinal))
            {
                rejectionCodes.Add("SYNTHETIC_CREDIT_PROVENANCE_REQUIRED");
            }
        }

        return new PolicyCatalogueValidation(
            rejectionCodes.Count == 0,
            new ReadOnlyCollection<string>(rejectionCodes));
    }

    private static IReadOnlyList<PolicySourceRecord> VerifyAndLoadGovernedArtifacts()
    {
        var rules = File.ReadAllText(RepositoryFiles.PathTo(
            "specs/002-aastmt-policy-rulebook/policy-rules.md"));
        var boundaries = File.ReadAllText(RepositoryFiles.PathTo(
            "specs/002-aastmt-policy-rulebook/policy-boundary-examples.md"));
        var sources = File.ReadAllText(RepositoryFiles.PathTo(
            "specs/002-aastmt-policy-rulebook/policy-sources.md"));

        RequireAll(rules, Version, KnownRuleTypes.Select(type => $"### {type}"));
        RequireAll(
            boundaries,
            Enumerable.Range(1, 19).Select(index => $"PB-{index:00}"));
        RequireAll(
            sources,
            [
                "SRC-GENERAL-2016",
                "SRC-DATA-SCIENCE",
                "DEMO-APPROVAL-2026.1",
                "DEMO-CAPACITY-FIRST-COMMIT",
                "DEMO-CONFLICT-EXACT"
            ]);

        var parsedSources = ParseSourceRegister(sources);
        if (parsedSources.Count != 12)
        {
            throw new InvalidDataException(
                $"Expected 12 governed source records but found {parsedSources.Count}.");
        }

        return parsedSources;
    }

    private static IReadOnlyList<PolicySourceRecord> ParseSourceRegister(string markdown)
    {
        return markdown.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.StartsWith("| `", StringComparison.Ordinal))
            .Select(line => line.Split(
                '|',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Select(cells =>
            {
                if (cells.Length != 7)
                {
                    throw new InvalidDataException("A policy source row does not contain seven fields.");
                }

                var id = TrimCode(cells[0]);
                var provenanceKind = TrimCode(cells[1]);
                var url = TrimCode(cells[3]);
                var approvalStatus = cells[6];
                var approvalActor = provenanceKind is "AhmedApprovedDemo" or "SyntheticDemo"
                    ? "Ahmed ELbamby"
                    : approvalStatus.Contains("Approved", StringComparison.OrdinalIgnoreCase)
                        ? "Ahmed ELbamby (demo-profile approval)"
                        : "No approved actor recorded";

                return new PolicySourceRecord(
                    id,
                    provenanceKind,
                    url,
                    DateOnly.ParseExact(cells[4], "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    approvalActor,
                    EffectiveFrom,
                    null,
                    Availability: "Recorded",
                    ReviewRequired: false,
                    Authority: cells[2],
                    AffectedFacts: cells[5],
                    ContentReference: url,
                    ApprovalStatus: approvalStatus);
            })
            .ToArray();
    }

    private static string TrimCode(string value) => value.Trim().Trim('`');

    private static void RequireAll(string artifact, string required, IEnumerable<string> values)
    {
        RequireAll(artifact, values.Prepend(required));
    }

    private static void RequireAll(string artifact, IEnumerable<string> values)
    {
        foreach (var value in values)
        {
            if (!artifact.Contains(value, StringComparison.Ordinal))
            {
                throw new InvalidDataException($"Governed SPEC-002 artifact is missing '{value}'.");
            }
        }
    }

    private static PolicySelection SelectPolicy(PolicyEvaluationInput input)
    {
        var matching = input.CandidateRulebooks
            .Where(candidate =>
                candidate.Approved &&
                string.Equals(candidate.LifecycleState, "Published", StringComparison.Ordinal) &&
                candidate.EffectiveFromUtc <= input.EvaluatedAtUtc &&
                (candidate.EffectiveToUtc is null || input.EvaluatedAtUtc < candidate.EffectiveToUtc) &&
                string.Equals(candidate.College, input.College, StringComparison.Ordinal) &&
                string.Equals(candidate.Program, input.Program, StringComparison.Ordinal) &&
                string.Equals(candidate.Term, input.Term, StringComparison.Ordinal))
            .ToArray();

        if (matching.Length == 0)
        {
            return new PolicySelection(
                Rulebook: null,
                FailureReasonCode: "POLICY_UNAVAILABLE",
                MatchingPublishedPolicyCount: 0,
                HighestMatchingPriority: null);
        }

        var highestPriority = matching.Max(candidate => candidate.Priority);
        var highest = matching
            .Where(candidate => candidate.Priority == highestPriority)
            .ToArray();
        if (highest.Length != 1)
        {
            return new PolicySelection(
                Rulebook: null,
                FailureReasonCode: "POLICY_SCOPE_AMBIGUOUS",
                MatchingPublishedPolicyCount: matching.Length,
                HighestMatchingPriority: highestPriority);
        }

        return new PolicySelection(
            highest[0],
            FailureReasonCode: null,
            MatchingPublishedPolicyCount: matching.Length,
            HighestMatchingPriority: highestPriority);
    }

    private static bool HasAmbiguousPublication(
        IReadOnlyList<PolicyRulebookCandidate> candidates)
    {
        for (var first = 0; first < candidates.Count; first++)
        {
            for (var second = first + 1; second < candidates.Count; second++)
            {
                var left = candidates[first];
                var right = candidates[second];
                if (left.Priority == right.Priority &&
                    string.Equals(left.College, right.College, StringComparison.Ordinal) &&
                    string.Equals(left.Program, right.Program, StringComparison.Ordinal) &&
                    string.Equals(left.Term, right.Term, StringComparison.Ordinal) &&
                    EffectivePeriodsOverlap(left, right))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool EffectivePeriodsOverlap(
        PolicyRulebookCandidate left,
        PolicyRulebookCandidate right)
    {
        var leftEnd = left.EffectiveToUtc ?? DateTimeOffset.MaxValue;
        var rightEnd = right.EffectiveToUtc ?? DateTimeOffset.MaxValue;
        return left.EffectiveFromUtc < rightEnd && right.EffectiveFromUtc < leftEnd;
    }

    private void AddCreditLoadResults(
        PolicyEvaluationInput input,
        ICollection<PolicyDecisionResult> results)
    {
        var normalLoadAllowed = input.PlannedCredits is >= 9 and <= 18;
        var normalCode = input.PlannedCredits switch
        {
            < 9 => "LOAD_BELOW_MINIMUM",
            > 18 => "LOAD_ABOVE_NORMAL_MAXIMUM",
            _ => "LOAD_ALLOWED"
        };
        var normalExplanation = input.PlannedCredits switch
        {
            < 9 => "A submitted regular plan must contain at least 9 credits.",
            > 18 => "A submitted regular plan cannot exceed the 18-credit normal maximum.",
            _ => "The regular plan is within the approved 9 through 18-credit range."
        };
        AddResult(results, normalLoadAllowed, normalCode, normalExplanation, "SRC-GENERAL-2016");

        var probationApplies = input.Gpa < 2.0m;
        var probationAllowed = !probationApplies || input.PlannedCredits <= 12;
        AddResult(
            results,
            probationAllowed,
            probationApplies
                ? probationAllowed ? "PROBATION_LOAD_ALLOWED" : "PROBATION_LOAD_EXCEEDED"
                : "PROBATION_RULE_NOT_APPLICABLE",
            probationApplies
                ? probationAllowed
                    ? "The plan is within the 12-credit probation maximum."
                    : "A GPA below 2.0 limits the plan to a 12-credit maximum."
                : "A GPA of at least 2.0 uses the normal load limit.",
            "SRC-GENERAL-2016");
    }

    private void AddResult(
        ICollection<PolicyDecisionResult> results,
        bool passed,
        string reasonCode,
        string explanation,
        string sourceId)
    {
        results.Add(new PolicyDecisionResult(
            reasonCode,
            passed,
            explanation,
            sourcesById[sourceId],
            OverridePossible: false));
    }

    private static string SelectSuccessfulReason(PolicyEvaluationInput input)
    {
        if (input.Gpa < 2.0m)
        {
            return "PROBATION_LOAD_ALLOWED";
        }

        if (input.Meetings.Count >= 2)
        {
            return "NO_MEETING_CONFLICT";
        }

        if (input.HasAnyHold)
        {
            return "NO_BLOCKING_HOLD";
        }

        return "LOAD_ALLOWED";
    }

    private static IReadOnlyDictionary<string, string> CreateInputSummary(
        PolicyEvaluationInput input,
        PolicySelection selection)
    {
        var meetings = input.Meetings
            .OrderBy(meeting => meeting.DayOfWeek)
            .ThenBy(meeting => meeting.StartsAt)
            .ThenBy(meeting => meeting.EndsAt)
            .ThenBy(meeting => meeting.Location, StringComparer.Ordinal)
            .Select(meeting =>
                $"{meeting.DayOfWeek}:{meeting.StartsAt:HH\\:mm}-{meeting.EndsAt:HH\\:mm}@{meeting.Location}");
        var candidates = input.CandidateRulebooks
            .OrderBy(candidate => candidate.Version, StringComparer.Ordinal)
            .Select(candidate =>
                $"{candidate.Version}:{candidate.LifecycleState}:{candidate.Priority}:" +
                $"{candidate.EffectiveFromUtc:O}:{candidate.EffectiveToUtc?.ToString("O", CultureInfo.InvariantCulture) ?? "open"}:" +
                $"{candidate.College}:{candidate.Program}:{candidate.Term}:{candidate.Approved}");
        var values = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["blockingHoldCount"] = input.HasBlockingHold ? "1" : "0",
            ["candidateRulebookCount"] = input.CandidateRulebooks.Count.ToString(CultureInfo.InvariantCulture),
            ["candidateRulebooks"] = string.Join(";", candidates),
            ["college"] = input.College,
            ["committedEnrollments"] = input.CommittedEnrollments.ToString(CultureInfo.InvariantCulture),
            ["earnedCredits"] = input.EarnedCredits.ToString(CultureInfo.InvariantCulture),
            ["gpa"] = input.Gpa.ToString("0.00", CultureInfo.InvariantCulture),
            ["groupCapacity"] = input.GroupCapacity.ToString(CultureInfo.InvariantCulture),
            ["holdCount"] = input.HasAnyHold ? "1" : "0",
            ["matchingPublishedPolicyCount"] = selection.MatchingPublishedPolicyCount.ToString(CultureInfo.InvariantCulture),
            ["meetings"] = string.Join(";", meetings),
            ["plannedCredits"] = input.PlannedCredits.ToString(CultureInfo.InvariantCulture),
            ["prerequisitesMet"] = input.PrerequisitesMet ? "true" : "false",
            ["program"] = input.Program,
            ["repeatInterpretationRequired"] = input.RequiresRepeatInterpretation ? "true" : "false",
            ["requiredEarnedCredits"] = input.RequiredEarnedCredits.ToString(CultureInfo.InvariantCulture),
            ["selectedPolicyPriority"] = selection.Rulebook?.Priority.ToString(CultureInfo.InvariantCulture) ?? "not-selected",
            ["standingCode"] = input.StandingCode ?? "missing",
            ["term"] = input.Term,
            ["withinRegistrationWindow"] = input.WithinRegistrationWindow ? "true" : "false"
        };
        return new ReadOnlyDictionary<string, string>(values);
    }

    private static string CreateFingerprint(
        string policyVersion,
        PolicyEvaluationInput input,
        IReadOnlyDictionary<string, string> summary,
        IReadOnlyList<PolicyDecisionResult> results,
        bool eligible,
        string reasonCode)
    {
        var canonical = string.Join(
            "|",
            policyVersion,
            input.EvaluatedAtUtc.ToString("O", CultureInfo.InvariantCulture),
            eligible ? "1" : "0",
            reasonCode,
            string.Join(";", summary.Select(pair => $"{pair.Key}={pair.Value}")),
            string.Join(
                ";",
                results.Select(result =>
                    $"{result.ReasonCode}:{(result.Passed ? 1 : 0)}:{result.Source.Id}:{result.Explanation}")));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))
            .ToLowerInvariant();
    }

    private static bool HasExactMeetingOverlap(IReadOnlyList<PolicyMeeting> meetings)
    {
        for (var first = 0; first < meetings.Count; first++)
        {
            for (var second = first + 1; second < meetings.Count; second++)
            {
                var left = meetings[first];
                var right = meetings[second];
                if (left.DayOfWeek == right.DayOfWeek &&
                    left.StartsAt < right.EndsAt &&
                    right.StartsAt < left.EndsAt)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static IReadOnlyList<PolicyBoundaryCase> CreateBoundaryCases() =>
    [
        Boundary("PB-01", new() { PlannedCredits = 8 }, false, "LOAD_BELOW_MINIMUM"),
        Boundary("PB-02", new() { PlannedCredits = 9 }, true, "LOAD_ALLOWED"),
        Boundary("PB-03", new() { PlannedCredits = 18 }, true, "LOAD_ALLOWED"),
        Boundary("PB-04", new() { PlannedCredits = 19 }, false, "LOAD_ABOVE_NORMAL_MAXIMUM"),
        Boundary("PB-05", new() { Gpa = 1.99m, PlannedCredits = 12 }, true, "PROBATION_LOAD_ALLOWED"),
        Boundary("PB-06", new() { Gpa = 1.99m, PlannedCredits = 13 }, false, "PROBATION_LOAD_EXCEEDED"),
        Boundary("PB-07", new() { Gpa = 2.00m, PlannedCredits = 18 }, true, "LOAD_ALLOWED"),
        Boundary("PB-08", new() { PrerequisitesMet = false }, false, "PREREQUISITE_NOT_COMPLETED"),
        Boundary(
            "PB-09",
            new() { Gpa = 2.40m, EarnedCredits = 95, RequiredEarnedCredits = 96 },
            false,
            "MINIMUM_EARNED_CREDITS_NOT_MET"),
        Boundary(
            "PB-10",
            new() { GroupCapacity = 30, CommittedEnrollments = 30 },
            false,
            "GROUP_FULL"),
        Boundary(
            "PB-11",
            new()
            {
                Meetings =
                [
                    new(DayOfWeek.Sunday, new(9, 0, 0), new(10, 0, 0), "A"),
                    new(DayOfWeek.Sunday, new(9, 30, 0), new(10, 30, 0), "B")
                ]
            },
            false,
            "MEETING_CONFLICT"),
        Boundary(
            "PB-12",
            new()
            {
                Meetings =
                [
                    new(DayOfWeek.Sunday, new(9, 0, 0), new(10, 0, 0), "A"),
                    new(DayOfWeek.Sunday, new(10, 0, 0), new(11, 0, 0), "B")
                ]
            },
            true,
            "NO_MEETING_CONFLICT"),
        Boundary("PB-13", new() { WithinRegistrationWindow = false }, false, "REGISTRATION_WINDOW_CLOSED"),
        Boundary(
            "PB-14",
            new() { HasAnyHold = true, HasBlockingHold = true },
            false,
            "REGISTRATION_HOLD"),
        Boundary(
            "PB-15",
            new() { HasAnyHold = true, HasBlockingHold = false },
            true,
            "NO_BLOCKING_HOLD"),
        Boundary("PB-16", new() { StandingCode = "Unknown" }, false, "ACADEMIC_STANDING_UNAVAILABLE"),
        Boundary("PB-17", new() { RequiresRepeatInterpretation = true }, false, "REPEAT_POLICY_UNAVAILABLE"),
        Boundary("PB-18", new() { CandidateRulebooks = [] }, false, "POLICY_UNAVAILABLE"),
        Boundary(
            "PB-19",
            new()
            {
                CandidateRulebooks =
                [
                    PolicyRulebookCandidate.PublishedDemo(Version, priority: 100),
                    PolicyRulebookCandidate.PublishedDemo("DEMO-POC-2026.1-CONFLICT", priority: 100)
                ]
            },
            false,
            "POLICY_SCOPE_AMBIGUOUS")
    ];

    private static PolicyBoundaryCase Boundary(
        string id,
        PolicyEvaluationInput input,
        bool expectedEligible,
        string expectedReasonCode) =>
        new(id, input, expectedEligible, expectedReasonCode);

    private sealed record PolicySelection(
        PolicyRulebookCandidate? Rulebook,
        string? FailureReasonCode,
        int MatchingPublishedPolicyCount,
        int? HighestMatchingPriority)
    {
        public bool Selected => Rulebook is not null;
    }

}

public sealed record PolicyEvaluationInput
{
    public decimal Gpa { get; init; } = 2.50m;
    public int PlannedCredits { get; init; } = 18;
    public IReadOnlyList<PolicyRulebookCandidate> CandidateRulebooks { get; init; } =
    [
        PolicyRulebookCandidate.PublishedDemo(Spec002PolicyTestHarness.Version, priority: 100)
    ];
    public DateTimeOffset EvaluatedAtUtc { get; init; } =
        new(2026, 7, 13, 9, 0, 0, TimeSpan.Zero);
    public string College { get; init; } = "College of Artificial Intelligence demo configuration";
    public string Program { get; init; } = "Data Science 19-course snapshot";
    public string Term { get; init; } = "DEMO-2026-1";
    public bool WithinRegistrationWindow { get; init; } = true;
    public string? StandingCode { get; init; } = "Active";
    public bool HasAnyHold { get; init; }
    public bool HasBlockingHold { get; init; }
    public bool PrerequisitesMet { get; init; } = true;
    public int EarnedCredits { get; init; } = 120;
    public int RequiredEarnedCredits { get; init; }
    public int GroupCapacity { get; init; } = 30;
    public int CommittedEnrollments { get; init; }
    public bool RequiresRepeatInterpretation { get; init; }
    public IReadOnlyList<PolicyMeeting> Meetings { get; init; } = Array.Empty<PolicyMeeting>();
}

public sealed record PolicyPublicationDraft
{
    public string RuleType { get; init; } = "CreditLoad";
    public string? ExecutableExpression { get; init; }
    public bool WaitlistEnabled { get; init; }
    public bool CapacityOverrideEnabled { get; init; }
    public IReadOnlyList<PolicyRulebookCandidate> CandidateRulebooks { get; init; } =
    [
        PolicyRulebookCandidate.PublishedDemo(Spec002PolicyTestHarness.Version, priority: 100)
    ];
    public bool HasCompleteProvenance { get; init; } = true;
}

public sealed record PolicyRulebookCandidate(
    string Version,
    string LifecycleState,
    bool Approved,
    string ApprovedBy,
    int Priority,
    DateTimeOffset EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc,
    string College,
    string Program,
    string Term)
{
    public static PolicyRulebookCandidate PublishedDemo(string version, int priority) =>
        new(
            Version: version,
            LifecycleState: "Published",
            Approved: true,
            ApprovedBy: "Ahmed ELbamby",
            Priority: priority,
            EffectiveFromUtc: new DateTimeOffset(2026, 7, 13, 0, 0, 0, TimeSpan.Zero),
            EffectiveToUtc: null,
            College: "College of Artificial Intelligence demo configuration",
            Program: "Data Science 19-course snapshot",
            Term: "DEMO-2026-1");
}

public sealed record PolicyCatalogueRow
{
    public string Code { get; init; } = "DS413";
    public string Title { get; init; } = "Graduation Project I";
    public string Sequence { get; init; } = "Term 7";
    public string DisplayedPrerequisite { get; init; } = "Minimum GPA 2.0 and 96 earned credits";
    public int Credits { get; init; } = 3;
    public PolicyFieldProvenance CodeProvenance { get; init; } =
        new("OfficialAASTMT", "SRC-DATA-SCIENCE");
    public PolicyFieldProvenance TitleProvenance { get; init; } =
        new("OfficialAASTMT", "SRC-DATA-SCIENCE");
    public PolicyFieldProvenance SequenceProvenance { get; init; } =
        new("OfficialAASTMT", "SRC-DATA-SCIENCE");
    public PolicyFieldProvenance PrerequisiteProvenance { get; init; } =
        new("OfficialAASTMT", "SRC-DATA-SCIENCE");
    public PolicyFieldProvenance CreditsProvenance { get; init; } =
        new("SyntheticDemo", "DEMO-CREDITS-3");
    public bool IsSyntheticGap { get; init; }
    public string? SyntheticGapLabel { get; init; }
    public string? SyntheticGapRationale { get; init; }
    public string? VisibleDisclaimer { get; init; }

    public static PolicyCatalogueRow ValidSyntheticGap() =>
        new()
        {
            Code = "DEMO-AI499",
            Title = "Synthetic Demo Elective",
            Sequence = "Demo term",
            DisplayedPrerequisite = "None",
            IsSyntheticGap = true,
            SyntheticGapLabel = "Synthetic curriculum gap",
            SyntheticGapRationale = "Fills a documented demo curriculum gap.",
            VisibleDisclaimer = "Not AASTMT-published.",
            CodeProvenance = new("SyntheticDemo", "DEMO-CURRICULUM-GAP"),
            TitleProvenance = new("SyntheticDemo", "DEMO-CURRICULUM-GAP"),
            SequenceProvenance = new("SyntheticDemo", "DEMO-CURRICULUM-GAP"),
            PrerequisiteProvenance = new("SyntheticDemo", "DEMO-CURRICULUM-GAP"),
            CreditsProvenance = new("SyntheticDemo", "DEMO-CREDITS-3")
        };
}

public sealed record PolicyFieldProvenance(string Kind, string? SourceReference);

public sealed record PolicyMeeting(
    DayOfWeek DayOfWeek,
    TimeOnly StartsAt,
    TimeOnly EndsAt,
    string Location);

public sealed record PolicyBoundaryCase(
    string Id,
    PolicyEvaluationInput Input,
    bool ExpectedEligible,
    string ExpectedReasonCode);

public sealed record PolicySourceRecord(
    string Id,
    string ProvenanceKind,
    string Url,
    DateOnly AccessedOn,
    string ApprovalActor,
    DateTimeOffset EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc,
    string Availability,
    bool ReviewRequired,
    string Authority,
    string AffectedFacts,
    string ContentReference,
    string ApprovalStatus);

public sealed record PolicyDecisionResult(
    string ReasonCode,
    bool Passed,
    string Explanation,
    PolicySourceRecord Source,
    bool OverridePossible);

public sealed record PolicyDecision(
    bool Eligible,
    string PolicyVersion,
    DateTimeOffset EvaluatedAtUtc,
    IReadOnlyDictionary<string, string> InputSummary,
    string ApprovedBy,
    DateTimeOffset EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc,
    IReadOnlyList<PolicyDecisionResult> Results,
    string ReasonCode,
    PolicyDecisionResult PrimaryResult,
    int? MaximumCredits,
    bool UsedFallback,
    bool AdminAlertRaised,
    string DeterministicFingerprint);

public sealed record PolicyPublicationValidation(
    bool Accepted,
    IReadOnlyList<string> RejectionCodes,
    bool RequiresPolicyAmendment,
    bool StoredExecutableContent);

public sealed record PolicyDecisionSnapshot(
    string PolicyVersion,
    DateTimeOffset EvaluatedAtUtc,
    IReadOnlyDictionary<string, string> InputSummary,
    PolicySourceRecord Source,
    string Explanation,
    string ReasonCode,
    string DeterministicFingerprint);

public sealed record PolicyHistoryView(
    PolicyDecisionSnapshot Original,
    string CurrentPolicyVersion);

public sealed record PolicySourceOutageReview(
    PolicySourceRecord CurrentSource,
    IReadOnlyList<PolicySourceRecord> CurrentSourceRegister,
    PolicyDecisionSnapshot HistoricalDecision,
    bool ReviewRequired,
    bool AdminAlertRequired,
    bool HistoricalDecisionChanged);

public sealed record PolicyCatalogueValidation(
    bool Accepted,
    IReadOnlyList<string> RejectionCodes);
