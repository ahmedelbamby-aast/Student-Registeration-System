using StudentRegistration.TestSupport;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.AcceptanceTests.Specs.Spec005;

[Collection(Spec005SqlAcceptanceCollection.Name)]
public sealed class AC_1Tests(Spec005SqlAcceptanceDatabase database)
{
    [Fact]
    public void Existing_student_offering_enrollment_is_database_guarded_and_mapped_to_a_stable_conflict()
    {
        var mapping = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/RegistrationModelConfiguration.cs");
        var store = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Registration/SqlRegistrationEndpointStore.cs");
        var endpoint = RepositoryFiles.Read(
            "src/StudentRegistration.Registration/Endpoints/Spec014Endpoints.cs");
        var sqlProof = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Registration/SqlSeatAllocatorConcurrencyTests.cs");

        RepositoryFiles.ContainsAll(
            mapping,
            "enrollment.StudentId",
            "enrollment.OfferingId",
            ".IsUnique()");
        RepositoryFiles.ContainsAll(
            store,
            "IsDuplicateEnrollment",
            "DbUpdateException",
            "Conflict(");
        RepositoryFiles.ContainsAll(
            endpoint,
            "StatusCodes.Status409Conflict",
            "REGISTRATION_CONFLICT");
        RepositoryFiles.ContainsAll(
            sqlProof,
            "Spec014SqlWorkstreamDatabase",
            "MigrateAsync",
            "SqlSeatAllocator");
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Real_sql_rejects_the_second_active_student_offering_row()
    {
        var graph = await database.SeedAsync(1, 1, 2);
        var secondSubmission = Guid.NewGuid();
        await using var context = database.CreateContext();
        await context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT [registration].[RegistrationSubmissions]
              ([Id],[StudentId],[TermId],[ClientRequestId],[PayloadHash],[ProcessingState],[ResultCode],[Reference],[ReceiptSnapshotJson],[DecisionSnapshotJson],[ReceivedAtUtc],[UpdatedAtUtc],[CompletedAtUtc])
            VALUES ({secondSubmission},{graph.StudentIds[0]},{graph.TermId},{Guid.NewGuid()},'duplicate','rejected','POLICY_CHANGED',NULL,NULL,{"{}"},'2026-07-19','2026-07-19','2026-07-19');
            """);
        await Assert.ThrowsAsync<SqlException>(() =>
            context.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT [registration].[Enrollments]
                  ([Id],[StudentId],[OfferingId],[GroupId],[SubmissionId],[State],[RegisteredAtUtc])
                VALUES ({Guid.NewGuid()},{graph.StudentIds[0]},{graph.OfferingId},{graph.GroupId},{secondSubmission},'active','2026-07-19');
                """));
        context.ChangeTracker.Clear();
        Assert.Equal(
            1,
            await context.Set<Enrollment>().CountAsync(enrollment =>
                enrollment.StudentId == graph.StudentIds[0] &&
                enrollment.OfferingId == graph.OfferingId));
    }
}
