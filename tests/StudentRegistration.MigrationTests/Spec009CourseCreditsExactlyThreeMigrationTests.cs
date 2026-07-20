using StudentRegistration.TestSupport;

namespace StudentRegistration.MigrationTests;

public sealed class Spec009CourseCreditsExactlyThreeMigrationTests
{
    [Fact]
    public void Migration_replaces_the_legacy_positive_credit_constraint_with_exactly_three()
    {
        var migration = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Migrations/" +
            "20260720182228_Spec009CourseCreditsExactlyThree.cs");

        RepositoryFiles.ContainsAll(
            migration,
            "CK_Courses_Credits",
            "[Credits] = 3");
        Assert.Contains("[Credits] > 0", migration, StringComparison.Ordinal);
    }
}
