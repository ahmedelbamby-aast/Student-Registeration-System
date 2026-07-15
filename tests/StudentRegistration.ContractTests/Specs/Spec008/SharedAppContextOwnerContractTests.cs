using System.Text.Json;

namespace StudentRegistration.ContractTests.Specs.Spec008;

public sealed class SharedAppContextOwnerContractTests
{
    [Fact]
    public void Spec006_remains_the_only_source_owner_of_shared_context_contracts()
    {
        using var ownership = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/entity-ownership.json"));
        var overrides = ownership.RootElement.GetProperty("artifactOverrides");

        Assert.Equal(
            "src/StudentRegistration.Contracts/AppContextDto.cs",
            overrides.GetProperty("006:AppContext").GetString());
        Assert.Equal(
            "src/StudentRegistration.Contracts/RegistrationWindowSummaryDto.cs",
            overrides.GetProperty("006:RegistrationWindowSummaryDto").GetString());

        Assert.Equal(
            "src/StudentRegistration.Contracts/AppContextDto.cs",
            Assert.Single(SourceDefinitions("public sealed record AppContextDto")));
        Assert.Equal(
            "src/StudentRegistration.Contracts/RegistrationWindowSummaryDto.cs",
            Assert.Single(SourceDefinitions(
                "public sealed record RegistrationWindowSummaryDto")));
    }

    [Fact]
    public void Shared_authenticated_context_contains_identity_and_academic_contributions_once()
    {
        var context = Spec008ContractAssertions.SharedContractType("AppContextDto");
        var window = Spec008ContractAssertions.SharedContractType(
            "RegistrationWindowSummaryDto");

        Spec008ContractAssertions.HasExactProperties(
            context,
            "ServerTimeUtc",
            "TimeZoneId",
            "TeachingTerm",
            "RegistrationTerm",
            "RegistrationWindowState",
            "RegistrationWindow",
            "ServiceState",
            "DisplayName",
            "AuthorizedRoles",
            "ActiveRole",
            "SessionState",
            "ExpiresAtUtc",
            "SupportReferencePath");
        Spec008ContractAssertions.HasExactProperties(
            window,
            "Id",
            "State",
            "OpensAtUtc",
            "ClosesAtUtc",
            "RowVersion");
    }

    [Fact]
    public void Spec008_academic_dtos_consume_and_do_not_redeclare_shared_context_types()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.Contracts/Academics/AcademicContextContracts.cs");

        Assert.DoesNotContain("record AppContextDto", source, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "record RegistrationWindowSummaryDto",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain("record PublicContextDto", source, StringComparison.Ordinal);
    }

    private static IEnumerable<string> SourceDefinitions(string declaration)
    {
        var sourceRoot = RepositoryFiles.PathTo("src");
        foreach (var path in Directory.EnumerateFiles(
            sourceRoot,
            "*.cs",
            SearchOption.AllDirectories))
        {
            if (path.Contains(
                $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                StringComparison.OrdinalIgnoreCase) ||
                path.Contains(
                    $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (File.ReadAllText(path).Contains(declaration, StringComparison.Ordinal))
            {
                yield return Path.GetRelativePath(RepositoryFiles.Root, path)
                    .Replace(Path.DirectorySeparatorChar, '/');
            }
        }
    }
}
