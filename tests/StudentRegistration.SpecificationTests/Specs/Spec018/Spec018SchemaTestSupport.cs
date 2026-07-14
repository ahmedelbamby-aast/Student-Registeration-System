using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec018;

internal static class Spec018SchemaTestSupport
{
    public const string Draft202012 =
        "https://json-schema.org/draft/2020-12/schema";

    public static JsonDocument ReadSchema(string path) =>
        JsonDocument.Parse(RepositoryFiles.Read(path));

    public static JsonDocument ReadMarkedSchema(string path, string marker)
    {
        var document = RepositoryFiles.Read(path);
        var startMarker = $"<!-- {marker}_START -->";
        var endMarker = $"<!-- {marker}_END -->";
        var start = document.IndexOf(startMarker, StringComparison.Ordinal);
        var end = document.IndexOf(endMarker, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Schema marker was not found: {startMarker}");
        Assert.True(end > start, $"Schema marker was not found: {endMarker}");

        var marked = document[(start + startMarker.Length)..end];
        var jsonStart = marked.IndexOf('{');
        var jsonEnd = marked.LastIndexOf('}');
        Assert.True(jsonStart >= 0 && jsonEnd > jsonStart, $"No JSON object was found for {marker}.");

        return JsonDocument.Parse(marked[jsonStart..(jsonEnd + 1)]);
    }

    public static void AssertClosedVersionedOwned(
        JsonElement schema,
        string expectedId)
    {
        Assert.Equal(Draft202012, schema.GetProperty("$schema").GetString());
        Assert.Equal(expectedId, schema.GetProperty("$id").GetString());
        Assert.Equal("SPEC-018", schema.GetProperty("x-owner-spec").GetString());
        Assert.Equal("1.0", schema.GetProperty("x-schema-version").GetString());
        Assert.True(schema.GetProperty("x-immutable-after-signoff").GetBoolean());
        Assert.Equal("object", schema.GetProperty("type").GetString());
        Assert.False(schema.GetProperty("additionalProperties").GetBoolean());
    }

    public static void AssertRequired(JsonElement schema, params string[] expected)
    {
        var actual = schema.GetProperty("required")
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(expected.Order(StringComparer.Ordinal), actual);
    }

    public static void AssertUtcTimestamp(JsonElement property)
    {
        Assert.Equal("string", property.GetProperty("type").GetString());
        Assert.Equal("date-time", property.GetProperty("format").GetString());
    }

    public static void AssertNoSensitivePropertyNames(JsonElement schema)
    {
        string[] forbidden =
        [
            "password",
            "pin",
            "credential",
            "authorization",
            "cookie",
            "token",
            "connectionstring",
            "sqltext",
            "requestbody",
            "responsebody",
            "universityid",
            "studentid",
            "email",
            "phone"
        ];

        foreach (var propertyName in PropertyNames(schema))
        {
            var normalized = propertyName
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .Replace("_", string.Empty, StringComparison.Ordinal)
                .ToLowerInvariant();
            Assert.DoesNotContain(
                forbidden,
                candidate => normalized.Contains(candidate, StringComparison.Ordinal));
        }
    }

    private static IEnumerable<string> PropertyNames(JsonElement element)
    {
        if (element.ValueKind is JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals("properties"))
                {
                    foreach (var contractProperty in property.Value.EnumerateObject())
                    {
                        yield return contractProperty.Name;
                    }
                }

                foreach (var child in PropertyNames(property.Value))
                {
                    yield return child;
                }
            }
        }
        else if (element.ValueKind is JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                foreach (var child in PropertyNames(item))
                {
                    yield return child;
                }
            }
        }
    }
}
