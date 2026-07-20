using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec014;

public sealed class Endpoint01ContractTests
{
    [Fact]
    public void Post_request_and_result_shapes_are_bounded_and_server_scoped()
    {
        var request = Spec014ContractAssertions.Interface("SubmitRegistrationRequest");
        Spec014ContractAssertions.ContainsAll(
            request,
            "planId: string",
            "expectedPlanRowVersion: string",
            "clientRequestId: string");
        Spec014ContractAssertions.Excludes(
            request,
            "studentId",
            "termId",
            "policyVersion",
            "groupIds");

        var final = Spec014ContractAssertions.Interface("RegistrationFinalResult");
        Spec014ContractAssertions.ContainsAll(
            final,
            "submissionId: string",
            "status: \"pendingApproval\" | \"accepted\" | \"rejected\" | \"expired\"",
            "resultCode: string",
            "registeredGroups: RegistrationGroupSnapshotDto[]",
            "receivedAtUtc: string",
            "completedAtUtc: string",
            "policySetId: string",
            "policyVersion: string",
            "planRowVersion: string",
            "reference?: string",
            "receiptSnapshot?: RegistrationReceiptSnapshotDto",
            "origin: \"studentSelfService\" | \"firstTermAutomatic\"",
            "requestedCredits: number",
            "lines: RegistrationSubmissionLineDto[]");

        var processing = Spec014ContractAssertions.Interface(
            "RegistrationInProgressResponse");
        Spec014ContractAssertions.ContainsAll(
            processing,
            "clientRequestId: string",
            "status: \"processing\"",
            "retryAfterSeconds: number",
            "resultUrl: string");
        Spec014ContractAssertions.Excludes(processing, "submissionId", "studentId");
    }

    [Fact]
    public void Receipt_preserves_policy_and_meeting_bound_staff_serialization()
    {
        Spec014ContractAssertions.ContainsAll(
            Spec014ContractAssertions.Interface("RegistrationReceiptSnapshotDto"),
            "term: TermSummaryDto",
            "groups: RegistrationGroupSnapshotDto[]",
            "totalCredits: number",
            "policySetId: string",
            "policyVersion: string",
            "submittedAtUtc: string");
        Spec014ContractAssertions.ContainsAll(
            Spec014ContractAssertions.Interface("RegistrationMeetingSnapshotDto"),
            "meetingId: string",
            "activityType: \"Lecture\" | \"Tutorial\" | \"Laboratory\"",
            "dayOfWeek: number",
            "staff: RegistrationMeetingStaffSnapshotDto[]");
        Spec014ContractAssertions.ContractContains(
            "`DayOfWeek` serialization `0..6`",
            "Staff remain nested under the meeting they teach",
            "Array order MUST NOT be used to infer a staff-to-meeting relationship");
    }

    [Fact]
    public void Post_requires_exact_permission_antiforgery_and_declares_finalized_statuses()
    {
        var post = Spec014ContractAssertions.Section(
            "### POST /api/student/terms/{termId}/registrations",
            "### GET /api/student/terms/{termId}/registrations/by-request/{clientRequestId}");
        Spec014ContractAssertions.ContainsAll(
            post,
            "authenticated `Student`",
            "`Registration.SubmitOwn` permission",
            "valid same-origin antiforgery token",
            "`400 ANTIFORGERY_INVALID`",
            "Student identity comes only from authentication",
            "The body does not contain studentId or termId",
            "- 201: newly committed durable `pendingApproval` self-service result or newly committed accepted first-term automatic result",
            "- 200: same-scope, same-payload durable lifecycle replay",
            "- 202: first same-scope claim is still uncommitted after at most 500 ms",
            "- 400: malformed input",
            "- 401/403: authentication/authorization",
            "- 409: business/version conflict or same-scope key reused with different canonical payload");
    }
}

internal static class Spec014ContractAssertions
{
    private const string ContractPath =
        "specs/014-registration-capacity-concurrency/contracts/api.md";

    private static readonly string Contract = Normalize(
        RepositoryFiles.Read(ContractPath));

    public static string Interface(string name)
    {
        var match = Regex.Match(
            Contract,
            $@"interface\s+{Regex.Escape(name)}\s*\{{(?<body>.*?)\}}",
            RegexOptions.CultureInvariant);
        Assert.True(match.Success, $"Missing finalized interface {name} in {ContractPath}.");
        return match.Groups["body"].Value;
    }

    public static string Section(string start, string end)
    {
        var startIndex = Contract.IndexOf(start, StringComparison.Ordinal);
        Assert.True(startIndex >= 0, $"Missing section {start} in {ContractPath}.");
        var endIndex = Contract.IndexOf(end, startIndex + start.Length, StringComparison.Ordinal);
        Assert.True(endIndex > startIndex, $"Missing section boundary {end} in {ContractPath}.");
        return Contract[startIndex..endIndex];
    }

    public static void ContractContains(params string[] fragments) =>
        ContainsAll(Contract, fragments);

    public static void ContainsAll(string value, params string[] fragments)
    {
        foreach (var fragment in fragments)
        {
            Assert.Contains(fragment, value, StringComparison.Ordinal);
        }
    }

    public static void Excludes(string value, params string[] fragments)
    {
        foreach (var fragment in fragments)
        {
            Assert.DoesNotContain(fragment, value, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string Normalize(string value) => Regex.Replace(
        value,
        @"\s+",
        " ").Trim();
}
