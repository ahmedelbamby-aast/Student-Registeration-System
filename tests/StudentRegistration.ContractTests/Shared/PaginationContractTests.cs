using System.Text.Json;
using System.Text.RegularExpressions;
using StudentRegistration.Contracts;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Shared;

public sealed class PaginationContractTests
{
    [Fact]
    public void Page_has_the_canonical_owner_and_exact_immutable_response_shape()
    {
        using var ownership = System.Text.Json.JsonDocument.Parse(
            RepositoryFiles.Read(".specify/entity-ownership.json"));
        Assert.Equal(
            "src/StudentRegistration.Contracts/Page.cs",
            ownership.RootElement.GetProperty("artifactOverrides")
                .GetProperty("006:Page")
                .GetString());

        Assert.True(typeof(Page<>).IsGenericTypeDefinition);
        Assert.True(typeof(Page<>).IsSealed);
        Assert.Equal(
            ["Items", "PageNumber", "PageSize", "Sort", "TotalCount"],
            typeof(Page<>).GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal)
                .ToArray());
    }

    [Fact]
    public void Page_copies_items_and_exposes_a_read_only_snapshot()
    {
        var suppliedItems = new List<string> { "AI101", "AI102" };

        var result = new Page<string>(
            suppliedItems,
            pageNumber: 1,
            pageSize: 20,
            totalCount: 2,
            sort: "code,id");

        suppliedItems[0] = "CHANGED";
        suppliedItems.Add("AI103");

        Assert.Equal(["AI101", "AI102"], result.Items);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal("code,id", result.Sort);
        var listView = Assert.IsAssignableFrom<IList<string>>(result.Items);
        Assert.True(listView.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => listView.Add("AI104"));

        using var document = JsonDocument.Parse(
            JsonSerializer.Serialize(result, JsonSerializerOptions.Web));
        Assert.Equal(5, document.RootElement.EnumerateObject().Count());
        Assert.Equal(1, document.RootElement.GetProperty("page").GetInt32());
        Assert.Equal(20, document.RootElement.GetProperty("pageSize").GetInt32());
        Assert.Equal("code,id", document.RootElement.GetProperty("sort").GetString());
    }

    [Fact]
    public void Page_round_trips_through_the_canonical_web_json_contract()
    {
        var original = new Page<string>(["AI101"], 1, 20, 1, "code,id");

        var json = JsonSerializer.Serialize(original, JsonSerializerOptions.Web);
        var roundTrip = JsonSerializer.Deserialize<Page<string>>(
            json,
            JsonSerializerOptions.Web);

        Assert.NotNull(roundTrip);
        Assert.Equal(original.Items, roundTrip.Items);
        Assert.Equal(original.PageNumber, roundTrip.PageNumber);
        Assert.Equal(original.PageSize, roundTrip.PageSize);
        Assert.Equal(original.TotalCount, roundTrip.TotalCount);
        Assert.Equal(original.Sort, roundTrip.Sort);
    }

    [Fact]
    public void Page_rejects_invalid_bounds_instead_of_silently_capping()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Page<string>(null!, 1, 20, 0, "id"));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Page<string>([], 0, 20, 0, "id"));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Page<string>([], 1, 0, 0, "id"));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Page<string>([], 1, 101, 0, "id"));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Page<string>([], 1, 20, -1, "id"));
        Assert.Throws<ArgumentException>(() =>
            new Page<string>([], 1, 20, 0, "   "));
        Assert.Throws<ArgumentException>(() =>
            new Page<string>(["AI101", "AI102"], 1, 1, 2, "id"));
        Assert.Throws<ArgumentException>(() =>
            new Page<string>(["AI101", "AI102"], 1, 20, 1, "id"));
    }

    [Fact]
    public void Approved_protocol_defines_request_defaults_error_and_canonical_sort_tie_breaker()
    {
        var apiContract = RepositoryFiles.Read(
            "specs/006-domain-class-api-contracts/contracts/api.md");
        var pagination = Regex.Replace(
            apiContract,
            @"\s+",
            " ");

        RepositoryFiles.ContainsAll(
            pagination,
            "Omitted `page` and `pageSize` mean `1` and `20`; maximum `pageSize` is `100`.",
            "`PAGE_SIZE_INVALID`; servers do not silently cap.",
            "A response contains no more items than its declared `pageSize`",
            "`totalCount` is never smaller than the returned item count.",
            "unique identifier as a deterministic tie-breaker.",
            "`Page.sort` echoes the applied canonical sort.");
    }
}
