using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Contracts;
using StudentRegistration.IdentityAccess.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.Persistence.Configurations;

public sealed class AcademicContextModelConfiguration :
    IEntityTypeConfiguration<AcademicTerm>,
    IEntityTypeConfiguration<RegistrationWindow>,
    IEntityTypeConfiguration<Student>,
    IEntityTypeConfiguration<StudentTermAcademicState>,
    IEntityTypeConfiguration<TranscriptAttempt>,
    IEntityTypeConfiguration<StudentHold>
{
    private const string Schema = "academics";

    private static readonly ValueConverter<DateTime, DateTime> UtcDateTimeConverter = new(
        value => value,
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> NullableUtcDateTimeConverter =
        new(
            value => value,
            value => value.HasValue
                ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                : value);

    private static readonly ValueConverter<TermState, string> TermStateConverter = new(
        value => TermStateToToken(value),
        value => TermStateFromToken(value));

    private static readonly ValueConverter<RegistrationWindowScopeType, string>
        WindowScopeConverter = new(
            value => WindowScopeToToken(value),
            value => WindowScopeFromToken(value));

    private static readonly ValueConverter<RegistrationWindowLifecycleState, string>
        WindowStateConverter = new(
            value => WindowStateToToken(value),
            value => WindowStateFromToken(value));

    private static readonly ValueConverter<TranscriptAttemptStatus, string>
        TranscriptStatusConverter = new(
            value => TranscriptStatusToToken(value),
            value => TranscriptStatusFromToken(value));

    public void Configure(EntityTypeBuilder<AcademicTerm> builder)
    {
        builder.ToTable(
            "AcademicTerms",
            Schema,
            table =>
            {
                table.HasCheckConstraint(
                    "CK_AcademicTerms_TeachingRange",
                    "[TeachingEndsOn] > [TeachingStartsOn]");
                table.HasCheckConstraint(
                    "CK_AcademicTerms_State",
                    "[State] IN ('draft', 'registrationOpen', 'registrationClosed', " +
                    "'teaching', 'completed', 'archived')");
            });
        ConfigureGuidKey(builder);

        builder.Property(term => term.Code)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(term => term.CreationClientRequestId)
            .IsRequired();
        builder.Property(term => term.CreationPayloadHash)
            .HasMaxLength(128)
            .IsRequired();
        builder.Property(term => term.DisplayName)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(term => term.TeachingStartsOn)
            .HasColumnType("date")
            .IsRequired();
        builder.Property(term => term.TeachingEndsOn)
            .HasColumnType("date")
            .IsRequired();
        builder.Property(term => term.TimeZoneId)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(term => term.State)
            .HasConversion(TermStateConverter)
            .HasMaxLength(30)
            .IsRequired();
        ConfigureRowVersion(builder.Property(term => term.Version));

        builder.HasIndex(term => term.Code)
            .IsUnique();
        builder.HasIndex(term => term.CreationClientRequestId)
            .IsUnique();
        builder.HasIndex(
                [nameof(AcademicTerm.State)],
                "UX_AcademicTerms_OneRegistrationOpen")
            .IsUnique()
            .HasFilter("[State] = 'registrationOpen'");
        builder.HasIndex(
                [nameof(AcademicTerm.State)],
                "UX_AcademicTerms_OneTeaching")
            .IsUnique()
            .HasFilter("[State] = 'teaching'");
        builder.HasIndex(term => new
        {
            term.State,
            term.TeachingStartsOn,
            term.Id
        });
    }

    public void Configure(EntityTypeBuilder<RegistrationWindow> builder)
    {
        builder.ToTable(
            "RegistrationWindows",
            Schema,
            table =>
            {
                table.HasCheckConstraint(
                    "CK_RegistrationWindows_Range",
                    "[ClosesAtUtc] > [OpensAtUtc]");
                table.HasCheckConstraint(
                    "CK_RegistrationWindows_Scope",
                    "([ScopeType] = 'all-students' AND [ScopeValue] IS NULL) OR " +
                    "([ScopeType] IN ('program', 'cohort') AND [ScopeValue] IS NOT NULL " +
                    "AND LEN(LTRIM(RTRIM([ScopeValue]))) > 0)");
                table.HasCheckConstraint(
                    "CK_RegistrationWindows_State",
                    "[State] IN ('draft', 'published', 'emergencyClosed', 'superseded')");
            });
        ConfigureGuidKey(builder);

        builder.Property(window => window.TermId)
            .IsRequired();
        builder.Property(window => window.ScopeType)
            .HasConversion(WindowScopeConverter)
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(window => window.ScopeValue)
            .HasMaxLength(100);
        ConfigureUtc(builder.Property(window => window.OpensAtUtc));
        ConfigureUtc(builder.Property(window => window.ClosesAtUtc));
        builder.Property(window => window.State)
            .HasConversion(WindowStateConverter)
            .HasMaxLength(30)
            .IsRequired();
        ConfigureRowVersion(builder.Property(window => window.Version));

        builder.HasOne<AcademicTerm>()
            .WithMany()
            .HasForeignKey(window => window.TermId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(window => new
        {
            window.TermId,
            window.State,
            window.OpensAtUtc,
            window.ClosesAtUtc
        });
        builder.HasIndex(window => new { window.TermId, window.Id });
    }

    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable(
            "Students",
            Schema,
            table =>
            {
                table.HasCheckConstraint(
                    "CK_Students_CurrentGpa",
                    "[CurrentGpa] >= 0 AND [CurrentGpa] <= 4");
                table.HasCheckConstraint(
                    "CK_Students_EarnedCredits",
                    "[EarnedCredits] >= 0");
            });
        ConfigureGuidKey(builder);

        builder.Property(student => student.ApplicationUserId)
            .IsRequired();
        builder.Property(student => student.ProgramCode)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(student => student.Cohort)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(student => student.CurrentGpa)
            .HasPrecision(4, 2)
            .IsRequired();
        builder.Property(student => student.EarnedCredits)
            .HasPrecision(6, 2)
            .IsRequired();
        builder.Property(student => student.Standing)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(student => student.IsActive)
            .IsRequired();
        ConfigureProvenance(
            builder.Property(student => student.Source),
            builder.Property(student => student.SourceReference));
        builder.Property(student => student.DataVersion)
            .HasMaxLength(100)
            .IsRequired();
        ConfigureUtc(builder.Property(student => student.DataAsOfUtc));
        ConfigureUtc(builder.Property(student => student.ImportedAtUtc));
        ConfigureRowVersion(builder.Property(student => student.Version));

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Student>(student => student.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(student => student.ApplicationUserId)
            .IsUnique();
        builder.HasIndex(student => new
        {
            student.ProgramCode,
            student.Cohort,
            student.Standing,
            student.Id
        });
    }

    public void Configure(EntityTypeBuilder<StudentTermAcademicState> builder)
    {
        builder.ToTable(
            "StudentTermAcademicStates",
            Schema,
            table =>
            {
                table.HasCheckConstraint(
                    "CK_StudentTermAcademicStates_GpaAtStart",
                    "[GpaAtStart] >= 0 AND [GpaAtStart] <= 4");
                table.HasCheckConstraint(
                    "CK_StudentTermAcademicStates_EarnedCreditsAtStart",
                    "[EarnedCreditsAtStart] >= 0");
                table.HasCheckConstraint(
                    "CK_StudentTermAcademicStates_ProgramTermOrdinal",
                    "[ProgramTermOrdinal] > 0");
            });
        ConfigureGuidKey(builder);

        builder.Property(state => state.StudentId)
            .IsRequired();
        builder.Property(state => state.TermId)
            .IsRequired();
        builder.Property(state => state.ProgramTermOrdinal)
            .IsRequired();
        builder.Property(state => state.GpaAtStart)
            .HasPrecision(4, 2)
            .IsRequired();
        builder.Property(state => state.EarnedCreditsAtStart)
            .HasPrecision(6, 2)
            .IsRequired();
        builder.Property(state => state.StandingAtStart)
            .HasMaxLength(100)
            .IsRequired();
        ConfigureProvenance(
            builder.Property(state => state.Source),
            builder.Property(state => state.SourceReference));
        builder.Property(state => state.DataVersion)
            .HasMaxLength(100)
            .IsRequired();
        ConfigureUtc(builder.Property(state => state.DataAsOfUtc));
        ConfigureRowVersion(builder.Property(state => state.Version));

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(state => state.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AcademicTerm>()
            .WithMany()
            .HasForeignKey(state => state.TermId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(state => new { state.StudentId, state.TermId })
            .IsUnique();
    }

    public void Configure(EntityTypeBuilder<TranscriptAttempt> builder)
    {
        builder.ToTable(
            "TranscriptAttempts",
            Schema,
            table =>
            {
                table.HasCheckConstraint(
                    "CK_TranscriptAttempts_Credits",
                    "[Credits] > 0");
                table.HasCheckConstraint(
                    "CK_TranscriptAttempts_NotSelfSuperseding",
                    "[SupersedesAttemptId] IS NULL OR [SupersedesAttemptId] <> [Id]");
                table.HasCheckConstraint(
                    "CK_TranscriptAttempts_Status",
                    "[Status] IN ('in-progress', 'passed', 'failed', 'withdrawn')");
            });
        ConfigureGuidKey(builder);

        builder.Property(attempt => attempt.StudentId)
            .IsRequired();
        builder.Property(attempt => attempt.TermId)
            .IsRequired();
        builder.Property(attempt => attempt.SupersedesAttemptId);
        builder.Property(attempt => attempt.CourseCode)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(attempt => attempt.Credits)
            .HasPrecision(6, 2)
            .IsRequired();
        builder.Property(attempt => attempt.GradeCode)
            .HasMaxLength(50);
        builder.Property(attempt => attempt.Status)
            .HasConversion(TranscriptStatusConverter)
            .HasMaxLength(30)
            .IsRequired();
        ConfigureProvenance(
            builder.Property(attempt => attempt.Source),
            builder.Property(attempt => attempt.SourceReference));
        ConfigureUtc(builder.Property(attempt => attempt.ImportedAtUtc));

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(attempt => attempt.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AcademicTerm>()
            .WithMany()
            .HasForeignKey(attempt => attempt.TermId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<TranscriptAttempt>()
            .WithMany()
            .HasForeignKey(attempt => attempt.SupersedesAttemptId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(attempt => attempt.SupersedesAttemptId)
            .IsUnique()
            .HasFilter("[SupersedesAttemptId] IS NOT NULL");
        builder.HasIndex(attempt => new { attempt.StudentId, attempt.CourseCode });
        builder.HasIndex(attempt => new
        {
            attempt.StudentId,
            attempt.ImportedAtUtc,
            attempt.Id
        });
    }

    public void Configure(EntityTypeBuilder<StudentHold> builder)
    {
        builder.ToTable(
            "StudentHolds",
            Schema,
            table => table.HasCheckConstraint(
                "CK_StudentHolds_EffectiveRange",
                "[EffectiveToUtc] IS NULL OR [EffectiveToUtc] > [EffectiveFromUtc]"));
        ConfigureGuidKey(builder);

        builder.Property(hold => hold.StudentId)
            .IsRequired();
        builder.Property(hold => hold.TermId)
            .IsRequired();
        builder.Property(hold => hold.Code)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(hold => hold.Message)
            .HasMaxLength(500)
            .IsRequired();
        builder.Property(hold => hold.BlocksRegistration)
            .IsRequired();
        ConfigureUtc(builder.Property(hold => hold.EffectiveFromUtc));
        ConfigureNullableUtc(builder.Property(hold => hold.EffectiveToUtc));
        ConfigureProvenance(
            builder.Property(hold => hold.Source),
            builder.Property(hold => hold.SourceReference));
        ConfigureUtc(builder.Property(hold => hold.ImportedAtUtc));

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(hold => hold.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AcademicTerm>()
            .WithMany()
            .HasForeignKey(hold => hold.TermId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(hold => new
        {
            hold.StudentId,
            hold.TermId,
            hold.EffectiveFromUtc,
            hold.EffectiveToUtc
        });
    }

    private static void ConfigureGuidKey<TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        builder.HasKey("Id");
        builder.Property<Guid>("Id")
            .ValueGeneratedNever();
    }

    private static void ConfigureRowVersion(PropertyBuilder<byte[]> property) =>
        property.IsRowVersion().IsRequired();

    private static void ConfigureUtc(PropertyBuilder<DateTime> property) =>
        property.HasConversion(UtcDateTimeConverter)
            .HasColumnType("datetime2")
            .IsRequired();

    private static void ConfigureNullableUtc(PropertyBuilder<DateTime?> property) =>
        property.HasConversion(NullableUtcDateTimeConverter)
            .HasColumnType("datetime2");

    private static void ConfigureProvenance(
        PropertyBuilder<string> source,
        PropertyBuilder<string> sourceReference)
    {
        source.HasMaxLength(200).IsRequired();
        sourceReference.HasMaxLength(200).IsRequired();
    }

    private static string TermStateToToken(TermState value) => value switch
    {
        TermState.Draft => "draft",
        TermState.RegistrationOpen => "registrationOpen",
        TermState.RegistrationClosed => "registrationClosed",
        TermState.Teaching => "teaching",
        TermState.Completed => "completed",
        TermState.Archived => "archived",
        _ => throw new InvalidOperationException("Unknown academic-term state.")
    };

    private static TermState TermStateFromToken(string value) => value switch
    {
        "draft" => TermState.Draft,
        "registrationOpen" => TermState.RegistrationOpen,
        "registrationClosed" => TermState.RegistrationClosed,
        "teaching" => TermState.Teaching,
        "completed" => TermState.Completed,
        "archived" => TermState.Archived,
        _ => throw new InvalidOperationException("Unknown persisted academic-term state.")
    };

    private static string WindowScopeToToken(RegistrationWindowScopeType value) => value switch
    {
        RegistrationWindowScopeType.AllStudents => "all-students",
        RegistrationWindowScopeType.Program => "program",
        RegistrationWindowScopeType.Cohort => "cohort",
        _ => throw new InvalidOperationException("Unknown registration-window scope.")
    };

    private static RegistrationWindowScopeType WindowScopeFromToken(string value) => value switch
    {
        "all-students" => RegistrationWindowScopeType.AllStudents,
        "program" => RegistrationWindowScopeType.Program,
        "cohort" => RegistrationWindowScopeType.Cohort,
        _ => throw new InvalidOperationException("Unknown persisted registration-window scope.")
    };

    private static string WindowStateToToken(RegistrationWindowLifecycleState value) => value switch
    {
        RegistrationWindowLifecycleState.Draft => "draft",
        RegistrationWindowLifecycleState.Published => "published",
        RegistrationWindowLifecycleState.EmergencyClosed => "emergencyClosed",
        RegistrationWindowLifecycleState.Superseded => "superseded",
        _ => throw new InvalidOperationException("Unknown registration-window state.")
    };

    private static RegistrationWindowLifecycleState WindowStateFromToken(string value) => value switch
    {
        "draft" => RegistrationWindowLifecycleState.Draft,
        "published" => RegistrationWindowLifecycleState.Published,
        "emergencyClosed" => RegistrationWindowLifecycleState.EmergencyClosed,
        "superseded" => RegistrationWindowLifecycleState.Superseded,
        _ => throw new InvalidOperationException("Unknown persisted registration-window state.")
    };

    private static string TranscriptStatusToToken(TranscriptAttemptStatus value) => value switch
    {
        TranscriptAttemptStatus.InProgress => "in-progress",
        TranscriptAttemptStatus.Passed => "passed",
        TranscriptAttemptStatus.Failed => "failed",
        TranscriptAttemptStatus.Withdrawn => "withdrawn",
        _ => throw new InvalidOperationException("Unknown transcript-attempt status.")
    };

    private static TranscriptAttemptStatus TranscriptStatusFromToken(string value) => value switch
    {
        "in-progress" => TranscriptAttemptStatus.InProgress,
        "passed" => TranscriptAttemptStatus.Passed,
        "failed" => TranscriptAttemptStatus.Failed,
        "withdrawn" => TranscriptAttemptStatus.Withdrawn,
        _ => throw new InvalidOperationException("Unknown persisted transcript-attempt status.")
    };
}
