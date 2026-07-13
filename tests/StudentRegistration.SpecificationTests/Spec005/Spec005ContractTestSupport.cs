using System.Text.Json;
using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Spec005;

internal static class Spec005ContractTestSupport
{
    private const string ErdPath = "docs/diagrams/ERD.md";
    private const string WorkstreamManifestPath = ".specify/workstream-manifest.json";

    public static string ReadBoundedContract(string path)
    {
        var contract = RepositoryFiles.Read(path);

        AssertContainsNormalized(contract, "Bounded delivery: design-time contract only");
        AssertContainsNormalized(contract, "Runtime source dependency: None");
        AssertContainsNormalized(
            contract,
            "Fail-closed boundary: unapproved production behavior remains blocked");

        return contract;
    }

    public static void AssertWorkstream(
        string name,
        string deliveryPath,
        string testPath,
        params string[] requirements)
    {
        using var manifest = JsonDocument.Parse(RepositoryFiles.Read(WorkstreamManifestPath));
        var workstream = manifest.RootElement
            .GetProperty("specs")
            .GetProperty("005")
            .EnumerateArray()
            .Single(item => item.GetProperty("name").GetString() == name);

        Assert.Equal(deliveryPath, workstream.GetProperty("deliveryPath").GetString());
        Assert.Equal(testPath, workstream.GetProperty("testPath").GetString());
        Assert.Equal(
            requirements.Order(StringComparer.Ordinal).ToArray(),
            workstream.GetProperty("requirements")
                .EnumerateArray()
                .Select(item => item.GetString()!)
                .Order(StringComparer.Ordinal)
                .ToArray());
    }

    public static void AssertMirrorsErd(string contract, params string[] rules)
    {
        var erd = RepositoryFiles.Read(ErdPath);
        foreach (var rule in rules)
        {
            AssertContainsNormalized(erd, rule);
            AssertContainsNormalized(contract, rule);
        }
    }

    public static void AssertContainsNormalized(string actual, params string[] expectedValues)
    {
        var normalizedActual = Normalize(actual);
        foreach (var expected in expectedValues)
        {
            Assert.Contains(Normalize(expected), normalizedActual, StringComparison.Ordinal);
        }
    }

    private static string Normalize(string value) =>
        Regex.Replace(
                value.Replace("`", string.Empty, StringComparison.Ordinal)
                    .Replace("*", string.Empty, StringComparison.Ordinal)
                    .Replace("\"", string.Empty, StringComparison.Ordinal),
                @"\s+",
                " ")
            .Trim();
}
