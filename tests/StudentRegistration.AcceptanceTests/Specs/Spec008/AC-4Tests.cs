using System.Reflection;

namespace StudentRegistration.AcceptanceTests.Specs.Spec008;

public sealed class AC_4Tests
{
    private static readonly DateOnly TeachingStartsOn = new(2026, 9, 20);
    private static readonly DateOnly TeachingEndsOn = new(2027, 1, 14);

    [Fact]
    public void Governed_term_edit_requires_permission_provenance_and_current_version()
    {
        RequireAcademicsType(
            "StudentRegistration.Academics.Application.RegistrationWindowService");
        RequireAcademicsType(
            "StudentRegistration.Academics.Application.AdminAcademicManagementService");

        var term = new GovernedTermDouble();
        var denied = term.Update(new TermEditCommand(
            Permission: null,
            Reason: "Publish the approved registration calendar.",
            Source: "demo-registrar",
            ExpectedVersion: 1,
            TeachingStartsOn,
            TeachingEndsOn,
            State: "registrationOpen"));

        Assert.Equal(CommandOutcome.Forbidden, denied);
        Assert.Empty(term.AuditEntries);

        var accepted = term.Update(new TermEditCommand(
            Permission: "AcademicTerms.Manage",
            Reason: "Publish the approved registration calendar.",
            Source: "demo-registrar",
            ExpectedVersion: 1,
            TeachingStartsOn,
            TeachingEndsOn,
            State: "registrationOpen"));

        Assert.Equal(CommandOutcome.Succeeded, accepted);
        Assert.Equal(TeachingStartsOn, term.TeachingStartsOn);
        Assert.Equal(TeachingEndsOn, term.TeachingEndsOn);
        Assert.Equal("registrationOpen", term.State);
        var audit = Assert.Single(term.AuditEntries);
        Assert.Equal("Publish the approved registration calendar.", audit.Reason);
        Assert.Equal("demo-registrar", audit.Source);
        Assert.Equal(1, audit.BeforeVersion);
        Assert.Equal(2, audit.AfterVersion);

        var stale = term.Update(new TermEditCommand(
            Permission: "AcademicTerms.Manage",
            Reason: "Retry the approved registration calendar.",
            Source: "demo-registrar",
            ExpectedVersion: 1,
            TeachingStartsOn,
            TeachingEndsOn,
            State: "registrationOpen"));

        Assert.Equal(CommandOutcome.StaleVersion, stale);
        Assert.Single(term.AuditEntries);
        Assert.Equal(2, term.Version);
    }

    [Fact]
    public void Governed_profile_correction_appends_history_audits_and_rejects_stale_versions()
    {
        RequireAcademicsType(
            "StudentRegistration.Academics.Application.StudentAcademicProfileService");
        RequireAcademicsType(
            "StudentRegistration.Academics.Application.AdminAcademicManagementService");

        var profile = new GovernedProfileDouble();
        var original = profile.Attempts.Single();
        var accepted = profile.Correct(new ProfileCorrectionCommand(
            Permission: "AcademicProfiles.Manage",
            Reason: "Correct the sourced final grade record.",
            Source: "demo-registrar",
            ExpectedStudentVersion: 4,
            ExpectedStudentTermVersion: 7,
            SupersedesAttemptId: original.Id,
            Grade: "B+"));

        Assert.Equal(CommandOutcome.Succeeded, accepted);
        Assert.Equal(2, profile.Attempts.Count);
        Assert.Equal(original, profile.Attempts[0]);
        Assert.Equal(original.Id, profile.Attempts[1].SupersedesAttemptId);
        Assert.Equal("B+", profile.Attempts[1].Grade);
        Assert.Equal(5, profile.StudentVersion);
        Assert.Equal(8, profile.StudentTermVersion);
        var audit = Assert.Single(profile.AuditEntries);
        Assert.Equal("Correct the sourced final grade record.", audit.Reason);
        Assert.Equal("demo-registrar", audit.Source);

        var stale = profile.Correct(new ProfileCorrectionCommand(
            Permission: "AcademicProfiles.Manage",
            Reason: "Retry the sourced final grade correction.",
            Source: "demo-registrar",
            ExpectedStudentVersion: 4,
            ExpectedStudentTermVersion: 7,
            SupersedesAttemptId: original.Id,
            Grade: "B+"));

        Assert.Equal(CommandOutcome.StaleVersion, stale);
        Assert.Equal(2, profile.Attempts.Count);
        Assert.Single(profile.AuditEntries);
    }

