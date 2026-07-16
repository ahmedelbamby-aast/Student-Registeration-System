using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Contracts;
using StudentRegistration.Infrastructure.SqlServer.Migrations;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec008;

public sealed class NFR_3EvidenceTests
{
    private const string EvidencePath = "docs/release-evidence/SPEC-008-NFR-3.md";
    private const string TestPath =
        "tests/StudentRegistration.QualityTests/Specs/Spec008/NFR-3EvidenceTests.cs";
    private const string MappingPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/AcademicContextModelConfiguration.cs";
    private const string TermPath =
        "src/StudentRegistration.Academics/Domain/AcademicTerm.cs";
    private const string ResolverPath =
        "src/StudentRegistration.Academics/Application/AcademicContextResolver.cs";
    private const string MigrationPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713010000_IdentityAcademicFoundation.cs";
    private const string ModelConnectionString =
        "Server=localhost;Database=Spec008Nfr3Model;User Id=sa;Password=NotUsed!42;TrustServerCertificate=True";
    private const string InstitutionalTimeZone = "Africa/Cairo";

    private static readonly (Type Entity, string Property, bool Nullable)[]
        UtcInstantProperties =
        [
            (typeof(RegistrationWindow), nameof(RegistrationWindow.OpensAtUtc), false),
            (typeof(RegistrationWindow), nameof(RegistrationWindow.ClosesAtUtc), false),
            (typeof(Student), nameof(Student.DataAsOfUtc), false),
            (typeof(Student), nameof(Student.ImportedAtUtc), false),
            (typeof(StudentTermAcademicState), nameof(StudentTermAcademicState.DataAsOfUtc), false),
            (typeof(TranscriptAttempt), nameof(TranscriptAttempt.ImportedAtUtc), false),
            (typeof(StudentHold), nameof(StudentHold.EffectiveFromUtc), false),
            (typeof(StudentHold), nameof(StudentHold.EffectiveToUtc), true),
            (typeof(StudentHold), nameof(StudentHold.ImportedAtUtc), false)
        ];

    [Fact]
    public void Every_spec008_instant_uses_datetime2_and_materializes_as_utc()
    {
        using var context = CreateContext();
        var sourceUtc = new DateTime(2042, 10, 27, 23, 45, 12, 345, DateTimeKind.Utc)
            .AddTicks(6_789);

        Assert.Equal(9, UtcInstantProperties.Length);
        foreach (var expected in UtcInstantProperties)
        {
            var entity = context.Model.FindEntityType(expected.Entity);
            Assert.NotNull(entity);
            var property = entity!.FindProperty(expected.Property);
            Assert.NotNull(property);
            Assert.Equal("datetime2", property!.GetColumnType());
            Assert.Equal(expected.Nullable, property.IsNullable);

            var converter = property.GetValueConverter() ??
                property.GetTypeMapping().Converter;
            Assert.NotNull(converter);
            var providerValue = Assert.IsType<DateTime>(
                converter!.ConvertToProvider(sourceUtc));
            var sqlMaterializedValue = DateTime.SpecifyKind(
                providerValue,
                DateTimeKind.Unspecified);
            var roundTripped = Assert.IsType<DateTime>(
                converter.ConvertFromProvider(sqlMaterializedValue));

            Assert.Equal(sourceUtc.Ticks, roundTripped.Ticks);
            Assert.Equal(DateTimeKind.Utc, roundTripped.Kind);
            if (expected.Nullable)
            {
                Assert.Null(converter.ConvertFromProvider(null));
            }
        }
    }

