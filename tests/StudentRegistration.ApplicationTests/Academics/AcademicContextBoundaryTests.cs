using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Academics.Domain;
using StudentRegistration.ApplicationTests.Infrastructure;
using StudentRegistration.Contracts;

namespace StudentRegistration.ApplicationTests.Academics;

public sealed class AcademicContextBoundaryTests
{
    private static readonly DateTime ServerNowUtc =
        new(2026, 7, 14, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Resolver_consumes_the_canonical_term_and_window_contracts()
    {
        var reader = ReadyReader(
            Window("open", ServerNowUtc.AddHours(-1), ServerNowUtc.AddHours(1), [1]));
        var resolver = CreateResolver(reader);

        var result = await resolver.ResolveAsync(
            new AcademicStudentScope("AI", "2026"));

        Assert.IsType<TermSummaryDto>(result.TeachingTerm);
        Assert.IsType<TermSummaryDto>(result.RegistrationTerm);
        Assert.IsType<RegistrationWindowSummaryDto>(result.RegistrationWindow);
        Assert.Equal(ServerNowUtc, result.ServerTimeUtc);
        Assert.Equal("Africa/Cairo", result.TimeZoneId);
        Assert.Equal(TermState.Teaching, result.TeachingTerm!.State);
        Assert.Equal(TermState.RegistrationOpen, result.RegistrationTerm!.State);
        Assert.Equal(RegistrationWindowState.Open, result.RegistrationWindowState);
        Assert.Equal(result.RegistrationWindowState, result.RegistrationWindow!.State);
        Assert.Equal("AQ==", result.RegistrationWindow.RowVersion);
        Assert.Equal(ServiceState.Available, result.ServiceState);
    }

    [Fact]
    public async Task Server_time_owns_the_exact_half_open_window_boundary()
    {
        var clock = new MutableTimeProvider(new DateTimeOffset(ServerNowUtc));
        var reader = ReadyReader(
            Window("boundary", ServerNowUtc, ServerNowUtc.AddHours(1), [2]));
        var resolver = CreateResolver(reader, clock);

        var atOpen = await resolver.ResolveAsync(new AcademicStudentScope("AI", "2026"));
        clock.UtcNow = new DateTimeOffset(ServerNowUtc.AddHours(1));
        var atClose = await resolver.ResolveAsync(new AcademicStudentScope("AI", "2026"));

        Assert.Equal(RegistrationWindowState.Open, atOpen.RegistrationWindowState);
        Assert.Equal(RegistrationWindowState.Closed, atClose.RegistrationWindowState);
        Assert.Equal(2, reader.SnapshotReads);
    }

    [Fact]
    public async Task No_registration_term_is_an_authoritative_none_result()
    {
        var reader = new FakeAcademicContextReader
        {
            Terms = [TeachingTerm()]
        };
        var resolver = CreateResolver(reader);

        var result = await resolver.ResolveAsync();
        var publicContext = await resolver.ResolvePublicAsync();

        Assert.Null(result.RegistrationTerm);
        Assert.Null(result.RegistrationWindow);
        Assert.Equal(RegistrationWindowState.None, result.RegistrationWindowState);
        Assert.Null(publicContext.RegistrationTermLabel);
        Assert.Equal(RegistrationWindowState.None, publicContext.RegistrationWindowState);
    }

    [Theory]
    [InlineData(TermState.RegistrationOpen)]
    [InlineData(TermState.Teaching)]
    public async Task Ambiguous_current_term_fails_without_a_partial_context(TermState state)
    {
        var reader = ReadyReader();
        reader.Terms =
        [
            .. reader.Terms,
            Term(Guid.Parse("00000000-0000-0000-0000-000000000099"), "DUP", state, [9])
        ];
        var resolver = CreateResolver(reader);

        var error = await Assert.ThrowsAsync<AcademicContextUnavailableException>(
            () => resolver.ResolveAsync());

        Assert.Equal("CONTEXT_UNAVAILABLE", error.Code);
        Assert.Null(error.PartialContext);
    }

    [Fact]
    public async Task Window_selection_is_open_then_earliest_upcoming_then_latest_closed_with_id_ties()
    {
        var reader = ReadyReader(
            Window("00000000-0000-0000-0000-000000000030", ServerNowUtc.AddDays(2), ServerNowUtc.AddDays(3), [3]),
            Window("00000000-0000-0000-0000-000000000020", ServerNowUtc.AddDays(1), ServerNowUtc.AddDays(3), [2]),
            Window("00000000-0000-0000-0000-000000000010", ServerNowUtc.AddDays(1), ServerNowUtc.AddDays(2), [1]),
            Window("00000000-0000-0000-0000-000000000040", ServerNowUtc.AddDays(-3), ServerNowUtc.AddDays(-1), [4]));
        var resolver = CreateResolver(reader);

        var upcoming = await resolver.ResolveAsync(new AcademicStudentScope("AI", "2026"));
        reader.Windows = reader.Windows
            .Where(window => window.OpensAtUtc < ServerNowUtc)
            .ToArray();
        var closed = await resolver.ResolveAsync(new AcademicStudentScope("AI", "2026"));

        Assert.Equal("00000000-0000-0000-0000-000000000010", upcoming.RegistrationWindow!.Id);
        Assert.Equal(RegistrationWindowState.Upcoming, upcoming.RegistrationWindowState);
        Assert.Equal("00000000-0000-0000-0000-000000000040", closed.RegistrationWindow!.Id);
        Assert.Equal(RegistrationWindowState.Closed, closed.RegistrationWindowState);
    }

    [Fact]
    public async Task Each_request_refetches_current_versions_and_composes_the_shared_app_context()
    {
        var reader = ReadyReader(
            Window("window", ServerNowUtc.AddHours(-1), ServerNowUtc.AddHours(1), [1]));
        var resolver = CreateResolver(reader);

        var first = await resolver.ResolveAsync(new AcademicStudentScope("AI", "2026"));
        reader.Windows =
        [
            Window("window", ServerNowUtc.AddHours(-1), ServerNowUtc.AddHours(1), [2])
        ];
        var current = await resolver.ResolveAsync(new AcademicStudentScope("AI", "2026"));
        var appContext = new AppContextDto(
            current.ServerTimeUtc,
            current.TimeZoneId,
            current.TeachingTerm,
            current.RegistrationTerm,
            current.RegistrationWindowState,
            current.RegistrationWindow,
            current.ServiceState,
            "Synthetic Student",
            ["Student"],
            "Student",
            SessionState.Active,
            ServerNowUtc.AddHours(1),
            "/support/reference");

        Assert.Equal("AQ==", first.RegistrationWindow!.RowVersion);
        Assert.Equal("Ag==", current.RegistrationWindow!.RowVersion);
        Assert.Same(current.RegistrationWindow, appContext.RegistrationWindow);
        Assert.Equal("/support/reference", appContext.SupportReferencePath);
        Assert.Equal(2, reader.SnapshotReads);
    }

    private static AcademicContextResolver CreateResolver(
        FakeAcademicContextReader reader,
        TimeProvider? timeProvider = null) =>
        new(
            reader,
            timeProvider ?? new MutableTimeProvider(new DateTimeOffset(ServerNowUtc)),
            new AcademicContextOptions("Africa/Cairo"));

    private static FakeAcademicContextReader ReadyReader(
        params RegistrationWindowContextRecord[] windows) =>
        new()
        {
            Terms = [TeachingTerm(), RegistrationTerm()],
            Windows = windows
        };

    private static AcademicTermContextRecord TeachingTerm() =>
        Term(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            "2026-T",
            TermState.Teaching,
            [1]);

    private static AcademicTermContextRecord RegistrationTerm() =>
        Term(
            Guid.Parse("00000000-0000-0000-0000-000000000002"),
            "2026-R",
            TermState.RegistrationOpen,
            [2]);

    private static AcademicTermContextRecord Term(
        Guid id,
        string code,
        TermState state,
        byte[] version) =>
        new(
            id,
            code,
            $"{code} label",
            "Africa/Cairo",
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 9, 30),
            state,
            version);

    private static RegistrationWindowContextRecord Window(
        string id,
        DateTime opensAtUtc,
        DateTime closesAtUtc,
        byte[] version) =>
        new(
            Guid.TryParse(id, out var parsedId) ? parsedId : Guid.Parse("00000000-0000-0000-0000-000000000011"),
            Guid.Parse("00000000-0000-0000-0000-000000000002"),
            RegistrationWindowScopeType.Program,
            "AI",
            opensAtUtc,
            closesAtUtc,
            version);
}
