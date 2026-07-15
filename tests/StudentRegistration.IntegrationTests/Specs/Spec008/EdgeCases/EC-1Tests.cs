using System.Reflection;

namespace StudentRegistration.IntegrationTests.Specs.Spec008.EdgeCases;

public sealed class EC_1Tests
{
    [Fact]
    public void Same_term_overlap_is_rejected_across_scopes_without_partial_write_or_audit()
    {
        RequireAcademicsType(
            "StudentRegistration.Academics.Application.RegistrationWindowService");

        var termId = Guid.Parse("00000000-0000-0000-0000-000000000100");
        var firstWindowId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var candidateWindowId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var existing = new WindowDouble(
            firstWindowId,
            termId,
            "program",
            OpensAtUtc: Utc(8),
            ClosesAtUtc: Utc(12),
            Lifecycle: "published",
            Version: 7);
        var candidate = new WindowDouble(
            candidateWindowId,
            termId,
            "all-students",
            OpensAtUtc: Utc(11),
            ClosesAtUtc: Utc(15),
            Lifecycle: "draft",
            Version: 3);
        var service = new WindowPublicationDouble(termId, [existing, candidate]);

        var result = service.Publish(candidateWindowId);

        Assert.NotEqual(existing.Scope, candidate.Scope);
        Assert.Equal("WINDOW_OVERLAP", result);
        Assert.Equal(
            [termId, candidateWindowId, firstWindowId],
            service.LockOrder);
        Assert.Equal("published", existing.Lifecycle);
        Assert.Equal(7, existing.Version);
        Assert.Equal("draft", candidate.Lifecycle);
        Assert.Equal(3, candidate.Version);
        Assert.Empty(service.AuditEntries);
    }

    private static DateTime Utc(int hour) =>
        new(2026, 7, 20, hour, 0, 0, DateTimeKind.Utc);

    private static void RequireAcademicsType(string fullName)
    {
        var type = Assembly.Load("StudentRegistration.Academics").GetType(fullName);
        Assert.True(type is not null, $"The required Academics service {fullName} is missing.");
    }

    private sealed class WindowDouble(
        Guid id,
        Guid termId,
        string scope,
        DateTime OpensAtUtc,
        DateTime ClosesAtUtc,
        string Lifecycle,
        int Version)
    {
        public Guid Id { get; } = id;
        public Guid TermId { get; } = termId;
        public string Scope { get; } = scope;
        public DateTime OpensAtUtc { get; } = OpensAtUtc;
        public DateTime ClosesAtUtc { get; } = ClosesAtUtc;
        public string Lifecycle { get; set; } = Lifecycle;
        public int Version { get; set; } = Version;
    }

    private sealed class WindowPublicationDouble(
        Guid termId,
        IReadOnlyList<WindowDouble> windows)
    {
        public List<Guid> LockOrder { get; } = [];
        public List<string> AuditEntries { get; } = [];

        public string Publish(Guid candidateId)
        {
            LockOrder.Add(termId);
            foreach (var window in windows
                .Where(window => window.TermId == termId)
                .OrderBy(window => window.Id))
            {
                LockOrder.Add(window.Id);
            }

            var candidate = Assert.Single(windows, window => window.Id == candidateId);
            var overlaps = windows.Any(window =>
                window.Id != candidate.Id &&
                window.TermId == candidate.TermId &&
                window.Lifecycle == "published" &&
                candidate.OpensAtUtc < window.ClosesAtUtc &&
                window.OpensAtUtc < candidate.ClosesAtUtc);
            if (overlaps)
            {
                return "WINDOW_OVERLAP";
            }

            candidate.Lifecycle = "published";
            candidate.Version++;
            AuditEntries.Add("registration-window-published");
            return "SUCCEEDED";
        }
    }
}
