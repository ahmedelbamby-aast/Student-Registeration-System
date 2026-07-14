using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec007;

public sealed class IdentityImportBatchModelTests
{
    [Fact]
    public void Import_batch_is_the_bounded_versioned_idempotency_record()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Domain/IdentityImportBatch.cs");

        RepositoryFiles.ContainsAll(
            source,
            "public sealed class IdentityImportBatch",
            "public Guid RequestedByUserId { get;",
            "public string ClientRequestId { get;",
            "public string SourceName { get;",
            "public string SourceHash { get;",
            "public string State { get;",
            "public string ErrorSummaryJson { get;",
            "public string? ResultSummaryJson { get;",
            "public DateTime ImportedAtUtc { get;",
            "public byte[] Version { get;");
        Assert.DoesNotContain("Password", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("RawRow", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Microsoft.EntityFrameworkCore", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Import_batch_accepts_only_governed_state_transitions()
    {
        var batch = new IdentityImportBatch(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "import-001",
            "demo-users.csv",
            "sha256:canonical-content",
            DateTime.UnixEpoch);

        Assert.Equal(IdentityImportStates.Uploaded, batch.State);

        batch.RecordValidation(IdentityImportStates.Validated, "[]");
        batch.Publish("{\"created\":2}");

        Assert.Equal(IdentityImportStates.Published, batch.State);
        Assert.Equal("{\"created\":2}", batch.ResultSummaryJson);
        Assert.Throws<InvalidOperationException>(
            () => batch.RecordValidation(IdentityImportStates.Invalid, "[]"));
    }

    [Fact]
    public void Candidate_row_is_an_immutable_normalized_batch_owned_staging_child()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Domain/IdentityImportCandidateRow.cs");

        RepositoryFiles.ContainsAll(
            source,
            "public sealed class IdentityImportCandidateRow",
            "public Guid IdentityImportBatchId { get;",
            "public int Ordinal { get;",
            "public string ExternalReference { get;",
            "public string Kind { get;",
            "public string? UniversityId { get;",
            "public string? UserName { get;",
            "public string? StaffNumber { get;",
            "public string DisplayName { get;",
            "public string Roles { get;");
        Assert.DoesNotContain("Password", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Raw", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("public byte[] Version", source, StringComparison.Ordinal);

        var student = new IdentityImportCandidateRow(
            Guid.NewGuid(),
            Guid.NewGuid(),
            1,
            "student-1",
            "student",
            "AI2600001",
            null,
            null,
            "Student One",
            ["Student"]);
        var staff = new IdentityImportCandidateRow(
            Guid.NewGuid(),
            Guid.NewGuid(),
            2,
            "staff-1",
            "staff",
            null,
            "lecturer.one",
            "S-001",
            "Lecturer One",
            ["TeachingAssistant", "Lecturer"]);

        Assert.Equal("Student", student.Roles);
        Assert.Equal("Lecturer,TeachingAssistant", staff.Roles);
    }
}
