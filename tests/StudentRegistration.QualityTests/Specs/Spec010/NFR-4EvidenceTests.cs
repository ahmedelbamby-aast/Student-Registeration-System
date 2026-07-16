using System.Security.Cryptography;
using System.Text;
using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec010;

public sealed class NFR_4EvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-010-NFR-4.md";
    private const string TestPath =
        "tests/StudentRegistration.QualityTests/Specs/Spec010/NFR-4EvidenceTests.cs";

    [Fact]
    public async Task Admin_list_is_bounded_and_forwards_filters_with_stable_sort()
    {
        var store = new CapturingOfferingStore();
        var service = new OfferingService(store);
        var termId = Id(1);

        var result = await service.ListAdminOfferingsAsync(
            new AdminOfferingQuery(
                termId,
                State: "published",
                Query: " csc ",
                Page: 2,
                PageSize: 100,
                Sort: null));

        Assert.Equal(OfferingOutcome.Found, result.Outcome);
        Assert.NotNull(result.Page);
        Assert.Equal(100, result.Page!.Items.Count);
        Assert.Equal(2, result.Page.Page);
        Assert.Equal(100, result.Page.PageSize);
        Assert.Equal(250, result.Page.TotalCount);
        Assert.Equal("courseCode,id", result.Page.Sort);

        var captured = Assert.Single(store.Queries);
        Assert.Equal(termId, captured.TermId);
        Assert.Equal("published", captured.State);
        Assert.Equal("csc", captured.Query);
        Assert.Equal(2, captured.Page);
        Assert.Equal(100, captured.PageSize);
        Assert.Equal("courseCode,id", captured.Sort);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    [InlineData(0, 20)]
    public async Task Invalid_page_bounds_fail_before_the_store(
        int page,
        int pageSize)
    {
        var store = new CapturingOfferingStore();
        var result = await new OfferingService(store).ListAdminOfferingsAsync(
            new AdminOfferingQuery(null, null, null, page, pageSize, null));

        Assert.Equal(OfferingOutcome.ValidationError, result.Outcome);
        Assert.Contains(
            result.ErrorCode,
            new[] { "PAGE_INVALID", "PAGE_SIZE_INVALID" });
        Assert.Empty(store.Queries);
    }

    [Fact]
    public async Task Invalid_query_and_sort_values_fail_before_the_store()
    {
        var store = new CapturingOfferingStore();
        var service = new OfferingService(store);

        var shortQuery = await service.ListAdminOfferingsAsync(
            new AdminOfferingQuery(null, null, " ab ", 1, 20, null));
        var longQuery = await service.ListAdminOfferingsAsync(
            new AdminOfferingQuery(null, null, new string('x', 51), 1, 20, null));
        var unsupportedSort = await service.ListAdminOfferingsAsync(
            new AdminOfferingQuery(null, null, "csc", 1, 20, "title,id"));

        Assert.All(
            new[] { shortQuery, longQuery, unsupportedSort },
            result =>
            {
                Assert.Equal(OfferingOutcome.ValidationError, result.Outcome);
                Assert.Equal("VALIDATION_ERROR", result.ErrorCode);
            });
        Assert.Empty(store.Queries);
    }

    [Fact]
    public void Evidence_is_source_bound_and_records_the_exact_page_contract()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-010 NFR-4 Bounded Admin List Evidence",
            "page 2",
            "page size 100",
            "total count 250",
            "courseCode,id",
            "term",
            "state",
            "query",
            "trimmed",
            "PAGE_SIZE_INVALID",
            "PAGE_INVALID",
            "VALIDATION_ERROR",
            "unsupported sort",
            "6 passed",
            "0 failed",
            $"Quality test normalized-LF SHA-256: `{SourceHash(TestPath)}`",
            "**Result: PASS.**");
        Assert.DoesNotMatch(
            @"(?i)\b(?:TODO|TBD|FIXME|PLACEHOLDER|NOT\s+RUN|NOT\s+EXECUTED)\b",
            evidence);
    }

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:000000000000}");

    private static string SourceHash(string relativePath)
    {
        var source = RepositoryFiles.Read(relativePath)
            .Replace("\r\n", "\n", StringComparison.Ordinal);
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(source)));
    }

    private sealed class CapturingOfferingStore : IOfferingStore
    {
        public List<AdminOfferingQuery> Queries { get; } = [];

        public Task<OfferingSnapshot?> LoadAsync(
            Guid offeringId,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<OfferingSnapshot> CreateAsync(
            CreateOfferingStoreCommand command,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<byte[]> UpdateGroupAsync(
            UpdateGroupStoreCommand command,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<AdminOfferingPage> ListAsync(
            AdminOfferingQuery query,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Queries.Add(query);
            return Task.FromResult(new AdminOfferingPage(
                Enumerable.Range(1, query.PageSize)
                    .Select(index => new OfferingSnapshot(
                        Id(1000 + index),
                        "published",
                        []))
                    .ToArray(),
                query.Page,
                query.PageSize,
                TotalCount: 250,
                query.Sort ?? "courseCode,id"));
        }
    }
}
