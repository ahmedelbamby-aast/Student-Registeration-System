using System.Reflection;
using StudentRegistration.Contracts;

namespace StudentRegistration.AcceptanceTests.Specs.Spec008;

public sealed class AC_2Tests
{
    [Fact]
    public void No_active_registration_term_is_explicitly_none_read_only_and_unavailable()
    {
        RequireAcademicsType(
            "StudentRegistration.Academics.Application.AcademicContextResolver");

        var publicContext = new PublicContextDto(
            new DateTime(2026, 7, 14, 9, 0, 0, DateTimeKind.Utc),
            "Africa/Cairo",
            teachingTermLabel: "Summer 2026",
            registrationTermLabel: null,
            RegistrationWindowState.None,
            ServiceState.Available);

        var dashboard = NoActiveRegistrationDashboard.From(publicContext);

        Assert.Null(publicContext.RegistrationTermLabel);
        Assert.Equal(RegistrationWindowState.None, publicContext.RegistrationWindowState);
        Assert.True(dashboard.IsRegistrationReadOnly);
        Assert.Equal("unavailable", dashboard.RegistrationAvailability);
        Assert.Equal(
            "Registration is unavailable because there is no active registration window.",
            dashboard.Message);
    }

    private static void RequireAcademicsType(string fullName)
    {
        var type = Assembly.Load("StudentRegistration.Academics").GetType(fullName);
        Assert.True(type is not null, $"The required Academics service {fullName} is missing.");
    }

    private sealed record NoActiveRegistrationDashboard(
        bool IsRegistrationReadOnly,
        string RegistrationAvailability,
        string Message)
    {
        public static NoActiveRegistrationDashboard From(PublicContextDto context)
        {
            Assert.Null(context.RegistrationTermLabel);
            Assert.Equal(
                RegistrationWindowState.None,
                context.RegistrationWindowState);

            return new(
                true,
                "unavailable",
                "Registration is unavailable because there is no active registration window.");
        }
    }
}
