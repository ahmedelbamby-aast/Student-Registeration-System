using System.Reflection;
using StudentRegistration.Academics.Domain;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec008;

public sealed class StudentTermAcademicStateModelTests
{
    private static readonly DateTime DataAsOfUtc =
        new(2026, 7, 13, 23, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Student_term_state_constructor_and_properties_match_the_approved_erd_fields()
    {
        var id = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var termId = Guid.NewGuid();

        var state = new StudentTermAcademicState(
            id,
            studentId,
            termId,
            3.25m,
            72m,
            "Active",
            "synthetic-demo",
            "student-term-fixture-v1:42:2026-1",
            "academic-profile-v1",
            DataAsOfUtc);

        Assert.Equal(id, state.Id);
        Assert.Equal(studentId, state.StudentId);
        Assert.Equal(termId, state.TermId);
        Assert.Equal(3.25m, state.GpaAtStart);
        Assert.Equal(72m, state.EarnedCreditsAtStart);
        Assert.Equal("Active", state.StandingAtStart);
        Assert.Equal("synthetic-demo", state.Source);
        Assert.Equal("student-term-fixture-v1:42:2026-1", state.SourceReference);
        Assert.Equal("academic-profile-v1", state.DataVersion);
        Assert.Equal(DataAsOfUtc, state.DataAsOfUtc);
        Assert.Empty(state.Version);

        Assert.Equal(
            [
                "id",
                "studentId",
                "termId",
                "gpaAtStart",
                "earnedCreditsAtStart",
                "standingAtStart",
                "source",
                "sourceReference",
                "dataVersion",
                "dataAsOfUtc",
            ],
            GetSolePublicConstructorParameterNames<StudentTermAcademicState>());
    }

    [Fact]
    public void Student_term_state_requires_nonempty_student_and_term_links()
    {
        Assert.Throws<ArgumentException>(() => CreateState(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => CreateState(studentId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => CreateState(termId: Guid.Empty));
    }

    [Theory]
    [InlineData("standingAtStart")]
    [InlineData("source")]
    [InlineData("sourceReference")]
    [InlineData("dataVersion")]
    public void Student_term_state_requires_complete_snapshot_and_provenance_values(string field)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => CreateState(
                standingAtStart: field == "standingAtStart" ? " " : "Active",
                source: field == "source" ? " " : "synthetic-demo",
                sourceReference: field == "sourceReference" ? " " : "student-term-42",
                dataVersion: field == "dataVersion" ? " " : "academic-profile-v1"));

        Assert.Equal(field, exception.ParamName);
    }

    [Theory]
    [InlineData(-0.01, 0)]
    [InlineData(0, -0.01)]
    public void Student_term_state_rejects_negative_snapshot_values(
        decimal gpaAtStart,
        decimal earnedCreditsAtStart)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateState(
                gpaAtStart: gpaAtStart,
                earnedCreditsAtStart: earnedCreditsAtStart));
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void Student_term_state_requires_a_utc_snapshot_instant(DateTimeKind kind)
    {
        Assert.Throws<ArgumentException>(
            () => CreateState(dataAsOfUtc: DateTime.SpecifyKind(DataAsOfUtc, kind)));
    }

    [Fact]
    public void Student_term_state_is_an_academics_boundary_without_registration_dependency()
    {
        var academicsProject = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/StudentRegistration.Academics.csproj");
        var publicProperties = typeof(StudentTermAcademicState)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        Assert.DoesNotContain(
            "StudentRegistration.Registration",
            academicsProject,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            publicProperties,
            property => property.PropertyType.Assembly.GetName().Name ==
                "StudentRegistration.Registration");
    }

    private static StudentTermAcademicState CreateState(
        Guid? id = null,
        Guid? studentId = null,
        Guid? termId = null,
        decimal gpaAtStart = 3.25m,
        decimal earnedCreditsAtStart = 72m,
        string standingAtStart = "Active",
        string source = "synthetic-demo",
        string sourceReference = "student-term-42",
        string dataVersion = "academic-profile-v1",
        DateTime? dataAsOfUtc = null) =>
        new(
            id ?? Guid.NewGuid(),
            studentId ?? Guid.NewGuid(),
            termId ?? Guid.NewGuid(),
            gpaAtStart,
            earnedCreditsAtStart,
            standingAtStart,
            source,
            sourceReference,
            dataVersion,
            dataAsOfUtc ?? DataAsOfUtc);

    private static string[] GetSolePublicConstructorParameterNames<T>()
    {
        var constructor = Assert.Single(typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        return constructor.GetParameters().Select(parameter => parameter.Name!).ToArray();
    }
}
