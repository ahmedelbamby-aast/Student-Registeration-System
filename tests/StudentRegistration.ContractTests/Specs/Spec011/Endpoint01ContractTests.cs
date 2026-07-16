using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec011;

public sealed class Endpoint01ContractTests
{
    [Fact]
    public void Discovery_contract_is_complete_bounded_and_versioned()
    {
        Spec011ContractAssertions.HasExactProperties(
            Spec011ContractAssertions.ContractType("EligibilityReasonDto"),
            "Code", "Passed", "Blocking", "Message", "RequiredValue",
            "CurrentValue", "PolicySetId", "PolicyVersion", "SourceReference",
            "SourceAccessedOn", "ApprovedBy", "EffectiveFromUtc",
            "EffectiveToUtc", "OverridePossible", "SupportReferencePath");
        Spec011ContractAssertions.HasExactProperties(
            Spec011ContractAssertions.ContractType("GroupNonSelectableReasonDto"),
            "Code", "Message");
        Spec011ContractAssertions.HasExactProperties(
            Spec011ContractAssertions.ContractType("GroupMeetingStaffDto"),
            "Role", "Name");
        Spec011ContractAssertions.HasExactProperties(
            Spec011ContractAssertions.ContractType("GroupMeetingDto"),
            "MeetingId", "Activity", "DayOfWeek", "StartLocal", "EndLocal",
            "RoomCode", "Location", "Staff");
        Spec011ContractAssertions.HasExactProperties(
            Spec011ContractAssertions.ContractType("GroupSummaryDto"),
            "GroupId", "GroupCode", "State", "Selectable", "Capacity",
            "EnrolledCount", "SeatsRemaining", "NonSelectableReasons",
            "Meetings", "RowVersion");
        var offering = Spec011ContractAssertions.ContractType(
            "OfferingEligibilityDto");
        Spec011ContractAssertions.HasExactProperties(
            offering,
            "OfferingId", "CourseCode", "Title", "Credits",
            "CurrentPlanCredits", "ProjectedPlanCredits",
            "DefaultTargetCredits", "MaximumAllowedCredits", "Eligible",
            "Reasons", "Groups", "InputSummary", "EvaluatedAtUtc",
            "AcademicContextVersion", "CatalogueVersion", "PolicySetId",
            "PolicyVersion", "OfferingRowVersion", "CurrentPlanVersion");
        Spec011ContractAssertions.HasPropertyType(
            offering,
            "InputSummary",
            "System.Collections.Generic.IReadOnlyDictionary`2[[System.String, System.Private.CoreLib, Version=10.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e],[System.String, System.Private.CoreLib, Version=10.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]");
    }

    [Fact]
    public void Discovery_endpoint_is_student_self_scoped_and_uses_canonical_page()
    {
        var endpoints = Spec011ContractAssertions.Endpoints();
        Assert.Equal(2, endpoints.Count);
        var endpoint = Spec011ContractAssertions.Endpoint(
            "GET",
            "/api/student/terms/{termId}/offerings");

        Spec011ContractAssertions.RequiresCatalogueReadAvailable(endpoint);
        Spec011ContractAssertions.HasNoClientStudentIdentifier(endpoint);
        Spec011ContractAssertions.HasQueryParameters(
            endpoint,
            "q", "eligibility", "credits", "day", "availability", "sort",
            "page", "pageSize");
        Spec011ContractAssertions.DeclaresPage(
            endpoint,
            StatusCodes.Status200OK,
            "OfferingEligibilityDto");
        Spec011ContractAssertions.DeclaresOutcomeHeaders(
            endpoint,
            "X-Eligibility-Policy-Version",
            "X-Eligibility-Reason-Codes",
            "X-Registration-Window-State",
            "X-Support-Reference-Path");
        Spec011ContractAssertions.DeclaresErrors(
            endpoint,
            StatusCodes.Status400BadRequest,
            StatusCodes.Status401Unauthorized,
            StatusCodes.Status403Forbidden,
            StatusCodes.Status404NotFound,
            StatusCodes.Status503ServiceUnavailable,
            StatusCodes.Status500InternalServerError);
        Spec011ContractAssertions.ContractContains(
            "Student plus `Catalogue.ReadAvailable`",
            "no Student identifier is accepted",
            "Unknown or duplicate scalar keys return",
            "`eligible`, `unavailable`, or `all`; default `eligible`",
            "`available`, `full`, `unavailable`, or `all`; default `all`",
            "`courseCode,id` (default)",
            "Every sort ends in immutable offering ID",
            "Eligibility is evaluated before result filtering, sorting, and paging",
            "including `sort` echoing the applied canonical sort",
            "values below 1 return `PAGE_SIZE_INVALID`",
            "`200 Page<OfferingEligibilityDto>`",
            "`X-Eligibility-Policy-Version`",
            "`X-Eligibility-Reason-Codes`",
            "`X-Registration-Window-State`",
            "`X-Support-Reference-Path`",
            "including an empty page",
            "`404 REGISTRATION_CONTEXT_NOT_FOUND`",
            "`503 DISCOVERY_UNAVAILABLE`");
    }
}
