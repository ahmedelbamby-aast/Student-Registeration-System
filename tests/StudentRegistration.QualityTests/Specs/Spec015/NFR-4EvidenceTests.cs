using System.Net.Http.Json;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Registration;
using StudentRegistration.IdentityAccess.Application.Authorization;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec015;

[Collection(Spec015SqlEvidenceCollection.Name)]
public sealed class NFR_4EvidenceTests(Spec015SqlEvidenceFixture fixture)
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Every_archived_snapshot_survives_the_real_upgrade_and_mutable_term_edits()
    {
        var applied = await fixture.AppliedMigrationsAsync();
        Assert.Equal(
            [
                "20260713010000_IdentityAcademicFoundation",
                "20260713020000_CatalogueScheduling",
                "20260713040000_DiscoveryPlanning",
                "20260713060000_Registration",
                "20260713070000_StaffAdminOperations",
                "20260717120000_Spec017ExportFilter",
                "20260717222551_Spec018RegistrationReadPerformance"
            ],
            applied);

        await fixture.RenameArchivedTermsAsync();
        using var client = fixture.CreateClient(
            fixture.TargetApplicationUserId,
            RolePolicies.Student,
            RolePolicies.RegistrationRecordsReadOwn);

        foreach (var expected in fixture.ArchivedTermIds.Select((termId, index) =>
            new
            {
                TermId = termId,
                DisplayName = $"Original archived term {index + 1:D2}",
                Location = $"Original archived room {index + 1:D2}"
            }))
        {
            var path = $"/api/student/registrations?page=1&pageSize=100&termId={expected.TermId:D}";
            var page = await client.GetFromJsonAsync<Page<RegistrationHistoryRowDto>>(path);
            var row = Assert.Single(page!.Items);
            Assert.Equal(expected.DisplayName, row.Term.DisplayName);

            var detail = await client.GetFromJsonAsync<RegistrationDetailDto>(
                $"/api/student/registrations/{row.SubmissionId:D}");
            Assert.Equal(expected.DisplayName, detail!.Receipt!.Term.DisplayName);
            Assert.Equal(expected.Location,
                detail.Receipt.Groups[0].Meetings[0].Location);
        }
    }

    [Fact]
    public void Production_hard_retention_remains_unapproved_and_fail_closed()
    {
        var contract = RepositoryFiles.Read(
            "specs/005-erd-data-lifecycle/contracts/immutable-history.md");
        var research = RepositoryFiles.Read(
            "specs/015-student-registration-records/research.md");

        RepositoryFiles.ContainsAll(
            contract,
            "Hard deletion and retention for real or production data remain unapproved",
            "fail closed",
            "RegistrationReceipt is a projection, not a second table or write path");
        RepositoryFiles.ContainsAll(
            research,
            "Institutional retention values remain governed by SPEC-005",
            "must fail closed",
            "no value is invented here");
        Assert.DoesNotMatch(@"(?i)production\s+retention\s*=\s*\d+", research);
    }
}
