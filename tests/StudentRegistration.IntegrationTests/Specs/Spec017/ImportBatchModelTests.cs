using System.Text.Json;
using StudentRegistration.Academics.Domain;
using StudentRegistration.StaffAdministration.Domain;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec017;

public sealed class ImportBatchModelTests
{
    [Fact]
    public void Spec017_consumes_the_single_academics_owned_import_batch()
    {
        using var ownership = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/entity-ownership.json"));

        Assert.Equal(
            "009",
            ownership.RootElement
                .GetProperty("canonicalOwners")
                .GetProperty(nameof(ImportBatch))
                .GetString());
        Assert.Equal(
            "StudentRegistration.Academics",
            typeof(ImportBatch).Assembly.GetName().Name);
        Assert.Single(
            typeof(ImportBatch).Assembly.GetTypes(),
            type => type.Name == nameof(ImportBatch));
        Assert.Null(typeof(RosterRow).Assembly.GetType(
            "StudentRegistration.StaffAdministration.Domain.ImportBatch"));
        Assert.True(RepositoryFiles.Exists(
            "src/StudentRegistration.Academics/Domain/ImportBatch.cs"));
    }
}
