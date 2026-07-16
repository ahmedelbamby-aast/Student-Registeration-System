using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Registration.Application;
using StudentRegistration.TestSupport;
using StudentRegistration.TestSupport.Spec011;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Infrastructure.SqlServer.Registration;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.QualityTests.Specs.Spec011;

public sealed class NFR_2EvidenceTests
{
    [Fact]
    public async Task Search_accepts_one_hundred_characters_and_rejects_one_hundred_one()
    {
        var acceptedText = new string('x', 100);
        var acceptedFixture = new Spec011ScenarioBuilder
        {
            CourseTitle = acceptedText
        };
        var accepted = await Spec011QualitySupport.Search(acceptedFixture)
            .SearchAsync(
                acceptedFixture.ApplicationUserId,
                acceptedFixture.TermId,
                new(acceptedText, "all", null, null, "all", null, 1, 20));

        Assert.Equal(OfferingSearchOutcome.Found, accepted.Outcome);
        Assert.Single(accepted.Page!.Items);

        var rejectedFixture = new Spec011ScenarioBuilder();
        var rejected = await Spec011QualitySupport.Search(rejectedFixture)
            .SearchAsync(
                rejectedFixture.ApplicationUserId,
                rejectedFixture.TermId,
                new(new string('x', 101), "all", null, null, "all", null, 1, 20));

        Assert.Equal(OfferingSearchOutcome.ValidationError, rejected.Outcome);
        Assert.Equal("VALIDATION_ERROR", rejected.ErrorCode);
        Assert.Null(rejected.Page);

        var oversizedPage = await Spec011QualitySupport.Search(rejectedFixture)
            .SearchAsync(
                rejectedFixture.ApplicationUserId,
                rejectedFixture.TermId,
                new(null, "all", null, null, "all", null, 1, 101));

        Assert.Equal(OfferingSearchOutcome.PageSizeInvalid, oversizedPage.Outcome);
        Assert.Equal("PAGE_SIZE_INVALID", oversizedPage.ErrorCode);
        Assert.Null(oversizedPage.Page);
    }

    [Fact]
    public async Task Sql_metacharacters_are_matched_as_literal_course_text()
    {
        const string maliciousText = "DS413%' OR 1=1 --";
        var fixture = new Spec011ScenarioBuilder
        {
            CourseTitle = $"Literal {maliciousText} marker"
        };

        var result = await Spec011QualitySupport.Search(fixture).SearchAsync(
            fixture.ApplicationUserId,
            fixture.TermId,
            new(maliciousText, "all", null, null, "all", null, 1, 20));

        Assert.Equal(OfferingSearchOutcome.Found, result.Outcome);
        var item = Assert.Single(result.Page!.Items);
        Assert.Contains(maliciousText, item.Title, StringComparison.Ordinal);
    }

    [Fact]
    public void Sql_adapter_scopes_roots_with_parameters_and_no_tracking()
    {
        using var context = new StudentRegistrationDbContext(
            new DbContextOptionsBuilder<StudentRegistrationDbContext>()
                .UseSqlServer(
                    "Server=localhost;Database=Spec011QualityModelOnly;User Id=sa;Password=NotUsed!42;TrustServerCertificate=True")
                .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll)
                .Options);
        var adapter = new RegistrationDiscoveryQueryAdapter(context);
        var termId = Guid.Parse("01100000-0000-0000-0000-000000000003");
        var applicationUserId =
            Guid.Parse("01100000-0000-0000-0000-000000000001");

        var offeringQuery = PrivateQuery<CourseOffering>(
            adapter,
            "OfferingsForTermQuery",
            termId);
        var studentQuery = PrivateQuery<StudentRegistration.Academics.Domain.Student>(
            adapter,
            "StudentForApplicationUserQuery",
            applicationUserId);
        var offeringSql = offeringQuery.ToQueryString();
        var studentSql = studentQuery.ToQueryString();

        Assert.Contains("DECLARE @termId", offeringSql, StringComparison.Ordinal);
        Assert.Contains("[c].[TermId] = @termId", offeringSql, StringComparison.Ordinal);
        Assert.Contains(
            "DECLARE @applicationUserId",
            studentSql,
            StringComparison.Ordinal);
        Assert.Contains(
            "[s].[ApplicationUserId] = @applicationUserId",
            studentSql,
            StringComparison.Ordinal);
        Assert.True(ContainsAsNoTracking(offeringQuery.Expression));
        Assert.True(ContainsAsNoTracking(studentQuery.Expression));

        var adapterSource = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationDiscoveryQueryAdapter.cs");
        RepositoryFiles.ContainsAll(
            adapterSource,
            ".Distinct()",
            ".Take(100)",
            "offeringIds.Contains(item.OfferingId)",
            "groupIds.Contains(item.GroupId)",
            "meetingIds.Contains(item.MeetingSlotId)");
    }

    [Fact]
    public void Evidence_records_the_complete_query_safety_gate()
    {
        var evidence = RepositoryFiles.Read(
            $"{Spec011QualitySupport.EvidenceDirectory}/SPEC-011-NFR-2.md");

        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-011 NFR-2 Query Safety Evidence",
            "NFR-2",
            "100 characters",
            "101 characters",
            "page size 101",
            "NFKC",
            "literal",
            "T043",
            "T044",
            "RegistrationDiscoveryQueryAdapter.cs",
            "AsNoTracking",
            "@termId",
            "@applicationUserId",
            "**Result: PASS.**");
        Assert.DoesNotMatch(
            @"(?i)\b(?:TODO|TBD|FIXME|PLACEHOLDER|PENDING|PARTIAL)\b",
            evidence);
    }

    private static IQueryable<T> PrivateQuery<T>(
        RegistrationDiscoveryQueryAdapter adapter,
        string methodName,
        Guid argument)
        where T : class
    {
        var method = typeof(RegistrationDiscoveryQueryAdapter).GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        return Assert.IsAssignableFrom<IQueryable<T>>(
            method!.Invoke(adapter, [argument]));
    }

    private static bool ContainsAsNoTracking(Expression expression)
    {
        if (expression is MethodCallExpression call &&
            call.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) &&
            call.Method.Name == nameof(EntityFrameworkQueryableExtensions.AsNoTracking))
        {
            return true;
        }

        return expression switch
        {
            MethodCallExpression nestedCall =>
                nestedCall.Arguments.Any(ContainsAsNoTracking),
            UnaryExpression unary => ContainsAsNoTracking(unary.Operand),
            _ => false,
        };
    }
}
