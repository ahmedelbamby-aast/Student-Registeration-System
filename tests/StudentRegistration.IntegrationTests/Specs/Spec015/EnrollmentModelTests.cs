using StudentRegistration.Registration.Domain;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec015;

public sealed class EnrollmentModelTests
{
    [Fact]
    public void Spec015_consumes_the_single_canonical_spec014_enrollment()
    {
        Assert.True(RepositoryFiles.Exists(
            "src/StudentRegistration.Registration/Domain/Enrollment.cs"));
        Assert.Equal(
            "StudentRegistration.Registration.Domain",
            typeof(Enrollment).Namespace);
        Assert.Single(
            typeof(Enrollment).Assembly.GetTypes(),
            type => type.Name == nameof(Enrollment));

        using var context = ModelContext.Create("Spec015EnrollmentModelOnly");
        Assert.Single(
            context.Model.GetEntityTypes(),
            entity => entity.ClrType == typeof(Enrollment));
    }
}
