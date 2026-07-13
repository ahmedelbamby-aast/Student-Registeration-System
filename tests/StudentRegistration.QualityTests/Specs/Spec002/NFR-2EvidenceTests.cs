using StudentRegistration.TestSupport;
using StudentRegistration.TestSupport.Spec002;

namespace StudentRegistration.QualityTests.Specs.Spec002;

public sealed class NFR_2EvidenceTests
{
    private static readonly string[] RequiredFixtureIds =
        Enumerable.Range(1, 19).Select(number => $"PB-{number:00}").ToArray();

    private static readonly IReadOnlyDictionary<string, ExpectedBoundaryInput> ExpectedInputs =
        new Dictionary<string, ExpectedBoundaryInput>(StringComparer.Ordinal)
        {
            ["PB-01"] = new(
                "Active, GPA 2.50, regular plan 8 credits; all other gates pass")
            {
                PlannedCredits = 8
            },
            ["PB-02"] = new(
                "Active, GPA 2.50, regular plan 9 credits; all other gates pass")
            {
                PlannedCredits = 9
            },
            ["PB-03"] = new(
                "Active, GPA 2.50, regular plan 18 credits; all other gates pass"),
            ["PB-04"] = new(
                "Active, GPA 2.50, regular plan 19 credits")
            {
                PlannedCredits = 19
            },
            ["PB-05"] = new(
                "Active, GPA 1.99, regular plan 12 credits; all other gates pass")
            {
                Gpa = 1.99m,
                PlannedCredits = 12
            },
            ["PB-06"] = new(
                "Active, GPA 1.99, regular plan 13 credits")
            {
                Gpa = 1.99m,
                PlannedCredits = 13
            },
            ["PB-07"] = new(
                "Active, GPA 2.00, regular plan 18 credits; all other gates pass")
            {
                Gpa = 2.00m
            },
            ["PB-08"] = new(
                "DS421 selected without completed DS413")
            {
                PrerequisitesMet = false
            },
            ["PB-09"] = new(
                "GPA 2.40, 95 earned credits, DS413 selected")
            {
                Gpa = 2.40m,
                EarnedCredits = 95,
                RequiredEarnedCredits = 96
            },
            ["PB-10"] = new(
                "Published group capacity 30 with 30 committed enrollments")
            {
                GroupCapacity = 30,
                CommittedEnrollments = 30
            },
            ["PB-11"] = new(
                "Two selected meetings share at least one instant")
            {
                Meetings =
                [
                    new(DayOfWeek.Sunday, new(9, 0, 0), new(10, 0, 0), "A"),
                    new(DayOfWeek.Sunday, new(9, 30, 0), new(10, 30, 0), "B")
                ]
            },
            ["PB-12"] = new(
                "First meeting ends 10:00 and next begins 10:00 at another location; no exact overlap")
            {
                Meetings =
                [
                    new(DayOfWeek.Sunday, new(9, 0, 0), new(10, 0, 0), "A"),
                    new(DayOfWeek.Sunday, new(10, 0, 0), new(11, 0, 0), "B")
                ]
            },
            ["PB-13"] = new(
                "Server time is outside configured OpensUtc/ClosesUtc")
            {
                WithinRegistrationWindow = false
            },
            ["PB-14"] = new(
                "One current hold has `blocksRegistration=true`")
            {
                HasAnyHold = true,
                HasBlockingHold = true
            },
            ["PB-15"] = new(
                "One current hold has `blocksRegistration=false`; all other gates pass")
            {
                HasAnyHold = true
            },
            ["PB-16"] = new(
                "Standing code is absent or not in the approved policy value set")
            {
                StandingCode = "Unknown"
            },
            ["PB-17"] = new(
                "Selected course has any earlier attempt requiring repeat interpretation")
            {
                RequiresRepeatInterpretation = true
            },
            ["PB-18"] = new(
                "No approved effective rulebook matches student and term")
            {
                CandidateRulebookCount = 0
            },
            ["PB-19"] = new(
                "Two approved rulebooks match the same context at the same highest priority")
            {
                CandidateRulebookCount = 2
            }
        };

