using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Domain;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.TestSupport.Spec011;

namespace StudentRegistration.IntegrationTests.Registration;

public sealed class OfferingSearchTests
{
    [Fact]
    public async Task Search_normalizes_filters_and_stably_sorts_after_evaluation()
    {
        var fixture = new Spec011ScenarioBuilder
        {
            CourseCode = "DS413",
            CourseTitle = "Project I"
        };
        var service = Service(fixture);
        var query = new OfferingSearchQuery(service);

        var result = await query.SearchAsync(
            fixture.ApplicationUserId,
            fixture.TermId,
            new("  project  ", "eligible", 3m, 1, "available", "title,id", 1, 20));

        Assert.Equal(OfferingSearchOutcome.Found, result.Outcome);
        var page = Assert.IsType<StudentRegistration.Contracts.Page<
            StudentRegistration.Registration.Domain.OfferingEligibility>>(result.Page);
        Assert.Single(page.Items);
        Assert.Equal("title,id", page.Sort);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task Invalid_page_is_rejected_without_results(int page, int pageSize)
    {
        var fixture = new Spec011ScenarioBuilder();
        var result = await new OfferingSearchQuery(Service(fixture)).SearchAsync(
            fixture.ApplicationUserId,
            fixture.TermId,
            new(null, null, null, null, null, null, page, pageSize));

        Assert.Equal(OfferingSearchOutcome.PageSizeInvalid, result.Outcome);
        Assert.Equal("PAGE_SIZE_INVALID", result.ErrorCode);
        Assert.Null(result.Page);
    }

    [Theory]
    [InlineData(" ", null, null)]
    [InlineData(null, "client-eligible", null)]
    [InlineData(null, null, "unknown-sort")]
    public async Task Invalid_or_client_authored_values_are_rejected(
        string? text,
        string? eligibility,
        string? sort)
    {
        var fixture = new Spec011ScenarioBuilder();
        var result = await new OfferingSearchQuery(Service(fixture)).SearchAsync(
            fixture.ApplicationUserId,
            fixture.TermId,
            new(text, eligibility, null, null, null, sort, 1, 20));

        Assert.Equal(OfferingSearchOutcome.ValidationError, result.Outcome);
        Assert.Equal("VALIDATION_ERROR", result.ErrorCode);
    }

    [Fact]
    public async Task Very_large_valid_page_is_empty_without_integer_overflow()
    {
        var fixture = new Spec011ScenarioBuilder();
        var result = await new OfferingSearchQuery(Service(fixture)).SearchAsync(
            fixture.ApplicationUserId,
            fixture.TermId,
            new(null, "all", null, null, "all", null, int.MaxValue, 100));

        Assert.Equal(OfferingSearchOutcome.Found, result.Outcome);
        Assert.Empty(result.Page!.Items);
        Assert.Equal(1, result.Page.TotalCount);
    }

    [Fact]
    public async Task Governed_eligibility_branches_remain_server_authoritative()
    {
        var scenarios = new (Spec011ScenarioBuilder Fixture, string Code)[]
        {
            (new() { WindowOpen = false }, "REGISTRATION_WINDOW_CLOSED"),
            (new() { Standing = "Suspended" }, "ACADEMIC_STANDING_UNAVAILABLE"),
            (new() { BlockingHold = true }, "REGISTRATION_HOLD"),
            (new() { PrerequisitePassed = false }, "PREREQUISITE_NOT_COMPLETED"),
            (new() { Gpa = 1.99m }, "MINIMUM_GPA_NOT_MET"),
            (new() { EarnedCredits = 95m }, "MINIMUM_EARNED_CREDITS_NOT_MET"),
            (new() { Capacity = 30, EnrolledCount = 30 }, "GROUP_FULL"),
            (new() { RegistrationPaused = true }, "REGISTRATION_PAUSED"),
            (new() { GroupState = "draft" }, "GROUP_UNPUBLISHED"),
            (new() { RoomAvailable = false }, "ROOM_UNAVAILABLE"),
            (new() { CompleteStaffing = false }, "GROUP_INCOMPLETE"),
            (new() { Conflict = true }, "MEETING_CONFLICT"),
            (new() { CurrentCoursePassed = true }, "REPEAT_POLICY_UNAVAILABLE")
        };

        foreach (var scenario in scenarios)
        {
            var offering = Assert.Single((await Service(scenario.Fixture)
                .EvaluateTermAsync(
                    scenario.Fixture.ApplicationUserId,
                    scenario.Fixture.TermId)).Items);
            Assert.False(offering.Eligible);
            Assert.True(
                offering.Reasons.Any(reason => reason.Code == scenario.Code)
                || offering.Groups.SelectMany(group => group.NonSelectableReasons)
                    .Any(reason => reason.Code == scenario.Code),
                $"Expected eligibility reason '{scenario.Code}' was not returned.");
        }

        var missingAcademic = new Spec011ScenarioBuilder
        {
            IncludeAcademicSnapshot = false
        };
        Assert.Equal(
            EligibilityEvaluationOutcome.ContextNotFound,
            (await Service(missingAcademic).EvaluateTermAsync(
                missingAcademic.ApplicationUserId,
                missingAcademic.TermId)).Outcome);

        foreach (var fixture in new[]
                 {
                     new Spec011ScenarioBuilder { IncludePolicy = false },
                     new Spec011ScenarioBuilder { IncludeCatalogue = false }
                 })
        {
            var result = await Service(fixture).EvaluateTermAsync(
                fixture.ApplicationUserId,
                fixture.TermId);
            Assert.Equal(EligibilityEvaluationOutcome.Found, result.Outcome);
            Assert.Contains(
                Assert.Single(result.Items).Reasons,
                reason => reason.Code == "DECISION_DATA_UNAVAILABLE");
        }

        var service = Service(new Spec011ScenarioBuilder());
        Assert.Equal(
            EligibilityEvaluationOutcome.ContextNotFound,
            (await service.EvaluateTermAtAsync(
                Guid.Empty,
                Guid.NewGuid(),
                DateTime.UtcNow)).Outcome);
        Assert.Equal(
            EligibilityEvaluationOutcome.OfferingNotFound,
            (await service.EvaluateOfferingAtAsync(
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow)).Outcome);
    }

    [Fact]
    public async Task Search_exercises_every_approved_filter_sort_and_validation_boundary()
    {
        var fixture = new Spec011ScenarioBuilder();
        var query = new OfferingSearchQuery(Service(fixture));
        var validSorts = new[]
        {
            "courseCode,id",
            "courseCode-desc,id",
            "title,id",
            "title-desc,id",
            "credits,id",
            "credits-desc,id"
        };
        foreach (var sort in validSorts)
        {
            var result = await query.SearchAsync(
                fixture.ApplicationUserId,
                fixture.TermId,
                new(null, "all", 3m, 1, "all", sort));
            Assert.Equal(OfferingSearchOutcome.Found, result.Outcome);
        }

        foreach (var request in new OfferingSearchRequest[]
                 {
                     new("missing", "all", null, null, "all", null),
                     new(null, "unavailable", null, null, "all", null),
                     new(null, "all", null, null, "full", null),
                     new(null, "all", null, 6, "available", null),
                     new(null, "all", 4m, null, "all", null)
                 })
        {
            var result = await query.SearchAsync(
                fixture.ApplicationUserId,
                fixture.TermId,
                request);
            Assert.Equal(OfferingSearchOutcome.Found, result.Outcome);
            Assert.Empty(result.Page!.Items);
        }

        foreach (var request in new OfferingSearchRequest[]
                 {
                     new(new string('x', 101), null, null, null, null, null),
                     new(null, null, null, -1, null, null),
                     new(null, null, null, 7, null, null),
                     new(null, null, 0.4m, null, null, null),
                     new(null, null, 30.01m, null, null, null),
                     new(null, null, 3.001m, null, null, null),
                     new(null, null, null, null, "invalid", null)
                 })
        {
            var result = await query.SearchAsync(
                fixture.ApplicationUserId,
                fixture.TermId,
                request);
            Assert.Equal(OfferingSearchOutcome.ValidationError, result.Outcome);
        }
    }

    [Fact]
    public async Task Eligibility_value_equality_and_guards_cover_every_governed_field()
    {
        var fixture = new Spec011ScenarioBuilder();
        var baseline = Assert.Single((await Service(fixture).EvaluateTermAsync(
            fixture.ApplicationUserId,
            fixture.TermId)).Items);

        Assert.Equal(baseline, Copy(baseline, 0));
        Assert.NotNull(baseline);
        for (var difference = 1; difference <= 22; difference++)
        {
            Assert.NotEqual(baseline, Copy(baseline, difference));
        }

        _ = baseline.GetHashCode();
        _ = Copy(baseline, 21).GetHashCode();

        for (var invalid = 101; invalid <= 112; invalid++)
        {
            Assert.ThrowsAny<ArgumentException>(() => Copy(baseline, invalid));
        }
    }

    [Fact]
    public async Task Eligibility_context_policy_and_credit_boundaries_fail_closed()
    {
        var fixture = new Spec011ScenarioBuilder();
        var service = Service(fixture);

        foreach (var context in new[]
                 {
                     (Guid.Empty, fixture.TermId, fixture.EvaluatedAtUtc),
                     (fixture.ApplicationUserId, Guid.Empty, fixture.EvaluatedAtUtc),
                     (fixture.ApplicationUserId, fixture.TermId,
                         DateTime.SpecifyKind(fixture.EvaluatedAtUtc, DateTimeKind.Local))
                 })
        {
            Assert.Equal(
                EligibilityEvaluationOutcome.ContextNotFound,
                (await service.EvaluateTermAtAsync(
                    context.Item1,
                    context.Item2,
                    context.Item3)).Outcome);
            await Assert.ThrowsAsync<ArgumentException>(() => service.EvaluateTermForCommitAsync(
                context.Item1,
                context.Item2,
                context.Item3));
        }

        foreach (var context in new[]
                 {
                     (Guid.Empty, fixture.OfferingId, fixture.EvaluatedAtUtc),
                     (fixture.ApplicationUserId, Guid.Empty, fixture.EvaluatedAtUtc),
                     (fixture.ApplicationUserId, fixture.OfferingId,
                         DateTime.SpecifyKind(fixture.EvaluatedAtUtc, DateTimeKind.Local))
                 })
        {
            Assert.Equal(
                EligibilityEvaluationOutcome.OfferingNotFound,
                (await service.EvaluateOfferingAtAsync(
                    context.Item1,
                    context.Item2,
                    context.Item3)).Outcome);
        }

        var noOfferingService = new EligibilityService(
            fixture.AcademicReader(),
            new PassiveOfferingReader([]),
            fixture.CurrentPlanReader(),
            new GroupSummaryProjection(),
            new FixedTimeProvider(fixture.EvaluatedAtUtc));
        Assert.Equal(
            EligibilityEvaluationOutcome.OfferingNotFound,
            (await noOfferingService.EvaluateOfferingAsync(
                fixture.ApplicationUserId,
                fixture.OfferingId)).Outcome);

        var noAcademic = new Spec011ScenarioBuilder { IncludeAcademicSnapshot = false };
        Assert.Equal(
            EligibilityEvaluationOutcome.OfferingNotFound,
            (await Service(noAcademic).EvaluateOfferingAsync(
                noAcademic.ApplicationUserId,
                noAcademic.OfferingId)).Outcome);

        var selected = new Spec011ScenarioBuilder
        {
            MinimumGpa = null,
            MinimumEarnedCredits = null,
            CandidateOfferingAlreadySelected = true
        };
        var selectedDecision = Assert.Single((await Service(selected).EvaluateTermAsync(
            selected.ApplicationUserId,
            selected.TermId)).Items);
        Assert.Equal(selected.CurrentPlanCredits, selectedDecision.ProjectedPlanCredits);
        Assert.DoesNotContain(selectedDecision.Reasons, reason =>
            reason.Code.StartsWith("MINIMUM_", StringComparison.Ordinal));

        var normalLimit = new Spec011ScenarioBuilder { CurrentPlanCredits = 18m };
        Assert.Contains(
            Assert.Single((await Service(normalLimit).EvaluateTermAsync(
                normalLimit.ApplicationUserId,
                normalLimit.TermId)).Items).Reasons,
            reason => reason.Code == "LOAD_ALLOWED"
                && reason.Passed
                && reason.Message.Contains("subject to approval", StringComparison.Ordinal));

        var probationLimit = new Spec011ScenarioBuilder
        {
            Gpa = 1.5m,
            CurrentPlanCredits = 12m
        };
        Assert.Contains(
            Assert.Single((await Service(probationLimit).EvaluateTermAsync(
                probationLimit.ApplicationUserId,
                probationLimit.TermId)).Items).Reasons,
            reason => reason.Code == "PROBATION_LOAD_EXCEEDED");

        var unavailableStanding = new Spec011ScenarioBuilder { Standing = " " };
        Assert.Equal(
            "unavailable",
            Assert.Single((await Service(unavailableStanding).EvaluateTermAsync(
                unavailableStanding.ApplicationUserId,
                unavailableStanding.TermId)).Items).InputSummary["standing"]);

        var unavailablePlan = new Spec011ScenarioBuilder { CurrentPlanVersion = " " };
        Assert.Equal(
            "unavailable",
            Assert.Single((await Service(unavailablePlan).EvaluateTermAsync(
                unavailablePlan.ApplicationUserId,
                unavailablePlan.TermId)).Items).CurrentPlanVersion);

        var validAcademic = fixture.AcademicSnapshot();
        var validPolicy = fixture.PolicySnapshot();
        var firstRule = validPolicy.Rules[0];
        var incompletePolicies = new[]
        {
            validPolicy with { PolicySetId = Guid.Empty },
            validPolicy with { Version = " " },
            validPolicy with { ApprovedBy = " " },
            validPolicy with { ApprovalReference = " " },
            validPolicy with { Rules = [] },
            validPolicy with { Rules = validPolicy.Rules.Skip(1).ToArray() },
            validPolicy with { Rules = validPolicy.Rules.Append(firstRule).ToArray() },
            validPolicy with
            {
                Rules = validPolicy.Rules.Select((rule, index) => index == 0
                    ? rule with { SourceReference = " " }
                    : rule).ToArray()
            },
            validPolicy with
            {
                Rules = validPolicy.Rules.Select((rule, index) => index == 0
                    ? rule with { SourceAccessedOn = default }
                    : rule).ToArray()
            }
        };
        for (var index = 0; index < incompletePolicies.Length; index++)
        {
            var policy = incompletePolicies[index];
            var result = await Service(
                    fixture,
                    new PassiveAcademicReader(validAcademic with { Policy = policy }))
                .EvaluateTermAsync(fixture.ApplicationUserId, fixture.TermId);
            Assert.True(
                result.Items.Count == 1,
                $"Incomplete policy case {index} returned {result.Outcome}/{result.ErrorCode}.");
            Assert.Contains(
                Assert.Single(result.Items).Reasons,
                reason => reason.Code == "POLICY_UNAVAILABLE");
        }

        var catalogueWithoutCourse = validAcademic.Catalogue! with { Courses = [] };
        var unavailable = Assert.Single((await Service(
                fixture,
                new PassiveAcademicReader(validAcademic with
                {
                    Catalogue = catalogueWithoutCourse,
                    AcademicContextVersion = []
                }),
                new PassiveOfferingReader(
                [fixture.OfferingSnapshot() with { RowVersion = [] }]))
            .EvaluateTermAsync(fixture.ApplicationUserId, fixture.TermId)).Items);
        Assert.Equal("unavailable", unavailable.AcademicContextVersion);
        Assert.Equal("unavailable", unavailable.OfferingRowVersion);
        Assert.Contains(unavailable.Reasons, reason => reason.Code == "CATALOGUE_UNAVAILABLE");
    }

    [Fact]
    public void Eligibility_reason_rejects_ambiguous_policy_provenance()
    {
        var started = new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc);
        EligibilityReason Create(
            bool blocking = false,
            bool overridePossible = false,
            DateTime? effectiveFrom = null,
            DateTime? effectiveTo = null,
            DateOnly? sourceDate = null) => new(
                "CODE",
                !blocking,
                blocking,
                "Message",
                " required ",
                " current ",
                Guid.NewGuid(),
                "v1",
                "source",
                sourceDate ?? new DateOnly(2026, 7, 13),
                "approver",
                effectiveFrom ?? started,
                effectiveTo,
                overridePossible,
                " /support ");

        var valid = Create(effectiveTo: started.AddDays(1));
        Assert.Equal("required", valid.RequiredValue);
        Assert.Equal("/support", valid.SupportReferencePath);
        Assert.Throws<ArgumentException>(() => Create(blocking: true, overridePossible: true));
        Assert.Throws<ArgumentException>(() => Create(
            effectiveFrom: DateTime.SpecifyKind(started, DateTimeKind.Local)));
        Assert.Throws<ArgumentException>(() => Create(
            effectiveTo: DateTime.SpecifyKind(started.AddDays(1), DateTimeKind.Local)));
        Assert.Throws<ArgumentException>(() => Create(effectiveTo: started));
        Assert.Throws<ArgumentException>(() => Create(sourceDate: default(DateOnly)));
    }

