using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Shared;

public sealed class PageModelTests
{
    private const string SchemaPath =
        "specs/006-domain-class-api-contracts/schemas/page.schema.json";

    [Fact]
    public void Schema_is_closed_versioned_and_owned_by_spec006()
    {
        using var schema = JsonDocument.Parse(RepositoryFiles.Read(SchemaPath));
        var root = schema.RootElement;

        Assert.Equal(
            "https://json-schema.org/draft/2020-12/schema",
            root.GetProperty("$schema").GetString());
        Assert.Equal(
            "https://student-registration.demo/schemas/page/1.0",
            root.GetProperty("$id").GetString());
        Assert.Equal("SPEC-006", root.GetProperty("x-owner-spec").GetString());
        Assert.Equal("1.0", root.GetProperty("x-schema-version").GetString());
        Assert.Equal("object", root.GetProperty("type").GetString());
        Assert.False(root.GetProperty("additionalProperties").GetBoolean());
    }

    [Fact]
    public void Schema_requires_bounded_pagination_and_echoes_the_applied_sort()
    {
        using var schema = JsonDocument.Parse(RepositoryFiles.Read(SchemaPath));
        var root = schema.RootElement;
        var properties = root.GetProperty("properties");
        var required = root.GetProperty("required")
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(
            new[] { "items", "page", "pageSize", "sort", "totalCount" },
            required.Order(StringComparer.Ordinal));

        var page = properties.GetProperty("page");
        Assert.Equal("integer", page.GetProperty("type").GetString());
        Assert.Equal(1, page.GetProperty("minimum").GetInt32());
        Assert.Equal(1, page.GetProperty("default").GetInt32());

        var pageSize = properties.GetProperty("pageSize");
        Assert.Equal("integer", pageSize.GetProperty("type").GetString());
        Assert.Equal(1, pageSize.GetProperty("minimum").GetInt32());
        Assert.Equal(100, pageSize.GetProperty("maximum").GetInt32());
        Assert.Equal(20, pageSize.GetProperty("default").GetInt32());

        var items = properties.GetProperty("items");
        Assert.Equal("array", items.GetProperty("type").GetString());
        Assert.Equal(100, items.GetProperty("maxItems").GetInt32());

        var totalCount = properties.GetProperty("totalCount");
        Assert.Equal("integer", totalCount.GetProperty("type").GetString());
        Assert.Equal(0, totalCount.GetProperty("minimum").GetInt32());

        var sort = properties.GetProperty("sort");
        Assert.Equal("string", sort.GetProperty("type").GetString());
        Assert.Equal(1, sort.GetProperty("minLength").GetInt32());
        Assert.Contains(
            "applied canonical sort",
            sort.GetProperty("description").GetString(),
            StringComparison.OrdinalIgnoreCase);
    }
}
