using System.Reflection;
using StudentRegistration.AcceptanceTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec008;

public sealed class AC_6Tests
{
    [Fact]
    public async Task Concurrent_overlapping_publications_have_exactly_one_winner()
    {
        var publisher = new WindowPublicationDouble();
        var termId = Guid.NewGuid();
        var start = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var first = PublishAfterStartAsync(
            publisher,
            start.Task,
            termId,
            new DateTime(2026, 8, 1, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 10, 18, 0, 0, DateTimeKind.Utc));
        var second = PublishAfterStartAsync(
            publisher,
            start.Task,
            termId,
            new DateTime(2026, 8, 5, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 12, 18, 0, 0, DateTimeKind.Utc));

        start.SetResult();
        var outcomes = await Task.WhenAll(first, second);

        Assert.Single(outcomes, outcome => outcome == PublicationOutcome.Published);
        Assert.Single(outcomes, outcome => outcome == PublicationOutcome.WindowOverlap);
        Assert.Equal(1, publisher.PublishedCount);
    }

    [Fact]
    public void Frozen_publication_contract_requires_stable_locking_and_one_commit()
    {
        var requirements = RepositoryFiles.Read(
            "specs/008-academic-term-student-profile/requirements.md");

        RepositoryFiles.ContainsAll(
            requirements,
            "AC-6: Concurrent window publication",
            "two draft windows overlap anywhere in the same term",
            "exactly one publication may commit",
            "409 STALE_VERSION or WINDOW_OVERLAP",
            "lock the AcademicTerm and then",
            "stable ID order",
            "no Published windows overlap anywhere in that");
    }

    [Fact]
    public void Production_publication_requires_the_window_service()
    {
        var service = Assembly.Load("StudentRegistration.Academics").GetType(
            "StudentRegistration.Academics.Application.RegistrationWindowService");

        Assert.True(
            service is not null,
            "RegistrationWindowService must own transactional publication before AC-6 can execute against production code.");
        Assert.Contains(
            service!.GetMethods(BindingFlags.Instance | BindingFlags.Public),
            method => method.Name.Contains("Publish", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Real_sql_publication_requires_the_academic_store_before_starting_docker()
    {
        Assert.Contains("mssql/server:2022", Spec008AcceptanceSqlServerFixture.SqlServerImage);

        var store = Assembly.Load("StudentRegistration.Infrastructure.SqlServer").GetType(
            "StudentRegistration.Infrastructure.SqlServer.Persistence.AcademicStore");

        Assert.True(
            store is not null,
            "AcademicStore is required for the real-SQL stable-lock publication proof; Docker is intentionally not started while that adapter is absent.");
    }

    private static async Task<PublicationOutcome> PublishAfterStartAsync(
        WindowPublicationDouble publisher,
        Task start,
        Guid termId,
        DateTime opensAtUtc,
        DateTime closesAtUtc)
    {
        await start;
        return await publisher.PublishAsync(termId, opensAtUtc, closesAtUtc);
    }

    private enum PublicationOutcome
    {
        Published,
        WindowOverlap
    }

    private sealed class WindowPublicationDouble
    {
        private readonly SemaphoreSlim _gate = new(1, 1);
        private readonly List<PublishedWindow> _published = [];

        public int PublishedCount => _published.Count;

        public async Task<PublicationOutcome> PublishAsync(
            Guid termId,
            DateTime opensAtUtc,
            DateTime closesAtUtc)
        {
            await _gate.WaitAsync();
            try
            {
                if (_published.Any(window =>
                    window.TermId == termId &&
                    opensAtUtc < window.ClosesAtUtc &&
                    window.OpensAtUtc < closesAtUtc))
                {
                    return PublicationOutcome.WindowOverlap;
                }

                _published.Add(new PublishedWindow(termId, opensAtUtc, closesAtUtc));
                return PublicationOutcome.Published;
            }
            finally
            {
                _gate.Release();
            }
        }
    }

    private sealed record PublishedWindow(
        Guid TermId,
        DateTime OpensAtUtc,
        DateTime ClosesAtUtc);
}
