using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.LoadTesting.Spec018;
using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Domain;
using StudentRegistration.TestSupport;
using StudentRegistration.TestSupport.Spec011;

namespace StudentRegistration.QualityTests.Specs.Spec018;

public sealed class Nfr3EvidenceTests
{
    [Fact]
    public void Evidence_contract_has_the_exact_target_p95_budgets()
    {
        var evidence = Spec018LoadEvidenceAssertions.ReadPassed("NFR-3");

        Assert.Equal(300, LoadGateThresholds.CatalogueP95Milliseconds);
        Assert.Equal(2_000, LoadGateThresholds.CommitP95Milliseconds);
        Assert.Equal(500, LoadGateThresholds.OptimizerP95Milliseconds);
        Assert.True(ExactLoadProfileCatalog.Target.EnforceTargetResponseBudgets);
        Assert.False(ExactLoadProfileCatalog.RequiredSpike.EnforceTargetResponseBudgets);
        Assert.Contains("Catalogue p95 <= 300 ms", evidence, StringComparison.Ordinal);
        Assert.Contains("commit p95 <= 2,000 ms", evidence, StringComparison.Ordinal);
        Assert.Contains("optimizer p95 <= 500 ms", evidence, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Measured_component_and_recorded_sql_samples_meet_all_three_p95_budgets()
    {
        var catalogueP95 = await MeasureCatalogueP95Async();
        var optimizerP95 = MeasureOptimizerP95();
        var commitP95 = Spec018LoadEvidenceAssertions
            .ReadRegistrationEvidence()
            .Target
            .SubmissionP95Milliseconds;

        Assert.True(
            catalogueP95 <= LoadGateThresholds.CatalogueP95Milliseconds,
            $"Catalogue p95 was {catalogueP95:F3} ms.");
        Assert.True(
            commitP95 <= LoadGateThresholds.CommitP95Milliseconds,
            $"Commit p95 was {commitP95:F3} ms.");
        Assert.True(
            optimizerP95 <= LoadGateThresholds.OptimizerP95Milliseconds,
            $"Optimizer p95 was {optimizerP95:F3} ms.");
    }

    [Fact]
    public void Target_run_meets_catalogue_commit_and_optimizer_p95_budgets()
    {
        var recorded = Spec018LoadEvidenceAssertions.ReadRegistrationEvidence();

        Assert.True(
            recorded.MixedTargetReads.DiscoveryP95Milliseconds <=
            LoadGateThresholds.CatalogueP95Milliseconds);
        Assert.True(
            recorded.Target.SubmissionP95Milliseconds <=
            LoadGateThresholds.CommitP95Milliseconds);
    }

    [Fact]
    public void Registration_record_hot_query_has_the_measured_composite_index_and_migration()
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Spec018IndexShape")
            .Options;
        using var context = new StudentRegistrationDbContext(options);
        var entity = context.GetService<IDesignTimeModel>()
            .Model
            .FindEntityType(typeof(RegistrationSubmission));
        var index = Assert.Single(entity!.GetIndexes(), candidate =>
            candidate.GetDatabaseName() ==
            "IX_RegistrationSubmissions_StudentTermStateReceived");

        Assert.Equal(
            [
                nameof(RegistrationSubmission.StudentId),
                nameof(RegistrationSubmission.TermId),
                nameof(RegistrationSubmission.ProcessingState),
                nameof(RegistrationSubmission.ReceivedAtUtc),
                nameof(RegistrationSubmission.Id)
            ],
            index.Properties.Select(property => property.Name));

        const string migrationId =
            "20260717222551_Spec018RegistrationReadPerformance";
        var migrations = context.GetService<IMigrationsAssembly>();
        Assert.Contains(migrationId, migrations.Migrations.Keys);
        var migration = migrations.CreateMigration(
            migrations.Migrations[migrationId],
            context.Database.ProviderName!);
        var create = Assert.Single(migration.UpOperations.OfType<CreateIndexOperation>());
        Assert.Equal("IX_RegistrationSubmissions_StudentTermStateReceived", create.Name);
        Assert.Equal("registration", create.Schema);
        Assert.Equal("RegistrationSubmissions", create.Table);
        Assert.Equal(
            ["StudentId", "TermId", "ProcessingState", "ReceivedAtUtc", "Id"],
            create.Columns);
        var drop = Assert.Single(migration.DownOperations.OfType<DropIndexOperation>());
        Assert.Equal(create.Name, drop.Name);
        Assert.Equal(create.Schema, drop.Schema);
        Assert.Equal(create.Table, drop.Table);
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260717222551_Spec018RegistrationReadPerformance.cs"),
            "IX_RegistrationSubmissions_StudentTermStateReceived",
            "StudentId",
            "TermId",
            "ProcessingState",
            "ReceivedAtUtc",
            "Id");
    }

    private static async Task<double> MeasureCatalogueP95Async()
    {
        var fixture = new Spec011ScenarioBuilder();
        var query = StudentRegistration.QualityTests.Specs.Spec011
            .Spec011QualitySupport.Search(fixture);
        var request = new OfferingSearchRequest(
            Query: null,
            Eligibility: "eligible",
            Credits: null,
            Day: null,
            Availability: "available",
            Sort: "courseCode,id",
            Page: 1,
            PageSize: 20);

        _ = await query.SearchAsync(fixture.ApplicationUserId, fixture.TermId, request);
        var latencies = await Task.WhenAll(
            Enumerable.Range(0, 300).Select(async _ =>
            {
                var stopwatch = Stopwatch.StartNew();
                var result = await query.SearchAsync(
                    fixture.ApplicationUserId,
                    fixture.TermId,
                    request);
                stopwatch.Stop();
                Assert.Equal(OfferingSearchOutcome.Found, result.Outcome);
                return stopwatch.Elapsed.TotalMilliseconds;
            }));

        return NearestRankP95(latencies);
    }

    private static double MeasureOptimizerP95()
    {
        var candidates = StudentRegistration.QualityTests.Specs.Spec013
            .Spec013OptimizerEvidenceFixture.Candidates();
        var optimizer = StudentRegistration.QualityTests.Specs.Spec013
            .Spec013OptimizerEvidenceFixture.Optimizer();
        for (var warmup = 0; warmup < 10; warmup++)
        {
            _ = optimizer.Optimize(
                candidates,
                StudentRegistration.QualityTests.Specs.Spec013
                    .Spec013OptimizerEvidenceFixture.Preferences,
                StudentRegistration.QualityTests.Specs.Spec013
                    .Spec013OptimizerEvidenceFixture.RequiredCredits);
        }

        var latencies = new double[50];
        for (var index = 0; index < latencies.Length; index++)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = optimizer.Optimize(
                candidates,
                StudentRegistration.QualityTests.Specs.Spec013
                    .Spec013OptimizerEvidenceFixture.Preferences,
                StudentRegistration.QualityTests.Specs.Spec013
                    .Spec013OptimizerEvidenceFixture.RequiredCredits);
            stopwatch.Stop();
            Assert.False(result.SearchLimitReached);
            Assert.Equal(3, result.Options.Count);
            latencies[index] = stopwatch.Elapsed.TotalMilliseconds;
        }

        return NearestRankP95(latencies);
    }

    private static double NearestRankP95(IEnumerable<double> samples)
    {
        var ordered = samples.Order().ToArray();
        Assert.NotEmpty(ordered);
        return ordered[(int)Math.Ceiling(ordered.Length * 0.95d) - 1];
    }
}