    private static OfferingEligibility Copy(OfferingEligibility source, int difference) =>
        new(
            difference == 1
                ? Guid.NewGuid()
                : difference == 101 ? Guid.Empty : source.OfferingId,
            difference == 2
                ? source.CourseCode + " changed"
                : difference == 108 ? " " : source.CourseCode,
            difference == 3 ? source.Title + " changed" : source.Title,
            difference == 4
                ? source.Credits + 1m
                : difference == 102 ? -1m : source.Credits,
            difference == 5
                ? source.CurrentPlanCredits + 1m
                : difference == 103 ? -1m : source.CurrentPlanCredits,
            difference == 6
                ? source.ProjectedPlanCredits + 1m
                : difference == 104 ? -1m : source.ProjectedPlanCredits,
            difference == 7
                ? source.DefaultTargetCredits + 1m
                : difference == 105 ? 0m : source.DefaultTargetCredits,
            difference == 8
                ? source.MaximumAllowedCredits + 1m
                : difference == 106 ? 0m : source.MaximumAllowedCredits,
            difference == 9 ? !source.Eligible : source.Eligible,
            difference switch
            {
                10 or 21 => [],
                109 => [null!],
                _ => source.Reasons
            },
            difference switch
            {
                11 or 21 => [],
                110 => [null!],
                _ => source.Groups
            },
            difference switch
            {
                12 => new Dictionary<string, string>(),
                13 => new Dictionary<string, string> { ["changed"] = "value" },
                21 => new Dictionary<string, string>(),
                22 => source.InputSummary.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value + " changed",
                    StringComparer.Ordinal),
                111 => new Dictionary<string, string> { [" "] = "value" },
                112 => new Dictionary<string, string> { ["key"] = " " },
                _ => source.InputSummary
            },
            difference == 14
                ? source.EvaluatedAtUtc.AddMinutes(1)
                : difference == 107
                    ? DateTime.SpecifyKind(source.EvaluatedAtUtc, DateTimeKind.Local)
                    : source.EvaluatedAtUtc,
            difference == 15 ? source.AcademicContextVersion + "x" : source.AcademicContextVersion,
            difference == 16 ? source.CatalogueVersion + "x" : source.CatalogueVersion,
            difference == 17 ? Guid.NewGuid() : source.PolicySetId,
            difference == 18 ? source.PolicyVersion + "x" : source.PolicyVersion,
            difference == 19 ? source.OfferingRowVersion + "x" : source.OfferingRowVersion,
            difference == 20 ? source.CurrentPlanVersion + "x" : source.CurrentPlanVersion);

    private static EligibilityService Service(
        Spec011ScenarioBuilder fixture,
        IEligibilityAcademicReader? academics = null,
        IEligibilityOfferingReader? offerings = null,
        ICurrentPlanReader? currentPlan = null) =>
        new(
            academics ?? fixture.AcademicReader(),
            offerings ?? fixture.OfferingReader(),
            currentPlan ?? fixture.CurrentPlanReader(),
            new GroupSummaryProjection(),
            new FixedTimeProvider(fixture.EvaluatedAtUtc));

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
    }
}