    [Fact]
    public void Delivered_migration_preserves_the_same_datetime2_and_timezone_schema()
    {
        var operations = new ExposedIdentityAcademicFoundation().BuildOperations();
        var academicTables = operations
            .OfType<CreateTableOperation>()
            .Where(operation => operation.Schema == "academics")
            .ToDictionary(operation => operation.Name, StringComparer.Ordinal);

        Assert.Equal(6, academicTables.Count);
        var migratedInstants = academicTables.Values
            .SelectMany(table => table.Columns.Select(column => new
            {
                Table = table.Name,
                Column = column.Name,
                column.ColumnType,
                column.IsNullable
            }))
            .Where(column => column.Column.EndsWith("Utc", StringComparison.Ordinal))
            .OrderBy(column => column.Table, StringComparer.Ordinal)
            .ThenBy(column => column.Column, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(9, migratedInstants.Length);
        Assert.All(migratedInstants, column => Assert.Equal("datetime2", column.ColumnType));
        Assert.Single(migratedInstants, column => column.IsNullable);
        Assert.Equal("EffectiveToUtc", Assert.Single(
            migratedInstants,
            column => column.IsNullable).Column);

        var termTable = academicTables["AcademicTerms"];
        var timeZone = Assert.Single(
            termTable.Columns,
            column => column.Name == nameof(AcademicTerm.TimeZoneId));
        Assert.Equal("nvarchar(100)", timeZone.ColumnType);
        Assert.Equal(100, timeZone.MaxLength);
        Assert.False(timeZone.IsNullable);
    }

    [Fact]
    public void Academic_term_persists_an_exact_resolvable_iana_identifier()
    {
        var term = Term(InstitutionalTimeZone);
        var resolved = TimeZoneInfo.FindSystemTimeZoneById(term.TimeZoneId);
        Assert.Equal(InstitutionalTimeZone, term.TimeZoneId);
        Assert.Equal(InstitutionalTimeZone, resolved.Id);

        using var context = CreateContext();
        var property = context.Model
            .FindEntityType(typeof(AcademicTerm))!
            .FindProperty(nameof(AcademicTerm.TimeZoneId))!;
        Assert.Equal("nvarchar(100)", property.GetColumnType());
        Assert.Equal(100, property.GetMaxLength());
        Assert.False(property.IsNullable);
        Assert.Null(property.GetValueConverter());
        Assert.Equal(InstitutionalTimeZone, property.GetGetter().GetClrValue(term));

        Assert.Throws<ArgumentException>(() => Term("Egypt Standard Time"));
        Assert.Throws<ArgumentException>(() => Term("Mars/Olympus"));
    }

    [Fact]
    public async Task Resolver_uses_the_matching_iana_term_and_fails_closed_on_mismatch()
    {
        var clock = new FixedTimeProvider(
            new DateTimeOffset(2042, 10, 28, 9, 0, 0, TimeSpan.Zero));
        var matching = new AcademicContextResolver(
            new SingleTermReader(TermRecord(InstitutionalTimeZone)),
            clock,
            new AcademicContextOptions(InstitutionalTimeZone));
        var context = await matching.ResolveAsync();

        Assert.Equal(InstitutionalTimeZone, context.TimeZoneId);
        Assert.Equal("2042-FALL", context.RegistrationTerm!.Code);
        Assert.Equal(DateTimeKind.Utc, context.ServerTimeUtc.Kind);

        var mismatched = new AcademicContextResolver(
            new SingleTermReader(TermRecord("Europe/London")),
            clock,
            new AcademicContextOptions(InstitutionalTimeZone));
        var failure = await Assert.ThrowsAsync<AcademicContextUnavailableException>(
            () => mismatched.ResolveAsync());
        Assert.Equal("CONTEXT_UNAVAILABLE", failure.Code);
        Assert.Null(failure.PartialContext);
    }

    [Fact]
    public void Evidence_is_bound_to_delivered_sources_and_rejects_placeholders()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        Assert.DoesNotMatch(
            new Regex(
                @"\b(?:PENDING|TODO|TBD|FIXME|PLACEHOLDER|NOT\s+RUN|NOT\s+EXECUTED)\b",
                RegexOptions.IgnoreCase),
            evidence);
        Assert.DoesNotMatch(@"\{\{[^}]+\}\}", evidence);
        Assert.DoesNotMatch(@"(?i)<\s*insert\b[^>]*>", evidence);

        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-008 NFR-3 UTC and IANA Timezone Evidence",
            "UTC `datetime2`",
            "DateTimeKind.Utc",
            "Africa/Cairo",
            "CONTEXT_UNAVAILABLE",
            "5 passed",
            "0 failed",
            "Recurring `DayOfWeek`/`TimeOnly` persistence evidence remains SPEC-010-owned.",
            $"NFR-3 test normalized-LF SHA-256: `{SourceHash(TestPath)}`",
            $"Academic mapping normalized-LF SHA-256: `{SourceHash(MappingPath)}`",
            $"AcademicTerm normalized-LF SHA-256: `{SourceHash(TermPath)}`",
            $"Context resolver normalized-LF SHA-256: `{SourceHash(ResolverPath)}`",
            $"Foundation migration normalized-LF SHA-256: `{SourceHash(MigrationPath)}`",
            "**Result: PASS.**");
    }

    private static StudentRegistrationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(ModelConnectionString)
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    private static AcademicTerm Term(string timeZoneId) => new(
        Guid.Parse("00000000-0000-0000-0000-000000008301"),
        "2042-FALL",
        Guid.Parse("00000000-0000-0000-0000-000000008302"),
        "SPEC008-NFR3-PAYLOAD",
        "Fall 2042",
        new DateOnly(2042, 9, 20),
        new DateOnly(2043, 1, 15),
        timeZoneId,
        TermState.RegistrationOpen);

    private static AcademicTermContextRecord TermRecord(string timeZoneId) => new(
        Guid.Parse("00000000-0000-0000-0000-000000008301"),
        "2042-FALL",
        "Fall 2042",
        timeZoneId,
        new DateOnly(2042, 9, 20),
        new DateOnly(2043, 1, 15),
        TermState.RegistrationOpen,
        [3]);

    private static string SourceHash(string relativePath)
    {
        var normalized = RepositoryFiles.Read(relativePath)
            .Replace("\r\n", "\n", StringComparison.Ordinal);
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class SingleTermReader(AcademicTermContextRecord term)
        : IAcademicContextReader
    {
        public Task<AcademicContextSnapshot> ResolveContextAsync(
            AcademicStudentScope? studentScope,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new AcademicContextSnapshot([term], []));
        }
    }

    private sealed class ExposedIdentityAcademicFoundation
        : IdentityAcademicFoundation
    {
        public IReadOnlyList<MigrationOperation> BuildOperations()
        {
            var builder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");
            base.Up(builder);
            return builder.Operations;
        }
    }
}
