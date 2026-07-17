using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec017;

internal static class Spec017ContractAssertions
{
    private const string ContractPath =
        "specs/017-admin-operations-audit-reporting/contracts/api.md";

    private static readonly string Contract = Normalize(
        RepositoryFiles.Read(ContractPath));

    public static string Interface(string name)
    {
        var match = Regex.Match(
            Contract,
            $@"interface\s+{Regex.Escape(name)}\s*\{{(?<body>.*?)\}}",
            RegexOptions.CultureInvariant);
        Assert.True(match.Success, $"Missing finalized interface {name} in {ContractPath}.");
        return match.Groups["body"].Value;
    }

    public static string Section(string start, string end)
    {
        var startIndex = Contract.IndexOf(start, StringComparison.Ordinal);
        Assert.True(startIndex >= 0, $"Missing section {start} in {ContractPath}.");
        var endIndex = Contract.IndexOf(end, startIndex + start.Length, StringComparison.Ordinal);
        Assert.True(endIndex > startIndex, $"Missing section boundary {end} in {ContractPath}.");
        return Contract[startIndex..endIndex];
    }

    public static void ContainsAll(string value, params string[] fragments)
    {
        foreach (var fragment in fragments)
        {
            Assert.Contains(fragment, value, StringComparison.Ordinal);
        }
    }

    public static void Excludes(string value, params string[] fragments)
    {
        foreach (var fragment in fragments)
        {
            Assert.DoesNotContain(fragment, value, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string Normalize(string value) => Regex.Replace(
        value,
        @"\s+",
        " ").Trim();
}
