using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Domain;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec009;

public sealed class NFR_1EvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-009-NFR-1.md";
    private const string TestPath =
        "tests/StudentRegistration.QualityTests/Specs/Spec009/NFR-1EvidenceTests.cs";
    private const int RowCount = 10_000;
    private static readonly TimeSpan MaximumDuration = TimeSpan.FromSeconds(30);

    [Fact]
    public void Ten_thousand_row_catalogue_validates_within_the_staging_gate()
    {
        var official = new CatalogueFieldProvenance(
            "https://aast.edu/en/programs-courses/program.php?language_id=1&program_id=283&unit_id=655",
            new DateOnly(2026, 7, 13),
            CatalogueSourceKind.OfficialSource,
            ["Credits", "IsActive"]);
        var synthetic = new CatalogueFieldProvenance(
            "SPEC-009-NFR-1-SYNTHETIC",
            new DateOnly(2026, 7, 16),
            CatalogueSourceKind.SyntheticDemo,
            ["Code", "Title", "Credits", "IsActive"]);
        var courses = Enumerable.Range(1, RowCount)
            .Select(index => new CatalogueCourseDefinition(
                $"Q{index:00000}",
                $"Synthetic quality course {index:00000}",
                3m,
                true,
                index,
                index == 1 ? [] : [$"Q{index - 1:00000}"],
                MinimumGpa: null,
                MinimumEarnedCredits: null,
                index % 2 == 0 ? official : synthetic))
            .ToArray();
        var content = new CatalogueDraftContent("NFR-1-10K", courses);
        var service = new CataloguePublicationService();

        var stopwatch = Stopwatch.StartNew();
        var result = service.Validate(content);
        stopwatch.Stop();

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.StartsWith("sha256:", result.CanonicalContentHash, StringComparison.Ordinal);
        Assert.Equal(RowCount, courses.Length);
        Assert.True(
            stopwatch.Elapsed < MaximumDuration,
            $"10,000-row validation took {stopwatch.Elapsed.TotalSeconds:F3}s.");
    }

    [Fact]
    public void Evidence_is_source_bound_and_records_the_exact_gate_without_placeholders()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-009 NFR-1 10,000-Row Validation Evidence",
            "10,000",
            "30 seconds",
            "official-source",
            "synthetic-demo",
            "1 passed",
            "0 failed",
            $"Quality test normalized-LF SHA-256: `{SourceHash(TestPath)}`",
            "**Result: PASS.**");
        Assert.DoesNotMatch(
            @"(?i)\b(?:PENDING|TODO|TBD|FIXME|PLACEHOLDER|NOT\s+RUN|NOT\s+EXECUTED)\b",
            evidence);
    }

    private static string SourceHash(string relativePath)
    {
        var source = RepositoryFiles.Read(relativePath)
            .Replace("\r\n", "\n", StringComparison.Ordinal);
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(source)));
    }
}
