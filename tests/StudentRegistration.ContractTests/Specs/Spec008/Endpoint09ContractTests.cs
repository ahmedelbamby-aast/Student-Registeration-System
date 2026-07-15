using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec008;

public sealed class Endpoint09ContractTests
{
    [Fact]
    public void Named_admin_context_adds_only_governed_resource_and_version_fields()
    {
        var context = Spec008ContractAssertions.AcademicContractType(
            "AdminStudentAcademicContextDto");

        Spec008ContractAssertions.HasExactProperties(
            context,
            "StudentId",
            "TermId",
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
            "Provenance",
            "StudentRowVersion",
            "StudentTermStateRowVersion");
        Spec008ContractAssertions.ExcludesProperties(
            context,
            "ApplicationUserId",
            "PasswordHash",
            "SecurityStamp",
            "Claims",
            "Permissions");

        var transcript = Spec008ContractAssertions.PageItemType(
            context,
            "TranscriptAttempts");
        var provenance = Spec008ContractAssertions.PageItemType(context, "Provenance");
        Assert.NotEqual(transcript, provenance);
    }

    [Fact]
    public void Admin_hold_shape_exposes_only_identifiers_needed_for_governed_correction()
    {
        var context = Spec008ContractAssertions.AcademicContractType(
            "AdminStudentAcademicContextDto");
        var hold = CollectionItemType(
            Spec008ContractAssertions.Property(context, "ActiveHolds").PropertyType);

        Assert.Equal("AdminAcademicHoldDto", hold.Name);
        Spec008ContractAssertions.HasExactProperties(
            hold,
            "TermId",
            "Code",
            "Message",
            "BlocksRegistration",
            "EffectiveFromUtc",
            "EffectiveToUtc",
            "Source",
            "HoldId",
            "SourceReference");
        Spec008ContractAssertions.ExcludesProperties(
            hold,
            "StudentId",
            "ApplicationUserId",
            "PasswordHash");
    }

    [Fact]
    public void Frozen_named_context_contract_requires_both_resource_keys_and_complete_bounded_data()
    {
        RepositoryFiles.ContainsAll(
            Spec008ContractAssertions.ApiContract(),
            "09 | `GET /api/admin/students/{studentId}/academic-context`",
            "Required `termId` plus independent transcript/provenance pages",
            "`AcademicProfiles.Manage` + named StudentId/TermId scope",
            "authorized `404`",
            "`409 PROFILE_NOT_READY`",
            "authorize before resource lookup or version disclosure",
            "Every active hold is returned together, up to 100",
            "Only the named Admin detail exposes `holdId`");
    }

    [Fact]
    public void Named_context_endpoint_is_direct_object_scoped_and_declares_denial_before_disclosure()
    {
        var endpoint = Spec008ContractAssertions.Endpoint(
            "GET",
            "/api/admin/students/{studentId}/academic-context");

        Spec008ContractAssertions.RequiresPolicy(endpoint, "AcademicProfiles.Manage");
        Assert.Null(endpoint.Metadata.GetMetadata<IAntiforgeryMetadata>());
        Spec008ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.Academics.AdminStudentAcademicContextDto");
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

        var names = Spec008ContractAssertions.Handler(endpoint)
            .GetParameters()
            .Select(parameter => parameter.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Contains("studentId", names);
        Assert.Contains("termId", names);
        Assert.Contains("transcriptPage", names);
        Assert.Contains("transcriptPageSize", names);
        Assert.Contains("provenancePage", names);
        Assert.Contains("provenancePageSize", names);
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
