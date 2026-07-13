using System.Text.Json;
using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec005.Entities;

internal sealed record ErdEntityExpectation(
    string EntityName,
    string ErdEntityName,
    string CanonicalOwnerSpec,
    string CanonicalSourcePath,
    string[] Fields,
    string[] Invariants);

internal static class ErdReferenceContractAssertions
{
    private const string ErdPath = "docs/diagrams/ERD.md";
    private const string OwnershipPath = ".specify/entity-ownership.json";
    private const string PersistencePath = ".specify/persistence-manifest.json";

    public static void AssertReference(
        string referencePath,
        ErdEntityExpectation expectation) =>
        AssertReferences(referencePath, [expectation]);

    public static void AssertReferences(
        string referencePath,
        params ErdEntityExpectation[] expectations)
    {
        var reference = RepositoryFiles.Read(referencePath);
        var erd = RepositoryFiles.Read(ErdPath);
        using var ownership = JsonDocument.Parse(RepositoryFiles.Read(OwnershipPath));
        using var persistence = JsonDocument.Parse(RepositoryFiles.Read(PersistencePath));
        var canonicalOwners = ownership.RootElement.GetProperty("canonicalOwners");
        var contributions = persistence.RootElement.GetProperty("contributions");

        AssertContainsNormalized(reference, "Runtime source dependency: None");

        foreach (var expectation in expectations)
        {
            Assert.StartsWith("src/", expectation.CanonicalSourcePath, StringComparison.Ordinal);
            var ownerId = expectation.CanonicalOwnerSpec["SPEC-".Length..];
            Assert.Equal(
                ownerId,
                canonicalOwners.GetProperty(expectation.EntityName).GetString());

            var contribution = contributions.GetProperty(ownerId);
            var contributionEntities = contribution.GetProperty("entities")
                .EnumerateArray()
                .Select(entity => entity.GetString())
                .ToArray();
            Assert.Contains(expectation.EntityName, contributionEntities);
            AssertContainsNormalized(
                reference,
                $"EF contribution: {contribution.GetProperty("path").GetString()} " +
                $"({contribution.GetProperty("mode").GetString()})");

            var referenceSection = ExtractReferenceSection(reference, expectation.EntityName);
            AssertContainsNormalized(
                referenceSection,
                $"Canonical entity: {expectation.EntityName}");
            AssertContainsNormalized(
                referenceSection,
                $"Canonical owner: {expectation.CanonicalOwnerSpec}");
            AssertContainsNormalized(
                referenceSection,
                $"Canonical source path: {expectation.CanonicalSourcePath}");

            var erdEntity = ExtractErdEntity(erd, expectation.ErdEntityName);
            var expectedFields = expectation.Fields.Select(Normalize).ToArray();
            Assert.Equal(expectedFields, ExtractErdFields(erdEntity));
            Assert.Equal(expectedFields, ExtractReferenceFields(referenceSection));

            foreach (var invariant in expectation.Invariants)
            {
                AssertContainsNormalized(erd, invariant);
                AssertContainsNormalized(referenceSection, invariant);
            }
        }
    }

    private static string ExtractReferenceSection(string reference, string entityName)
    {
        var match = Regex.Match(
            reference,
            $@"(?ms)^###\s+{Regex.Escape(entityName)}\s*\r?\n(?<body>.*?)(?=^###\s+|\z)");
        Assert.True(match.Success, $"ERD reference section was not found: {entityName}");
        return match.Groups["body"].Value;
    }

    private static string[] ExtractReferenceFields(string referenceSection)
    {
        var match = Regex.Match(
            referenceSection,
            @"(?ms)^####\s+Fields\s*\r?\n(?<body>.*?)(?=^####\s+|\z)");
        Assert.True(match.Success, "ERD reference Fields section was not found.");

        return Regex.Matches(match.Groups["body"].Value, @"(?m)^\s*-\s+(?<field>.+?)\s*$")
            .Select(field => Normalize(field.Groups["field"].Value))
            .ToArray();
    }

    private static string[] ExtractErdFields(string erdEntity) =>
        Regex.Matches(erdEntity, @"(?m)^\s*(?<field>\S.*?)\s*$")
            .Select(field => Normalize(field.Groups["field"].Value))
            .ToArray();

    private static string ExtractErdEntity(string erd, string erdEntityName)
    {
        var match = Regex.Match(
            erd,
            $@"(?ms)^\s{{2}}{Regex.Escape(erdEntityName)}\s+\{{\r?\n(?<body>.*?)^\s{{2}}\}}\s*$");
        Assert.True(match.Success, $"ERD entity block was not found: {erdEntityName}");
        return match.Groups["body"].Value;
    }

    private static void AssertContainsNormalized(string actual, string expected) =>
        Assert.Contains(Normalize(expected), Normalize(actual), StringComparison.Ordinal);

    private static string Normalize(string value) =>
        Regex.Replace(value.Replace("`", string.Empty, StringComparison.Ordinal)
                .Replace("*", string.Empty, StringComparison.Ordinal)
                .Replace("\"", string.Empty, StringComparison.Ordinal), @"\s+", " ")
            .Trim();
}
