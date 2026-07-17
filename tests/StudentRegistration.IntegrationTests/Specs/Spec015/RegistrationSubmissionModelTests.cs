using Microsoft.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Registration.Domain;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec015;

public sealed class RegistrationSubmissionModelTests
{
    [Fact]
    public void Spec015_consumes_the_single_canonical_spec014_submission()
    {
        Assert.True(RepositoryFiles.Exists(
            "src/StudentRegistration.Registration/Domain/RegistrationSubmission.cs"));
        Assert.Equal(
            "StudentRegistration.Registration.Domain",
            typeof(RegistrationSubmission).Namespace);
        Assert.Single(
            typeof(RegistrationSubmission).Assembly.GetTypes(),
            type => type.Name == nameof(RegistrationSubmission));

        using var context = ModelContext.Create("Spec015SubmissionModelOnly");
        Assert.Single(
            context.Model.GetEntityTypes(),
            entity => entity.ClrType == typeof(RegistrationSubmission));
    }
}

internal static class ModelContext
{
    public static StudentRegistrationDbContext Create(string databaseName)
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<
                StudentRegistrationDbContext>()
            .UseSqlServer(
                $"Server=localhost;Database={databaseName};User Id=sa;Password=NotUsed!42;TrustServerCertificate=True")
            .Options;
        return new StudentRegistrationDbContext(options);
    }
}
