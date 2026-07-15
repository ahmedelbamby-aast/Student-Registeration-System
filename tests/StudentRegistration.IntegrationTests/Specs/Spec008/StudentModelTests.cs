using System.Reflection;
using StudentRegistration.Academics.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec008;

public sealed class StudentModelTests
{
    private static readonly DateTime DataAsOfUtc =
        new(2026, 7, 13, 23, 0, 0, DateTimeKind.Utc);

    private static readonly DateTime ImportedAtUtc =
        new(2026, 7, 14, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Student_constructor_and_properties_match_the_approved_erd_fields()
    {
        var id = Guid.NewGuid();
        var applicationUserId = Guid.NewGuid();

        var student = new Student(
            id,
            applicationUserId,
            "AI",
            "2026",
            3.25m,
            72m,
            "Active",
            false,
            "synthetic-demo",
            "student-fixture-v1:42",
            "academic-profile-v1",
            DataAsOfUtc,
            ImportedAtUtc);

        Assert.Equal(id, student.Id);
        Assert.Equal(applicationUserId, student.ApplicationUserId);
        Assert.Equal("AI", student.ProgramCode);
        Assert.Equal("2026", student.Cohort);
        Assert.Equal(3.25m, student.CurrentGpa);
        Assert.Equal(72m, student.EarnedCredits);
        Assert.Equal("Active", student.Standing);
        Assert.False(student.IsActive);
        Assert.Equal("synthetic-demo", student.Source);
        Assert.Equal("student-fixture-v1:42", student.SourceReference);
        Assert.Equal("academic-profile-v1", student.DataVersion);
        Assert.Equal(DataAsOfUtc, student.DataAsOfUtc);
        Assert.Equal(ImportedAtUtc, student.ImportedAtUtc);
        Assert.Empty(student.Version);

        Assert.Equal(
            [
                "id",
                "applicationUserId",
                "programCode",
                "cohort",
                "currentGpa",
                "earnedCredits",
                "standing",
                "isActive",
                "source",
                "sourceReference",
                "dataVersion",
                "dataAsOfUtc",
                "importedAtUtc",
            ],
            GetSolePublicConstructorParameterNames<Student>());
    }

    [Fact]
    public void Student_requires_nonempty_identity_links()
    {
        Assert.Throws<ArgumentException>(() => CreateStudent(id: Guid.Empty));
        Assert.Throws<ArgumentException>(
            () => CreateStudent(applicationUserId: Guid.Empty));
    }

    [Theory]
    [InlineData("programCode")]
    [InlineData("cohort")]
    [InlineData("standing")]
    [InlineData("source")]
    [InlineData("sourceReference")]
    [InlineData("dataVersion")]
    public void Student_requires_complete_academic_and_provenance_values(string field)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => CreateStudent(
                programCode: field == "programCode" ? " " : "AI",
                cohort: field == "cohort" ? " " : "2026",
                standing: field == "standing" ? " " : "Active",
                source: field == "source" ? " " : "synthetic-demo",
                sourceReference: field == "sourceReference" ? " " : "student-42",
                dataVersion: field == "dataVersion" ? " " : "academic-profile-v1"));

        Assert.Equal(field, exception.ParamName);
    }

    [Theory]
    [InlineData(-0.01, 0)]
    [InlineData(0, -0.01)]
    public void Student_rejects_negative_gpa_or_earned_credits(
        decimal currentGpa,
        decimal earnedCredits)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateStudent(
                currentGpa: currentGpa,
                earnedCredits: earnedCredits));
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void Student_requires_utc_profile_instants(DateTimeKind kind)
    {
        var nonUtcAsOf = DateTime.SpecifyKind(DataAsOfUtc, kind);
        var nonUtcImported = DateTime.SpecifyKind(ImportedAtUtc, kind);

        Assert.Throws<ArgumentException>(
            () => CreateStudent(dataAsOfUtc: nonUtcAsOf));
        Assert.Throws<ArgumentException>(
            () => CreateStudent(importedAtUtc: nonUtcImported));
    }

    [Fact]
    public void Student_keeps_identity_credentials_and_fixture_mechanics_out_of_the_model()
    {
        var publicProperties = typeof(Student).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        Assert.DoesNotContain(publicProperties, property => property.Name == "UniversityId");
        Assert.DoesNotContain(publicProperties, property => property.Name == "FixtureOrdinal");
    }

    private static Student CreateStudent(
        Guid? id = null,
        Guid? applicationUserId = null,
        string programCode = "AI",
        string cohort = "2026",
        decimal currentGpa = 3.25m,
        decimal earnedCredits = 72m,
        string standing = "Active",
        bool isActive = true,
        string source = "synthetic-demo",
        string sourceReference = "student-42",
        string dataVersion = "academic-profile-v1",
        DateTime? dataAsOfUtc = null,
        DateTime? importedAtUtc = null) =>
        new(
            id ?? Guid.NewGuid(),
            applicationUserId ?? Guid.NewGuid(),
            programCode,
            cohort,
            currentGpa,
            earnedCredits,
            standing,
            isActive,
            source,
            sourceReference,
            dataVersion,
            dataAsOfUtc ?? DataAsOfUtc,
            importedAtUtc ?? ImportedAtUtc);

    private static string[] GetSolePublicConstructorParameterNames<T>()
    {
        var constructor = Assert.Single(typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        return constructor.GetParameters().Select(parameter => parameter.Name!).ToArray();
    }
}
