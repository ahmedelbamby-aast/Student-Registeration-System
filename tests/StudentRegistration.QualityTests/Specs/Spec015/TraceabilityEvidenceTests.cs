using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec015;

public sealed class TraceabilityEvidenceTests
{
    [Fact]
    public void Every_normative_identifier_route_and_endpoint_is_traced()
    {
        var trace = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-015-traceability.md");
        var identifiers = Enumerable.Range(1, 8).Select(number => $"FR-{number}")
            .Concat(Enumerable.Range(1, 4).Select(number => $"NFR-{number}"))
            .Concat(Enumerable.Range(1, 3).Select(number => $"SC-{number}"))
            .Concat(Enumerable.Range(1, 6).Select(number => $"AC-{number}"))
            .Concat(Enumerable.Range(1, 4).Select(number => $"EC-{number}"))
            .Concat(["STU-06", "STU-07", "OS-1", "OS-2", "OS-3", "OS-4"]);

        Assert.All(identifiers,
            identifier => Assert.Contains(identifier, trace, StringComparison.Ordinal));
        RepositoryFiles.ContainsAll(
            trace,
            "GET `/api/student/registrations`",
            "GET `/api/student/registrations/{submissionId}`",
            "GET `/api/student/registrations/current/timetable`",
            "GET `/api/admin/students/{studentId}/terms/{termId}/registrations`",
            "GET `/api/admin/students/{studentId}/terms/{termId}/registrations/{submissionId}`",
            "SPEC-015-scope-review.md",
            "Release is rejected");
        Assert.DoesNotMatch(@"(?i)\b(?:TODO|TBD|FIXME|PLACEHOLDER)\b", trace);
    }

    [Fact]
    public void Nfr_documents_pass_and_approval_is_explicitly_bounded()
    {
        foreach (var number in Enumerable.Range(1, 4))
        {
            var document = RepositoryFiles.Read(
                $"docs/release-evidence/SPEC-015-NFR-{number}.md");
            Assert.Matches(@"(?im)^\s*\*\*Result:\*\*\s+PASS\b", document);
        }

        var approval = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-015-release-approval.md");
        RepositoryFiles.ContainsAll(
            approval,
            "Release decision:** APPROVED",
            "Product owner | Approved",
            "Domain owner | Approved",
            "QA | Approved",
            "Security | Approved",
            "Accessibility | Approved",
            "Data and concurrency | Approved",
            "Operations | Approved",
            "Approver:** Ahmed Elbamby",
            "bounded non-production SPEC-015 demo release only",
            "not production approval");
    }
}
