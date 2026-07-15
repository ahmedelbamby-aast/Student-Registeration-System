using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec008;

public sealed class Endpoint03ContractTests
{
    [Fact]
    public void Student_academic_context_has_independent_pages_and_complete_safe_holds()
    {
        var context = Spec008ContractAssertions.AcademicContractType(
            "StudentAcademicContextDto");

        Spec008ContractAssertions.HasExactProperties(
            context,
            "UniversityId",
            "ProgramCode",
            "Cohort",
            "CurrentGpa",
            "EarnedCredits",
            "Standing",
            "TranscriptSummary",
            "TranscriptAttempts",
            "ActiveHolds",
            "DataVersion",
            "DataAsOfUtc",
            "Provenance");

        var transcript = Spec008ContractAssertions.PageItemType(
            context,
            "TranscriptAttempts");
        var provenance = Spec008ContractAssertions.PageItemType(context, "Provenance");
        Assert.NotEqual(transcript, provenance);

        var holdsProperty = Spec008ContractAssertions.Property(context, "ActiveHolds");
        Assert.False(
            holdsProperty.PropertyType.IsGenericType &&
            holdsProperty.PropertyType.GetGenericTypeDefinition().FullName ==
                "StudentRegistration.Contracts.Page`1",
            "Active holds must be returned as one complete bounded set, not a page.");
        Assert.Equal(
            typeof(DateTime),
            Spec008ContractAssertions.Property(context, "DataAsOfUtc").PropertyType);
        Assert.Equal(
            typeof(string),
            Spec008ContractAssertions.Property(context, "DataVersion").PropertyType);
    }

    [Fact]
    public void Student_transcript_summary_attempt_provenance_and_hold_shapes_are_bounded_and_safe()
    {
        var context = Spec008ContractAssertions.AcademicContractType(
            "StudentAcademicContextDto");
        var summary = Spec008ContractAssertions.Property(context, "TranscriptSummary").PropertyType;
        var attempt = Spec008ContractAssertions.PageItemType(context, "TranscriptAttempts");
        var provenance = Spec008ContractAssertions.PageItemType(context, "Provenance");
        var hold = CollectionItemType(
            Spec008ContractAssertions.Property(context, "ActiveHolds").PropertyType);

        Spec008ContractAssertions.HasExactProperties(
            summary,
            "AttemptedCredits",
            "EarnedCredits",
            "AttemptCount");
        Spec008ContractAssertions.HasExactProperties(
            attempt,
            "AttemptId",
            "SupersedesAttemptId",
            "CourseCode",
            "TermCode",
            "Credits",
            "Grade",
            "Status",
            "Provenance");
        Spec008ContractAssertions.HasExactProperties(
            provenance,
            "Source",
            "Reference",
            "ImportedAtUtc");
        Spec008ContractAssertions.HasExactProperties(
            hold,
            "TermId",
            "Code",
            "Message",
            "BlocksRegistration",
            "EffectiveFromUtc",
            "EffectiveToUtc",
            "Source");
        Spec008ContractAssertions.ExcludesProperties(
            hold,
            "HoldId",
            "SourceReference",
            "StudentId",
            "RowVersion");
    }

    [Fact]
    public void Student_profile_contract_freezes_independent_pages_and_hold_overflow_fail_closed()
    {
        RepositoryFiles.ContainsAll(
            Spec008ContractAssertions.ApiContract(),
            "Transcript and provenance pages are independent",
            "Every active hold is returned together, up to 100",
            "More than 100 returns",
            "`409 PROFILE_NOT_READY`",
            "never returns a partial hold set",
            "03 | `GET /api/students/me/academic-context`",
            "Student + `AcademicProfile.ReadOwn` + ApplicationUser ownership");
    }

    [Fact]
    public void Student_profile_endpoint_is_self_scoped_separately_paged_and_declares_overflow()
    {
        var endpoint = Spec008ContractAssertions.Endpoint(
            "GET",
            "/api/students/me/academic-context");

        Spec008ContractAssertions.RequiresPolicy(endpoint, "AcademicProfile.ReadOwn");
        Spec008ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.Academics.StudentAcademicContextDto");
        foreach (var status in new[]
        {
            StatusCodes.Status400BadRequest,
            StatusCodes.Status401Unauthorized,
            StatusCodes.Status403Forbidden,
            StatusCodes.Status404NotFound,
            StatusCodes.Status409Conflict,
            StatusCodes.Status500InternalServerError,
            StatusCodes.Status503ServiceUnavailable
        })
        {
            Spec008ContractAssertions.DeclaresResponse(endpoint, status);
        }

        var parameterNames = Spec008ContractAssertions.Handler(endpoint)
            .GetParameters()
            .Select(parameter => parameter.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Contains("transcriptPage", parameterNames);
        Assert.Contains("transcriptPageSize", parameterNames);
        Assert.Contains("provenancePage", parameterNames);
        Assert.Contains("provenancePageSize", parameterNames);
        Assert.DoesNotContain("studentId", parameterNames);
    }

    private static Type CollectionItemType(Type collectionType)
    {
        var enumerable = collectionType
            .GetInterfaces()
            .Append(collectionType)
            .FirstOrDefault(type =>
                type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        Assert.True(
            enumerable is not null,
            $"{collectionType.FullName} must be a typed read-only collection.");
        return enumerable!.GetGenericArguments()[0];
    }
}