    [Fact]
    public void Every_approved_PB_01_through_PB_19_input_executes_with_its_governed_outcome()
    {
        var harness = Spec002PolicyTestHarness.Load();
        var fixtures = harness.BoundaryCases.OrderBy(fixture => fixture.Id).ToArray();
        var governedRows = ReadGovernedRows();

        Assert.Equal(RequiredFixtureIds, fixtures.Select(fixture => fixture.Id));
        Assert.Equal(RequiredFixtureIds, governedRows.Keys.Order(StringComparer.Ordinal));
        Assert.Equal(RequiredFixtureIds, ExpectedInputs.Keys.Order(StringComparer.Ordinal));

        foreach (var fixture in fixtures)
        {
            var governed = governedRows[fixture.Id];
            var expectedInput = ExpectedInputs[fixture.Id];

            AssertInputMatchesGovernedRow(fixture, governed, expectedInput);
            Assert.Equal(governed.ExpectedEligible, fixture.ExpectedEligible);
            Assert.Equal(governed.ExpectedReasonCode, fixture.ExpectedReasonCode);

            var decision = harness.Evaluate(fixture.Input);

            Assert.True(
                decision.Eligible == fixture.ExpectedEligible,
                $"{fixture.Id} expected eligible={fixture.ExpectedEligible} but was {decision.Eligible}.");
            Assert.True(
                string.Equals(
                    fixture.ExpectedReasonCode,
                    decision.ReasonCode,
                    StringComparison.Ordinal),
                $"{fixture.Id} expected {fixture.ExpectedReasonCode} but was {decision.ReasonCode}.");

            if (fixture.Id == "PB-18")
            {
                Assert.Empty(fixture.Input.CandidateRulebooks);
                Assert.Equal("0", decision.InputSummary["candidateRulebookCount"]);
                Assert.Equal("0", decision.InputSummary["matchingPublishedPolicyCount"]);
                Assert.True(decision.AdminAlertRaised);
            }

            if (fixture.Id == "PB-19")
            {
                AssertEqualScopeAndPriorityCandidates(fixture.Input.CandidateRulebooks);
                Assert.Equal("2", decision.InputSummary["candidateRulebookCount"]);
                Assert.Equal("2", decision.InputSummary["matchingPublishedPolicyCount"]);

                var publication = harness.ValidatePublication(
                    new PolicyPublicationDraft
                    {
                        CandidateRulebooks = fixture.Input.CandidateRulebooks
                    });

                Assert.False(publication.Accepted);
                Assert.Equal(["POLICY_SCOPE_AMBIGUOUS"], publication.RejectionCodes);
            }
        }
    }

    private static void AssertInputMatchesGovernedRow(
        PolicyBoundaryCase fixture,
        GovernedBoundary governed,
        ExpectedBoundaryInput expected)
    {
        Assert.Equal(expected.RelevantInput, governed.RelevantInput);

        var input = fixture.Input;
        Assert.Equal(expected.Gpa, input.Gpa);
        Assert.Equal(expected.PlannedCredits, input.PlannedCredits);
        Assert.Equal(expected.WithinRegistrationWindow, input.WithinRegistrationWindow);
        Assert.Equal(expected.StandingCode, input.StandingCode);
        Assert.Equal(expected.HasAnyHold, input.HasAnyHold);
        Assert.Equal(expected.HasBlockingHold, input.HasBlockingHold);
        Assert.Equal(expected.PrerequisitesMet, input.PrerequisitesMet);
        Assert.Equal(expected.EarnedCredits, input.EarnedCredits);
        Assert.Equal(expected.RequiredEarnedCredits, input.RequiredEarnedCredits);
        Assert.Equal(expected.GroupCapacity, input.GroupCapacity);
        Assert.Equal(expected.CommittedEnrollments, input.CommittedEnrollments);
        Assert.Equal(expected.RequiresRepeatInterpretation, input.RequiresRepeatInterpretation);
        Assert.Equal(expected.CandidateRulebookCount, input.CandidateRulebooks.Count);

        var expectedMeetings = expected.Meetings.Select(ToComparableMeeting);
        var actualMeetings = input.Meetings.Select(ToComparableMeeting);
        Assert.Equal(expectedMeetings, actualMeetings);

        if (fixture.Id == "PB-11")
        {
            var first = input.Meetings[0];
            var second = input.Meetings[1];
            Assert.Equal(first.DayOfWeek, second.DayOfWeek);
            Assert.True(first.StartsAt < second.EndsAt && second.StartsAt < first.EndsAt);
        }

        if (fixture.Id == "PB-12")
        {
            var first = input.Meetings[0];
            var second = input.Meetings[1];
            Assert.Equal(first.DayOfWeek, second.DayOfWeek);
            Assert.Equal(new TimeOnly(10, 0), first.EndsAt);
            Assert.Equal(first.EndsAt, second.StartsAt);
            Assert.NotEqual(first.Location, second.Location);
            Assert.False(first.StartsAt < second.EndsAt && second.StartsAt < first.EndsAt);
        }
    }