    private static void RequireAcademicsType(string fullName)
    {
        var type = Assembly.Load("StudentRegistration.Academics").GetType(fullName);
        Assert.True(type is not null, $"The required Academics service {fullName} is missing.");
    }

    private enum CommandOutcome
    {
        Succeeded,
        Forbidden,
        StaleVersion
    }

    private sealed record AuditEntry(
        string Reason,
        string Source,
        int BeforeVersion,
        int AfterVersion);

    private sealed record TermEditCommand(
        string? Permission,
        string Reason,
        string Source,
        int ExpectedVersion,
        DateOnly TeachingStartsOn,
        DateOnly TeachingEndsOn,
        string State);

    private sealed class GovernedTermDouble
    {
        public int Version { get; private set; } = 1;
        public DateOnly? TeachingStartsOn { get; private set; }
        public DateOnly? TeachingEndsOn { get; private set; }
        public string State { get; private set; } = "draft";
        public List<AuditEntry> AuditEntries { get; } = [];

        public CommandOutcome Update(TermEditCommand command)
        {
            if (command.Permission != "AcademicTerms.Manage")
            {
                return CommandOutcome.Forbidden;
            }

            Assert.False(string.IsNullOrWhiteSpace(command.Reason));
            Assert.False(string.IsNullOrWhiteSpace(command.Source));
            if (command.ExpectedVersion != Version)
            {
                return CommandOutcome.StaleVersion;
            }

            var beforeVersion = Version;
            TeachingStartsOn = command.TeachingStartsOn;
            TeachingEndsOn = command.TeachingEndsOn;
            State = command.State;
            Version++;
            AuditEntries.Add(new(
                command.Reason,
                command.Source,
                beforeVersion,
                Version));
            return CommandOutcome.Succeeded;
        }
    }

    private sealed record TranscriptAttemptDouble(
        Guid Id,
        Guid? SupersedesAttemptId,
        string Grade,
        string Source);

    private sealed record ProfileCorrectionCommand(
        string? Permission,
        string Reason,
        string Source,
        int ExpectedStudentVersion,
        int ExpectedStudentTermVersion,
        Guid SupersedesAttemptId,
        string Grade);

    private sealed class GovernedProfileDouble
    {
        public GovernedProfileDouble()
        {
            Attempts.Add(new(
                Guid.NewGuid(),
                SupersedesAttemptId: null,
                Grade: "B",
                Source: "synthetic-demo"));
        }

        public int StudentVersion { get; private set; } = 4;
        public int StudentTermVersion { get; private set; } = 7;
        public List<TranscriptAttemptDouble> Attempts { get; } = [];
        public List<AuditEntry> AuditEntries { get; } = [];

        public CommandOutcome Correct(ProfileCorrectionCommand command)
        {
            if (command.Permission != "AcademicProfiles.Manage")
            {
                return CommandOutcome.Forbidden;
            }

            Assert.False(string.IsNullOrWhiteSpace(command.Reason));
            Assert.False(string.IsNullOrWhiteSpace(command.Source));
            if (command.ExpectedStudentVersion != StudentVersion ||
                command.ExpectedStudentTermVersion != StudentTermVersion)
            {
                return CommandOutcome.StaleVersion;
            }

            var prior = Assert.Single(
                Attempts,
                attempt => attempt.Id == command.SupersedesAttemptId);
            Attempts.Add(new(
                Guid.NewGuid(),
                prior.Id,
                command.Grade,
                command.Source));
            var beforeVersion = StudentVersion;
            StudentVersion++;
            StudentTermVersion++;
            AuditEntries.Add(new(
                command.Reason,
                command.Source,
                beforeVersion,
                StudentVersion));
            return CommandOutcome.Succeeded;
        }
    }
}
