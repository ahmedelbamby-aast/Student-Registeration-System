using System.Globalization;
using StudentRegistration.TestSupport;
using StudentRegistration.TestSupport.Spec002;

namespace StudentRegistration.QualityTests.Specs.Spec002;

public sealed class NFR_4EvidenceTests
{
    private const string SourceRegisterPath =
        "specs/002-aastmt-policy-rulebook/policy-sources.md";

    private const string SourceTableHeader =
        "| ID | Classification | Authority | URL/reference | Accessed | Affected facts | Approval/status |";

    private const string SourceTableSeparator =
        "|---|---|---|---|---|---|---|";

    private static readonly IReadOnlyDictionary<string, string> NotSelectedReasonByFixture =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["PB-18"] = "POLICY_UNAVAILABLE",
            ["PB-19"] = "POLICY_SCOPE_AMBIGUOUS"
        };

    [Fact]
    public void Harness_sources_match_every_canonical_policy_source_table_field()
    {
        var canonicalSources = ReadCanonicalSourceRows();
        var harness = Spec002PolicyTestHarness.Load();

        Assert.Equal(12, canonicalSources.Count);
        Assert.Equal(12, harness.Sources.Count);
        Assert.Equal(
            canonicalSources.Select(source => source.Id).Order(StringComparer.Ordinal),
            harness.Sources.Select(source => source.Id).Order(StringComparer.Ordinal));

        Assert.Equal(
            harness.Sources.Count,
            harness.Sources.Select(source => source.Id).Distinct(StringComparer.Ordinal).Count());

        var harnessSourcesById = harness.Sources.ToDictionary(
            source => source.Id,
            StringComparer.Ordinal);

        foreach (var canonical in canonicalSources)
        {
            var source = harnessSourcesById[canonical.Id];

            Assert.Equal(canonical.Id, source.Id);
            Assert.Equal(canonical.Classification, source.ProvenanceKind);
            Assert.Equal(canonical.Authority, source.Authority);
            Assert.Equal(canonical.UrlOrReference, source.Url);
            Assert.Equal(canonical.AccessedOn, source.AccessedOn);
            Assert.Equal(canonical.AffectedFacts, source.AffectedFacts);
            Assert.Equal(canonical.ApprovalStatus, source.ApprovalStatus);
            Assert.Equal(canonical.ContentReference, source.ContentReference);

            AssertAuditableSourceReference(source.Url);
            AssertAuditableSourceReference(source.ContentReference);
            Assert.False(string.IsNullOrWhiteSpace(source.ApprovalActor));
            Assert.NotEqual(default, source.EffectiveFromUtc);
            Assert.True(source.EffectiveToUtc is null
                || source.EffectiveToUtc > source.EffectiveFromUtc);
        }
    }

    [Fact]
    public void Decision_bindings_are_canonical_and_policy_selection_metadata_uses_explicit_sentinels()
    {
        var harness = Spec002PolicyTestHarness.Load();
        var sourcesById = harness.Sources.ToDictionary(
            source => source.Id,
            StringComparer.Ordinal);
        var selectedDecisionCount = 0;
        var notSelectedDecisionCount = 0;
        var inspectedBindingCount = 0;

        foreach (var fixture in harness.BoundaryCases)
        {
            var decision = harness.Evaluate(fixture.Input);

            Assert.NotEmpty(decision.Results);
            if (NotSelectedReasonByFixture.TryGetValue(fixture.Id, out var expectedReasonCode))
            {
                notSelectedDecisionCount++;
                Assert.False(decision.Eligible);
                Assert.Equal("NOT_SELECTED", decision.PolicyVersion);
                Assert.Equal("Not selected", decision.ApprovedBy);
                Assert.Equal(default, decision.EffectiveFromUtc);
                Assert.Null(decision.EffectiveToUtc);
                Assert.Null(decision.MaximumCredits);
                Assert.False(decision.UsedFallback);
                Assert.True(decision.AdminAlertRaised);
                Assert.Equal(expectedReasonCode, decision.ReasonCode);
            }
            else
            {
                selectedDecisionCount++;
                Assert.Equal(Spec002PolicyTestHarness.Version, decision.PolicyVersion);
                Assert.False(string.IsNullOrWhiteSpace(decision.ApprovedBy));
                Assert.NotEqual(default, decision.EffectiveFromUtc);
                Assert.True(decision.EffectiveToUtc is null
                    || decision.EffectiveToUtc > decision.EffectiveFromUtc);
                Assert.NotNull(decision.MaximumCredits);
                Assert.False(decision.UsedFallback);
                Assert.False(decision.AdminAlertRaised);
            }

            foreach (var result in decision.Results)
            {
                inspectedBindingCount++;
                Assert.True(
                    sourcesById.TryGetValue(result.Source.Id, out var canonicalSource),
                    $"Decision result '{result.ReasonCode}' references unknown source '{result.Source.Id}'.");
                Assert.Equal(canonicalSource, result.Source);
            }
        }

        Assert.Equal(17, selectedDecisionCount);
        Assert.Equal(2, notSelectedDecisionCount);
        Assert.Equal(189, inspectedBindingCount);
    }

    private static IReadOnlyList<CanonicalPolicySource> ReadCanonicalSourceRows()
    {
        var lines = RepositoryFiles.Read(SourceRegisterPath)
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var headerIndex = Array.FindIndex(
            lines,
            line => string.Equals(line, SourceTableHeader, StringComparison.Ordinal));

        Assert.True(headerIndex >= 0, "The canonical policy source table header is missing.");
        Assert.True(headerIndex + 2 < lines.Length, "The canonical policy source table has no data rows.");
        Assert.Equal(SourceTableSeparator, lines[headerIndex + 1]);

        var rows = lines
            .Skip(headerIndex + 2)
            .TakeWhile(line => line.StartsWith('|'))
            .Select(ParseCanonicalSourceRow)
            .ToArray();

        Assert.Equal(rows.Length, rows.Select(row => row.Id).Distinct(StringComparer.Ordinal).Count());
        return rows;
    }

    private static CanonicalPolicySource ParseCanonicalSourceRow(string row)
    {
        var cells = row.Trim().Trim('|').Split('|', StringSplitOptions.TrimEntries);
        Assert.Equal(7, cells.Length);

        var urlOrReference = TrimCode(cells[3]);
        return new CanonicalPolicySource(
            Id: TrimCode(cells[0]),
            Classification: TrimCode(cells[1]),
            Authority: cells[2],
            UrlOrReference: urlOrReference,
            AccessedOn: DateOnly.ParseExact(
                cells[4],
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture),
            AffectedFacts: cells[5],
            ApprovalStatus: cells[6],
            ContentReference: urlOrReference);
    }

    private static string TrimCode(string value) => value.Trim().Trim('`');

    private static void AssertAuditableSourceReference(string sourceReference)
    {
        Assert.False(string.IsNullOrWhiteSpace(sourceReference));

        var isHttps = Uri.TryCreate(sourceReference, UriKind.Absolute, out var uri)
            && uri.Scheme == Uri.UriSchemeHttps;
        var isRepositoryReference = !Path.IsPathRooted(sourceReference)
            && (sourceReference.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                || sourceReference.Contains(".md#", StringComparison.OrdinalIgnoreCase));

        Assert.True(
            isHttps || isRepositoryReference,
            $"Source reference '{sourceReference}' is neither HTTPS nor a repository Markdown reference.");
    }

    private sealed record CanonicalPolicySource(
        string Id,
        string Classification,
        string Authority,
        string UrlOrReference,
        DateOnly AccessedOn,
        string AffectedFacts,
        string ApprovalStatus,
        string ContentReference);
}
