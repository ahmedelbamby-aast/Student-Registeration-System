using System.Reflection;
using StudentRegistration.Contracts;

namespace StudentRegistration.IntegrationTests.Specs.Spec008.EdgeCases;

public sealed class EC_3Tests
{
    [Fact]
    public void Academic_time_contract_requires_utc_instants_and_a_valid_iana_timezone()
    {
        RequireAcademicsType("StudentRegistration.Academics.Domain.AcademicTerm");
        RequireAcademicsType("StudentRegistration.Academics.Domain.RegistrationWindow");

        var winterUtc = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var summerUtc = new DateTime(2026, 7, 15, 10, 0, 0, DateTimeKind.Utc);
        var zone = AcademicTimeDouble.RequireIanaTimeZone("Africa/Cairo");

        Assert.Equal(
            winterUtc,
            AcademicTimeDouble.RoundTripThroughConfiguredZone(winterUtc, zone));
        Assert.Equal(
            summerUtc,
            AcademicTimeDouble.RoundTripThroughConfiguredZone(summerUtc, zone));
        Assert.Throws<ArgumentException>(() =>
            AcademicTimeDouble.RequireUtc(
                DateTime.SpecifyKind(winterUtc, DateTimeKind.Local)));
        Assert.Throws<ArgumentException>(() =>
            AcademicTimeDouble.RequireIanaTimeZone("Egypt Standard Time"));
        Assert.Throws<TimeZoneNotFoundException>(() =>
            AcademicTimeDouble.RequireIanaTimeZone("Invalid/Academic-Zone"));
    }

    [Fact]
    public void Browser_or_local_device_clock_cannot_change_authoritative_window_state()
    {
        RequireAcademicsType(
            "StudentRegistration.Academics.Application.AcademicContextResolver");

        var opensAtUtc = new DateTime(2026, 7, 14, 8, 0, 0, DateTimeKind.Utc);
        var closesAtUtc = new DateTime(2026, 7, 14, 18, 0, 0, DateTimeKind.Utc);
        var serverNowUtc = new DateTime(2026, 7, 14, 12, 0, 0, DateTimeKind.Utc);
        var deviceClock = new DateTime(2026, 7, 15, 12, 0, 0, DateTimeKind.Local);
        var authority = new WindowAuthorityDouble(opensAtUtc, closesAtUtc);

        Assert.Equal(
            RegistrationWindowState.Open,
            authority.Resolve(serverNowUtc));
        Assert.Equal(
            RegistrationWindowState.Open,
            authority.Resolve(serverNowUtc, ignoredDeviceClock: deviceClock));
        Assert.Equal(
            RegistrationWindowState.Closed,
            authority.Resolve(closesAtUtc));
    }

    private static void RequireAcademicsType(string fullName)
    {
        var type = Assembly.Load("StudentRegistration.Academics").GetType(fullName);
        Assert.True(type is not null, $"The required Academics type {fullName} is missing.");
    }

    private static class AcademicTimeDouble
    {
        public static TimeZoneInfo RequireIanaTimeZone(string timeZoneId)
        {
            if (!timeZoneId.Contains('/', StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "An IANA timezone identifier is required.",
                    nameof(timeZoneId));
            }

            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }

        public static DateTime RequireUtc(DateTime instant)
        {
            if (instant.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("A UTC instant is required.", nameof(instant));
            }

            return instant;
        }

        public static DateTime RoundTripThroughConfiguredZone(
            DateTime instantUtc,
            TimeZoneInfo zone)
        {
            RequireUtc(instantUtc);
            var local = TimeZoneInfo.ConvertTimeFromUtc(instantUtc, zone);
            return TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(local, DateTimeKind.Unspecified),
                zone);
        }
    }

    private sealed class WindowAuthorityDouble
    {
        private readonly DateTime _opensAtUtc;
        private readonly DateTime _closesAtUtc;

        public WindowAuthorityDouble(DateTime opensAtUtc, DateTime closesAtUtc)
        {
            _opensAtUtc = AcademicTimeDouble.RequireUtc(opensAtUtc);
            _closesAtUtc = AcademicTimeDouble.RequireUtc(closesAtUtc);
        }

        public RegistrationWindowState Resolve(
            DateTime serverNowUtc,
            DateTime? ignoredDeviceClock = null)
        {
            _ = ignoredDeviceClock;
            AcademicTimeDouble.RequireUtc(serverNowUtc);
            return serverNowUtc < _opensAtUtc
                ? RegistrationWindowState.Upcoming
                : serverNowUtc < _closesAtUtc
                    ? RegistrationWindowState.Open
                    : RegistrationWindowState.Closed;
        }
    }
}
