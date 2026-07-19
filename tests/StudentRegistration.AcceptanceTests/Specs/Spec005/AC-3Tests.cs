using StudentRegistration.TestSupport;
using Microsoft.EntityFrameworkCore;

namespace StudentRegistration.AcceptanceTests.Specs.Spec005;

[Collection(Spec005SqlAcceptanceCollection.Name)]
public sealed class AC_3Tests(Spec005SqlAcceptanceDatabase database)
{
    [Fact]
    public void Superseding_policy_preserves_the_stored_historical_decision_snapshot()
    {
        var policy = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/Domain/PolicySet.cs");
        var submission = RepositoryFiles.Read(
            "src/StudentRegistration.Registration/Domain/RegistrationSubmission.cs");
        var mapping = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/RegistrationModelConfiguration.cs");

        RepositoryFiles.ContainsAll(
            policy,
            "Published",
            "Superseded",
            "Only a published policy set can be superseded");
        RepositoryFiles.ContainsAll(
            submission,
            "DecisionSnapshotJson",
            "CompleteAccepted",
            "CompleteRejected");
        RepositoryFiles.ContainsAll(
            mapping,
            "DecisionSnapshotJson",
            "nvarchar(max)",
            "ProcessingState");
        Assert.DoesNotContain(
            "public set;",
            submission,
            StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Real_sql_supersession_does_not_rewrite_the_historical_submission_snapshot()
    {
        var graph = await database.SeedAsync(1, 0, 1);
        var policy1 = Guid.NewGuid();
        var policy2 = Guid.NewGuid();
        await using var context = database.CreateContext();
        await context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT [academics].[PolicySets]
              ([Id],[VersionCode],[TermId],[ProgramId],[ScopeCode],[EffectiveFromUtc],[EffectiveToUtc],[State])
            VALUES ({policy1},'2026.1',{graph.TermId},NULL,'AI-DS','2026-07-01',NULL,'published');
            UPDATE [academics].[PolicySets] SET [State]='superseded',[EffectiveToUtc]='2026-08-01' WHERE [Id]={policy1};
            INSERT [academics].[PolicySets]
              ([Id],[VersionCode],[TermId],[ProgramId],[ScopeCode],[EffectiveFromUtc],[EffectiveToUtc],[State])
            VALUES ({policy2},'2026.2',{graph.TermId},NULL,'AI-DS','2026-08-01',NULL,'published');
            """);
        var snapshot = await context.Database.SqlQuery<string>($"""
            SELECT [DecisionSnapshotJson] AS [Value]
            FROM [registration].[RegistrationSubmissions]
            WHERE [Id] = {graph.SubmissionIds[0]}
            """).SingleAsync();
        Assert.Contains("2026.1", snapshot, StringComparison.Ordinal);
        Assert.Equal(
            "superseded",
            await context.Database.SqlQuery<string>($"""
                SELECT [State] AS [Value] FROM [academics].[PolicySets] WHERE [Id]={policy1}
                """).SingleAsync());
        Assert.Equal(
            "published",
            await context.Database.SqlQuery<string>($"""
                SELECT [State] AS [Value] FROM [academics].[PolicySets] WHERE [Id]={policy2}
                """).SingleAsync());
    }
}
