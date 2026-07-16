using System.Security.Cryptography;
using System.Text;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Academics.Domain;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec009;

public sealed class NFR_4EvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-009-NFR-4.md";
    private const string TestPath =
        "tests/StudentRegistration.QualityTests/Specs/Spec009/NFR-4EvidenceTests.cs";
    private static readonly DateTimeOffset PublishedAtUtc =
        new(2026, 7, 16, 14, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task Publication_and_source_evidence_capture_required_audit_dimensions()
    {
        var store = new CapturingStore();
        var clock = new FixedTimeProvider(PublishedAtUtc);
        var service = new PublicationConfirmationService(
            store,
            clock,
            Enumerable.Range(32, 32).Select(value => (byte)value).ToArray());
        var aggregateId = Guid.Parse("00000000-0000-0000-0000-000000009401");
        var dependencies = new Dictionary<string, string>
        {
            ["POLICY"] = "DEMO-POC-2026.1",
            ["TERM"] = "2026-FALL"
        };
        var preview = service.CreatePreview(new(
            PublicationScopeKind.Catalogue,
            "AI-DS",
            aggregateId,
            "admin:nfr-4",
            "draft-rv-4",
            "sha256:NFR4",
            dependencies,
            PublishedAtUtc.AddMinutes(10)));

        var result = await service.ConfirmAsync(new(
            PublicationScopeKind.Catalogue,
            "AI-DS",
            aggregateId,
            "admin:nfr-4",
            "draft-rv-4",
            "sha256:NFR4",
            dependencies,
            preview.Token,
            "NFR4-CLIENT",
            "Publish the reviewed catalogue and policy evidence.",
            "SPEC-009-NFR-4",
            "nfr-4-correlation"));

        Assert.Equal(PublicationConfirmationOutcome.Published, result.Outcome);
        Assert.NotNull(store.Command);
        Assert.Equal("admin:nfr-4", store.Command!.ActorReference);
        Assert.Equal(
            "Publish the reviewed catalogue and policy evidence.",
            store.Command.Reason);
        Assert.Equal("SPEC-009-NFR-4", store.Command.Source);
        Assert.Equal("nfr-4-correlation", store.Command.CorrelationId);
        Assert.Equal(PublishedAtUtc, store.Command.OccurredAtUtc);

        var catalogue = CataloguePublicationService.CreateDemoCurriculum();
        Assert.Equal(19, catalogue.Courses.Count);
        Assert.All(catalogue.Courses, course =>
        {
            Assert.Equal(CatalogueSourceKind.OfficialSource, course.Provenance.SourceKind);
            Assert.Equal(new DateOnly(2026, 7, 13), course.Provenance.AccessedOn);
            Assert.StartsWith("https://aast.edu/", course.Provenance.SourceReference);
            Assert.Contains("Credits", course.Provenance.SyntheticFields);
        });

        var policy = PolicyAdministrationService.CreateDemoPolicySet(
            Guid.Parse("00000000-0000-0000-0000-000000009402"));
        Assert.All(policy.Rules, rule =>
        {
            Assert.True(Enum.IsDefined(rule.ValueType));
            Assert.False(string.IsNullOrWhiteSpace(rule.Value));
            Assert.False(string.IsNullOrWhiteSpace(rule.SourceReference));
            Assert.True(Enum.IsDefined(rule.SourceKind));
        });
    }

    [Fact]
    public void Evidence_is_source_bound_privacy_safe_and_complete()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-009 NFR-4 Audit and Provenance Evidence",
            "actor",
            "reason",
            "source",
            "access date",
            "value classification",
            "server timestamp",
            "synthetic field",
            "1 passed",
            "0 failed",
            $"Quality test normalized-LF SHA-256: `{SourceHash(TestPath)}`",
            "**Result: PASS.**");
        Assert.DoesNotMatch(
            @"(?i)\b(?:PENDING|TODO|TBD|FIXME|PLACEHOLDER|NOT\s+RUN|NOT\s+EXECUTED)\b",
            evidence);
        Assert.DoesNotMatch(
            @"(?im)\b(?:password|credential|cookie|security[_ -]?stamp)\s*[:=]\s*\S+",
            evidence);
    }

    private static string SourceHash(string relativePath)
    {
        var source = RepositoryFiles.Read(relativePath)
            .Replace("\r\n", "\n", StringComparison.Ordinal);
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(source)));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class CapturingStore : IPublicationConfirmationStore
    {
        public PublicationConfirmationStoreCommand? Command { get; private set; }

        public Task<PublicationConfirmationStoreResult> ConfirmAsync(
            PublicationConfirmationStoreCommand command,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Command = command;
            return Task.FromResult(new PublicationConfirmationStoreResult(
                PublicationConfirmationStoreOutcome.Published,
                Guid.Parse("00000000-0000-0000-0000-000000009499"),
                "draft-rv-5"));
        }
    }
}