    private static void AssertEqualScopeAndPriorityCandidates(
        IReadOnlyList<PolicyRulebookCandidate> candidates)
    {
        Assert.Equal(2, candidates.Count);
        var first = candidates[0];
        var second = candidates[1];

        Assert.NotEqual(first.Version, second.Version);
        Assert.Equal("Published", first.LifecycleState);
        Assert.Equal("Published", second.LifecycleState);
        Assert.True(first.Approved);
        Assert.True(second.Approved);
        Assert.Equal(first.Priority, second.Priority);
        Assert.Equal(first.College, second.College);
        Assert.Equal(first.Program, second.Program);
        Assert.Equal(first.Term, second.Term);
        Assert.Equal(first.EffectiveFromUtc, second.EffectiveFromUtc);
        Assert.Equal(first.EffectiveToUtc, second.EffectiveToUtc);
    }

    private static (DayOfWeek Day, TimeOnly Start, TimeOnly End, string Location)
        ToComparableMeeting(PolicyMeeting meeting) =>
        (meeting.DayOfWeek, meeting.StartsAt, meeting.EndsAt, meeting.Location);

    private static (DayOfWeek Day, TimeOnly Start, TimeOnly End, string Location)
        ToComparableMeeting(ExpectedMeeting meeting) =>
        (meeting.Day, meeting.Start, meeting.End, meeting.Location);

    private static IReadOnlyDictionary<string, GovernedBoundary> ReadGovernedRows()
    {
        var markdown = RepositoryFiles.Read(
            "specs/002-aastmt-policy-rulebook/policy-boundary-examples.md");

        return markdown.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.StartsWith("| PB-", StringComparison.Ordinal))
            .Select(line => line.Split(
                '|',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .ToDictionary(
                cells => cells[0],
                cells => new GovernedBoundary(
                    cells[2],
                    string.Equals(cells[3], "Pass", StringComparison.Ordinal),
                    cells[4].Trim('`')),
                StringComparer.Ordinal);
    }

    private sealed record GovernedBoundary(
        string RelevantInput,
        bool ExpectedEligible,
        string ExpectedReasonCode);

    private sealed record ExpectedBoundaryInput(string RelevantInput)
    {
        public decimal Gpa { get; init; } = 2.50m;
        public int PlannedCredits { get; init; } = 18;
        public bool WithinRegistrationWindow { get; init; } = true;
        public string StandingCode { get; init; } = "Active";
        public bool HasAnyHold { get; init; }
        public bool HasBlockingHold { get; init; }
        public bool PrerequisitesMet { get; init; } = true;
        public int EarnedCredits { get; init; } = 120;
        public int RequiredEarnedCredits { get; init; }
        public int GroupCapacity { get; init; } = 30;
        public int CommittedEnrollments { get; init; }
        public bool RequiresRepeatInterpretation { get; init; }
        public int CandidateRulebookCount { get; init; } = 1;
        public IReadOnlyList<ExpectedMeeting> Meetings { get; init; } = [];
    }

    private sealed record ExpectedMeeting(
        DayOfWeek Day,
        TimeOnly Start,
        TimeOnly End,
        string Location);
}
