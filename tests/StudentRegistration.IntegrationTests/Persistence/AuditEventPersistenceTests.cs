using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using StudentRegistration.Infrastructure.SqlServer.Audit;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Persistence;

public sealed class AuditEventPersistenceTests
{
    private const string MappingPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/AuditEventModelConfiguration.cs";
    private const string WriterPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Audit/AuditTransactionWriter.cs";

    [Fact]
    public void Mapping_is_append_only_and_matches_the_canonical_model()
    {
        var mapping = RepositoryFiles.Read(MappingPath);
        RepositoryFiles.ContainsAll(
            mapping,
            "IEntityTypeConfiguration<AuditEvent>",
            "ToTable(\"AuditEvents\", \"audit\")",
            "HasKey",
            "ValueGeneratedNever",
            "ActorReference",
            "SubjectReference",
            "Action",
            "EntityType",
            "EntityId",
            "Reason",
            "BeforeSummaryJson",
            "AfterSummaryJson",
            "CorrelationId",
            "OccurredAtUtc",
            "IsRequired",
            "HasMaxLength");

        var writer = RepositoryFiles.Read(WriterPath);
        RepositoryFiles.ContainsAll(
            writer,
            "IAuditEventWriter",
            "Database.CurrentTransaction",
            "AuditEvents.Add",
            "AUDIT_CALLER_TRANSACTION_REQUIRED");
        Assert.DoesNotContain("SaveChanges", writer, StringComparison.Ordinal);
        Assert.DoesNotContain("BeginTransaction", writer, StringComparison.Ordinal);
        Assert.DoesNotContain("Commit", writer, StringComparison.Ordinal);
        Assert.DoesNotContain("Remove", writer, StringComparison.Ordinal);
        Assert.DoesNotContain("Update", writer, StringComparison.Ordinal);
        Assert.DoesNotContain("SPEC-017", writer, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("StaffAdministration", writer, StringComparison.Ordinal);
    }

    [Fact]
    public void Runtime_model_has_one_required_append_only_audit_mapping()
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(
                "Server=localhost;Database=Spec004ModelOnly;User Id=sa;Password=NotUsed!42;TrustServerCertificate=True")
            .Options;
        using var context = new StudentRegistrationDbContext(options);

        var entity = context.Model.FindEntityType(typeof(AuditEvent));
        Assert.NotNull(entity);
        Assert.Equal("AuditEvents", entity.GetTableName());
        Assert.Equal("audit", entity.GetSchema());
        Assert.Single(entity.FindPrimaryKey()!.Properties);
        Assert.Equal(ValueGenerated.Never, entity.FindProperty(nameof(AuditEvent.Id))!.ValueGenerated);

        var requiredProperties = new[]
        {
            nameof(AuditEvent.ActorReference),
            nameof(AuditEvent.SubjectReference),
            nameof(AuditEvent.Action),
            nameof(AuditEvent.EntityType),
            nameof(AuditEvent.EntityId),
            nameof(AuditEvent.Reason),
            nameof(AuditEvent.CorrelationId),
            nameof(AuditEvent.OccurredAtUtc)
        };
        Assert.All(
            requiredProperties,
            propertyName => Assert.False(entity.FindProperty(propertyName)!.IsNullable));
    }

    [Fact]
    public void Audit_contract_and_writer_do_not_depend_on_downstream_spec_017()
    {
        var port = RepositoryFiles.Read(
            "src/StudentRegistration.Contracts/Auditing/IAuditEventWriter.cs");
        var writer = RepositoryFiles.Read(WriterPath);
        var mapping = RepositoryFiles.Read(MappingPath);

        foreach (var source in new[] { port, writer, mapping })
        {
            Assert.DoesNotContain("SPEC-017", source, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("StaffAdministration", source, StringComparison.Ordinal);
        }
    }
}
