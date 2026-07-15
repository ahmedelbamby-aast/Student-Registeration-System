using System.Reflection;
using StudentRegistration.Contracts;

namespace StudentRegistration.AcceptanceTests.Specs.Spec008;

public sealed class AC_1Tests
{
    [Fact]
    public void Server_instant_not_device_clock_controls_the_half_open_window()
    {
        var opensAtUtc = new DateTime(2026, 7, 14, 8, 0, 0, DateTimeKind.Utc);
        var closesAtUtc = new DateTime(2026, 7, 14, 18, 0, 0, DateTimeKind.Utc);
        var assembly = Assembly.Load("StudentRegistration.Academics");
        var windowType = assembly.GetType(
            "StudentRegistration.Academics.Domain.RegistrationWindow");
        var scopeType = assembly.GetType(
            "StudentRegistration.Academics.Domain.RegistrationWindowScopeType");
        var lifecycleType = assembly.GetType(
            "StudentRegistration.Academics.Domain.RegistrationWindowLifecycleState");
        Assert.NotNull(windowType);
        Assert.NotNull(scopeType);
        Assert.NotNull(lifecycleType);

        var window = Activator.CreateInstance(
            windowType!,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Enum.Parse(scopeType!, "AllStudents"),
            null,
            opensAtUtc,
            closesAtUtc,
            Enum.Parse(lifecycleType!, "Published"));
        Assert.NotNull(window);
        var getComputedState = windowType!.GetMethod(
            "GetComputedState",
            BindingFlags.Public | BindingFlags.Instance,
            [typeof(DateTime)]);
        Assert.NotNull(getComputedState);

        var authoritativeServerTime = opensAtUtc;
        var deviceClockOneDayAhead = authoritativeServerTime.AddDays(1);

        Assert.Equal(
            RegistrationWindowState.Open,
            getComputedState!.Invoke(window, [authoritativeServerTime]));
        Assert.Equal(
            RegistrationWindowState.Closed,
            getComputedState.Invoke(window, [deviceClockOneDayAhead]));
        Assert.Equal(
            RegistrationWindowState.Open,
            getComputedState.Invoke(window, [closesAtUtc.AddTicks(-1)]));
        Assert.Equal(
            RegistrationWindowState.Closed,
            getComputedState.Invoke(window, [closesAtUtc]));
    }
}
