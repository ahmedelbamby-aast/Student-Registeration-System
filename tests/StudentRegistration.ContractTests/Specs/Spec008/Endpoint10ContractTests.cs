using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec008;

public sealed class Endpoint10ContractTests
{
    [Fact]
    public void Correction_request_has_only_term_versions_provenance_and_bounded_operations()
    {
        var request = Spec008ContractAssertions.AcademicContractType(
            "AcademicProfileCorrectionRequest");

        Spec008ContractAssertions.HasExactProperties(
            request,
            "TermId",
            "ExpectedStudentRowVersion",
            "ExpectedStudentTermStateRowVersion",
            "Reason",
            "Source",
            "Operations");
        Spec008ContractAssertions.ExcludesProperties(
            request,
            "StudentId",
            "Role",
            "Claims",
            "Permissions",
            "Student",
            "TranscriptAttempts",
            "ActiveHolds");

        Assert.Equal(
            typeof(string),
            Spec008ContractAssertions.Property(
                request,
                "ExpectedStudentRowVersion").PropertyType);
        Assert.Equal(
            typeof(string),
            Spec008ContractAssertions.Property(
                request,
                "ExpectedStudentTermStateRowVersion").PropertyType);
        Assert.Equal(
            "AcademicProfileCorrectionOperation",
            CollectionItemType(
                Spec008ContractAssertions.Property(request, "Operations").PropertyType).Name);
    }

    [Fact]
    public void Correction_operations_are_the_exact_discriminated_allow_list()
    {
        var operation = Spec008ContractAssertions.AcademicContractType(
            "AcademicProfileCorrectionOperation");
        var discriminators = operation
            .GetCustomAttributes(typeof(JsonDerivedTypeAttribute), inherit: false)
            .Cast<JsonDerivedTypeAttribute>()
            .Select(attribute => attribute.TypeDiscriminator as string)
            .Where(discriminator => discriminator is not null)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(operation.IsAbstract);
        Assert.Equal(
            new[]
            {
                "remove-hold",
                "set-earned-credits",
                "set-gpa",
                "set-standing",
                "upsert-hold",
                "upsert-transcript-attempt"
            },
            discriminators);
        Assert.All(
            operation.Assembly.GetTypes().Where(type => type.BaseType == operation),
            derived => Assert.False(derived.IsAbstract));
    }

    [Fact]
    public void Frozen_correction_contract_requires_bounds_versions_append_only_history_and_atomic_audit()
    {
        RepositoryFiles.ContainsAll(
            Spec008ContractAssertions.ApiContract(),
            "A profile correction contains 1 through 20 operations",
            "`reason` is 10 through 500 trimmed characters",
            "`source` and every",
            "`sourceReference` are nonblank and at most 200 trimmed characters",
            "expectedStudentRowVersion",
            "expectedStudentTermStateRowVersion",
            "upsert-transcript-attempt",
            "Transcript attempts are immutable",
            "`supersedesAttemptId` identifies a current",
            "leaf for the same student, course, and transcript term",
            "A prior attempt has at most one direct successor",
            "atomic privacy-safe audit",
            "10 | `PATCH /api/admin/students/{studentId}/academic-profile`",
            "`409 STALE_VERSION/INVALID_SUPERSESSION/PROFILE_NOT_READY`");
    }

    [Fact]
    public void Correction_endpoint_requires_profile_management_antiforgery_and_complete_status_matrix()
    {
        var endpoint = Spec008ContractAssertions.Endpoint(
            "PATCH",
            "/api/admin/students/{studentId}/academic-profile");

        Spec008ContractAssertions.RequiresPolicy(endpoint, "AcademicProfiles.Manage");
        var antiforgery = endpoint.Metadata.GetMetadata<IAntiforgeryMetadata>();
        Assert.True(antiforgery?.RequiresValidation == true);
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

        var parameters = Spec008ContractAssertions.Handler(endpoint).GetParameters();
        Assert.Contains(
            parameters,
            parameter => string.Equals(
                parameter.Name,
                "studentId",
                StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            parameters,
            parameter => parameter.ParameterType.FullName ==
                "StudentRegistration.Contracts.Academics.AcademicProfileCorrectionRequest");

        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "src/StudentRegistration.Academics/Endpoints/Spec008Endpoints.cs"),
            "AcademicProfileOutcome.ValidationError",
            "ValidationError(context)",
            "VALIDATION_ERROR");
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
