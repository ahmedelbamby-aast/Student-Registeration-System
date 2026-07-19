using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.Infrastructure.SqlServer.Registration;
using StudentRegistration.IntegrationTests.Registration;
using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Domain;
using StudentRegistration.Registration.Endpoints;

namespace StudentRegistration.IntegrationTests.Specs.Spec005.EdgeCases;

[Collection(Spec014SqlWorkstreamCollection.Name)]
public sealed class EC_3Tests(Spec014SqlWorkstreamDatabase database)
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Stale_rowversion_returns_409_with_current_version_and_does_not_lose_the_committed_update()
    {
        var seed = await database.SeedAsync(groupCapacities: [30]);
        byte[] expectedVersion;
        await using (var setup = database.CreateContext())
        {
            var plan = new RegistrationPlan(
                Guid.NewGuid(),
                seed.StudentId,
                seed.TermId,
                0m,
                RegistrationPlanState.Draft);
            setup.Add(plan);
            await setup.SaveChangesAsync();
            expectedVersion = plan.Version.ToArray();
        }

        var initialVersion = Convert.ToBase64String(expectedVersion);
        var winningValidation = Validation(seed, includeSelection: true);
        var losingValidation = Validation(seed, includeSelection: false);
        var winningCommand = new RegistrationPlanStoreCommand(
            seed.StudentId,
            seed.TermId,
            initialVersion,
            [new(
                Guid.NewGuid(),
                seed.OfferingId,
                seed.GroupIds[0],
                "offering/committed",
                "group/committed")],
            3m,
            RegistrationPlanState.Draft,
            [],
            winningValidation);
        var losingCommand = new RegistrationPlanStoreCommand(
            seed.StudentId,
            seed.TermId,
            initialVersion,
            [],
            0m,
            RegistrationPlanState.Draft,
            [],
            losingValidation);

        RegistrationPlanStoreResult winner;
        RegistrationPlanStoreResult stale;
        await using (var firstEditor = database.CreateContext())
        await using (var secondEditor = database.CreateContext())
        {
            winner = await new RegistrationPlanSqlServerAdapter(firstEditor)
                .ReplaceAsync(winningCommand);
            stale = await new RegistrationPlanSqlServerAdapter(secondEditor)
                .ReplaceAsync(losingCommand);
        }

        Assert.Equal(RegistrationPlanStoreOutcome.Updated, winner.Outcome);
        Assert.Equal(RegistrationPlanStoreOutcome.StaleVersion, stale.Outcome);
        Assert.NotNull(stale.Plan);
        var currentVersion = Convert.ToBase64String(stale.Plan.Version);
        Assert.NotEqual(initialVersion, currentVersion);
        Assert.Equal(seed.GroupIds[0], Assert.Single(stale.Plan.Items).SelectedGroupId);

        await using (var verification = database.CreateContext())
        {
            var persisted = await verification.Set<RegistrationPlan>()
                .AsNoTracking()
                .Include(plan => plan.Items)
                .SingleAsync(plan =>
                    plan.StudentId == seed.StudentId && plan.TermId == seed.TermId);
            Assert.Equal(currentVersion, Convert.ToBase64String(persisted.Version));
            Assert.Equal(seed.GroupIds[0], Assert.Single(persisted.Items).SelectedGroupId);
        }

        var view = new RegistrationPlanView(
            stale.Plan.Id,
            seed.TermId,
            currentVersion,
            [],
            stale.Plan.TotalCredits,
            RegistrationPlanService.DefaultTargetCredits,
            RegistrationPlanService.MaximumAllowedCredits,
            [],
            [],
            [],
            stale.Plan.Validation ?? losingValidation,
            stale.Plan.ReviewBlocked);
        var operation = new RegistrationPlanOperationResult(
            RegistrationPlanOperationOutcome.StaleVersion,
            view,
            "STALE_VERSION");
        var resultMethod = typeof(Spec012Endpoints).GetMethod(
            "Result",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("SPEC-012 result mapper is missing.");
        var http = new DefaultHttpContext();
        http.TraceIdentifier = "spec005-ec3";
        http.Response.Body = new MemoryStream();
        http.RequestServices = new ServiceCollection()
            .AddLogging()
            .ConfigureHttpJsonOptions(_ => { })
            .BuildServiceProvider();
        var httpResult = Assert.IsAssignableFrom<IResult>(
            resultMethod.Invoke(null, [http, operation]));

        await httpResult.ExecuteAsync(http);
        Assert.Equal(StatusCodes.Status409Conflict, http.Response.StatusCode);
        http.Response.Body.Position = 0;
        using var body = await JsonDocument.ParseAsync(http.Response.Body);
        Assert.Equal(
            "STALE_VERSION",
            body.RootElement.GetProperty("error").GetProperty("code").GetString());
        Assert.Equal(
            currentVersion,
            body.RootElement.GetProperty("error").GetProperty("currentVersion").GetString());
        Assert.Equal(
            currentVersion,
            body.RootElement.GetProperty("currentPlan").GetProperty("rowVersion").GetString());
    }

    private static ValidationSnapshot Validation(
        Spec014SqlSeed seed,
        bool includeSelection) =>
        new(
            new DateTime(2026, 7, 19, 0, 0, 0, DateTimeKind.Utc),
            "academic/spec005-ec3",
            "policy/spec005-ec3",
            "catalogue/spec005-ec3",
            includeSelection
                ? new Dictionary<Guid, string>
                {
                    [seed.OfferingId] = "offering/committed"
                }
                : new Dictionary<Guid, string>(),
            includeSelection
                ? new Dictionary<Guid, string>
                {
                    [seed.GroupIds[0]] = "group/committed"
                }
                : new Dictionary<Guid, string>());
}
